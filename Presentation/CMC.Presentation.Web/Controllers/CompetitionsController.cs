using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Controllers;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Core.Wrappers;
using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Kernel.Infrastructure.Persistence.Services;
using CMC.Presentation.Application.ActionFilters;
using CMC.Presentation.Application.DTOs.Competitions;
using CMC.Presentation.Application.DTOs.Identity;
using CMC.Presentation.Application.DTOs.Players;
using CMC.Presentation.Application.DTOs.Questions;
using CMC.Presentation.Application.Services.Competitions;
using CMC.Presentation.Application.Services.Identity.Interfaces;
using CMC.Presentation.Application.Services.Players;
using CMC.Presentation.Application.Services.Questions;
using CMC.Presentation.Application.Services.Settings;
using CMC.Presentation.Domain.Entities;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Diagnostics.Tracing.Parsers.AspNet;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Pipelines.Sockets.Unofficial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CMC.Presentation.Web.Controllers
{
    public class CompetitionsController : BaseController
    {
        readonly IPlayerService _playerService;
        readonly ICompetitionsService _competitionsService;
        readonly ILookupsService _lookupService;
        readonly IApplicationLogger _logger;
        readonly IStringLocalizer<PlayersController> _localizer;
        readonly ISettingsService _settingService;
        readonly IQuestionsService _questionsService;
        readonly IUserService _userService;
        readonly ICompositeViewEngine _viewEngine;
        readonly ICompetitionUpdateQueue _competitionUpdateQueue;


        public static IHttpContextAccessor _httpContextAccessor { get { return new HttpContextAccessor(); } }

        public CompetitionsController(IPlayerService playerService,
            ICompetitionsService competitionsService,
            ILookupsService lookupService,
            IUserService userService,
            IApplicationLogger logger,
            IStringLocalizer<PlayersController> localizer,
            ISettingsService settingsService,
            IQuestionsService questionsService,
            ICompetitionUpdateQueue competitionUpdateQueue,
            ICompositeViewEngine viewEngine)
        {
            _competitionsService = competitionsService;
            _lookupService = lookupService;
            _playerService = playerService;
            _userService = userService;
            _logger = logger;
            _localizer = localizer;
            _settingService = settingsService;
            _questionsService = questionsService;
            _competitionUpdateQueue = competitionUpdateQueue;
            _viewEngine = viewEngine;

        }

        [RolePermission(PermissionCodes.WebCompetitionView)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var loggedInUser = await _userService.GetLoggedInUser();
                ViewBag.Hosts = await _userService.GetHosts();
                ViewData["CanDelete"] = loggedInUser.PermissionCodes.Contains(PermissionCodes.WebCompetitionDelete);
                ViewData["CanCreate"] = loggedInUser.PermissionCodes.Contains(PermissionCodes.WebCompetitionCreate);
                ViewData["IsHost"] = loggedInUser.GroupCode == GroupsEnum.Host;
                return View();
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-CompetitionController", null, null, false);
                return RedirectToAction("Index", "Error");
            }
        }

        [RolePermission(PermissionCodes.WebCompetitionView)]
        [HttpGet]
        public async Task<IActionResult> GetAllCompetitions([FromQuery] SearchCompetitionDTO searchCompetitionDTO)
        {
            try
            {
                var loggedInUser = await _userService.GetLoggedInUser();
                if (loggedInUser.GroupCode == GroupsEnum.Host)
                    searchCompetitionDTO.HostId = loggedInUser.Id;

                var result = await _competitionsService.GetCompetitions(searchCompetitionDTO);
                return Json(result);
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-GetAllCompetitions", searchCompetitionDTO, null, false);
                return Json(new { });
            }
        }

        [RolePermission(PermissionCodes.WebCompetitionCreate)]
        public async Task<IActionResult> CreateCompetition(int? id)
        {
            try
            {
                CompetitionsDTO competitionsDTO = new CompetitionsDTO();
                if (id.HasValue)
                {
                    var data = await _competitionsService.GetCompetition(id.Value);
                    if (data.Succeeded)
                        competitionsDTO = data.Data;
                }

                var cityMallTeam = await _playerService.GetPlayers(true);
                var otherTeam = await _playerService.GetPlayers(false);
                competitionsDTO.CityMallTeam = cityMallTeam.Data;
                competitionsDTO.OtherTeam = otherTeam.Data;
                competitionsDTO.Hosts = await _userService.GetHosts();
                competitionsDTO.CompetitionQuestionTypes = new List<LookupModel>()
                {
                    new LookupModel()
                    {
                        Id = (int)CompetitionQuestionType.Rounds,
                        Name = _localizer["CompeitionQuestionsRound"].Value
                    },
                    new LookupModel()
                    {
                        Id = (int)CompetitionQuestionType.QuestionsPerPlayer,
                        Name = _localizer["CompeitionQuestionsPerPlayer"].Value
                    }
                };

                competitionsDTO.Categories = await _questionsService.GetCategories(false);
                var ParentCompetition = await _competitionsService.GetCompetitionsLookup();
                if (ParentCompetition.Succeeded)
                    competitionsDTO.ParentCompetition = ParentCompetition.Data;

                competitionsDTO.QuestionArchiveType = await _lookupService.GetLookupItems(LookupTypes.QuestionArchiveType);

                // Populate Available Competitions for exclusion (exclude current competition if editing)
                var availableCompetitions = await _competitionsService.GetCompetitionsLookup();
                if (availableCompetitions.Succeeded)
                {
                    competitionsDTO.AvailableCompetitions = id.HasValue ?
                        availableCompetitions.Data.Where(c => c.Id != id.Value).ToList() :
                        availableCompetitions.Data;
                }

                var lastScores = await _competitionsService.GetLatestScores();
                if (lastScores.Succeeded)
                    competitionsDTO.LatestScores = lastScores.Data;

                return View("Create", competitionsDTO);
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-CreateCompetition", id, null, false);
                return RedirectToAction("Index", "Error");
            }
        }


        [HttpPost]
        [RolePermission(PermissionCodes.WebCompetitionCreate)]
        public async Task<IActionResult> CreateCompetition(CompetitionsDTO competitionsDTO)
        {
            try
            {
                var result = await _competitionsService.AddOrUpdateCompetition(competitionsDTO);
                string msg = competitionsDTO.Id.HasValue ? _localizer["CompetitionHasBeenUpdatedSuccessfully"].Value : _localizer["CompetitionHasBeenCreatedSuccessfully"].Value;
                return Json(new { isSuccess = result.Succeeded, resultCode = result.StatusCode, brokenRoles = result.BrokenRules, msg = msg });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-CreateCompetition", competitionsDTO, null, false);
                return Json(new { isSuccess = false });
            }
        }


        [HttpDelete]
        [RolePermission(PermissionCodes.WebCompetitionDelete)]
        public async Task<IActionResult> DeleteCompetition(int id)
        {
            try
            {
                var result = await _competitionsService.DeleteCompetition(id);
                return Json(new { isSuccess = result.Succeeded, msg = _localizer["Alert_CompetitionDeletedSuccessfully"].Value });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-DeleteCompetition", id, null, false);
                return Json(new { isSuccess = false });
            }
        }

        [RolePermission(PermissionCodes.WebCompetitionStart)]
        public async Task<IActionResult> StartCompetition(int id)
        {
            try
            {
                _httpContextAccessor.HttpContext.Session.Remove("CompetitionScoreDetails");
                var competitionStart = await _competitionsService.StartCompetiton(id);
                string viewContent = string.Empty;
                if (competitionStart.Succeeded)
                {
                    if (competitionStart.Data.IsSessionData)
                    {
                        CompetitionStateDto competitionStateDto = new CompetitionStateDto()
                        {
                            Id = competitionStart.Data.Id,
                            CityMallPlayed = competitionStart.Data.CityMallPlayed,
                            CurrentStep = competitionStart.Data.CurrentStep,
                            cityMallSelectedId = competitionStart.Data.cityMallSelectedId,
                            IsBattled = competitionStart.Data.IsBattled,
                            OtherTeamPlayed = competitionStart.Data.OtherTeamPlayed,
                            OtherTeamSelectedId = competitionStart.Data.OtherTeamSelectedId,
                            QuestionId = competitionStart.Data.QuestionId,
                            CagegoryId = competitionStart.Data.CurrentQuestion?.CategoryId ?? 0
                        };

                        ViewData["CurrentCompetition"] = competitionStateDto;

                        // Get Background competition
                        var bgCompetition = await _competitionsService.GetBackgroundAttachment();
                        if (bgCompetition.Succeeded && !string.IsNullOrEmpty(bgCompetition.Data))
                            competitionStart.Data.StartBackground = bgCompetition.Data;


                        //Get Players Profile pictures
                        var playersProfileCompetition = await _competitionsService.GetPlayersProfilePictures(competitionStart.Data);
                        if (playersProfileCompetition.Succeeded)
                            competitionStart.Data = playersProfileCompetition.Data;

                        // competition was already started
                        if (competitionStart.Data.CurrentStep == "initial" || competitionStart.Data.CurrentStep == "battle-mode")
                        {
                            PartialViewResult otpPartialView = PartialView("PartialViews/_AllPlayers", competitionStart.Data);
                            viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);
                        }
                        else if (competitionStart.Data.CurrentStep == "categories")
                        {
                            competitionStart.Data.Categories = await _questionsService.GetCategories(true);
                            PartialViewResult otpPartialView = PartialView("PartialViews/_SelectCategory", competitionStart.Data);
                            viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);
                        }
                        else if (competitionStart.Data.CurrentStep == "question")
                        {
                            PartialViewResult otpPartialView = PartialView("PartialViews/_Question", competitionStart.Data);
                            viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);
                        }
                    }
                    else
                    {
                        // Get Background competition
                        var bgCompetition = await _competitionsService.GetBackgroundAttachment();
                        if (bgCompetition.Succeeded && !string.IsNullOrEmpty(bgCompetition.Data))
                            competitionStart.Data.StartBackground = bgCompetition.Data;


                        //Get Players Profile pictures
                        var playersProfileCompetition = await _competitionsService.GetPlayersProfilePictures(competitionStart.Data);
                        if (playersProfileCompetition.Succeeded)
                            competitionStart.Data = playersProfileCompetition.Data;


                        PartialViewResult otpPartialView = PartialView("PartialViews/_AllPlayers", competitionStart.Data);
                        viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);
                    }

                    ViewData["Partial"] = viewContent;
                    return View(competitionStart.Data);
                }
                else if (competitionStart.StatusCode == (int)HttpStatusCode.NotAuthenticated)
                {
                    // means the parent competition not ended
                    return RedirectToAction("Index", "Error", new { message = "CompetitionNotEndedValidation" });
                }
                else
                    return RedirectToAction("Index", "Error");
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-StartCompetition", id, null, false);
                return RedirectToAction("Index", "Error");
            }
        }


        [RolePermission(PermissionCodes.WebCompetitionCreate, PermissionCodes.WebCompetitionStart)]
        public async Task<IActionResult> ViewCompetition(int id)
        {
            try
            {
                ViewCompetitionScoresDTO viewScore = new ViewCompetitionScoresDTO();
                var data = await _competitionsService.ViewCompetitionScore(id);
                if (data.Succeeded)
                    viewScore = data.Data;

                viewScore.Categories = await _questionsService.GetCategories(false);
                viewScore.CompetitionQuestionTypes = new List<LookupModel>()
                {
                    new LookupModel()
                    {
                        Id = (int)CompetitionQuestionType.Rounds,
                        Name = _localizer["CompeitionQuestionsRound"].Value
                    },
                    new LookupModel()
                    {
                        Id = (int)CompetitionQuestionType.QuestionsPerPlayer,
                        Name = _localizer["CompeitionQuestionsPerPlayer"].Value
                    }
                };

                return View("View", viewScore);

            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-ViewCompetition", id, null, false);
                return RedirectToAction("Index", "Error");
            }
        }

        [HttpPost]
        [RolePermission(PermissionCodes.WebCompetitionStart)]
        public async Task<IActionResult> PlayerVsPlayer(PlayerVsPlayerDTO playerVsPlayerDTO)
        {
            try
            {
                var currentCompetition = JsonConvert.DeserializeObject<CompetitionStartDTO>(_httpContextAccessor.HttpContext.Session.GetString("CompetitionStart"));
                currentCompetition.TeamCityMall.ForEach(a => a.IsVSPlayer = false);
                currentCompetition.OtherTeam.ForEach(a => a.IsVSPlayer = false);

                var cityMallPlayer = currentCompetition.TeamCityMall.Where(a => a.Id == playerVsPlayerDTO.CityMallPlayerId).SingleOrDefault();
                cityMallPlayer.IsVSPlayer = true;
                var otherPlayer = currentCompetition.OtherTeam.Where(a => a.Id == playerVsPlayerDTO.OtherPlayerId).SingleOrDefault();
                otherPlayer.IsVSPlayer = true;

                var competitonString = JsonConvert.SerializeObject(currentCompetition);
                _httpContextAccessor.HttpContext.Session.SetString("CompetitionStart", competitonString);

                //Get Players Profile pictures
                var playersProfileCompetition = await _competitionsService.GetPlayersProfilePictures(currentCompetition);
                if (playersProfileCompetition.Succeeded)
                    currentCompetition = playersProfileCompetition.Data;

                PartialViewResult otpPartialView = PartialView("PartialViews/_PlayerVsPlayer", currentCompetition);
                string viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);

                return Json(new { isSuccess = true, partial = viewContent });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-PlayerVsPlayer", playerVsPlayerDTO, null, false);
                return Json(new { isSuccess = false });
            }
        }

        [HttpGet]
        [RolePermission(PermissionCodes.WebCompetitionStart)]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var currentCompetition = JsonConvert.DeserializeObject<CompetitionStartDTO>(_httpContextAccessor.HttpContext.Session.GetString("CompetitionStart"));

                if (currentCompetition.TeamCityMall.Count == 1 && !currentCompetition.TeamCityMall.Where(a => a.IsVSPlayer).Any())
                {
                    //Means Final 
                    currentCompetition.TeamCityMall.FirstOrDefault().IsVSPlayer = true;
                    currentCompetition.OtherTeam.FirstOrDefault().IsVSPlayer = true;
                }

                var competitonString = JsonConvert.SerializeObject(currentCompetition);
                _httpContextAccessor.HttpContext.Session.SetString("CompetitionStart", competitonString);

                currentCompetition.Categories = await _questionsService.GetCategories(true);

                PartialViewResult otpPartialView = PartialView("PartialViews/_SelectCategory", currentCompetition);
                string viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);

                return Json(new { isSuccess = true, partial = viewContent });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-GetCategories", null, null, false);
                return Json(new { isSuccess = false });
            }
        }


        [HttpGet]
        [RolePermission(PermissionCodes.WebCompetitionStart)]
        public async Task<IActionResult> GetQuestion(int categoryId)
        {
            try
            {
                var currentCompetition = JsonConvert.DeserializeObject<CompetitionStartDTO>(_httpContextAccessor.HttpContext.Session.GetString("CompetitionStart"));

                var randomQuestion = await _questionsService.GetRandomQuestionPerCategory(categoryId, currentCompetition.Questions.Select(a => a.Id.Value).ToList());
                if (randomQuestion.Succeeded)
                {
                    QuestionVM randomQ = new QuestionVM()
                    {
                        Answers = randomQuestion.Data.Answers
                            .Select(a => new AnswerOptions
                            {
                                Id = a.Id,
                                IsAnswer = a.IsAnswer,
                                IsImg = a.IsImg,
                                TextAr = a.TextAr,
                                TextEn = a.TextEn,
                                Img = null,      // avoid saving image
                                ImgPath = null   // avoid saving path
                            }).ToList(),
                        AnswertType = randomQuestion.Data.AnswertType,
                        Categories = randomQuestion.Data.Categories,
                        CategoryId = randomQuestion.Data.CategoryId,
                        Id = randomQuestion.Data.Id,
                        Points = randomQuestion.Data.Points,
                        TextAr = randomQuestion.Data.TextAr,
                        TextEn = randomQuestion.Data.TextEn,
                        Time = randomQuestion.Data.Time,
                        Img = null,
                        ImgPath = null
                    };
                    currentCompetition.CurrentQuestion = randomQ;
                    currentCompetition.Questions.Add(randomQ);
                    currentCompetition.TotalCurrentCompetitionQuestions.Add(randomQ);

                    var competitonString = JsonConvert.SerializeObject(currentCompetition);
                    _httpContextAccessor.HttpContext.Session.SetString("CompetitionStart", competitonString);

                    currentCompetition.CurrentQuestion = randomQuestion.Data;
                }
                else
                    return Json(new { isSuccess = false });


                //Get Players Profile pictures
                var playersProfileCompetition = await _competitionsService.GetPlayersProfilePictures(currentCompetition);
                if (playersProfileCompetition.Succeeded)
                    currentCompetition = playersProfileCompetition.Data;

                PartialViewResult otpPartialView = PartialView("PartialViews/_Question", currentCompetition);
                string viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);


                return Json(new { isSuccess = true, partial = viewContent });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-GetQuestion", categoryId, null, false);
                return Json(new { isSuccess = false });
            }
        }

        [HttpGet]
        [RolePermission(PermissionCodes.WebCompetitionStart)]
        public async Task<IActionResult> ChangeQuestion(int categoryId)
        {
            try
            {
                var currentCompetition = JsonConvert.DeserializeObject<CompetitionStartDTO>(_httpContextAccessor.HttpContext.Session.GetString("CompetitionStart"));

                var randomQuestion = await _questionsService.GetRandomQuestionPerCategory(categoryId, currentCompetition.Questions.Select(a => a.Id.Value).ToList());
                if (randomQuestion.Succeeded)
                {
                    QuestionVM randomQ = new QuestionVM()
                    {
                        Answers = randomQuestion.Data.Answers
                             .Select(a => new AnswerOptions
                             {
                                 Id = a.Id,
                                 IsAnswer = a.IsAnswer,
                                 IsImg = a.IsImg,
                                 TextAr = a.TextAr,
                                 TextEn = a.TextEn,
                                 Img = null,      // avoid saving image
                                 ImgPath = null   // avoid saving path
                             }).ToList(),
                        AnswertType = randomQuestion.Data.AnswertType,
                        Categories = randomQuestion.Data.Categories,
                        CategoryId = randomQuestion.Data.CategoryId,
                        ChangedQuestion = true,
                        Id = randomQuestion.Data.Id,
                        Points = randomQuestion.Data.Points,
                        TextAr = randomQuestion.Data.TextAr,
                        TextEn = randomQuestion.Data.TextEn,
                        Time = randomQuestion.Data.Time,
                        Img = null,
                        ImgPath = null,
                    };
                    
                    currentCompetition.CurrentQuestion = randomQ;
                    currentCompetition.Questions.Add(randomQ);
                    currentCompetition.TotalCurrentCompetitionQuestions.Add(randomQ);

                    var competitonString = JsonConvert.SerializeObject(currentCompetition);
                    _httpContextAccessor.HttpContext.Session.SetString("CompetitionStart", competitonString);

                    randomQuestion.Data.ChangedQuestion = true;
                    currentCompetition.CurrentQuestion = randomQuestion.Data;
                }
                else
                    return Json(new { isSuccess = false });

                PartialViewResult otpPartialView = PartialView("PartialViews/_QuestionContent", currentCompetition);
                string viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);


                return Json(new { isSuccess = true, partial = viewContent });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-GetQuestion", categoryId, null, false);
                return Json(new { isSuccess = false });
            }
        }

        [HttpPost]
        [RolePermission(PermissionCodes.WebCompetitionStart)]
        public async Task<IActionResult> AnswerQuestion(AnswerOnQuestionDTO answerOnQuestionDTO)
        {
            try
            {
                bool IsAr = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar";
                var currentCompetition = JsonConvert.DeserializeObject<CompetitionStartDTO>(_httpContextAccessor.HttpContext.Session.GetString("CompetitionStart"));

                //Get Players Profile pictures
                var playersProfileCompetition = await _competitionsService.GetPlayersProfilePictures(currentCompetition);
                if (playersProfileCompetition.Succeeded)
                    currentCompetition = playersProfileCompetition.Data;


                //Get Specific player who answered on question
                CompetitionsPlayerDTO competitionsPlayerDTO = null;
                if (answerOnQuestionDTO.IsCityMallPlayer)
                    competitionsPlayerDTO = currentCompetition.TeamCityMall.Where(a => a.Id == answerOnQuestionDTO.PlayerId).SingleOrDefault();
                else
                    competitionsPlayerDTO = currentCompetition.OtherTeam.Where(a => a.Id == answerOnQuestionDTO.PlayerId).SingleOrDefault();

                // Get Details of question and answer
                CompetitonQuestions competitonQuestion = new CompetitonQuestions();
                competitonQuestion.QuestionId = answerOnQuestionDTO.QuestionId;
                var question = await _questionsService.GetQuestion(answerOnQuestionDTO.QuestionId.Value);
                competitonQuestion.QuestionText = IsAr ? question.Data.TextAr : question.Data.TextEn;

                bool IsAnswer = false;
                if (answerOnQuestionDTO.AnswerId.HasValue)
                {
                    competitonQuestion.AnswerId = answerOnQuestionDTO.AnswerId;
                    var answer = question.Data.Answers.Where(a => a.Id == answerOnQuestionDTO.AnswerId.Value).SingleOrDefault();
                    competitonQuestion.AnswerText = IsAr ? answer.TextAr : answer.TextEn;

                    if (answer.IsAnswer)
                    {
                        var roundPoints = _competitionsService.GetRoundPoints(currentCompetition.Id, currentCompetition.CurrentRound);
                        answerOnQuestionDTO.Points = roundPoints;
                        competitionsPlayerDTO.Points = (competitionsPlayerDTO.Points + roundPoints);
                        competitionsPlayerDTO.Time = (competitionsPlayerDTO.Time + answerOnQuestionDTO.Time ?? 0);
                        competitonQuestion.IsCorrectAnswer = answerOnQuestionDTO.IsCorrectAnswer = IsAnswer = true;
                    }
                }
                competitionsPlayerDTO.competitonQuestions.Add(competitonQuestion);

                // Add details to database.
                var resultAddOnDb = await _competitionsService.AnswerOnQuestions(currentCompetition.Id, answerOnQuestionDTO);

                string viewContent = "";
                bool IsFinished = false;
                bool ResetPlayers = false;
                bool ContinueRounds = false;
                bool ScoresAreTheSame = false;
                //Check if the score is the same
                int CityMallPoints = currentCompetition.TeamCityMall.Sum(a => a.Points);
                int OtherTeamPoints = currentCompetition.OtherTeam.Sum(a => a.Points);

                //Check if the rounds finished
                if (currentCompetition.IsQuestionsTypeIsRound)
                {
                    var TotalQuestionPerRound = currentCompetition.TeamCityMall.Count;
                    var PendingQuestionsPerRound = (TotalQuestionPerRound * currentCompetition.CurrentRound) - currentCompetition.TotalCurrentCompetitionQuestions.Where(a => !a.ChangedQuestion).Count();
                    if (PendingQuestionsPerRound <= 0)
                    {
                        if (IsAnswer || (answerOnQuestionDTO.IsCityMallPlayerAnswered && answerOnQuestionDTO.IsOtherPlayerAnswered))
                        {
                            //Round 1 or 2 .. finished
                            if (currentCompetition.TotalRound - currentCompetition.CurrentRound > 0)
                            {
                                //Still the full competition not finished
                                IsFinished = false;
                                ResetPlayers = true;
                                ContinueRounds = true;
                            }
                            else
                            {
                                // Rounds finished 
                                //Check if the points is the same.
                                if (CityMallPoints == OtherTeamPoints)
                                {
                                    IsFinished = false;
                                    ContinueRounds = false;
                                    ResetPlayers = true;
                                    ScoresAreTheSame = true;
                                }
                                else
                                {
                                    // Full Competition Finished
                                    IsFinished = true;
                                    ResetPlayers = false;
                                    ContinueRounds = false;
                                }
                            }
                        }
                    }

                    if (IsFinished)
                    {
                        //Update Competition with Winning team and scores.
                        CompetitionsDTO competitionsDTO = new CompetitionsDTO();
                        competitionsDTO.Id = currentCompetition.Id;

                        if (CityMallPoints > OtherTeamPoints)
                        {
                            var cityMallPlayer = currentCompetition.TeamCityMall
                                .OrderByDescending(a => a.Points)
                                .ThenBy(a => a.Time)
                                .FirstOrDefault();

                            competitionsDTO.WinningPlayer = new PlayerDTO()
                            {
                                Id = cityMallPlayer.Id,
                                IsEmployee = true
                            };
                        }
                        else
                        {
                            var OtherWinningPlaye = currentCompetition.OtherTeam
                                .OrderByDescending(a => a.Points)
                                .ThenBy(a => a.Time)
                                .FirstOrDefault();

                            competitionsDTO.WinningPlayer = new PlayerDTO()
                            {
                                Id = OtherWinningPlaye.Id,
                            };
                        }

                        competitionsDTO.Team1Score = CityMallPoints;
                        competitionsDTO.Team2Score = OtherTeamPoints;
                        var resultFinishCompetition = await _competitionsService.FinishCompetition(competitionsDTO);

                        PartialViewResult otpPartialView = PartialView("PartialViews/_FullScoreTeams", currentCompetition);
                        viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);
                    }
                    else
                    {
                        PartialViewResult otpPartialView = PartialView("PartialViews/_AllPlayers", currentCompetition);
                        viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);
                    }

                    //If Continue rounds, then update the time.
                    string nextRoundText = "";
                    string roundScoresView = "";
                    bool showRoundScores = false;

                    if (ContinueRounds)
                    {
                        currentCompetition.CurrentRound = (currentCompetition.CurrentRound + 1);
                        currentCompetition.RoundTime = _competitionsService.GetRoundTime(currentCompetition.Id, currentCompetition.CurrentRound);
                        currentCompetition.RoundPoints = _competitionsService.GetRoundPoints(currentCompetition.Id, currentCompetition.CurrentRound);
                        nextRoundText = $"{_localizer["MoveToRound"].Value} {_localizer[$"Round{currentCompetition.CurrentRound}"].Value}";

                        // Generate round scores modal for completed round
                        showRoundScores = true;

                        // Create RoundScoresModalViewModel
                        var roundScoresViewModel = new RoundScoresModalViewModel
                        {
                            CurrentRound = currentCompetition.CurrentRound - 1, // Previous round that just finished
                            Team1Name = currentCompetition.Team1Name,
                            Team2Name = currentCompetition.Team2Name,
                            TotalCompletedQuestions = currentCompetition.TotalCurrentCompetitionQuestions.Count,
                            IsFinalCompetition = currentCompetition.IsFinalCompetition,
                            TeamCityMall = currentCompetition.TeamCityMall.Select(p => new RoundScoresPlayerViewModel
                            {
                                Id = p.Id,
                                Name = p.Name,
                                ProfilePicture = p.ProfilePicture,
                                Points = p.Points,
                                TotalQuestions = p.competitonQuestions.Count,
                                CorrectAnswers = p.competitonQuestions.Count(q => q.IsCorrectAnswer == true),
                                Time = p.Time
                            }).OrderByDescending(p => p.Points).ThenBy(p => p.Time).ToList(),
                            OtherTeam = currentCompetition.OtherTeam.Select(p => new RoundScoresPlayerViewModel
                            {
                                Id = p.Id,
                                Name = p.Name,
                                ProfilePicture = p.ProfilePicture,
                                Points = p.Points,
                                TotalQuestions = p.competitonQuestions.Count,
                                CorrectAnswers = p.competitonQuestions.Count(q => q.IsCorrectAnswer == true),
                                Time = p.Time
                            }).OrderByDescending(p => p.Points).ThenBy(p => p.Time).ToList()
                        };

                        PartialViewResult roundScoresPartial = PartialView("PartialViews/_RoundScoresModal", roundScoresViewModel);
                        roundScoresView = ConvertViewToString(this.ControllerContext, roundScoresPartial, _viewEngine);
                    }
                    else if (ScoresAreTheSame)
                    {
                        nextRoundText = _localizer["ContinueRound"].Value;

                        // Show scores when scores are the same (tie-breaker round)
                        showRoundScores = true;

                        // Create RoundScoresModalViewModel for tie-breaker
                        var roundScoresViewModel = new RoundScoresModalViewModel
                        {
                            CurrentRound = currentCompetition.CurrentRound,
                            Team1Name = currentCompetition.Team1Name,
                            Team2Name = currentCompetition.Team2Name,
                            TotalCompletedQuestions = currentCompetition.TotalCurrentCompetitionQuestions.Count,
                            IsFinalCompetition = currentCompetition.IsFinalCompetition,
                            TeamCityMall = currentCompetition.TeamCityMall.Select(p => new RoundScoresPlayerViewModel
                            {
                                Id = p.Id,
                                Name = p.Name,
                                ProfilePicture = p.ProfilePicture,
                                Points = p.Points,
                                TotalQuestions = p.competitonQuestions.Count,
                                CorrectAnswers = p.competitonQuestions.Count(q => q.IsCorrectAnswer == true),
                                Time = p.Time
                            }).OrderByDescending(p => p.Points).ThenBy(p => p.Time).ToList(),
                            OtherTeam = currentCompetition.OtherTeam.Select(p => new RoundScoresPlayerViewModel
                            {
                                Id = p.Id,
                                Name = p.Name,
                                ProfilePicture = p.ProfilePicture,
                                Points = p.Points,
                                TotalQuestions = p.competitonQuestions.Count,
                                CorrectAnswers = p.competitonQuestions.Count(q => q.IsCorrectAnswer == true),
                                Time = p.Time
                            }).OrderByDescending(p => p.Points).ThenBy(p => p.Time).ToList()
                        };

                        PartialViewResult roundScoresPartial = PartialView("PartialViews/_RoundScoresModal", roundScoresViewModel);
                        roundScoresView = ConvertViewToString(this.ControllerContext, roundScoresPartial, _viewEngine);
                    }

                    currentCompetition.TeamCityMall.ForEach(a => a.ProfilePicture = null);
                    currentCompetition.OtherTeam.ForEach(a => a.ProfilePicture = null);

                    var competitonString = JsonConvert.SerializeObject(currentCompetition);
                    _httpContextAccessor.HttpContext.Session.SetString("CompetitionStart", competitonString);

                    return Json(new
                    {
                        isSuccess = true,
                        correct = IsAnswer,
                        partial = viewContent,
                        finished = IsFinished,
                        reset = ResetPlayers,
                        continueRound = ContinueRounds,
                        roundText = nextRoundText,
                        isFinalComp = currentCompetition.IsFinalCompetition,
                        isScoreView = showRoundScores,
                        scorePartial = roundScoresView
                    });
                }
                else
                {
                    // Questions for each player
                    var totalQuestionToBeAnswered = (currentCompetition.QuestionPerPlayer * currentCompetition.TeamCityMall.Count);
                    bool allCityMallPlayersGotQuestions = currentCompetition.TeamCityMall.All(player => player.competitonQuestions.Count >= totalQuestionToBeAnswered);
                    bool allOtherPlayersGotQuestions = currentCompetition.OtherTeam.All(player => player.competitonQuestions.Count >= totalQuestionToBeAnswered);
                    string getScoresView = "";
                    bool gotToScore = false;

                    if (!allCityMallPlayersGotQuestions || !allOtherPlayersGotQuestions)
                    {
                        //still number of question not completed for both players
                        PartialViewResult otpPartialView = PartialView("PartialViews/_AllPlayers", currentCompetition);
                        viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);
                    }
                    else if ((allCityMallPlayersGotQuestions && allOtherPlayersGotQuestions) && (CityMallPoints == OtherTeamPoints))
                    {
                        // Both players answers and both has same points
                        ResetPlayers = true;
                        gotToScore = true; // Show scores when tie occurs

                        // Create RoundScoresModalViewModel for tie scenario
                        var tieScoresViewModel = new RoundScoresModalViewModel
                        {
                            CurrentRound = currentCompetition.CurrentRound,
                            Team1Name = currentCompetition.Team1Name,
                            Team2Name = currentCompetition.Team2Name,
                            TotalCompletedQuestions = currentCompetition.TotalCurrentCompetitionQuestions.Count,
                            IsFinalCompetition = currentCompetition.IsFinalCompetition,
                            TeamCityMall = currentCompetition.TeamCityMall.Select(p => new RoundScoresPlayerViewModel
                            {
                                Id = p.Id,
                                Name = p.Name,
                                ProfilePicture = p.ProfilePicture,
                                Points = p.Points,
                                TotalQuestions = p.competitonQuestions.Count,
                                CorrectAnswers = p.competitonQuestions.Count(q => q.IsCorrectAnswer == true),
                                Time = p.Time
                            }).OrderByDescending(p => p.Points).ThenBy(p => p.Time).ToList(),
                            OtherTeam = currentCompetition.OtherTeam.Select(p => new RoundScoresPlayerViewModel
                            {
                                Id = p.Id,
                                Name = p.Name,
                                ProfilePicture = p.ProfilePicture,
                                Points = p.Points,
                                TotalQuestions = p.competitonQuestions.Count,
                                CorrectAnswers = p.competitonQuestions.Count(q => q.IsCorrectAnswer == true),
                                Time = p.Time
                            }).OrderByDescending(p => p.Points).ThenBy(p => p.Time).ToList()
                        };

                        PartialViewResult otpPartialViewScore = PartialView("PartialViews/_RoundScoresModal", tieScoresViewModel);
                        getScoresView = ConvertViewToString(this.ControllerContext, otpPartialViewScore, _viewEngine);

                        PartialViewResult otpPartialView = PartialView("PartialViews/_AllPlayers", currentCompetition);
                        viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);
                    }
                    else
                    {
                        // option to show Final score
                        gotToScore = true;

                        // Use modal for non-final competitions and full screen for final competitions
                        if (currentCompetition.IsFinalCompetition)
                        {
                            PartialViewResult otpPartialViewScore = PartialView("PartialViews/_FullScoreTeams", currentCompetition);
                            getScoresView = ConvertViewToString(this.ControllerContext, otpPartialViewScore, _viewEngine);
                        }
                        else
                        {
                            // Create RoundScoresModalViewModel for regular competition end
                            var endScoresViewModel = new RoundScoresModalViewModel
                            {
                                CurrentRound = currentCompetition.CurrentRound,
                                Team1Name = currentCompetition.Team1Name,
                                Team2Name = currentCompetition.Team2Name,
                                TotalCompletedQuestions = currentCompetition.TotalCurrentCompetitionQuestions.Count,
                                IsFinalCompetition = currentCompetition.IsFinalCompetition,
                                TeamCityMall = currentCompetition.TeamCityMall.Select(p => new RoundScoresPlayerViewModel
                                {
                                    Id = p.Id,
                                    Name = p.Name,
                                    ProfilePicture = p.ProfilePicture,
                                    Points = p.Points,
                                    TotalQuestions = p.competitonQuestions.Count,
                                    CorrectAnswers = p.competitonQuestions.Count(q => q.IsCorrectAnswer == true),
                                    Time = p.Time
                                }).OrderByDescending(p => p.Points).ThenBy(p => p.Time).ToList(),
                                OtherTeam = currentCompetition.OtherTeam.Select(p => new RoundScoresPlayerViewModel
                                {
                                    Id = p.Id,
                                    Name = p.Name,
                                    ProfilePicture = p.ProfilePicture,
                                    Points = p.Points,
                                    TotalQuestions = p.competitonQuestions.Count,
                                    CorrectAnswers = p.competitonQuestions.Count(q => q.IsCorrectAnswer == true),
                                    Time = p.Time
                                }).OrderByDescending(p => p.Points).ThenBy(p => p.Time).ToList()
                            };

                            PartialViewResult otpPartialViewScore = PartialView("PartialViews/_RoundScoresModal", endScoresViewModel);
                            getScoresView = ConvertViewToString(this.ControllerContext, otpPartialViewScore, _viewEngine);
                        }

                        ResetPlayers = true;
                        PartialViewResult otpPartialView = PartialView("PartialViews/_AllPlayers", currentCompetition);
                        viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);
                    }

                    currentCompetition.TeamCityMall.ForEach(a => a.ProfilePicture = null);
                    currentCompetition.OtherTeam.ForEach(a => a.ProfilePicture = null);

                    var competitonString = JsonConvert.SerializeObject(currentCompetition);
                    _httpContextAccessor.HttpContext.Session.SetString("CompetitionStart", competitonString);

                    return Json(new
                    {
                        isSuccess = true,
                        correct = IsAnswer,
                        reset = ResetPlayers,
                        partial = viewContent,
                        isScoreView = gotToScore,
                        scorePartial = getScoresView,
                        isFinalComp = currentCompetition.IsFinalCompetition,
                        IsAllPlayerGotQuestions = (allCityMallPlayersGotQuestions && allOtherPlayersGotQuestions),
                        isCityMallFullQuestion = allCityMallPlayersGotQuestions,
                        isOtherPlayerFullQuestion = allOtherPlayersGotQuestions
                    });
                }
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-AnswerQuestion", answerOnQuestionDTO, null, false);
                return Json(new { isSuccess = false });
            }
        }

        [HttpGet]
        [RolePermission(PermissionCodes.WebCompetitionStart)]
        public async Task<IActionResult> GetFullScore()
        {
            try
            {
                _httpContextAccessor.HttpContext.Session.Remove("CompetitionScoreDetails");

                var currentCompetition = JsonConvert.DeserializeObject<CompetitionStartDTO>(_httpContextAccessor.HttpContext.Session.GetString("CompetitionStart"));
                PartialViewResult otpPartialView = PartialView("PartialViews/_FullScoreTeams", currentCompetition);
                string viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);
                return Json(new { isSuccess = true, partial = viewContent });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-GetFullScore", null, null, false);
                return Json(new { isSuccess = false });
            }
        }

        [HttpGet]
        [RolePermission(PermissionCodes.WebCompetitionStart)]
        public async Task<IActionResult> GetScoreDetails()
        {
            try
            {
                var currentCompetition = JsonConvert.DeserializeObject<CompetitionStartDTO>(_httpContextAccessor.HttpContext.Session.GetString("CompetitionStart"));
                PartialViewResult otpPartialView = PartialView("PartialViews/_TeamScoreDetails", currentCompetition);
                string viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);
                return Json(new { isSuccess = true, partial = viewContent, });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-GetScoreDetails", null, null, false);
                return Json(new { isSuccess = false });
            }
        }

        [HttpGet]
        [RolePermission(PermissionCodes.WebCompetitionCreate, PermissionCodes.WebCompetitionStart)]
        public async Task<IActionResult> GetModalPlayer(int playerId, bool isCityMall)
        {
            try
            {
                if (string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Session.GetString("CompetitionScoreDetails")) && string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Session.GetString("CompetitionStart")))
                    return Json(new { isSuccess = false });

                if (!string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Session.GetString("CompetitionStart")))
                {

                    var currentCompetition = JsonConvert.DeserializeObject<CompetitionStartDTO>(_httpContextAccessor.HttpContext.Session.GetString("CompetitionStart"));
                    CompetitionsPlayerDTO competitionsPlayerDTO = null;
                    if (isCityMall)
                        competitionsPlayerDTO = currentCompetition.TeamCityMall.Where(a => a.Id == playerId).SingleOrDefault();
                    else
                        competitionsPlayerDTO = currentCompetition.OtherTeam.Where(a => a.Id == playerId).SingleOrDefault();

                    PartialViewResult otpPartialView = PartialView("PartialViews/_PlayerScoreModal", competitionsPlayerDTO);
                    string viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);
                    return Json(new { isSuccess = true, partial = viewContent, });
                }
                else
                {
                    var currentCompetition = JsonConvert.DeserializeObject<ViewCompetitionScoresDTO>(_httpContextAccessor.HttpContext.Session.GetString("CompetitionScoreDetails"));
                    CompetitionsPlayerDTO competitionsPlayerDTO = null;
                    if (isCityMall)
                        competitionsPlayerDTO = currentCompetition.TeamCityMall.Where(a => a.Id == playerId).SingleOrDefault();
                    else
                        competitionsPlayerDTO = currentCompetition.OtherTeam.Where(a => a.Id == playerId).SingleOrDefault();

                    PartialViewResult otpPartialView = PartialView("PartialViews/_PlayerScoreModal", competitionsPlayerDTO);
                    string viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);
                    return Json(new { isSuccess = true, partial = viewContent });
                }
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-GetModalPlayer", $"PlayerId:{playerId} - isCityMall:{isCityMall}", null, false);
                return Json(new { isSuccess = false });
            }
        }


        [HttpGet]
        [RolePermission(PermissionCodes.WebCompetitionCreate, PermissionCodes.WebCompetitionStart)]
        public async Task<IActionResult> GetPlayerScoreDetails(int competitionId, int playerId)
        {
            try
            {
                var result = await _competitionsService.GetPlayerScoreDetails(competitionId, playerId);
                if (!result.Succeeded)
                    return RedirectToAction("Index", "Error");

                return View("PlayerScoreDetails", result.Data);
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index-GetPlayerScoreDetails", $"competitionId:{competitionId} - PlayerId:{playerId}", null, false);
                return RedirectToAction("Index", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> FinishFinalCompetition()
        {
            try
            {
                var currentCompetition = JsonConvert.DeserializeObject<CompetitionStartDTO>(_httpContextAccessor.HttpContext.Session.GetString("CompetitionStart"));

                //Update Competition with Winning team and scores.
                CompetitionsDTO competitionsDTO = new CompetitionsDTO();
                competitionsDTO.Id = currentCompetition.Id;
                int CityMallPoints = currentCompetition.TeamCityMall.Sum(a => a.Points);
                int OtherTeamPoints = currentCompetition.OtherTeam.Sum(a => a.Points);

                if (CityMallPoints > OtherTeamPoints)
                {
                    var cityMallPlayer = currentCompetition.TeamCityMall
                        .OrderByDescending(a => a.Points)
                        .ThenBy(a => a.Time)
                        .FirstOrDefault();

                    competitionsDTO.WinningPlayer = new PlayerDTO()
                    {
                        Id = cityMallPlayer.Id,
                        IsEmployee = true
                    };
                }
                else
                {
                    var OtherWinningPlaye = currentCompetition.OtherTeam
                        .OrderByDescending(a => a.Points)
                        .ThenBy(a => a.Time)
                        .FirstOrDefault();

                    competitionsDTO.WinningPlayer = new PlayerDTO()
                    {
                        Id = OtherWinningPlaye.Id,
                    };
                }

                competitionsDTO.Team1Score = CityMallPoints;
                competitionsDTO.Team2Score = OtherTeamPoints;
                var resultFinishCompetition = await _competitionsService.FinishCompetition(competitionsDTO);

                return Json(new { isSuccess = true });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false });
            }
        }

        [HttpPost]
        public IActionResult UpdateCurrentStep(CompetitionStateDto competitionStateDto)
        {
            try
            {
                var sessionData = _httpContextAccessor.HttpContext.Session.GetString("CompetitionStart");
                if (string.IsNullOrEmpty(sessionData))
                {
                    return Json(new { isSuccess = false, msg = "Session expired" });
                }

                if (competitionStateDto != null && competitionStateDto.Id != 0)
                {
                    var currentCompetition = JsonConvert.DeserializeObject<CompetitionStartDTO>(sessionData);

                    currentCompetition.CurrentStep = competitionStateDto.CurrentStep;
                    currentCompetition.CityMallPlayed = competitionStateDto.CityMallPlayed;
                    currentCompetition.OtherTeamPlayed = competitionStateDto.OtherTeamPlayed;
                    currentCompetition.IsBattled = competitionStateDto.IsBattled;
                    currentCompetition.cityMallSelectedId = competitionStateDto.cityMallSelectedId;
                    currentCompetition.OtherTeamSelectedId = competitionStateDto.OtherTeamSelectedId;
                    currentCompetition.QuestionId = competitionStateDto.QuestionId;

                    // Update session immediately
                    var updatedSessionData = JsonConvert.SerializeObject(currentCompetition);
                    _httpContextAccessor.HttpContext.Session.SetString("CompetitionStart", updatedSessionData);

                    // Queue the database update to avoid tracking issues
                    _competitionUpdateQueue.QueueUpdate(competitionStateDto);
                }
                

                // Return immediately without waiting
                return Json(new { isSuccess = true, msg = "Queued for update" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateCurrentStep", competitionStateDto, null, false);
                return Json(new { isSuccess = false, msg = "Error occurred" });
            }
        }
    }
}

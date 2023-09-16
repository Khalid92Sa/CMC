using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Controllers;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Infrastructure;
using CMC.Presentation.Application.ActionFilters;
using CMC.Presentation.Application.DTOs.Competitions;
using CMC.Presentation.Application.DTOs.Players;
using CMC.Presentation.Application.Services.Competitions;
using CMC.Presentation.Application.Services.Identity.Interfaces;
using CMC.Presentation.Application.Services.Players;
using CMC.Presentation.Application.Services.Questions;
using CMC.Presentation.Application.Services.Settings;
using CMC.Presentation.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CMC.Presentation.Web.Controllers
{
    public class CompetitionsController : BaseController
    {
        readonly IPlayerService _playerService;
        readonly ICompetitionsService _competitionsService;
        readonly IApplicationLogger _logger;
        readonly IStringLocalizer<PlayersController> _localizer;
        readonly ISettingsService _settingService;
        readonly IQuestionsService _questionsService;
        readonly IUserService _userService;
        readonly ICompositeViewEngine _viewEngine;
        public static IHttpContextAccessor _httpContextAccessor { get { return new HttpContextAccessor(); } }

        public CompetitionsController(IPlayerService playerService,
            ICompetitionsService competitionsService,
            IUserService userService,
            IApplicationLogger logger,
            IStringLocalizer<PlayersController> localizer,
            ISettingsService settingsService,
            IQuestionsService questionsService,
            ICompositeViewEngine viewEngine)
        {
            _competitionsService = competitionsService;
            _playerService = playerService;
            _userService = userService;
            _logger = logger;
            _localizer = localizer;
            _settingService = settingsService;
            _questionsService = questionsService;
            _viewEngine = viewEngine;

        }

        [RolePermission(PermissionCodes.WebCompetitionView)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var loggedInUser = _userService.GetLoggedInUser();
                ViewBag.Hosts = await _userService.GetHosts();
                ViewData["CanDelete"] = loggedInUser.PermissionCodes.Contains(PermissionCodes.WebCompetitionDelete);
                ViewData["CanCreate"] = loggedInUser.PermissionCodes.Contains(PermissionCodes.WebCompetitionCreate);
                ViewData["IsHost"] = loggedInUser.GroupCode == GroupsEnum.Host;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [RolePermission(PermissionCodes.WebCompetitionView)]
        [HttpGet]
        public async Task<IActionResult> GetAllCompetitions([FromQuery] SearchCompetitionDTO searchCompetitionDTO)
        {
            var loggedInUser = _userService.GetLoggedInUser();
            if (loggedInUser.GroupCode == GroupsEnum.Host)
                searchCompetitionDTO.HostId = loggedInUser.Id;

            var result = await _competitionsService.GetCompetitions(searchCompetitionDTO);
            return Json(result);
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
                var lastScores = await _competitionsService.GetLatestScores();
                if (lastScores.Succeeded)
                    competitionsDTO.LatestScores = lastScores.Data;

                if (competitionsDTO.QuestionCount < 1)
                    competitionsDTO.QuestionCount = 1;

                return View("Create", competitionsDTO);
            }
            catch (Exception ex)
            {
                throw ex;
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
                if (competitionStart.Succeeded)
                {

                    PartialViewResult otpPartialView = PartialView("PartialViews/_FullScoreTeams", competitionStart.Data);
                    string viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);
                    ViewData["Partial"] = viewContent;

                    return View(competitionStart.Data);
                }
                else
                    throw new Exception("Error");
            }
            catch (Exception ex)
            {
                throw ex;
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

                return View("View", viewScore);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [RolePermission(PermissionCodes.WebCompetitionStart)]
        public IActionResult PlayerVsPlayer(PlayerVsPlayerDTO playerVsPlayerDTO)
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

                PartialViewResult otpPartialView = PartialView("PartialViews/_PlayerVsPlayer", currentCompetition);
                string viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);

                return Json(new { isSuccess = true, partial = viewContent });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false });
            }
        }

        [HttpGet]
        [RolePermission(PermissionCodes.WebCompetitionStart)]
        public async Task<IActionResult> GetCategories(/*GetCategoryByPlayerDTO getCategoryByPlayer*/)
        {
            try
            {
                var currentCompetition = JsonConvert.DeserializeObject<CompetitionStartDTO>(_httpContextAccessor.HttpContext.Session.GetString("CompetitionStart"));
                //currentCompetition.TeamCityMall.ForEach(a => a.IsStarting = false);
                //currentCompetition.OtherTeam.ForEach(a => a.IsStarting = false);

                //if(getCategoryByPlayer.IsCityMallTeam)
                //{
                //    var cityMallPlayer = currentCompetition.TeamCityMall.Where(a => a.Id == getCategoryByPlayer.playerId).SingleOrDefault();
                //    cityMallPlayer.IsStarting = true;
                //}
                //else
                //{
                //    var otherPlayer = currentCompetition.OtherTeam.Where(a => a.Id == getCategoryByPlayer.playerId).SingleOrDefault();
                //    otherPlayer.IsStarting = true;
                //}


                if(currentCompetition.TeamCityMall.Count == 1 && !currentCompetition.TeamCityMall.Where(a => a.IsVSPlayer).Any())
                {
                    //Means Final 
                    currentCompetition.TeamCityMall.FirstOrDefault().IsVSPlayer = true;
                    currentCompetition.OtherTeam.FirstOrDefault().IsVSPlayer = true;

                }

                if (currentCompetition.Categories == null || currentCompetition.Categories.Count == 0)
                    currentCompetition.Categories = await _questionsService.GetCategories();


                var competitonString = JsonConvert.SerializeObject(currentCompetition);
                _httpContextAccessor.HttpContext.Session.SetString("CompetitionStart", competitonString);

                PartialViewResult otpPartialView = PartialView("PartialViews/_SelectCategory", currentCompetition);
                string viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);

                return Json(new { isSuccess = true, partial = viewContent });
            }
            catch (Exception ex)
            {
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
                    currentCompetition.Questions.Add(randomQuestion.Data);
                    currentCompetition.CurrentQuestion = randomQuestion.Data;
                }
                else
                    return Json(new { isSuccess = false });

                var competitonString = JsonConvert.SerializeObject(currentCompetition);
                _httpContextAccessor.HttpContext.Session.SetString("CompetitionStart", competitonString);

                PartialViewResult otpPartialView = PartialView("PartialViews/_Question", currentCompetition);
                string viewContent = ConvertViewToString(this.ControllerContext, otpPartialView, _viewEngine);

                return Json(new { isSuccess = true, partial = viewContent });
            }
            catch (Exception ex)
            {
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
                
                //Get Specific player who ansewred on question
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
                competitonQuestion.Points = answerOnQuestionDTO.Points;

                bool IsAnswer = false;
                if (answerOnQuestionDTO.AnswerId.HasValue)
                {
                    competitonQuestion.AnswerId = answerOnQuestionDTO.AnswerId;
                    var answer = question.Data.Answers.Where(a => a.Id == answerOnQuestionDTO.AnswerId.Value).SingleOrDefault();
                    competitonQuestion.AnswerText = IsAr ? answer.TextAr : answer.TextEn;

                    if (answer.IsAnswer)
                    {
                        competitionsPlayerDTO.Points = (competitionsPlayerDTO.Points + question.Data.Points);
                        competitonQuestion.IsCorrectAnswer = answerOnQuestionDTO.IsCorrectAnswer = IsAnswer = true;
                    }
                }
                competitionsPlayerDTO.competitonQuestions.Add(competitonQuestion);

                // Add details to database.
                var resultAddOnDb = await _competitionsService.AnswerOnQuestions(currentCompetition.Id, answerOnQuestionDTO);

                var competitonString = JsonConvert.SerializeObject(currentCompetition);
                _httpContextAccessor.HttpContext.Session.SetString("CompetitionStart", competitonString);

                string viewContent = "";
                bool IsFinished = false;
                bool ResetPlayers = false;
                //Check if the score is the same
                int CityMallPoints = currentCompetition.TeamCityMall.Sum(a => a.Points);
                int OtherTeamPoints = currentCompetition.OtherTeam.Sum(a => a.Points);


                //Check if the rounds finished
                if (currentCompetition.Questions.Count >= currentCompetition.TotalQuestion)
                {
                    if (CityMallPoints == OtherTeamPoints)
                        ResetPlayers = true;
                    else
                        IsFinished = true;
                }
                else if (currentCompetition.TotalQuestion > currentCompetition.TeamCityMall.Count)
                    ResetPlayers = true;




                if (IsFinished)
                {
                    //Update Competition with Winning team and scores.
                    CompetitionsDTO competitionsDTO = new CompetitionsDTO();
                    competitionsDTO.Id = currentCompetition.Id;

                    if (CityMallPoints > OtherTeamPoints)
                    {
                        var cityMallPlayer = currentCompetition.TeamCityMall.OrderByDescending(a => a.Points).FirstOrDefault();
                        competitionsDTO.WinningPlayer = new PlayerDTO()
                        {
                            Id = cityMallPlayer.Id,
                            IsEmployee = true
                        };
                    }
                    else
                    {
                        var OtherWinningPlaye = currentCompetition.OtherTeam.OrderByDescending(a => a.Points).FirstOrDefault();
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


                return Json(new { isSuccess = true, correct = IsAnswer, partial = viewContent, finished = IsFinished, reset = ResetPlayers });

            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false });
            }
        }

        [HttpGet]
        [RolePermission(PermissionCodes.WebCompetitionStart)]
        public IActionResult GetFullScore()
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
                return Json(new { isSuccess = false });
            }
        }

        [HttpGet]
        [RolePermission(PermissionCodes.WebCompetitionStart)]
        public IActionResult GetScoreDetails()
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
                return Json(new { isSuccess = false });
            }
        }

        [HttpGet]
        [RolePermission(PermissionCodes.WebCompetitionCreate,PermissionCodes.WebCompetitionStart)]
        public IActionResult GetModalPlayer(int playerId,bool isCityMall)
        {
            try
            {
                if(string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Session.GetString("CompetitionScoreDetails")) && string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Session.GetString("CompetitionStart")))
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
                    return Json(new { isSuccess = true, partial = viewContent, });
                }
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false });
            }
        }
    }
}

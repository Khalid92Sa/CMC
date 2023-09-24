using AutoMapper;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Helpers;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Presentation.Application.DTOs.Competitions;
using CMC.Presentation.Application.DTOs.Questions;
using CMC.Presentation.Application.Helpers;
using CMC.Presentation.Application.Services.Identity.Interfaces;
using CMC.Presentation.Application.Services.Players;
using CMC.Presentation.Application.Services.Questions;
using CMC.Presentation.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.Services.Competitions
{
    public class CompetitionsService : BaseServiceHandler, ICompetitionsService
    {
        readonly IMapper _mapper;
        readonly IApplicationLogger _logger;
        readonly IRepository<Competition> _competitionRepository;
        readonly IRepository<Team> _teamRepository;
        readonly IRepository<CompetitionQuestion> _compQuestRepository;
        readonly IUserService _userService;
        readonly IQuestionsService _questionsService;
        readonly IStringLocalizer<PlayerService> _localizer;
        public static IHttpContextAccessor _httpContextAccessor { get { return new HttpContextAccessor(); } }

        public CompetitionsService(IMapper mapper,
            IApplicationLogger logger,
            IRepository<Competition> competitionRepository,
            IRepository<CompetitionQuestion> compQuestRepository,
            IRepository<Team> teamRepository,
            IUserService userService,
            IQuestionsService questionsService,
            IUnitOfWork unitOfWork,
            IStringLocalizer<PlayerService> localizer,
            IValidatorFactory validatorFactory) : base(validatorFactory, unitOfWork)
        {
            _mapper = mapper;
            _logger = logger;
            _localizer = localizer;
            _competitionRepository = competitionRepository;
            _teamRepository = teamRepository;
            _compQuestRepository = compQuestRepository;
            _userService = userService;
            _questionsService = questionsService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<object>> Validate(object obj)
        {
            var valid = await ValidateAsync(obj);
            return valid.ConvertToResponseOf<object>(obj);
        }

        /// <summary>
        /// Add or update competition
        /// </summary>
        /// <param name="competitionsDTO"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Response> AddOrUpdateCompetition(CompetitionsDTO competitionsDTO)
        {
            try
            {
                // validate login model for required fields.
                var validModel = await Validate(competitionsDTO);
                if (!validModel.Succeeded)
                    return new Response<CompetitionsDTO>()
                    {
                        BrokenRules = validModel.BrokenRules,
                        StatusCode = (int)HttpStatusCode.BusinessRuleViolation
                    };


                Competition competition = new Competition();
                if (competitionsDTO.Id.HasValue)
                {
                    // Update
                    competition = await _competitionRepository.GetAll(a => a.Id == competitionsDTO.Id.Value && a.IsDeleted != true)
                        .Include(a => a.Team1)
                        .Include(a => a.Team2)
                        .SingleOrDefaultAsync();

                    if (competition != null)
                    {
                        competition.ModifiedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                        competition.ModifiedOn = DateTime.Now;
                    }
                    else
                        return new Response()
                        {
                            Succeeded = false,
                            StatusCode = (int)HttpStatusCode.NotFound
                        };
                }

                //Map fields
                competition.Name = competitionsDTO.Name;
                competition.QuestionsCount = competitionsDTO.QuestionCount;
                if (competitionsDTO.StartDate.HasValue)
                    competition.StartDate = competitionsDTO.StartDate;
                if (competitionsDTO.HostID.HasValue)
                    competition.HostID = competitionsDTO.HostID;


                if (competition.Team1 == null)
                    competition.Team1 = new Team();
                if (competition.Team2 == null)
                    competition.Team2 = new Team();

                //Team1 - CityMall
                competition.Team1.TeamName = competitionsDTO.Team1Name;
                //Player1

                if (competitionsDTO.Team1.Player1.HasValue)
                    competition.Team1.Player1Id = competitionsDTO.Team1.Player1.Value;
                else
                    competition.Team1.Player1Id = null;

                //Player2
                if (competitionsDTO.Team1.Player2.HasValue)
                    competition.Team1.Player2Id = competitionsDTO.Team1.Player2.Value;
                else
                    competition.Team1.Player2Id = null;

                //Player3
                if (competitionsDTO.Team1.Player3.HasValue)
                    competition.Team1.Player3Id = competitionsDTO.Team1.Player3.Value;
                else
                    competition.Team1.Player3Id = null;

                //Player4
                if (competitionsDTO.Team1.Player4.HasValue)
                    competition.Team1.Player4Id = competitionsDTO.Team1.Player4.Value;
                else
                    competition.Team1.Player4Id = null;



                //Team2
                competition.Team2.TeamName = competitionsDTO.Team2Name;
                //Player1
                if (competitionsDTO.Team2.Player1.HasValue)
                    competition.Team2.Player1Id = competitionsDTO.Team2.Player1.Value;
                else
                    competition.Team2.Player1Id = null;

                //Player2
                if (competitionsDTO.Team2.Player2.HasValue)
                    competition.Team2.Player2Id = competitionsDTO.Team2.Player2.Value;
                else
                    competition.Team2.Player2Id = null;

                //Player3
                if (competitionsDTO.Team2.Player3.HasValue)
                    competition.Team2.Player3Id = competitionsDTO.Team2.Player3.Value;
                else
                    competition.Team2.Player3Id = null;

                //Player4
                if (competitionsDTO.Team2.Player4.HasValue)
                    competition.Team2.Player4Id = competitionsDTO.Team2.Player4.Value;
                else
                    competition.Team2.Player4Id = null;

                //Save Or Update
                if (competitionsDTO.Id.HasValue)
                {
                    //Update team then competition
                    _teamRepository.Update(competition.Team1);
                    _teamRepository.Update(competition.Team2);
                    await _teamRepository.UnitOfWork.SaveChangesAsync();

                    _competitionRepository.Update(competition);
                }
                else
                {
                    // Create new Competition
                    competition.CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    competition.CreatedOn = DateTime.Now;
                    competition.Team1.CreatedBy = competition.Team2.CreatedBy = competition.CreatedBy;
                    competition.Team1.CreatedOn = competition.Team2.CreatedOn = competition.CreatedOn;

                    await _competitionRepository.InsertAsync(competition);
                }

                await _competitionRepository.UnitOfWork.SaveChangesAsync();

                return new Response()
                {
                    Succeeded = true,
                    StatusCode = (int)HttpStatusCode.Ok
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "AddOrUpdateCompetition", competitionsDTO, null, false);
                return new Response()
                {
                    Message = ex.Message,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<Response> FinishCompetition(CompetitionsDTO competitionsDTO)
        {
            try
            {
                var competition = await _competitionRepository.GetAll(a => a.Id == competitionsDTO.Id.Value && a.IsDeleted != true)
                    .Include(a => a.Team1)
                    .Include(a => a.Team2)
                    .SingleOrDefaultAsync();

                competition.EndDate = DateTime.Now;
                competition.WinningPlayerId = competitionsDTO.WinningPlayer.Id;
                if (competitionsDTO.WinningPlayer.IsEmployee)
                    competition.WinningTeamId = competition.Team1Id;
                else
                    competition.WinningTeamId = competition.Team2Id;

                competition.Team1Score = competitionsDTO.Team1Score.Value;
                competition.Team2Score = competitionsDTO.Team2Score.Value;

                _competitionRepository.Update(competition);
                await _competitionRepository.UnitOfWork.SaveChangesAsync();

                return new Response()
                {
                    Succeeded = true
                };
            }
            catch (Exception ex)
            {
                return new Response()
                {
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Get all competitions for Host
        /// </summary>
        /// <param name="hostId"></param>
        /// <returns></returns>
        public async Task<Response<List<CompetitionsDTO>>> GetCompetitionByHostId(int hostId)
        {
            try
            {
                List<CompetitionsDTO> CompetitionsDto = new List<CompetitionsDTO>();
                var competitions = await _competitionRepository.GetAll(a => a.HostID == hostId && a.IsDeleted != true).Include(a => a.Host).ToListAsync();
                if (competitions.Count > 0)
                {
                    competitions.ForEach(competition =>
                    {
                        CompetitionsDto.Add(new CompetitionsDTO()
                        {
                            Name = competition.Name,
                            HostID = competition.HostID,
                            StartDate = competition.StartDate,
                            EndDate = competition.EndDate
                        });
                    });
                }

                return new Response<List<CompetitionsDTO>>
                {
                    Data = CompetitionsDto,
                    Succeeded = true
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetCompetitionByHostId", hostId, null, false);
                return new Response<List<CompetitionsDTO>>()
                {
                    Message = ex.Message,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        /// <summary>
        /// Get Competitions by search
        /// </summary>
        /// <param name="searchCompetitionDTO"></param>
        /// <returns></returns>
        public async Task<PagedResult<CompetitionListDTO>> GetCompetitions(SearchCompetitionDTO searchCompetitionDTO)
        {
            try
            {
                PagedResult<CompetitionListDTO> response = new PagedResult<CompetitionListDTO>();
                var competitions = _competitionRepository.GetAll(a => a.IsDeleted != true)
                    .Include(a => a.Host)
                    .AsQueryable();

                var result = competitions
                        .WhereIf(!string.IsNullOrEmpty(searchCompetitionDTO.CompetitionName), a => a.Name.Contains(searchCompetitionDTO.CompetitionName))
                        .WhereIf(searchCompetitionDTO.CompetitonStartDate.HasValue, a => a.StartDate == searchCompetitionDTO.CompetitonStartDate)
                        .WhereIf(searchCompetitionDTO.HostId.HasValue, a => a.HostID == searchCompetitionDTO.HostId)
                        .OrderByDescending(a => a.CreatedOn)
                        .ToQueryResultAsync(searchCompetitionDTO.PageNumber, searchCompetitionDTO.PageSize);

                response.PageSize = result.Result.PageSize;
                response.CurrentPage = result.Result.CurrentPage;
                response.TotalCount = result.Result.TotalCount;
                response.BrokenRules = result.Result.BrokenRules;
                response.Data = result.Result.Data.Select(x => new CompetitionListDTO
                {
                    Id = x.Id,
                    CompetitionName = x.Name,
                    CompetitionStartDate = x.StartDate.HasValue ? x.StartDate.Value.ToString("dd-MM-yyyy") : null,
                    CompetitionEndDate = x.EndDate.HasValue ? x.EndDate.Value.ToString("dd-MM-yyyy") : null,
                    HostName = !string.IsNullOrEmpty(x.Host.Name) ? Security.Decrypt(x.Host.Name) : null,
                    IsFinished = x.EndDate.HasValue
                });

                return response;
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetCompetitions", null, null, false);
                return new PagedResult<CompetitionListDTO>
                {
                    Message = ex.Message,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        /// <summary>
        /// Get Competition by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<Response<CompetitionsDTO>> GetCompetition(int Id)
        {
            try
            {
                var competition = await _competitionRepository.GetAll(a => a.Id == Id)
                    .Include(a => a.Team1)
                    .Include(a => a.Team2)
                    .SingleOrDefaultAsync();

                if (competition != null)
                {
                    return new Response<CompetitionsDTO>()
                    {
                        Succeeded = true,
                        Data = new CompetitionsDTO()
                        {
                            Id = competition.Id,
                            Name = competition.Name,
                            StartDate = competition.StartDate,
                            QuestionCount = competition.QuestionsCount,
                            Team1Name = competition.Team1.TeamName,
                            Team1 = new TeamDTO()
                            {
                                Id = competition.Team1.Id,
                                Player1 = competition.Team1.Player1Id,
                                Player2 = competition.Team1.Player2Id,
                                Player3 = competition.Team1.Player3Id,
                                Player4 = competition.Team1.Player4Id,
                            },
                            Team2Name = competition.Team2.TeamName,
                            Team2 = new TeamDTO()
                            {
                                Id = competition.Team2.Id,
                                Player1 = competition.Team2.Player1Id,
                                Player2 = competition.Team2.Player2Id,
                                Player3 = competition.Team2.Player3Id,
                                Player4 = competition.Team2.Player4Id
                            },
                            HostID = competition.HostID
                        }
                    };
                }
                else
                    return new Response<CompetitionsDTO>()
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Response<ViewCompetitionScoresDTO>> ViewCompetitionScore(int Id)
        {
            try
            {
                ViewCompetitionScoresDTO response = new ViewCompetitionScoresDTO();
                var competition = await _competitionRepository.GetAll(a => a.Id == Id && a.IsDeleted != true)
                       .Include(a => a.CompetitionQuestions)
                       .ThenInclude(a => a.Question)
                       .Include(a => a.CompetitionQuestions)
                       .ThenInclude(a=>a.Answer)
                       .Include(a=>a.WinningPlayer)
                       .Include(a=>a.WinningTeam)
                       .Include(a => a.Team1)
                           .ThenInclude(t => t.Player1)
                       .Include(a => a.Team1)
                           .ThenInclude(t => t.Player2)
                       .Include(a => a.Team1)
                           .ThenInclude(t => t.Player3)
                       .Include(a => a.Team1)
                           .ThenInclude(t => t.Player4)
                       .Include(a => a.Team2)
                           .ThenInclude(t => t.Player1)
                       .Include(a => a.Team2)
                           .ThenInclude(t => t.Player2)
                       .Include(a => a.Team2)
                           .ThenInclude(t => t.Player3)
                       .Include(a => a.Team2)
                           .ThenInclude(t => t.Player4)
                           .FirstOrDefaultAsync();


                bool IsAr = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar";

                response.Id = competition.Id;
                response.Name = competition.Name;
                response.StartDate = competition.StartDate;
                response.EndDate = competition.EndDate;
                response.WinningTeamName = competition.WinningTeam.Id == competition.Team1Id ? _localizer["CityMallTeam"].Value : _localizer["VisitorsTeam"].Value;
                response.WinningPlayerName = competition.WinningPlayer.Name;
                response.TotalWinningPlayerScore = competition.CompetitionQuestions.Where(a => a.PlayerId == competition.WinningPlayerId.Value).Sum(a => a.Point).Value;


                //Fill Team 1 - CityMall
                response.Team1Name = competition.Team1.TeamName;

                //Player 1
                CompetitionsPlayerDTO cityMall_Player1 = new CompetitionsPlayerDTO();
                cityMall_Player1.Id = competition.Team1.Player1.Id;
                cityMall_Player1.Name = competition.Team1.Player1.Name;
                cityMall_Player1.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team1.Player1.Id).Sum(a => a.Point).Value;
                competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team1.Player1.Id).ToList().ForEach(question =>
                {
                    cityMall_Player1.competitonQuestions.Add(new CompetitonQuestions()
                    {
                        Points = question.Point ?? 0,
                        QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                        AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                        IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                    });
                });
                response.TeamCityMall.Add(cityMall_Player1);




                if (competition.Team1.Player2 != null)
                {
                    //Player2
                    CompetitionsPlayerDTO cityMall_Player2 = new CompetitionsPlayerDTO();
                    cityMall_Player2.Id = competition.Team1.Player2.Id;
                    cityMall_Player2.Name = competition.Team1.Player2.Name;
                    cityMall_Player2.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team1.Player2.Id).Sum(a => a.Point).Value;
                    competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team1.Player2.Id).ToList().ForEach(question =>
                    {
                        cityMall_Player2.competitonQuestions.Add(new CompetitonQuestions()
                        {
                            Points = question.Point ?? 0,
                            QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                            AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                            IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                        });
                    });
                    response.TeamCityMall.Add(cityMall_Player2);
                }



                if (competition.Team1.Player3 != null)
                {
                    //Player3
                    CompetitionsPlayerDTO cityMall_Player3 = new CompetitionsPlayerDTO();
                    cityMall_Player3.Id = competition.Team1.Player3.Id;
                    cityMall_Player3.Name = competition.Team1.Player3.Name;
                    cityMall_Player3.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team1.Player3.Id).Sum(a => a.Point).Value;
                    competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team1.Player3.Id).ToList().ForEach(question =>
                    {
                        cityMall_Player3.competitonQuestions.Add(new CompetitonQuestions()
                        {
                            Points = question.Point ?? 0,
                            QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                            AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                            IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                        });
                    });
                    response.TeamCityMall.Add(cityMall_Player3);
                }



                if (competition.Team1.Player4 != null)
                {
                    //Player4
                    CompetitionsPlayerDTO cityMall_Player4 = new CompetitionsPlayerDTO();
                    cityMall_Player4.Id = competition.Team1.Player4.Id;
                    cityMall_Player4.Name = competition.Team1.Player4.Name;
                    cityMall_Player4.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team1.Player4.Id).Sum(a => a.Point).Value;
                    competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team1.Player4.Id).ToList().ForEach(question =>
                    {
                        cityMall_Player4.competitonQuestions.Add(new CompetitonQuestions()
                        {
                            Points = question.Point ?? 0,
                            QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                            AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                            IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                        });
                    });
                    response.TeamCityMall.Add(cityMall_Player4);
                }







                //Fill Team 2 - Vistors
                response.Team2Name = competition.Team2.TeamName;

                //Player 1
                CompetitionsPlayerDTO Visitors_Player1 = new CompetitionsPlayerDTO();
                Visitors_Player1.Id = competition.Team2.Player1.Id;
                Visitors_Player1.Name = competition.Team2.Player1.Name;
                Visitors_Player1.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team2.Player1.Id).Sum(a => a.Point).Value;
                competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team2.Player1.Id).ToList().ForEach(question =>
                {
                    Visitors_Player1.competitonQuestions.Add(new CompetitonQuestions()
                    {
                        Points = question.Point ?? 0,
                        QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                        AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                        IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                    });
                });
                response.OtherTeam.Add(Visitors_Player1);




                if (competition.Team2.Player2 != null)
                {
                    //Player2
                    CompetitionsPlayerDTO Visitors_Player2 = new CompetitionsPlayerDTO();
                    Visitors_Player2.Id = competition.Team2.Player2.Id;
                    Visitors_Player2.Name = competition.Team2.Player2.Name;
                    Visitors_Player2.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team2.Player2.Id).Sum(a => a.Point).Value;
                    competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team2.Player2.Id).ToList().ForEach(question =>
                    {
                        Visitors_Player2.competitonQuestions.Add(new CompetitonQuestions()
                        {
                            Points = question.Point ?? 0,
                            QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                            AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                            IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                        });
                    });
                    response.OtherTeam.Add(Visitors_Player2);
                }



                if (competition.Team2.Player3 != null)
                {
                    //Player3
                    CompetitionsPlayerDTO Visitors_Player3 = new CompetitionsPlayerDTO();
                    Visitors_Player3.Id = competition.Team2.Player3.Id;
                    Visitors_Player3.Name = competition.Team2.Player3.Name;
                    Visitors_Player3.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team2.Player3.Id).Sum(a => a.Point).Value;
                    competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team2.Player3.Id).ToList().ForEach(question =>
                    {
                        Visitors_Player3.competitonQuestions.Add(new CompetitonQuestions()
                        {
                            Points = question.Point ?? 0,
                            QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                            AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                            IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                        });
                    });
                    response.OtherTeam.Add(Visitors_Player3);
                }



                if (competition.Team2.Player4 != null)
                {
                    //Player4
                    CompetitionsPlayerDTO Visitors_Player4 = new CompetitionsPlayerDTO();
                    Visitors_Player4.Id = competition.Team2.Player4.Id;
                    Visitors_Player4.Name = competition.Team2.Player4.Name;
                    Visitors_Player4.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team2.Player4.Id).Sum(a => a.Point).Value;
                    competition.CompetitionQuestions.Where(a => a.PlayerId == competition.Team2.Player4.Id).ToList().ForEach(question =>
                    {
                        Visitors_Player4.competitonQuestions.Add(new CompetitonQuestions()
                        {
                            Points = question.Point ?? 0,
                            QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                            AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                            IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                        });
                    });
                    response.OtherTeam.Add(Visitors_Player4);
                }


                var competitonString = JsonConvert.SerializeObject(response);
                _httpContextAccessor.HttpContext.Session.SetString("CompetitionScoreDetails", competitonString);

                return new Response<ViewCompetitionScoresDTO>()
                {
                    Data = response,
                    Succeeded = true
                };
            }
            catch (Exception ex)
            {
                return new Response<ViewCompetitionScoresDTO>()
                {
                    Message = ex.Message
                };
            }
        }

        public async Task<Response<List<LatestCompeitionsScore>>> GetLatestScores()
        {
            try
            {
                List<LatestCompeitionsScore> lastScores = new List<LatestCompeitionsScore>();
                var competitions = await _competitionRepository.GetAll(a => a.IsDeleted != true && a.EndDate.HasValue && a.WinningPlayerId.HasValue)
                       .Include(a => a.CompetitionQuestions)
                            .ThenInclude(a=>a.Player)
                       .Include(a => a.WinningPlayer)
                       .Include(a => a.WinningTeam)
                       .Include(a => a.Team1)
                           .ThenInclude(t => t.Player1)
                       .Include(a => a.Team1)
                           .ThenInclude(t => t.Player2)
                       .Include(a => a.Team1)
                           .ThenInclude(t => t.Player3)
                       .Include(a => a.Team1)
                           .ThenInclude(t => t.Player4)
                       .Include(a => a.Team2)
                           .ThenInclude(t => t.Player1)
                       .Include(a => a.Team2)
                           .ThenInclude(t => t.Player2)
                       .Include(a => a.Team2)
                           .ThenInclude(t => t.Player3)
                       .Include(a => a.Team2)
                           .ThenInclude(t => t.Player4)
                           .OrderByDescending(a => a.EndDate.Value)
                           .Take(5)
                           .ToListAsync();

                if(competitions!=null && competitions.Count > 0)
                {
                    foreach (var competition in competitions)
                    {
                        LatestCompeitionsScore latestCompeitionsScore = new LatestCompeitionsScore();
                        latestCompeitionsScore.CompeititonName = competition.Name;
                        latestCompeitionsScore.EndDate = competition.EndDate.Value;
                        latestCompeitionsScore.WinningTeamName = competition.WinningTeam.Id == competition.Team1Id ? _localizer["CityMallTeam"].Value : _localizer["VisitorsTeam"].Value;
                        var cityMallWinningPlayer = competition.CompetitionQuestions
                                .Where(a => a.IsTeam1 == true)
                                .GroupBy(a => a.PlayerId)
                                .Select(g => new
                                {
                                    Player = g.First().Player,
                                    TotalPoints = g.Sum(a => a.Point)
                                })
                                .OrderByDescending(x => x.TotalPoints)
                                .FirstOrDefault();

                        if (cityMallWinningPlayer != null)
                        {
                            latestCompeitionsScore.WinningCityMallPlayerName = cityMallWinningPlayer.Player.Name;
                            latestCompeitionsScore.CityMallPlayerPoints = cityMallWinningPlayer.TotalPoints ?? 0;
                        }

                        var OtherTeamWinningPlayer = competition.CompetitionQuestions
                                .Where(a => a.IsTeam1 == false)
                                .GroupBy(a => a.PlayerId)
                                .Select(g => new
                                {
                                    Player = g.First().Player,
                                    TotalPoints = g.Sum(a => a.Point)
                                })
                                .OrderByDescending(x => x.TotalPoints)
                                .FirstOrDefault();

                        if (OtherTeamWinningPlayer != null)
                        {
                            latestCompeitionsScore.WinningOtherPlayerName = OtherTeamWinningPlayer.Player.Name;
                            latestCompeitionsScore.OtherPlayerPoints = OtherTeamWinningPlayer.TotalPoints ?? 0;
                        }

                        lastScores.Add(latestCompeitionsScore);
                    }
                }

                return new Response<List<LatestCompeitionsScore>>()
                {
                    Succeeded = true,
                    Data = lastScores
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Delete competition
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Response> DeleteCompetition(int id)
        {
            try
            {
                var competition = await _competitionRepository.FindAsync(id);
                if (competition != null)
                {
                    competition.IsDeleted = true;
                    competition.DeletedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    competition.DeletedOn = DateTime.Now;
                    _competitionRepository.Update(competition);
                    await _competitionRepository.UnitOfWork.SaveChangesAsync();

                    return new Response { StatusCode = (int)HttpStatusCode.Ok, Succeeded = true };
                }
                else
                    return new Response()
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
            }
            catch (Exception ex)
            {
                return new Response()
                {
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                };
            }
        }

        /// <summary>
        /// Start competitons
        /// </summary>
        /// <returns></returns>
        public async Task<Response<CompetitionStartDTO>> StartCompetiton(int id)
        {
            try
            {
                var competition = await _competitionRepository.GetAll(a => a.Id == id && a.IsDeleted != true && !a.EndDate.HasValue)
                    .Include(a => a.CompetitionQuestions)
                       .ThenInclude(a => a.Question)
                       .ThenInclude(a=>a.Answers)
                       .Include(a => a.CompetitionQuestions)
                       .ThenInclude(a => a.Answer)
                        .Include(a => a.Team1)
                            .ThenInclude(t => t.Player1)
                        .Include(a => a.Team1)
                            .ThenInclude(t => t.Player2)
                        .Include(a => a.Team1)
                            .ThenInclude(t => t.Player3)
                        .Include(a => a.Team1)
                            .ThenInclude(t => t.Player4)
                        .Include(a => a.Team2)
                            .ThenInclude(t => t.Player1)
                        .Include(a => a.Team2)
                            .ThenInclude(t => t.Player2)
                        .Include(a => a.Team2)
                            .ThenInclude(t => t.Player3)
                        .Include(a => a.Team2)
                            .ThenInclude(t => t.Player4)
                            .FirstOrDefaultAsync();

                bool IsAr = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar";


                if (competition != null)
                {
                    CompetitionStartDTO competitionStartDTO = new CompetitionStartDTO();
                    competitionStartDTO.Id = id;
                    competitionStartDTO.TotalQuestion = competition.QuestionsCount;
                    competitionStartDTO.TeamCityMall = new List<CompetitionsPlayerDTO>();
                    competitionStartDTO.OtherTeam = new List<CompetitionsPlayerDTO>();

                    bool IsCompetitionStartedBefore = competition.CompetitionQuestions != null && competition.CompetitionQuestions.Count > 0;

                    //Add CompetitionQuestions
                    foreach(var question in competition.CompetitionQuestions)
                    {
                        QuestionVM questionVM = new QuestionVM();
                        questionVM.Id = question.Question.Id;
                        questionVM.CategoryId = question.Question.CategoryID;
                        questionVM.TextEn = question.Question.TextEn;
                        questionVM.TextAr = question.Question.TextAr;
                        questionVM.Points = question.Question.Points;
                        questionVM.Time = question.Question.Timer;

                        List<AnswerOptions> answerOptions = new List<AnswerOptions>();
                        foreach(var answer in question.Question.Answers.Where(a => a.IsDeleted != true).ToList())
                        {
                            AnswerOptions option = new AnswerOptions();
                            option.Id = answer.Id;
                            option.TextEn = answer.TextEn;
                            option.TextAr = answer.TextAr;
                            option.IsImg = answer.IsImg ?? false;
                            option.ImgPath = answer.ImgPath;
                            option.IsAnswer = answer.IsAnswer;
                            answerOptions.Add(option);
                        }
                        questionVM.Answers = answerOptions;
                        competitionStartDTO.Questions.Add(questionVM);
                    }


                    //City Mall Team
                    if (competition.Team1.Player1 != null)
                    {
                        CompetitionsPlayerDTO cityMallPlayer1 = new CompetitionsPlayerDTO();
                        cityMallPlayer1.Id = competition.Team1.Player1.Id;
                        cityMallPlayer1.Name = competition.Team1.Player1.Name;
                        cityMallPlayer1.Points = 0;
                        if (IsCompetitionStartedBefore)
                        {
                            cityMallPlayer1.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == cityMallPlayer1.Id).Sum(a => a.Point).Value;
                            competition.CompetitionQuestions.Where(a => a.PlayerId == cityMallPlayer1.Id).ToList().ForEach(question =>
                            {
                                cityMallPlayer1.competitonQuestions.Add(new CompetitonQuestions()
                                {
                                    Points = question.Point ?? 0,
                                    QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                                    AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                                    IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                                });
                            });
                        }
                        competitionStartDTO.TeamCityMall.Add(cityMallPlayer1);
                    }

                    if (competition.Team1.Player2 != null)
                    {
                        CompetitionsPlayerDTO cityMallPlayer2 = new CompetitionsPlayerDTO();
                        cityMallPlayer2.Id = competition.Team1.Player2.Id;
                        cityMallPlayer2.Name = competition.Team1.Player2.Name;
                        cityMallPlayer2.Points = 0;
                        if (IsCompetitionStartedBefore)
                        {
                            cityMallPlayer2.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == cityMallPlayer2.Id).Sum(a => a.Point).Value;
                            competition.CompetitionQuestions.Where(a => a.PlayerId == cityMallPlayer2.Id).ToList().ForEach(question =>
                            {
                                cityMallPlayer2.competitonQuestions.Add(new CompetitonQuestions()
                                {
                                    Points = question.Point ?? 0,
                                    QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                                    AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                                    IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                                });
                            });
                        }
                        competitionStartDTO.TeamCityMall.Add(cityMallPlayer2);
                    }

                    if (competition.Team1.Player3 != null)
                    {
                        CompetitionsPlayerDTO cityMallPlayer3 = new CompetitionsPlayerDTO();
                        cityMallPlayer3.Id = competition.Team1.Player3.Id;
                        cityMallPlayer3.Name = competition.Team1.Player3.Name;
                        cityMallPlayer3.Points = 0;
                        if (IsCompetitionStartedBefore)
                        {
                            cityMallPlayer3.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == cityMallPlayer3.Id).Sum(a => a.Point).Value;
                            competition.CompetitionQuestions.Where(a => a.PlayerId == cityMallPlayer3.Id).ToList().ForEach(question =>
                            {
                                cityMallPlayer3.competitonQuestions.Add(new CompetitonQuestions()
                                {
                                    Points = question.Point ?? 0,
                                    QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                                    AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                                    IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                                });
                            });
                        }
                        competitionStartDTO.TeamCityMall.Add(cityMallPlayer3);
                    }

                    if (competition.Team1.Player4 != null)
                    {
                        CompetitionsPlayerDTO cityMallPlayer4 = new CompetitionsPlayerDTO();
                        cityMallPlayer4.Id = competition.Team1.Player4.Id;
                        cityMallPlayer4.Name = competition.Team1.Player4.Name;
                        cityMallPlayer4.Points = 0;
                        if (IsCompetitionStartedBefore)
                        {
                            cityMallPlayer4.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == cityMallPlayer4.Id).Sum(a => a.Point).Value;
                            competition.CompetitionQuestions.Where(a => a.PlayerId == cityMallPlayer4.Id).ToList().ForEach(question =>
                            {
                                cityMallPlayer4.competitonQuestions.Add(new CompetitonQuestions()
                                {
                                    Points = question.Point ?? 0,
                                    QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                                    AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                                    IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                                });
                            });
                        }
                        competitionStartDTO.TeamCityMall.Add(cityMallPlayer4);
                    }


                    // Other Team
                    if (competition.Team2.Player1 != null)
                    {
                        CompetitionsPlayerDTO otherPlayer1 = new CompetitionsPlayerDTO();
                        otherPlayer1.Id = competition.Team2.Player1.Id;
                        otherPlayer1.Name = competition.Team2.Player1.Name;
                        otherPlayer1.Points = 0;
                        if (IsCompetitionStartedBefore)
                        {
                            otherPlayer1.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == otherPlayer1.Id).Sum(a => a.Point).Value;
                            competition.CompetitionQuestions.Where(a => a.PlayerId == otherPlayer1.Id).ToList().ForEach(question =>
                            {
                                otherPlayer1.competitonQuestions.Add(new CompetitonQuestions()
                                {
                                    Points = question.Point ?? 0,
                                    QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                                    AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                                    IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                                });
                            });
                        }
                        competitionStartDTO.OtherTeam.Add(otherPlayer1);
                    }

                    if (competition.Team2.Player2 != null)
                    {
                        CompetitionsPlayerDTO otherPlayer2 = new CompetitionsPlayerDTO();
                        otherPlayer2.Id = competition.Team2.Player2.Id;
                        otherPlayer2.Name = competition.Team2.Player2.Name;
                        otherPlayer2.Points = 0;
                        if (IsCompetitionStartedBefore)
                        {
                            otherPlayer2.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == otherPlayer2.Id).Sum(a => a.Point).Value;
                            competition.CompetitionQuestions.Where(a => a.PlayerId == otherPlayer2.Id).ToList().ForEach(question =>
                            {
                                otherPlayer2.competitonQuestions.Add(new CompetitonQuestions()
                                {
                                    Points = question.Point ?? 0,
                                    QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                                    AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                                    IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                                });
                            });
                        }
                        competitionStartDTO.OtherTeam.Add(otherPlayer2);

                    }

                    if (competition.Team2.Player3 != null)
                    {
                        CompetitionsPlayerDTO otherPlayer3 = new CompetitionsPlayerDTO();
                        otherPlayer3.Id = competition.Team2.Player3.Id;
                        otherPlayer3.Name = competition.Team2.Player3.Name;
                        otherPlayer3.Points = 0;
                        if (IsCompetitionStartedBefore)
                        {
                            otherPlayer3.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == otherPlayer3.Id).Sum(a => a.Point).Value;
                            competition.CompetitionQuestions.Where(a => a.PlayerId == otherPlayer3.Id).ToList().ForEach(question =>
                            {
                                otherPlayer3.competitonQuestions.Add(new CompetitonQuestions()
                                {
                                    Points = question.Point ?? 0,
                                    QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                                    AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                                    IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                                });
                            });
                        }
                        competitionStartDTO.OtherTeam.Add(otherPlayer3);
                    }

                    if (competition.Team2.Player4 != null)
                    {
                        CompetitionsPlayerDTO otherPlayer4 = new CompetitionsPlayerDTO();
                        otherPlayer4.Id = competition.Team2.Player4.Id;
                        otherPlayer4.Name = competition.Team2.Player4.Name;
                        otherPlayer4.Points = 0;
                        if (IsCompetitionStartedBefore)
                        {
                            otherPlayer4.Points = competition.CompetitionQuestions.Where(a => a.PlayerId == otherPlayer4.Id).Sum(a => a.Point).Value;
                            competition.CompetitionQuestions.Where(a => a.PlayerId == otherPlayer4.Id).ToList().ForEach(question =>
                            {
                                otherPlayer4.competitonQuestions.Add(new CompetitonQuestions()
                                {
                                    Points = question.Point ?? 0,
                                    QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn,
                                    AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn,
                                    IsCorrectAnswer = question.IsCorrectAnswer ?? false,
                                });
                            });
                        }
                        competitionStartDTO.OtherTeam.Add(otherPlayer4);
                    }


                    //Fill Categories
                    competitionStartDTO.Categories = await _questionsService.GetCategories();

                    var competitonString = JsonConvert.SerializeObject(competitionStartDTO);
                    _httpContextAccessor.HttpContext.Session.SetString("CompetitionStart", competitonString);

                    return new Response<CompetitionStartDTO>()
                    {
                        Succeeded = true,
                        Data = competitionStartDTO
                    };
                }
                else
                    return new Response<CompetitionStartDTO>()
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Players answered on questions
        /// </summary>
        /// <param name="answerOnQuestionDTO"></param>
        /// <returns></returns>
        public async Task<Response> AnswerOnQuestions(int competitionId, AnswerOnQuestionDTO answerOnQuestionDTO)
        {
            try
            {
                CompetitionQuestion competitionQuestion = new CompetitionQuestion();
                competitionQuestion.CompetitionId = competitionId;
                competitionQuestion.PlayerId = answerOnQuestionDTO.PlayerId;
                competitionQuestion.IsTeam1 = answerOnQuestionDTO.IsCityMallPlayer;
                competitionQuestion.QuestionId = answerOnQuestionDTO.QuestionId.Value;
                competitionQuestion.AnswerId = answerOnQuestionDTO.AnswerId;
                competitionQuestion.IsCorrectAnswer = answerOnQuestionDTO.IsCorrectAnswer;
                if (competitionQuestion.IsCorrectAnswer == true)
                    competitionQuestion.Point = answerOnQuestionDTO.Points;

                competitionQuestion.CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                competitionQuestion.CreatedOn = DateTime.Now;

                await _compQuestRepository.InsertAsync(competitionQuestion);
                await _compQuestRepository.UnitOfWork.SaveChangesAsync();
                return new Response()
                {
                    Succeeded = true
                };
            }
            catch (Exception ex)
            {
                return new Response()
                {
                    Succeeded = false,
                    Message = ex.Message
                };
            }
        }
    }
}

using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Helpers;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Kernel.Domain.Entities;
using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Presentation.Application.DTOs.Competitions;
using CMC.Presentation.Application.DTOs.Questions;
using CMC.Presentation.Application.Services.Questions;
using CMC.Presentation.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        readonly IApplicationLogger _logger;
        readonly IRepository<Competition> _competitionRepository;
        readonly IRepository<Team> _teamRepository;
        readonly IRepository<CompetitionQuestion> _compQuestRepository;
        readonly IRepository<Attachment> _attachmentRepository;
        readonly IQuestionsService _questionsService;
        readonly IRepository<Player> _playerRepository;
        readonly IStringLocalizer<CompetitionsService> _localizer;
        public static IHttpContextAccessor _httpContextAccessor { get { return new HttpContextAccessor(); } }

        public CompetitionsService(
            IApplicationLogger logger,
            IRepository<Competition> competitionRepository,
            IRepository<CompetitionQuestion> compQuestRepository,
            IRepository<Team> teamRepository,
            IRepository<Player> playerRepository,
            IRepository<Attachment> attachmentRepository,
            IQuestionsService questionsService,
            IUnitOfWork unitOfWork,
            IStringLocalizer<CompetitionsService> localizer,
            IValidatorFactory validatorFactory) : base(validatorFactory, unitOfWork)
        {
            _logger = logger;
            _localizer = localizer;
            _competitionRepository = competitionRepository;
            _teamRepository = teamRepository;
            _playerRepository = playerRepository;
            _attachmentRepository = attachmentRepository;
            _compQuestRepository = compQuestRepository;
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
                    competition = await _competitionRepository.GetAll(a => a.Id == competitionsDTO.Id.Value && a.IsDeleted != true && a.EndDate == null)
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
                competition.StartDate = competitionsDTO.StartDate;
                competition.HostID = competitionsDTO.HostID;
                competition.IsFinalCompetition = competitionsDTO.IsFinalCompetition;

                competition.CompetitionQuestionType = competitionsDTO.CompettionQuestionType;
                if (competitionsDTO.CategoriesIds.Count > 0)
                    competition.CategoriesIds = string.Join(",", competitionsDTO.CategoriesIds);
                else
                    competition.CategoriesIds = null;

                competition.RoundCount = competitionsDTO.RoundCount;
                if (competitionsDTO.CompettionQuestionType == (int)CompetitionQuestionType.QuestionsPerPlayer)
                {
                    competitionsDTO.RoundCount = competition.RoundCount = 1;
                    competition.QuestionForEachPlayer = competitionsDTO.QuestionForEachPlayer;
                }
                else
                    competition.QuestionForEachPlayer = null;


                for (int i = 1; i <= 4; i++)
                {
                    int roundNumber = i;

                    if (competitionsDTO.RoundCount >= roundNumber)
                    {
                        string propertyNameTime = $"Round{roundNumber}Time";
                        string propertyNamePoints = $"Round{roundNumber}Points";

                        int? roundTime = (int?)competitionsDTO.GetType().GetProperty(propertyNameTime).GetValue(competitionsDTO);
                        int? roundPoints = (int?)competitionsDTO.GetType().GetProperty(propertyNamePoints).GetValue(competitionsDTO);

                        competition.GetType().GetProperty(propertyNameTime).SetValue(competition, roundTime);
                        competition.GetType().GetProperty(propertyNamePoints).SetValue(competition, roundPoints);
                    }
                    else
                    {
                        string propertyNameTime = $"Round{roundNumber}Time";
                        string propertyNamePoints = $"Round{roundNumber}Points";

                        competition.GetType().GetProperty(propertyNameTime).SetValue(competition, null);
                        competition.GetType().GetProperty(propertyNamePoints).SetValue(competition, null);
                    }
                }

                competition.ParentId = competitionsDTO.ParentId;

                // Map Archive Settings
                competition.ArchiveType = competitionsDTO.ArchiveType;
                competition.ArchiveMonths = competitionsDTO.ArchiveMonths;
                competition.ArchiveFromDate = competitionsDTO.ArchiveFromDate;
                competition.ArchiveToDate = competitionsDTO.ArchiveToDate;

                if (competitionsDTO.ExcludedCompetitionIds != null && competitionsDTO.ExcludedCompetitionIds.Count > 0)
                    competition.ExcludedCompetitionIds = string.Join(",", competitionsDTO.ExcludedCompetitionIds);
                else
                    competition.ExcludedCompetitionIds = null;

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
                var competition = await _competitionRepository.FindAsync(competitionsDTO.Id);

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
                await _logger.LogError(ex, "FinishCompetition", competitionsDTO, null, false);
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
        /// Get Competitions lookup
        /// </summary>
        /// <returns></returns>
        public async Task<Response<List<LookupModel>>> GetCompetitionsLookup()
        {
            try
            {
                var competitions = await _competitionRepository.GetAll(a => a.IsDeleted != true)
                    .OrderByDescending(a => a.EndDate)
                    .Select(a => new LookupModel()
                    {
                        Id = a.Id,
                        NameEn = a.Name,
                        NameAr = a.Name
                    }).ToListAsync();

                return new Response<List<LookupModel>>()
                {
                    Succeeded = true,
                    Data = competitions
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetCompetitionsLookup", null, null, false);
                throw;
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
                await _logger.LogError(ex, "GetCompetitions_Search", searchCompetitionDTO, null, false);
                return new PagedResult<CompetitionListDTO>
                {
                    Message = ex.Message,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = new List<CompetitionListDTO>()
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
                    CompetitionStartDTO competitionStartDto = competition.StateData != null ? JsonConvert.DeserializeObject<CompetitionStartDTO>(competition.StateData) : null;

                    return new Response<CompetitionsDTO>()
                    {
                        Succeeded = true,
                        Data = new CompetitionsDTO()
                        {
                            Id = competition.Id,
                            Name = competition.Name,
                            StartDate = competition.StartDate,
                            HostID = competition.HostID,
                            CategoriesIds = !string.IsNullOrEmpty(competition.CategoriesIds) ?
                                            competition.CategoriesIds.Split(',').Select(int.Parse).ToList() : new List<int>() { 0 },
                            RoundCount = competition.RoundCount,
                            CompettionQuestionType = competition.CompetitionQuestionType,
                            IsFinalCompetition = competition.IsFinalCompetition ?? false,
                            QuestionForEachPlayer = competition.QuestionForEachPlayer,
                            ParentId = competition.ParentId,
                            ArchiveType = competition.ArchiveType,
                            ArchiveMonths = competition.ArchiveMonths,
                            ArchiveFromDate = competition.ArchiveFromDate,
                            ArchiveToDate = competition.ArchiveToDate,
                            ExcludedCompetitionIds = !string.IsNullOrEmpty(competition.ExcludedCompetitionIds) ?
                                                     competition.ExcludedCompetitionIds.Split(',').Select(int.Parse).ToList() :
                                                     new List<int>(),
                            CompetitionStartDTO = competitionStartDto,
                            CurrentStep = competition.CurrentStep,
                            Round1Points = competition.Round1Points,
                            Round1Time = competition.Round1Time,
                            Round2Points = competition.Round2Points,
                            Round2Time = competition.Round2Time,
                            Round3Points = competition.Round3Points,
                            Round3Time = competition.Round3Time,
                            Round4Points = competition.Round4Points,
                            Round4Time = competition.Round4Time,
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
                await _logger.LogError(ex, "GetCompetition_Id", Id, null, false);
                throw ex;
            }
        }

        public async Task<Response<ViewCompetitionScoresDTO>> ViewCompetitionScore(int Id)
        {
            ViewCompetitionScoresDTO response = new ViewCompetitionScoresDTO();
            try
            {
                var competition = await _competitionRepository.GetAll(a => a.Id == Id)
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
                    .Include(a => a.Host)
                    .Include(a => a.WinningPlayer)
                    .Include(a => a.WinningTeam)
                    .Include(a => a.CompetitionQuestions)
                    .Include(a=>a.Parent)
                   
                    .SingleOrDefaultAsync();

                if (competition != null)
                {
                    response.Id = competition.Id;
                    response.Name = competition.Name;
                    response.HostName = !string.IsNullOrEmpty(competition.Host.Name) ? Security.Decrypt(competition.Host.Name) : null;
                    response.StartDate = competition.StartDate;
                    response.EndDate = competition.EndDate;
                    response.WinningTeamName = competition.WinningTeam.Id == competition.Team1Id ? competition.Team1.TeamName : competition.Team2.TeamName;
                    response.WinningPlayerName = competition.WinningPlayer.Name;
                    response.TotalWinningPlayerScore = competition.CompetitionQuestions.Where(a => a.PlayerId == competition.WinningPlayerId.Value).Sum(a => a.Point).Value;
                    response.CategoriesIds = !string.IsNullOrEmpty(competition.CategoriesIds) ?
                                            competition.CategoriesIds.Split(',').Select(int.Parse).ToList() : new List<int>() { 0 };
                    response.IsFinalCompetition = competition.IsFinalCompetition ?? false;
                    response.CompettionQuestionTypeId = competition.CompetitionQuestionType ?? 0;
                    response.CompettionQuestionType = competition.CompetitionQuestionType == (int)CompetitionQuestionType.Rounds ? _localizer["CompeitionQuestionsRound"].Value : _localizer["CompeitionQuestionsPerPlayer"].Value;
                    response.QuestionForEachPlayer = competition.QuestionForEachPlayer;

                    response.RoundCount = competition.RoundCount;
                    response.ParentCompetitionName = competition.ParentId.HasValue ? competition.Parent.Name : _localizer["None"].Value;
                    response.Round1Points = competition.Round1Points;
                    response.Round1Time = competition.Round1Time;
                    response.Round2Points = competition.Round2Points;
                    response.Round2Time = competition.Round2Time;
                    response.Round3Points = competition.Round3Points;
                    response.Round3Time = competition.Round3Time;
                    response.Round4Points = competition.Round4Points;
                    response.Round4Time = competition.Round4Time;
                    response.Team1Name = competition.Team1.TeamName;
                    response.Team2Name = competition.Team2.TeamName;


                    //    //Player 1
                    CompetitionsPlayerDTO cityMall_Player1 = new CompetitionsPlayerDTO();
                    cityMall_Player1.Id = competition.Team1.Player1.Id;
                    cityMall_Player1.Name = competition.Team1.Player1.Name;
                    response.TeamCityMall.Add(cityMall_Player1);


                    if (competition.Team1.Player2 != null)
                    {
                        //Player2
                        CompetitionsPlayerDTO cityMall_Player2 = new CompetitionsPlayerDTO();
                        cityMall_Player2.Id = competition.Team1.Player2.Id;
                        cityMall_Player2.Name = competition.Team1.Player2.Name;
                        response.TeamCityMall.Add(cityMall_Player2);
                    }


                    if (competition.Team1.Player3 != null)
                    {
                        //Player3
                        CompetitionsPlayerDTO cityMall_Player3 = new CompetitionsPlayerDTO();
                        cityMall_Player3.Id = competition.Team1.Player3.Id;
                        cityMall_Player3.Name = competition.Team1.Player3.Name;
                        response.TeamCityMall.Add(cityMall_Player3);
                    }


                    if (competition.Team1.Player4 != null)
                    {
                        //Player4
                        CompetitionsPlayerDTO cityMall_Player4 = new CompetitionsPlayerDTO();
                        cityMall_Player4.Id = competition.Team1.Player4.Id;
                        cityMall_Player4.Name = competition.Team1.Player4.Name;
                        response.TeamCityMall.Add(cityMall_Player4);
                    }






                    //    //Fill Team 2 - Vistors
                    //    //Player 1
                    CompetitionsPlayerDTO Visitors_Player1 = new CompetitionsPlayerDTO();
                    Visitors_Player1.Id = competition.Team2.Player1.Id;
                    Visitors_Player1.Name = competition.Team2.Player1.Name;
                    response.OtherTeam.Add(Visitors_Player1);




                    if (competition.Team2.Player2 != null)
                    {
                        //Player2
                        CompetitionsPlayerDTO Visitors_Player2 = new CompetitionsPlayerDTO();
                        Visitors_Player2.Id = competition.Team2.Player2.Id;
                        Visitors_Player2.Name = competition.Team2.Player2.Name;
                        response.OtherTeam.Add(Visitors_Player2);
                    }



                    if (competition.Team2.Player3 != null)
                    {
                        //Player3
                        CompetitionsPlayerDTO Visitors_Player3 = new CompetitionsPlayerDTO();
                        Visitors_Player3.Id = competition.Team2.Player3.Id;
                        Visitors_Player3.Name = competition.Team2.Player3.Name;
                        response.OtherTeam.Add(Visitors_Player3);
                    }



                    if (competition.Team2.Player4 != null)
                    {
                        //Player4
                        CompetitionsPlayerDTO Visitors_Player4 = new CompetitionsPlayerDTO();
                        Visitors_Player4.Id = competition.Team2.Player4.Id;
                        Visitors_Player4.Name = competition.Team2.Player4.Name;
                        response.OtherTeam.Add(Visitors_Player4);
                    }

                    return new Response<ViewCompetitionScoresDTO>()
                    {
                        Succeeded = true,
                        Data = response
                    };
                }
                else
                    return new Response<ViewCompetitionScoresDTO>()
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "ViewCompetitionScore", Id, null, false);
                throw ex;
            }
        }

        public async Task<Response<List<LatestCompeitionsScore>>> GetLatestScores()
        {
            try
            {
                List<LatestCompeitionsScore> lastScores = new List<LatestCompeitionsScore>();
                var competitions = await _competitionRepository.GetAll(a => a.IsDeleted != true && a.EndDate.HasValue && a.WinningPlayerId.HasValue)
                       .Include(a => a.CompetitionQuestions)
                            .ThenInclude(a => a.Player)
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

                if (competitions != null && competitions.Count > 0)
                {
                    foreach (var competition in competitions)
                    {
                        LatestCompeitionsScore latestCompeitionsScore = new LatestCompeitionsScore();
                        latestCompeitionsScore.CompeititonName = competition.Name;
                        latestCompeitionsScore.EndDate = competition.EndDate.Value;
                        latestCompeitionsScore.Team1Name = competition.Team1.TeamName;
                        latestCompeitionsScore.Team2Name = competition.Team2.TeamName;
                        latestCompeitionsScore.WinningTeamName = competition.WinningTeam.Id == competition.Team1Id ? competition.Team1.TeamName: competition.Team2.TeamName;
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
                await _logger.LogError(ex, "GetLatestScores", null, null, false);
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
                await _logger.LogError(ex, "DeleteCompetition", id, null, false);
                return new Response()
                {
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                };
            }
        }

        /// <summary>
        /// Get all questions to be excluded based on competition archive settings
        /// </summary>
        /// <param name="competition">Current competition with archive settings</param>
        /// <returns>List of questions to exclude</returns>
        private async Task<List<CompetitionQuestion>> GetAllExcludedQuestionsAsync(Competition competition)
        {
            List<CompetitionQuestion> excludedQuestions = new List<CompetitionQuestion>();

            try
            {
                // Always include parent competition questions (existing behavior)
                if (competition.Parent != null)
                {
                    var parentQuestions = await GetAllQuestionsForCompetitionAndParentsAsync(competition.Parent.Id);
                    excludedQuestions.AddRange(parentQuestions);
                }

                // Apply archive settings based on archive type - Direct enum casting!
                var archiveType = (QuestionArchiveTypeEnum)(competition.ArchiveType ?? 0);

                switch (archiveType)
                {
                    case QuestionArchiveTypeEnum.None:
                        // Only parent questions already added above
                        break;

                    case QuestionArchiveTypeEnum.TimeBased:
                        if (competition.ArchiveMonths.HasValue)
                        {
                            var cutoffDate = DateTime.Now.AddMonths(-competition.ArchiveMonths.Value);
                            var timeBasedQuestions = await _compQuestRepository.GetAll(cq =>
                                cq.Competition.EndDate.HasValue &&
                                cq.Competition.EndDate.Value >= cutoffDate &&
                                cq.Competition.Id != competition.Id)
                                .Include(cq => cq.Question)
                                .ToListAsync();

                            excludedQuestions.AddRange(timeBasedQuestions);
                        }
                        break;

                    case QuestionArchiveTypeEnum.CompetitionBased:
                        if (!string.IsNullOrEmpty(competition.ExcludedCompetitionIds))
                        {
                            var excludedCompIds = competition.ExcludedCompetitionIds.Split(',').Select(int.Parse).ToList();
                            var competitionBasedQuestions = await _compQuestRepository.GetAll(cq =>
                                excludedCompIds.Contains(cq.CompetitionId))
                                .Include(cq => cq.Question)
                                .ToListAsync();

                            excludedQuestions.AddRange(competitionBasedQuestions);
                        }
                        break;

                    case QuestionArchiveTypeEnum.Global:
                        var globalQuestions = await _compQuestRepository.GetAll(cq =>
                            cq.Competition.EndDate.HasValue &&
                            cq.Competition.Id != competition.Id)
                            .Include(cq => cq.Question)
                            .ToListAsync();

                        excludedQuestions.AddRange(globalQuestions);
                        break;

                    case QuestionArchiveTypeEnum.DateRange:
                        if (competition.ArchiveFromDate.HasValue && competition.ArchiveToDate.HasValue)
                        {
                            var dateRangeQuestions = await _compQuestRepository.GetAll(cq =>
                                cq.Competition.EndDate.HasValue &&
                                cq.Competition.EndDate.Value >= competition.ArchiveFromDate.Value &&
                                cq.Competition.EndDate.Value <= competition.ArchiveToDate.Value &&
                                cq.Competition.Id != competition.Id)
                                .Include(cq => cq.Question)
                                .ToListAsync();

                            excludedQuestions.AddRange(dateRangeQuestions);
                        }
                        break;
                }

                // Remove duplicates based on QuestionId
                return excludedQuestions.GroupBy(q => q.QuestionId).Select(g => g.First()).ToList();
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetAllExcludedQuestionsAsync", competition.Id, null, false);
                throw;
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
                       .ThenInclude(a => a.Answers)
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
                        .Include(a => a.Parent)
                            .FirstOrDefaultAsync();

                bool IsAr = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar";


                if (competition != null)
                {
                    CompetitionStartDTO competitionStartDTO = new CompetitionStartDTO();
                    competitionStartDTO.Id = id;

                    if (competition.ParentId.HasValue)
                    {
                        var parentCompetition = await _competitionRepository.GetAll(a => a.Id == competition.ParentId.Value).SingleOrDefaultAsync();
                        if (!parentCompetition.EndDate.HasValue)
                        {
                            return new Response<CompetitionStartDTO>()
                            {
                                StatusCode = (int)HttpStatusCode.NotAuthenticated
                            };
                        }
                    }


                    competitionStartDTO.IsFinalCompetition = competition.IsFinalCompetition ?? false;
                    competitionStartDTO.IsQuestionsTypeIsRound = competition.CompetitionQuestionType == (int)CompetitionQuestionType.Rounds;
                    if (!competitionStartDTO.IsQuestionsTypeIsRound)
                        competitionStartDTO.QuestionPerPlayer = competition.QuestionForEachPlayer ?? 0;

                    competitionStartDTO.TotalRound = competition.RoundCount;
                    competitionStartDTO.CurrentRound = 1;

                    competitionStartDTO.RoundTime = competition.Round1Time ?? 0;
                    competitionStartDTO.RoundPoints = competition.Round1Points ?? 0;

                    competitionStartDTO.TeamCityMall = new List<CompetitionsPlayerDTO>();
                    competitionStartDTO.OtherTeam = new List<CompetitionsPlayerDTO>();
                    competitionStartDTO.Team1Name = competition.Team1.TeamName;
                    competitionStartDTO.Team2Name = competition.Team2.TeamName;


                    List<CompetitionQuestion> AllQuestionsWasAskedBefore = await GetAllExcludedQuestionsAsync(competition);

                    bool IsCompetitionStartedBefore = competition.CompetitionQuestions != null && competition.CompetitionQuestions.Count > 0;

                    //Add Previous Competition Questions
                    foreach (var question in AllQuestionsWasAskedBefore)
                    {
                        QuestionVM questionVM = new QuestionVM();
                        questionVM.Id = question.Question.Id;
                        questionVM.CategoryId = question.Question.CategoryID;
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
                            cityMallPlayer1.Points = AllQuestionsWasAskedBefore.Where(a => a.PlayerId == cityMallPlayer1.Id).Sum(a => a.Point).Value;

                        //if (competition.Team1.Player1.HasProfilePicture == true)
                        //{
                        //    var attachment = await _attachmentRepository.GetAll(a =>
                        //        a.EntityId == cityMallPlayer1.Id &&
                        //        a.EntityType == (int)AttachmentTypes.PlayerProfilePicture &&
                        //        a.IsDeleted != true).SingleOrDefaultAsync();

                        //    cityMallPlayer1.ProfilePicture = attachment != null ? Convert.ToBase64String(attachment.FileData) : null;
                        //}

                        competitionStartDTO.TeamCityMall.Add(cityMallPlayer1);
                    }

                    if (competition.Team1.Player2 != null)
                    {
                        CompetitionsPlayerDTO cityMallPlayer2 = new CompetitionsPlayerDTO();
                        cityMallPlayer2.Id = competition.Team1.Player2.Id;
                        cityMallPlayer2.Name = competition.Team1.Player2.Name;
                        cityMallPlayer2.Points = 0;
                        if (IsCompetitionStartedBefore)
                            cityMallPlayer2.Points = AllQuestionsWasAskedBefore.Where(a => a.PlayerId == cityMallPlayer2.Id).Sum(a => a.Point).Value;

                        //if (competition.Team1.Player2.HasProfilePicture == true)
                        //{
                        //    var attachment = await _attachmentRepository.GetAll(a =>
                        //        a.EntityId == cityMallPlayer2.Id &&
                        //        a.EntityType == (int)AttachmentTypes.PlayerProfilePicture &&
                        //        a.IsDeleted != true).SingleOrDefaultAsync();

                        //    cityMallPlayer2.ProfilePicture = attachment != null ? Convert.ToBase64String(attachment.FileData) : null;
                        //}

                        competitionStartDTO.TeamCityMall.Add(cityMallPlayer2);
                    }

                    if (competition.Team1.Player3 != null)
                    {
                        CompetitionsPlayerDTO cityMallPlayer3 = new CompetitionsPlayerDTO();
                        cityMallPlayer3.Id = competition.Team1.Player3.Id;
                        cityMallPlayer3.Name = competition.Team1.Player3.Name;
                        cityMallPlayer3.Points = 0;
                        if (IsCompetitionStartedBefore)
                            cityMallPlayer3.Points = AllQuestionsWasAskedBefore.Where(a => a.PlayerId == cityMallPlayer3.Id).Sum(a => a.Point).Value;

                        //if (competition.Team1.Player3.HasProfilePicture == true)
                        //{
                        //    var attachment = await _attachmentRepository.GetAll(a =>
                        //        a.EntityId == cityMallPlayer3.Id &&
                        //        a.EntityType == (int)AttachmentTypes.PlayerProfilePicture &&
                        //        a.IsDeleted != true).SingleOrDefaultAsync();

                        //    cityMallPlayer3.ProfilePicture = attachment != null ? Convert.ToBase64String(attachment.FileData) : null;
                        //}

                        competitionStartDTO.TeamCityMall.Add(cityMallPlayer3);
                    }

                    if (competition.Team1.Player4 != null)
                    {
                        CompetitionsPlayerDTO cityMallPlayer4 = new CompetitionsPlayerDTO();
                        cityMallPlayer4.Id = competition.Team1.Player4.Id;
                        cityMallPlayer4.Name = competition.Team1.Player4.Name;
                        cityMallPlayer4.Points = 0;
                        if (IsCompetitionStartedBefore)
                            cityMallPlayer4.Points = AllQuestionsWasAskedBefore.Where(a => a.PlayerId == cityMallPlayer4.Id).Sum(a => a.Point).Value;

                        //if (competition.Team1.Player4.HasProfilePicture == true)
                        //{
                        //    var attachment = await _attachmentRepository.GetAll(a =>
                        //        a.EntityId == cityMallPlayer4.Id &&
                        //        a.EntityType == (int)AttachmentTypes.PlayerProfilePicture &&
                        //        a.IsDeleted != true).SingleOrDefaultAsync();

                        //    cityMallPlayer4.ProfilePicture = attachment != null ? Convert.ToBase64String(attachment.FileData) : null;
                        //}

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
                            otherPlayer1.Points = AllQuestionsWasAskedBefore.Where(a => a.PlayerId == otherPlayer1.Id).Sum(a => a.Point).Value;

                        //if (competition.Team2.Player1.HasProfilePicture == true)
                        //{
                        //    var attachment = await _attachmentRepository.GetAll(a =>
                        //        a.EntityId == otherPlayer1.Id &&
                        //        a.EntityType == (int)AttachmentTypes.PlayerProfilePicture &&
                        //        a.IsDeleted != true).SingleOrDefaultAsync();

                        //    otherPlayer1.ProfilePicture = attachment != null ? Convert.ToBase64String(attachment.FileData) : null;
                        //}

                        competitionStartDTO.OtherTeam.Add(otherPlayer1);
                    }

                    if (competition.Team2.Player2 != null)
                    {
                        CompetitionsPlayerDTO otherPlayer2 = new CompetitionsPlayerDTO();
                        otherPlayer2.Id = competition.Team2.Player2.Id;
                        otherPlayer2.Name = competition.Team2.Player2.Name;
                        otherPlayer2.Points = 0;
                        if (IsCompetitionStartedBefore)
                            otherPlayer2.Points = AllQuestionsWasAskedBefore.Where(a => a.PlayerId == otherPlayer2.Id).Sum(a => a.Point).Value;

                        //if (competition.Team2.Player2.HasProfilePicture == true)
                        //{
                        //    var attachment = await _attachmentRepository.GetAll(a =>
                        //        a.EntityId == otherPlayer2.Id &&
                        //        a.EntityType == (int)AttachmentTypes.PlayerProfilePicture &&
                        //        a.IsDeleted != true).SingleOrDefaultAsync();

                        //    otherPlayer2.ProfilePicture = attachment != null ? Convert.ToBase64String(attachment.FileData) : null;
                        //}

                        competitionStartDTO.OtherTeam.Add(otherPlayer2);

                    }

                    if (competition.Team2.Player3 != null)
                    {
                        CompetitionsPlayerDTO otherPlayer3 = new CompetitionsPlayerDTO();
                        otherPlayer3.Id = competition.Team2.Player3.Id;
                        otherPlayer3.Name = competition.Team2.Player3.Name;
                        otherPlayer3.Points = 0;
                        if (IsCompetitionStartedBefore)
                            otherPlayer3.Points = AllQuestionsWasAskedBefore.Where(a => a.PlayerId == otherPlayer3.Id).Sum(a => a.Point).Value;

                        //if (competition.Team2.Player3.HasProfilePicture == true)
                        //{
                        //    var attachment = await _attachmentRepository.GetAll(a =>
                        //        a.EntityId == otherPlayer3.Id &&
                        //        a.EntityType == (int)AttachmentTypes.PlayerProfilePicture &&
                        //        a.IsDeleted != true).SingleOrDefaultAsync();

                        //    otherPlayer3.ProfilePicture = attachment != null ? Convert.ToBase64String(attachment.FileData) : null;
                        //}

                        competitionStartDTO.OtherTeam.Add(otherPlayer3);
                    }

                    if (competition.Team2.Player4 != null)
                    {
                        CompetitionsPlayerDTO otherPlayer4 = new CompetitionsPlayerDTO();
                        otherPlayer4.Id = competition.Team2.Player4.Id;
                        otherPlayer4.Name = competition.Team2.Player4.Name;
                        otherPlayer4.Points = 0;
                        if (IsCompetitionStartedBefore)
                            otherPlayer4.Points = AllQuestionsWasAskedBefore.Where(a => a.PlayerId == otherPlayer4.Id).Sum(a => a.Point).Value;

                        //if (competition.Team2.Player4.HasProfilePicture == true)
                        //{
                        //    var attachment = await _attachmentRepository.GetAll(a =>
                        //        a.EntityId == otherPlayer4.Id &&
                        //        a.EntityType == (int)AttachmentTypes.PlayerProfilePicture &&
                        //        a.IsDeleted != true).SingleOrDefaultAsync();

                        //    otherPlayer4.ProfilePicture = attachment != null ? Convert.ToBase64String(attachment.FileData) : null;
                        //}

                        competitionStartDTO.OtherTeam.Add(otherPlayer4);
                    }


                    //Fill Categories
                    var allCategories = await _questionsService.GetCategories();
                    if (!string.IsNullOrWhiteSpace(competition.CategoriesIds))
                    {
                        var allowedCategories = competition.CategoriesIds.Split(',').Select(int.Parse).ToList();
                        competitionStartDTO.Categories = allCategories.Where(a => allowedCategories.Contains(a.Id)).ToList();
                    }
                    else
                        competitionStartDTO.Categories = allCategories;



                    if (competition.StateData != null)
                    {
                        _httpContextAccessor.HttpContext.Session.SetString("CompetitionStart", competition.StateData);
                        competitionStartDTO = JsonConvert.DeserializeObject<CompetitionStartDTO>(competition.StateData);
                        competitionStartDTO.IsSessionData = true;
                    }
                    else
                    {
                        var competitonString = JsonConvert.SerializeObject(competitionStartDTO);
                        _httpContextAccessor.HttpContext.Session.SetString("CompetitionStart", competitonString);
                    }


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
                await _logger.LogError(ex, "StartCompetiton", id, null, false);
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
                {
                    competitionQuestion.Time = answerOnQuestionDTO.Time;
                    competitionQuestion.Point = answerOnQuestionDTO.Points;
                }

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
                await _logger.LogError(ex, "AnswerOnQuestions", answerOnQuestionDTO, $"CompetitionId:{competitionId}", false);
                return new Response()
                {
                    Succeeded = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Get All questions for parent or old competitons
        /// </summary>
        /// <param name="competitionId"></param>
        /// <returns></returns>
        private async Task<List<CompetitionQuestion>> GetAllQuestionsForCompetitionAndParentsAsync(int competitionId)
        {
            List<CompetitionQuestion> allQuestions = new List<CompetitionQuestion>();
            try
            {
                async Task GetQuestions(int id)
                {
                    var competition = await _competitionRepository.GetAll(a => a.Id == id && a.IsDeleted != true)
                        .Include(a => a.CompetitionQuestions)
                           .ThenInclude(a => a.Question)
                        .Include(a => a.Parent)
                        .FirstOrDefaultAsync();

                    if (competition != null)
                    {
                        if (competition.CompetitionQuestions != null)
                        {
                            allQuestions.AddRange(competition.CompetitionQuestions);
                        }

                        if (competition.Parent != null)
                        {
                            await GetQuestions(competition.Parent.Id);
                        }
                    }
                }

                await GetQuestions(competitionId);

                return allQuestions;
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetAllQuestionsForCompetitionAndParentsAsync", competitionId, null, false);
                throw ex;
            }
        }

        /// <summary>
        /// Get Rounds time
        /// </summary>
        /// <param name="competionId"></param>
        /// <param name="round"></param>
        /// <returns></returns>
        public int GetRoundTime(int competionId, int round)
        {
            var competition = _competitionRepository.Find(competionId);
            int time = 0;
            switch (round)
            {
                case 1:
                    time = competition.Round1Time ?? 0;
                    break;
                case 2:
                    time = competition.Round2Time ?? 0;
                    break;
                case 3:
                    time = competition.Round3Time ?? 0;
                    break;
                case 4:
                    time = competition.Round4Time ?? 0;
                    break;
            }
            return time;
        }

        /// <summary>
        /// Get round points
        /// </summary>
        /// <param name="competionId"></param>
        /// <param name="round"></param>
        /// <returns></returns>
        public int GetRoundPoints(int competionId, int round)
        {
            var competition = _competitionRepository.Find(competionId);
            int points = 0;
            switch (round)
            {
                case 1:
                    points = competition.Round1Points ?? 0;
                    break;
                case 2:
                    points = competition.Round2Points ?? 0;
                    break;
                case 3:
                    points = competition.Round3Points ?? 0;
                    break;
                case 4:
                    points = competition.Round4Points ?? 0;
                    break;
            }
            return points;
        }

        /// <summary>
        /// Get Score details for player
        /// </summary>
        /// <param name="competitionId"></param>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public async Task<Response<CompetitionsPlayerDTO>> GetPlayerScoreDetails(int competitionId, int playerId)
        {
            try
            {
                CompetitionsPlayerDTO competitionsPlayerDTO = new CompetitionsPlayerDTO();
                bool IsAr = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar";

                var competitonQuestions = await _compQuestRepository.GetAll(a => a.PlayerId == playerId && a.CompetitionId == competitionId)
                    .Include(a=>a.Player)
                    .Include(a => a.Question)
                    .ThenInclude(a => a.Answers)
                    .Include(a => a.Answer)
                    .ToListAsync();

                if(competitonQuestions.Count==0)
                {
                    //Player didn't answer on questions
                    var player = await _playerRepository.FindAsync(playerId);
                    competitionsPlayerDTO.Name = player.Name;
                    competitionsPlayerDTO.competitonQuestions = new List<CompetitonQuestions>();
                    return new Response<CompetitionsPlayerDTO>()
                    {
                        Succeeded = true,
                        Data = competitionsPlayerDTO
                    };
                }

                competitionsPlayerDTO.Name = competitonQuestions.FirstOrDefault().Player.Name;
                competitionsPlayerDTO.Points = competitonQuestions.Sum(a => a.Point).Value;

                List<CompetitonQuestions> listQuestions = new List<CompetitonQuestions>();

                foreach (var question in competitonQuestions)
                {
                    CompetitonQuestions competitonQuestion = new CompetitonQuestions();
                    competitonQuestion.QuestionText = IsAr ? question.Question.TextAr : question.Question.TextEn;
                    if (question.Question.HasImg == true)
                    {
                        competitonQuestion.IsQuestionImg = true;
                        var attachmentQuestion = await _attachmentRepository.GetAll(a => a.EntityType == (int)AttachmentTypes.Questions && a.EntityId == question.Question.Id && a.IsDeleted != true).SingleOrDefaultAsync();
                        if (attachmentQuestion != null)
                            competitonQuestion.QuestionImg = Convert.ToBase64String(attachmentQuestion.FileData);
                    }
                    competitonQuestion.AnswerText = IsAr ? question.Answer.TextAr : question.Answer.TextEn;
                    competitonQuestion.IsCorrectAnswer = question.IsCorrectAnswer ?? false;
                    if (question.Answer.IsImg == true)
                    {
                        competitonQuestion.IsAnswerImg = true;
                        var attachmentAnswer = await _attachmentRepository.GetAll(a => a.EntityType == (int)AttachmentTypes.Answers && a.EntityId == question.Answer.Id && a.IsDeleted != true).SingleOrDefaultAsync();
                        if (attachmentAnswer != null)
                            competitonQuestion.AnswerImg = Convert.ToBase64String(attachmentAnswer.FileData);
                    }
                    competitonQuestion.Time = question.Time;
                    competitonQuestion.Points = question.Point;
                    listQuestions.Add(competitonQuestion);
                }

                competitionsPlayerDTO.competitonQuestions = listQuestions;
               
                return new Response<CompetitionsPlayerDTO>()
                {
                    Succeeded = true,
                    Data = competitionsPlayerDTO
                };

            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetPlayerScoreDetails", $"competitionId:{competitionId} - PlayerId:{playerId}", null, false);
                return new Response<CompetitionsPlayerDTO>()
                {
                    Message = ex.Message
                };
            }
        }

        public async Task<Response> UpdateCompeititonState(CompetitionStartDTO competitionStartDTO)
        {
            try
            {
                Competition competition = await _competitionRepository.GetFirstOrDefaultAsync(
                    selector: x => x,
                    predicate: x => x.Id == competitionStartDTO.Id,
                    disableTracking: true);
                
                if (competition == null)
                    return new Response() { Succeeded = false, Message = "competition not found", StatusCode = (int)HttpStatusCode.NotFound };

                competition.LastStateUpdate = DateTime.Now;
                competition.CurrentStep = competitionStartDTO.CurrentStep;
                competition.StateData = JsonConvert.SerializeObject(competitionStartDTO);

                _competitionRepository.Update(competition);
                await _competitionRepository.UnitOfWork.SaveChangesAsync();

                return new Response()
                {
                    Succeeded = true,
                    Message = "Success"
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "UpdateCompeititonState", $"competitionId:{competitionStartDTO.Id}", null, false);
                return new Response<CompetitionsPlayerDTO>()
                {
                    Message = ex.Message
                };
            }
        }

        public async Task<Response<string>> GetBackgroundAttachment()
        {
            try
            {
                //Check start background
                var attachmentBackground = await _attachmentRepository.GetAll(a => a.EntityId == 1 && a.EntityType == (int)AttachmentTypes.BackgroundImg).SingleOrDefaultAsync();
                if (attachmentBackground != null)
                {
                    string file = Convert.ToBase64String(attachmentBackground.FileData);
                    return new Response<string>()
                    {
                        Succeeded = true,
                        Data = file,
                        StatusCode = (int)HttpStatusCode.Ok
                    };
                }

                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "File not found"
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetBackgroundAttachment", null, null, false);
                return new Response<string>()
                {
                    Message = ex.Message
                };
            }
        }

        public async Task<Response<CompetitionStartDTO>> GetPlayersProfilePictures(CompetitionStartDTO competitionStartDTO)
        {
            try
            {
                List<int> team1 = competitionStartDTO.TeamCityMall.Select(a=>a.Id).ToList();
                List<int> team2 = competitionStartDTO.OtherTeam.Select(a => a.Id).ToList();

                var players1 = await _playerRepository.GetAll(a => team1.Contains(a.Id) && a.HasProfilePicture == true).Select(a => a.Id).ToListAsync();
                var players2 = await _playerRepository.GetAll(a => team2.Contains(a.Id) && a.HasProfilePicture == true).Select(a => a.Id).ToListAsync();

                foreach(var player in players1)
                {
                    var competitionPlayer = competitionStartDTO.TeamCityMall.FirstOrDefault(a => a.Id == player);
                    if (competitionPlayer != null)
                    {
                        var attachment = await _attachmentRepository.GetAll(a =>
                                                   a.EntityId == player &&
                                                                              a.EntityType == (int)AttachmentTypes.PlayerProfilePicture &&
                                                                                                         a.IsDeleted != true).SingleOrDefaultAsync();

                        competitionPlayer.ProfilePicture = attachment != null ? Convert.ToBase64String(attachment.FileData) : null;
                    }
                }

                foreach(var player in players2)
                {
                    var competitionPlayer = competitionStartDTO.OtherTeam.FirstOrDefault(a => a.Id == player);
                    if (competitionPlayer != null)
                    {
                        var attachment = await _attachmentRepository.GetAll(a =>
                                                   a.EntityId == player &&
                                                                              a.EntityType == (int)AttachmentTypes.PlayerProfilePicture &&
                                                                                                         a.IsDeleted != true).SingleOrDefaultAsync();

                        competitionPlayer.ProfilePicture = attachment != null ? Convert.ToBase64String(attachment.FileData) : null;
                    }
                }

                return new Response<CompetitionStartDTO>()
                {
                    Data = competitionStartDTO,
                    Succeeded = true
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetBackgroundAttachment", $"CompetitionId:{competitionStartDTO.Id}", null, false);
                return new Response<CompetitionStartDTO>()
                {
                    Message = ex.Message
                };
            }
        }
    }
}

using AutoMapper;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Helpers;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Presentation.Application.DTOs.Competitions;
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
            IValidatorFactory validatorFactory) : base(validatorFactory, unitOfWork)
        {
            _mapper = mapper;
            _logger = logger;
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
                if (competitionsDTO.StartDate.HasValue)
                    competition.StartDate = competitionsDTO.StartDate;
                //if (competitionsDTO.EndDate.HasValue)
                //    competition.EndDate = competitionsDTO.EndDate;
                if (competitionsDTO.HostID.HasValue)
                    competition.HostID = competitionsDTO.HostID;

                if (competition.Team1 == null)
                    competition.Team1 = new Team();
                if (competition.Team2 == null)
                    competition.Team2 = new Team();

                //Team1
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
                var competitions = await _competitionRepository.GetAll(a => a.HostID == hostId).Include(a => a.Host).ToListAsync();
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
                    HostName = !string.IsNullOrEmpty(x.Host.Name) ? Security.Decrypt(x.Host.Name) : null
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
                            Team1 = new TeamDTO()
                            {
                                Id = competition.Team1.Id,
                                Player1 = competition.Team1.Player1Id,
                                Player2 = competition.Team1.Player2Id,
                                Player3 = competition.Team1.Player3Id,
                                Player4 = competition.Team1.Player4Id,
                            },
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
                var competition = await _competitionRepository.GetAll(a => a.Id == id && a.IsDeleted != true)
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

                if (competition != null)
                {
                    CompetitionStartDTO competitionStartDTO = new CompetitionStartDTO();
                    competitionStartDTO.Id = id;
                    competitionStartDTO.TeamCityMall = new List<CompetitionsPlayerDTO>();
                    competitionStartDTO.OtherTeam = new List<CompetitionsPlayerDTO>();

                    //City Mall Team
                    if (competition.Team1.Player1 != null)
                    {
                        CompetitionsPlayerDTO cityMallPlayer1 = new CompetitionsPlayerDTO();
                        cityMallPlayer1.Id = competition.Team1.Player1.Id;
                        cityMallPlayer1.Name = competition.Team1.Player1.Name;
                        cityMallPlayer1.Points = 0;
                        competitionStartDTO.TeamCityMall.Add(cityMallPlayer1);
                    }

                    if (competition.Team1.Player2 != null)
                    {
                        CompetitionsPlayerDTO cityMallPlayer2 = new CompetitionsPlayerDTO();
                        cityMallPlayer2.Id = competition.Team1.Player2.Id;
                        cityMallPlayer2.Name = competition.Team1.Player2.Name;
                        cityMallPlayer2.Points = 0;
                        competitionStartDTO.TeamCityMall.Add(cityMallPlayer2);

                    }

                    if (competition.Team1.Player3 != null)
                    {
                        CompetitionsPlayerDTO cityMallPlayer3 = new CompetitionsPlayerDTO();
                        cityMallPlayer3.Id = competition.Team1.Player3.Id;
                        cityMallPlayer3.Name = competition.Team1.Player3.Name;
                        cityMallPlayer3.Points = 0;
                        competitionStartDTO.TeamCityMall.Add(cityMallPlayer3);
                    }

                    if (competition.Team1.Player4 != null)
                    {
                        CompetitionsPlayerDTO cityMallPlayer4 = new CompetitionsPlayerDTO();
                        cityMallPlayer4.Id = competition.Team1.Player4.Id;
                        cityMallPlayer4.Name = competition.Team1.Player4.Name;
                        cityMallPlayer4.Points = 0;
                        competitionStartDTO.TeamCityMall.Add(cityMallPlayer4);
                    }


                    // Other Team
                    if (competition.Team2.Player1 != null)
                    {
                        CompetitionsPlayerDTO otherPlayer1 = new CompetitionsPlayerDTO();
                        otherPlayer1.Id = competition.Team2.Player1.Id;
                        otherPlayer1.Name = competition.Team2.Player1.Name;
                        otherPlayer1.Points = 0;
                        competitionStartDTO.OtherTeam.Add(otherPlayer1);
                    }

                    if (competition.Team2.Player2 != null)
                    {
                        CompetitionsPlayerDTO otherPlayer2 = new CompetitionsPlayerDTO();
                        otherPlayer2.Id = competition.Team2.Player2.Id;
                        otherPlayer2.Name = competition.Team2.Player2.Name;
                        otherPlayer2.Points = 0;
                        competitionStartDTO.OtherTeam.Add(otherPlayer2);

                    }

                    if (competition.Team2.Player3 != null)
                    {
                        CompetitionsPlayerDTO otherPlayer3 = new CompetitionsPlayerDTO();
                        otherPlayer3.Id = competition.Team2.Player3.Id;
                        otherPlayer3.Name = competition.Team2.Player3.Name;
                        otherPlayer3.Points = 0;
                        competitionStartDTO.OtherTeam.Add(otherPlayer3);
                    }

                    if (competition.Team2.Player4 != null)
                    {
                        CompetitionsPlayerDTO otherPlayer4 = new CompetitionsPlayerDTO();
                        otherPlayer4.Id = competition.Team2.Player4.Id;
                        otherPlayer4.Name = competition.Team2.Player4.Name;
                        otherPlayer4.Points = 0;
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
    }
}

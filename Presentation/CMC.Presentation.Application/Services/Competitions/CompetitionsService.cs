using AutoMapper;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Presentation.Application.DTOs.Competitions;
using CMC.Presentation.Application.DTOs.Players;
using CMC.Presentation.Application.Services.Players;
using CMC.Presentation.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.Services.Competitions
{
    public class CompetitionsService : BaseServiceHandler, ICompetitionsService
    {
        readonly IMapper _mapper;
        readonly IApplicationLogger _logger;
        readonly IRepository<Competition> _competitionRepository;
        readonly IRepository<CompetitionQuestion> _compQuestRepository;
        readonly IStringLocalizer<PlayerService> _localizer;
        public static IHttpContextAccessor _httpContextAccessor { get { return new HttpContextAccessor(); } }

        public CompetitionsService(IMapper mapper,
            IApplicationLogger logger,
            IRepository<Competition> competitionRepository,
            IRepository<CompetitionQuestion> compQuestRepository,
            IUnitOfWork unitOfWork,
            IValidatorFactory validatorFactory) : base(validatorFactory, unitOfWork)
        {
            _mapper = mapper;
            _logger = logger;
            _competitionRepository = competitionRepository;
            _compQuestRepository = compQuestRepository;
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
                    competition = await _competitionRepository.GetAll(a => a.Id == competitionsDTO.Id.Value).SingleOrDefaultAsync();
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
                if (competitionsDTO.EndDate.HasValue)
                    competition.EndDate = competitionsDTO.EndDate;
                if (competitionsDTO.HostID.HasValue)
                    competition.HostID = competitionsDTO.HostID;
                
                //Save Or Update
                if (competitionsDTO.Id.HasValue)
                    _competitionRepository.Update(competition);
                else
                    await _competitionRepository.InsertAsync(competition);

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
                var competitions = await _competitionRepository.GetAll(a => a.HostID == hostId).Include(a=>a.Host).ToListAsync();
                if (competitions.Count > 0)
                {
                    competitions.ForEach(competition =>
                    {
                        CompetitionsDto.Add(new CompetitionsDTO()
                        {
                            Name = competition.Name,
                            HostName = competition.Host.Name,
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
    }
}

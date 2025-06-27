using AutoMapper;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Helpers;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Kernel.Domain.Entities;
using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Presentation.Application.DTOs.Players;
using CMC.Presentation.Application.DTOs.Questions;
using CMC.Presentation.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.Services.Players
{
    public class PlayerService : BaseServiceHandler, IPlayerService
    {
        readonly IMapper _mapper;
        readonly IApplicationLogger _logger;
        readonly IRepository<Player> _playerRepository;
        readonly IRepository<Attachment> _attachmentRepository;
        readonly IStringLocalizer<PlayerService> _localizer;
        public static IHttpContextAccessor _httpContextAccessor { get { return new HttpContextAccessor(); } }

        public PlayerService(IMapper mapper,
            IApplicationLogger logger,
            IRepository<Player> playerRepository,
            IStringLocalizer<PlayerService> localizer,
            IRepository<Attachment> attachmentRepository,
            IUnitOfWork unitOfWork,
            IValidatorFactory validatorFactory) : base(validatorFactory, unitOfWork)
        {
            _mapper = mapper;
            _logger = logger;
            _playerRepository = playerRepository;
            _attachmentRepository = attachmentRepository;
            _localizer = localizer;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<object>> Validate(object obj)
        {
            var valid = await ValidateAsync(obj);
            return valid.ConvertToResponseOf<object>(obj);
        }

        /// <summary>
        /// Add Or update Player
        /// </summary>
        /// <param name="playerDTO"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<Response> AddOrUpdatePlayer(PlayerDTO playerDTO)
        {
            try
            {
                // validate login model for required fields.
                var validModel = await Validate(playerDTO);
                if (!validModel.Succeeded)
                    return new Response<PlayerDTO>()
                    {
                        BrokenRules = validModel.BrokenRules,
                        StatusCode = (int)HttpStatusCode.BusinessRuleViolation
                    };


                Player player = new Player();
                if (playerDTO.Id.HasValue)
                {
                    // Update
                    player = await _playerRepository.GetAll(a => a.Id == playerDTO.Id.Value && a.IsDeleted != true).SingleOrDefaultAsync();
                    if (player != null)
                    {
                        player.ModifiedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                        player.ModifiedOn = DateTime.Now;
                    }
                    else
                        return new Response()
                        {
                            Succeeded = false,
                            StatusCode = (int)HttpStatusCode.NotFound
                        };
                }
                else
                {
                    player.CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    player.CreatedOn = DateTime.Now;
                }

                player.Name = playerDTO.Name;
                player.PhoneNumber = playerDTO.PhoneNumber;
                player.EmailAddress = playerDTO.EmailAddress;
                player.IsEmployee = playerDTO.IsEmployee;

                if (playerDTO.Id.HasValue)
                {
                    player.IsBlocked = playerDTO.IsBlocked;
                    player.Comment = playerDTO.Comment;
                    _playerRepository.Update(player);
                }
                else
                    await _playerRepository.InsertAsync(player);

                await _playerRepository.UnitOfWork.SaveChangesAsync();


                if (playerDTO.ProfilePicture != null)
                {
                    var currentAttachment = await _attachmentRepository.GetAll(a =>
                        a.EntityId == player.Id &&
                        a.EntityType == (int)AttachmentTypes.PlayerProfilePicture &&
                        a.IsDeleted != true).SingleOrDefaultAsync();

                    bool isUpdateAttachment = currentAttachment != null;

                    if (currentAttachment == null)
                        currentAttachment = new Attachment()
                        {
                            CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId")),
                            CreatedOn = DateTime.Now
                        };
                    else
                    {
                        currentAttachment.ModifiedOn = DateTime.Now;
                        currentAttachment.ModifiedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await playerDTO.ProfilePicture.CopyToAsync(memoryStream);
                        currentAttachment.FileName = playerDTO.ProfilePicture.FileName;
                        currentAttachment.FileData = memoryStream.ToArray();
                        currentAttachment.EntityId = player.Id;
                        currentAttachment.EntityType = (int)AttachmentTypes.PlayerProfilePicture;

                        if (isUpdateAttachment)
                            _attachmentRepository.Update(currentAttachment);
                        else
                            await _attachmentRepository.InsertAsync(currentAttachment);

                        // Update player to indicate it has a profile picture
                        player.HasProfilePicture = true;
                        _playerRepository.Update(player);
                        await _playerRepository.UnitOfWork.SaveChangesAsync();

                        await _attachmentRepository.UnitOfWork.SaveChangesAsync();
                    }
                }

                return new Response()
                {
                    Succeeded = true,
                    StatusCode = (int)HttpStatusCode.Ok
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "AddOrUpdatePlayer", playerDTO, null, false);
                return new Response()
                {
                    Message = ex.Message,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        /// <summary>
        /// Get Players
        /// </summary>
        /// <returns></returns>
        public async Task<PagedResult<PlayerDTO>> GetPlayers(SearchPlayersDTO searchPlayers)
        {
            try
            {

                PagedResult<PlayerDTO> response = new PagedResult<PlayerDTO>();
                var players = _playerRepository.GetAll(a => a.IsDeleted != true).AsQueryable();

                var result = players
                        .WhereIf(!string.IsNullOrEmpty(searchPlayers.Name), a => a.Name.Contains(searchPlayers.Name))
                        .WhereIf(!string.IsNullOrEmpty(searchPlayers.PhoneNumber), a => a.PhoneNumber.Contains(searchPlayers.PhoneNumber))
                        .WhereIf(searchPlayers.PlayerType == Enums.PlayerSearchTypes.CityMallEmployee, a => a.IsEmployee)
                        .WhereIf(searchPlayers.PlayerType == Enums.PlayerSearchTypes.NonEmployee, a => !a.IsEmployee)
                        .OrderByDescending(a => a.CreatedOn)
                        .ToQueryResultAsync(searchPlayers.PageNumber, searchPlayers.PageSize);

                response.PageSize = result.Result.PageSize;
                response.CurrentPage = result.Result.CurrentPage;
                response.TotalCount = result.Result.TotalCount;
                response.BrokenRules = result.Result.BrokenRules;
                response.Data = result.Result.Data.Select(x => new PlayerDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    PhoneNumber = x.PhoneNumber,
                    EmailAddress = x.EmailAddress,
                    IsEmployee = x.IsEmployee,
                    IsBlocked = x.IsBlocked ?? false
                });

                return response;
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetPlayers", searchPlayers, null, false);
                return new PagedResult<PlayerDTO>
                {
                    Message = ex.Message,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        /// <summary>
        /// Get Player by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Response<PlayerDTO>> GetPlayer(int id)
        {
            try
            {
                var player = await _playerRepository.FindAsync(id);
                if (player != null && player.IsDeleted != true)
                {
                    var playerDTO = new PlayerDTO()
                    {
                        Name = player.Name,
                        Id = player.Id,
                        EmailAddress = player.EmailAddress,
                        IsEmployee = player.IsEmployee,
                        PhoneNumber = player.PhoneNumber,
                        IsBlocked = player.IsBlocked ?? false,
                        Comment = player.Comment
                    };

                    // Get profile picture if it exists
                    if (player.HasProfilePicture == true)
                    {
                        var attachmentImg = await _attachmentRepository.GetAll(a =>
                            a.EntityId == player.Id &&
                            a.EntityType == (int)AttachmentTypes.PlayerProfilePicture &&
                            a.IsDeleted != true).SingleOrDefaultAsync();

                        if (attachmentImg != null)
                            playerDTO.ProfilePicturePath = Convert.ToBase64String(attachmentImg.FileData);
                    }

                    return new Response<PlayerDTO>()
                    {
                        Succeeded = true,
                        Data = playerDTO
                    };
                }
                else
                    return new Response<PlayerDTO>()
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetPlayer", id, null, false);
                return new Response<PlayerDTO>
                {
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                };
            }
        }


        /// <summary>
        /// Delete Player
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Response> DeletePlayer(int id)
        {
            try
            {
                var player = await _playerRepository.FindAsync(id);
                if (player != null)
                {
                    player.IsDeleted = true;
                    player.DeletedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    player.DeletedOn = DateTime.Now;
                    _playerRepository.Update(player);
                    await _playerRepository.UnitOfWork.SaveChangesAsync();

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
                await _logger.LogError(ex, "DeletePlayer", id, null, false);
                return new Response()
                {
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                };
            }
        }

        /// <summary>
        /// Delete Player Profile Picture
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Response> DeletePlayerProfilePicture(int id)
        {
            try
            {
                var player = await _playerRepository.FindAsync(id);
                if (player != null)
                {
                    // Find and mark attachment as deleted
                    var attachment = await _attachmentRepository.GetAll(a =>
                        a.EntityId == id &&
                        a.EntityType == (int)AttachmentTypes.PlayerProfilePicture &&
                        a.IsDeleted != true).SingleOrDefaultAsync();

                    if (attachment != null)
                    {
                        attachment.IsDeleted = true;
                        attachment.DeletedOn = DateTime.Now;
                        attachment.DeletedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                        _attachmentRepository.Update(attachment);

                        // Update player to indicate it no longer has a profile picture
                        player.HasProfilePicture = false;
                        player.ModifiedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                        player.ModifiedOn = DateTime.Now;
                        _playerRepository.Update(player);

                        await _attachmentRepository.UnitOfWork.SaveChangesAsync();
                        await _playerRepository.UnitOfWork.SaveChangesAsync();
                    }

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
                await _logger.LogError(ex, "DeletePlayerProfilePicture", id, null, false);
                return new Response()
                {
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                };
            }
        }

        /// <summary>
        /// Get All players based on if they are a City mall team or not.
        /// </summary>
        /// <param name="isCityMall"></param>
        /// <returns></returns>
        public async Task<Response<List<LookupModel>>> GetPlayers(bool isCityMall)
        {
            try
            {
                var players = await _playerRepository.GetAll(a => /*a.IsEmployee == isCityMall &&*/ a.IsDeleted != true).Select(player => new LookupModel()
                {
                    Id = player.Id,
                    Name = player.Name,
                    NameAr = player.Name,
                    NameEn = player.Name
                }).ToListAsync();
                
                return new Response<List<LookupModel>>()
                {
                    Succeeded = true,
                    Data = players,
                    StatusCode = (int)HttpStatusCode.Ok
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetPlayers", isCityMall, null, false);
                return new Response<List<LookupModel>>()
                {
                    Succeeded = false,
                    Message = ex.Message
                };
            }
        }
    }
}

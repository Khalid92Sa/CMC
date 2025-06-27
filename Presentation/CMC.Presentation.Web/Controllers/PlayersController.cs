using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Controllers;
using CMC.Kernel.Core.Infrastructure;
using CMC.Presentation.Application.ActionFilters;
using CMC.Presentation.Application.DTOs.Players;
using CMC.Presentation.Application.DTOs.Questions;
using CMC.Presentation.Application.Services.Players;
using CMC.Presentation.Application.Services.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Threading.Tasks;

namespace CMC.Presentation.Web.Controllers
{
    public class PlayersController : BaseController
    {
        readonly IPlayerService _playerService;
        readonly IApplicationLogger _logger;
        readonly IStringLocalizer<PlayersController> _localizer;
        readonly ISettingsService _settingService;

        public PlayersController(IPlayerService playerService,
            IApplicationLogger logger,
            IStringLocalizer<PlayersController> localizer,
            ISettingsService settingsService)
        {
            _playerService = playerService;
            _logger = logger;
            _localizer = localizer;
            _settingService = settingsService;
        }

        [RolePermission(PermissionCodes.WebPlayerView)]

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [RolePermission(PermissionCodes.WebPlayerView)]
        public async Task<IActionResult> GetAllPlayers([FromQuery] SearchPlayersDTO searchPlayerDto)
        {
            try
            {
                var result = await _playerService.GetPlayers(searchPlayerDto);
                return Json(result);
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetAllPlayers", searchPlayerDto, null, false);
                return Json(new { });
            }
        }


        /// <summary>
        /// Create new player
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [RolePermission(PermissionCodes.WebPlayerCreate)]
        public async Task<IActionResult> CreatePlayer(int? id)
        {
            try
            {
                PlayerDTO playerDTO = new PlayerDTO();
                if(id.HasValue)
                {
                    var playerDb = await _playerService.GetPlayer(id.Value);
                    playerDTO = playerDb.Data;
                }
                return View("Create",playerDTO);
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "CreatePlayer", id, null, false);
                return RedirectToAction("Index", "Error");
            }
        }


        [HttpPost]
        [RolePermission(PermissionCodes.WebPlayerCreate)]
        public async Task<IActionResult> CreatePlayer(PlayerDTO playerDTO)
        {
            try
            {
                var result = await _playerService.AddOrUpdatePlayer(playerDTO);
                string msg = playerDTO.Id.HasValue ? _localizer["PlayerUpdatedSuccessfully"].Value : _localizer["PlayerAddedSuccessfully"].Value;
                return Json(new { isSuccess = result.Succeeded, resultCode = result.StatusCode, brokenRoles = result.BrokenRules,msg = msg });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "CreatePlayer", playerDTO, null, false);
                return Json(new { isSuccess = false });
            }
        }


        /// <summary>
        /// Delete Player
        /// </summary>
        /// <param name="id"></param>
        /// <param name="withQuestions"></param>
        /// <returns></returns>
        [HttpDelete]
        [RolePermission(PermissionCodes.WebPlayerDelete)]
        public async Task<IActionResult> DeletePlayer(int id)
        {
            try
            {
                var result = await _playerService.DeletePlayer(id);
                return Json(new { isSuccess = result.Succeeded, msg = _localizer["Alert_PlayerDeletedSuccessfully"].Value });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "DeletePlayer", id, null, false);
                return Json(new { isSuccess = false });
            }
        }


        /// <summary>
        /// Delete Player Profile Picture
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [RolePermission(PermissionCodes.WebPlayerDelete)]
        public async Task<IActionResult> DeletePlayerProfilePicture(int id)
        {
            try
            {
                var result = await _playerService.DeletePlayerProfilePicture(id);
                return Json(new { isSuccess = result.Succeeded, msg = _localizer["Alert_ProfilePictureDeletedSuccessfully"].Value });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "DeletePlayerProfilePicture", id, null, false);
                return Json(new { isSuccess = false });
            }
        }
    }
}

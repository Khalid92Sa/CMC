using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Controllers;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Infrastructure;
using CMC.Presentation.Application.ActionFilters;
using CMC.Presentation.Application.DTOs.Competitions;
using CMC.Presentation.Application.Services.Competitions;
using CMC.Presentation.Application.Services.Identity.Interfaces;
using CMC.Presentation.Application.Services.Players;
using CMC.Presentation.Application.Services.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Linq;
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
        readonly IUserService _userService;
        readonly ICompositeViewEngine _viewEngine;
        public static IHttpContextAccessor _httpContextAccessor { get { return new HttpContextAccessor(); } }

        public CompetitionsController(IPlayerService playerService,
            ICompetitionsService competitionsService,
            IUserService userService,
            IApplicationLogger logger,
            IStringLocalizer<PlayersController> localizer,
            ISettingsService settingsService,
            ICompositeViewEngine viewEngine)
        {
            _competitionsService = competitionsService;
            _playerService = playerService;
            _userService = userService;
            _logger = logger;
            _localizer = localizer;
            _settingService = settingsService;
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
                var competitionStart = await _competitionsService.StartCompetiton(id);
                if (competitionStart.Succeeded)
                {
                    PartialViewResult otpPartialView = PartialView("PartialViews/_AllPlayers", competitionStart.Data);
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


        [HttpPost]
        [RolePermission(PermissionCodes.WebCompetitionStart)]
        public IActionResult PlayerVsPlayer(PlayerVsPlayerDTO playerVsPlayerDTO)
        {
            try
            {
                var currentCompetition = JsonConvert.DeserializeObject<CompetitionStartDTO>(_httpContextAccessor.HttpContext.Session.GetString("CompetitionStart"));
                var cityMallPlayer = currentCompetition.TeamCityMall.Where(a => a.Id == playerVsPlayerDTO.CityMallPlayerId).SingleOrDefault();
                cityMallPlayer.IsVSPlayer = true;
                var otherPlayer = currentCompetition.OtherTeam.Where(a=>a.Id == playerVsPlayerDTO.OtherPlayerId).SingleOrDefault();
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


        [HttpPost]
        [RolePermission(PermissionCodes.WebCompetitionStart)]
        public async Task<IActionResult> GetCategories(GetCategoryByPlayerDTO getCategoryByPlayer)
        {
            try
            {

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

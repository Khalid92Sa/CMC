using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Controllers;
using CMC.Kernel.Core.Infrastructure;
using CMC.Presentation.Application.ActionFilters;
using CMC.Presentation.Application.DTOs;
using CMC.Presentation.Application.Services.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CMC.Presentation.Web.Controllers
{
    public class SettingsController : BaseController
    {
        private readonly ISettingsService _settingsService;
        private readonly IApplicationLogger _logger;
        private readonly IStringLocalizer<SettingsController> _localizer;

        public SettingsController(ISettingsService settingsService, IStringLocalizer<SettingsController> localizer, IApplicationLogger logger)
        {
            _settingsService = settingsService;
            _localizer = localizer;
            _logger = logger;
        }

        [RolePermission(PermissionCodes.SystemSettings)]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Load current settings
                var model = new SettingDTO
                {
                    SystemFontSize = await _settingsService.GetValue<string>(SystemSettings.SystemFontSize),
                    CompetitionFontSize = await _settingsService.GetValue<string>(SystemSettings.CompetitionFontSize)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Controller-Settings-Index", null, null, false);
                return View(new SettingDTO());
            }
        }

        [RolePermission(PermissionCodes.SystemSettings)]
        [HttpPost]
        public async Task<IActionResult> UpdateSetting(SettingDTO settingDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var brokenRoles = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new
                        {
                            propertyName = x.Key,
                            message = x.Value.Errors.First().ErrorMessage
                        })
                        .ToList();

                    return Json(new { resultCode = 422, brokenRoles });
                }

                var result = await _settingsService.UpdateSystemSettings(settingDTO);
                string message = result.Succeeded ? _localizer["SystemSettingsUpdatedSuccessfully"].Value : _localizer["ErrorOccurred"].Value;

                return Json(new
                {
                    isSuccess = result.Succeeded,
                    resultCode = result.Succeeded ? 200 : 400,
                    msg = message
                });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Controller-UpdateSetting", settingDTO, null, false);
                return Json(new
                {
                    isSuccess = false,
                    resultCode = 500,
                    msg = _localizer["ErrorOccurred"].Value
                });
            }
        }

        //[RolePermission(PermissionCodes.SystemSettings)]
        //[HttpGet]
        //public async Task<IActionResult> GetFontSizes()
        //{
        //    try
        //    {
        //        var systemFontSize = await _settingsService.GetValue<string>("SystemFontSize");
        //        var competitionFontSize = await _settingsService.GetValue<string>("CompetitionFontSize");

        //        return Json(new
        //        {
        //            isSuccess = true,
        //            systemFontSize = systemFontSize ?? "13px",
        //            competitionFontSize = competitionFontSize ?? "20px"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logger.LogError(ex, "Controller-GetFontSizes", null, null, false);
        //        return Json(new { isSuccess = false });
        //    }
        //}
    }
}
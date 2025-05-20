using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Controllers;
using CMC.Kernel.Core.Infrastructure;
using CMC.Presentation.Application.ActionFilters;
using CMC.Presentation.Application.DTOs;
using CMC.Presentation.Application.DTOs.Competitions;
using CMC.Presentation.Application.Services.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
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
        public IActionResult Index()
        {
            return View();
        }

        [RolePermission(PermissionCodes.SystemSettings)]
        [HttpPost]
        public async Task<IActionResult> UpdateSetting(SettingDTO settingDTO)
        {
            try
            {
                var result = await _settingsService.UpdateSystemSettings(settingDTO);
                string message = result.Succeeded ? _localizer["SystemSettingsUpdatedSuccessfully"].Value : _localizer["ErrorOccurred"].Value;
                return Json(new { isSuccess = result.Succeeded, msg = message });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Controller-UpdateSetting", settingDTO, null, false);
                return Json(new { isSuccess = false });
            }
        }
    }
}

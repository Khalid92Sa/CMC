using CMC.Kernel.Core.Wrappers;
using CMC.Presentation.Application.DTOs.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using CMC.Kernel.Core.Controllers;
using CMC.Presentation.Application.Services.Identity.Interfaces;
using CMC.Kernel.Core.Enums;
using System.Threading.Tasks.Sources;
using CMC.Presentation.Application.DTOs.Players;
using CMC.Kernel.Core.Infrastructure;
using CMC.Presentation.Application.Services.Settings;
using Microsoft.Extensions.Localization;
using CMC.Kernel.Infrastructure.Persistence.Services;
using CMC.Presentation.Application.ActionFilters;
using CMC.Kernel.Core.Constants;

namespace CMC.Presentation.Web.Controllers
{
    public class UsersController : BaseController
    {
        #region Fields
        readonly IUserService _userService;
        readonly IApplicationLogger _logger;
        readonly IGroupPermissionService _groupPermissionService;
        readonly IStringLocalizer<UsersController> _localizer;
        readonly ISettingsService _settingService;
        #endregion

        #region Ctor
        public UsersController(IUserService userService,
             IApplicationLogger logger,
            IStringLocalizer<UsersController> localizer,
            IGroupPermissionService groupPermissionService,
            ISettingsService settingsService)
        {
            _userService = userService;
            _logger = logger;
            _localizer = localizer;
            _settingService = settingsService;
            _groupPermissionService = groupPermissionService;
        }
        #endregion

        #region Methods

        [RolePermission(PermissionCodes.WebUsersView)]
        public async Task<IActionResult> Index()
        {
            ViewBag.Groups = await _groupPermissionService.GetGroups();
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] SearchUserDTO searchUserDTO)
        {
            var result = await _userService.GetUsers(searchUserDTO);
            return Json(result);
        }

        public IActionResult Login()
        {
            return View();
        }


        /// <summary>
        /// Login 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO login)
        {
            try
            {
                var result = await _userService.Login(login);
                return Json(new { resultCode = result.StatusCode, brokenRoles = result.BrokenRules });
            }
            catch (Exception ex)
            {
                return Json(new { resultCode = (int)HttpStatusCode.BadRequest });
            }
        }


        /// <summary>
        /// Logout
        /// </summary>
        /// <returns></returns>
        public IActionResult Logout()
        {
            var keysSession = _httpContextAccessor.HttpContext.Session.Keys;
            foreach(var key in keysSession)
            {
                _httpContextAccessor.HttpContext.Session.Remove(key);
            }
            return View("Login");
        }



        /// <summary>
        /// Add new user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [RolePermission(PermissionCodes.WebUsersAdd)]
        public async Task<IActionResult> AddUser(int? id)
        {
            try
            {
                UserDTO userDTO = new UserDTO();
                if (id.HasValue)
                {
                    var userDb = await _userService.GetUser(id.Value);
                    if (userDb.Succeeded)
                        userDTO = userDb.Data;
                    else
                        throw new Exception("User not found");
                }
                userDTO.Groups = await _groupPermissionService.GetGroups();
                return View(userDTO);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Add Or Update user
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [RolePermission(PermissionCodes.WebUsersAdd)]
        public async Task<IActionResult> AddUser(UserDTO userDTO)
        {
            try
            {
                var result = await _userService.CreateOrUpdateUser(userDTO);
                string msg = userDTO.Id.HasValue ? _localizer["UserHasBeenUpdatedSuccessfully"].Value : _localizer["UserHasBeenSavedSuccessfully"].Value;
                return Json(new { isSuccess = result.Succeeded, resultCode = result.StatusCode, brokenRoles = result.BrokenRules, msg = msg });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false });
            }
        }


       /// <summary>
       /// Delete User
       /// </summary>
       /// <param name="id"></param>
       /// <returns></returns>
        [HttpDelete]
        [RolePermission(PermissionCodes.WebUsersDelete)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userService.DeleteUser(id);
                return Json(new { isSuccess = result.Succeeded, msg = _localizer["Alert_UserDeletedSuccessfully"].Value });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false });
            }
        }

        #endregion
    }
}

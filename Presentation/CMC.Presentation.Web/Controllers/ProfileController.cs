using CMC.Kernel.Core.Wrappers;
using CMC.Presentation.Application.DTOs.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using CMC.Kernel.Core.Controllers;
using CMC.Presentation.Application.Services.Identity.Interfaces;
using CMC.Kernel.Core.Enums;

namespace CMC.Presentation.Web.Controllers
{
    public class ProfileController : BaseController
    {
        #region Fields
        readonly IUserService _userService;
        #endregion

        #region Ctor
        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }
        #endregion

        #region Methods
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
        


        public IActionResult Logout()
        {
            _httpContextAccessor.HttpContext.Session.Remove("UserId");
            _httpContextAccessor.HttpContext.Session.Remove("UserFullName");

            return View("Login");
        }
        #endregion
    }
}

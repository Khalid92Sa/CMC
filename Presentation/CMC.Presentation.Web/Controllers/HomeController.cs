using CMC.Kernel.Core.Configurations;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Controllers;
using CMC.Presentation.Application.ActionFilters;
using CMC.Presentation.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CMC.Presentation.Web.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {

            return View();
        }


        [HttpPost]
        public IActionResult SetCulture(string culture, string returnUrl)
        {
            try
            {
                Response.Cookies.Append(
                       CookieNames.SelectedLanguage,
                       culture,
                       new CookieOptions { Expires = DateTimeOffset.Now.AddYears(1) }
                   );

                return Json(new { isSuccess = true, url = returnUrl });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, msg = ex.Message, interalMsg = ex.InnerException != null ? ex.InnerException.Message : "" });
            }
        }
    }
}

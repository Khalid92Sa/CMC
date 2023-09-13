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
        public HomeController()
        {
        }

        public IActionResult Index()
        {

            return View();
        }


        [HttpPost]
        public IActionResult SetCulture(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                        CookieNames.SelectedLanguage,
                        culture,
                        new CookieOptions { Expires = DateTimeOffset.Now.AddYears(1) }
                    );
            return Json(new { url = returnUrl });
        }
    }
}

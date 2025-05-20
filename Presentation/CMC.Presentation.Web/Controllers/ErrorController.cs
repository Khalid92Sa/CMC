using CMC.Kernel.Core.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;

namespace CMC.Presentation.Web.Controllers
{
    public class ErrorController : BaseController
    {
        readonly IStringLocalizer<ErrorController> _localizer;
        public ErrorController(IStringLocalizer<ErrorController> localizer)
        {
            _localizer = localizer;
        }
        public IActionResult Index(string? message,int? statusCode)
        {
            if(statusCode == 404)
            {
                ViewBag.NotFound = true;
            }
            else
            {
                if (!string.IsNullOrEmpty(message))
                    ViewBag.Message = _localizer[message].Value;
                ViewBag.GenericError = true;
            }

            return View("Error");
        }
    }
}

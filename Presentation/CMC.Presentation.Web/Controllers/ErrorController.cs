using Microsoft.AspNetCore.Mvc;

namespace CMC.Presentation.Web.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index(int? statusCode)
        {
            if(statusCode == 404)
            {
                ViewBag.NotFound = true;
            }
            else
            {
                ViewBag.GenericError = true;
            }

            return View("Error");
        }
    }
}

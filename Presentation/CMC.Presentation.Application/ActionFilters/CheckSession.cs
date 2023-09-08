using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace CMC.Presentation.Application.ActionFilters
{
    public class CheckSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            httpContext.Session.SetString("UserId", "1");
            httpContext.Session.SetString("UserFullName", "Admin");
            // Check if session contains a value for the sessionId
            if (string.IsNullOrEmpty(httpContext.Session.GetString("UserId")))
            {
                // Redirect to the Login action of the Profile controller
                context.Result = new RedirectToActionResult("Login", "Profile", null);
            }

            base.OnActionExecuting(context);
        }
    }
}

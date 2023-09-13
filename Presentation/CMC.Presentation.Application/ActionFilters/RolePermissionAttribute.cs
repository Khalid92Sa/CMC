using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using CMC.Presentation.Application.Services.Identity.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.ActionFilters
{
    public class RolePermissionAttribute : RolePermissionCommonBaseAttribute
    {
        public RolePermissionAttribute(params string[] permission)
                 : base(Array.ConvertAll(permission, value => value))
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var serviceProvider = context.HttpContext.RequestServices;
            var userService = serviceProvider.GetService<IUserService>();
            var httpContext = context.HttpContext;
            if (!string.IsNullOrEmpty(httpContext.Session.GetString("UserId")))
            {
                //Update User
                int userId = int.Parse(httpContext.Session.GetString("UserId"));
                var result = userService.CheckCurrentUserPermissions(userId, _requiredPermission).GetAwaiter().GetResult();
                if (!result)
                {
                    //UnAuthorize
                    context.Result = new RedirectToActionResult("Login", "Users", null);
                }
            }
            else
            {
                context.Result = new RedirectToActionResult("Login", "Users", null);
            }

            base.OnActionExecuting(context);
        }
    }
}

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.ActionFilters
{
    public abstract class RolePermissionCommonBaseAttribute : ActionFilterAttribute
    {
        protected string[] _requiredPermission;

        public RolePermissionCommonBaseAttribute(params string[] permission)
        {
            _requiredPermission = permission;
        }
    }
}

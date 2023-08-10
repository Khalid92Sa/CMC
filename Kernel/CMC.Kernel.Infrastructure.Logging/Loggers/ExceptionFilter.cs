using Microsoft.AspNetCore.Mvc.Filters;
using CMC.Kernel.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Logging.Loggers
{
    public class ExceptionFilter : ExceptionFilterAttribute, IAsyncExceptionFilter
    {
        private readonly IApplicationLogger _logger;
        public ExceptionFilter(IApplicationLogger logger)
        {
            _logger = logger;
        }

        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            var s = new StackTrace(context.Exception);
            var r = s.GetFrame(0);
            var methodName = GetMethodName(r.GetMethod());
            var className = GetClassName(r.GetMethod());
            await _logger.LogError(context.Exception, methodName);
        }


        private string GetMethodName(System.Reflection.MethodBase method)
        {
            string _methodName = method.DeclaringType.FullName;

            if (_methodName.Contains(">") || _methodName.Contains("<"))
            {
                _methodName = _methodName.Split('<', '>')[1];
            }
            else
            {
                _methodName = method.Name;
            }

            return _methodName;
        }

        private string GetClassName(System.Reflection.MethodBase method)
        {
            string className = method.DeclaringType.FullName;

            if (className.Contains(">") || className.Contains("<"))
            {
                className = className.Split('+')[0];
            }
            return className;
        }
    }

}

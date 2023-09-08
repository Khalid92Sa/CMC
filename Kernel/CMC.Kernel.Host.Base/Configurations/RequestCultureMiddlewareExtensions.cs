using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using CMC.Kernel.Core.Constants;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace CMC.Kernel.Host.Base.Configurations
{
    public static class RequestCultureMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestCultureMiddleWare(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestCultureMiddleware>();
        }
    }

    public class RequestCultureMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestCultureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var selectedLanguage = context.Request.Cookies[CookieNames.SelectedLanguage];
                if (string.IsNullOrWhiteSpace(selectedLanguage))
                {
                    selectedLanguage = SupportedCultures.Arabic;
                }

                var culture = new CultureInfo(selectedLanguage);
                culture.NumberFormat.NumberDecimalSeparator = ".";
                culture.NumberFormat.CurrencyDecimalSeparator = ".";
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;


                ////To add Headers AFTER everything you need to do this
                //context.Response.OnStarting(state =>
                //{
                //    var httpContext = (HttpContext)state;
                //    CookieOptions option = new CookieOptions();
                //    option.Expires = DateTime.Now.AddYears(1);
                //    httpContext.Response.Cookies.Append(CookieNames.SelectedLanguage, selectedLanguage, option);

                //    return Task.CompletedTask;
                //}, context);

                await _next(context);
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}

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
            string selectedLanguage = "";

            string path = context.Request.Path;
            if (!string.IsNullOrEmpty(path))
            {
                if (path != "/" && path.Length == 3 && !path.EndsWith('/'))
                    path = $"{path}/";

                if (path == "/")
                    selectedLanguage = GetDefaultCulture(context);
                else if (path.Length >= 3 && path[0] == '/' && path[3] == '/')
                {
                    selectedLanguage = context.Request.Path.Value.Substring(1, 2);
                    if (selectedLanguage != Languages.English && selectedLanguage != Languages.Arabic)
                        selectedLanguage = GetDefaultCulture(context);

                    if (selectedLanguage == Languages.English)
                        selectedLanguage = SupportedCultures.EnglishUs;
                    else
                        selectedLanguage = SupportedCultures.Arabic;

                    context.Response.Cookies.Append(
                        CookieNames.SelectedLanguage,
                        selectedLanguage,
                        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                        );
                }
                else
                    selectedLanguage = GetDefaultCulture(context);
            }
            else
                selectedLanguage = GetDefaultCulture(context);


            if (string.IsNullOrWhiteSpace(selectedLanguage))
                selectedLanguage = SupportedCultures.Arabic;

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

        private string GetDefaultCulture(HttpContext context)
        {
            string defaultLanguage = context.Request.Cookies[CookieNames.SelectedLanguage];
            if (string.IsNullOrWhiteSpace(defaultLanguage))
                defaultLanguage = SupportedCultures.Arabic;
            return defaultLanguage;
        }
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using CMC.Kernel.Core.Constants;
using System.Globalization;
using System.Threading.Tasks;

namespace CMC.Kernel.Host.Base.Configurations
{
    /// <summary>
    /// API Request Culture Extensions
    /// </summary>
    public static class APIRequestCultureExtensions
    {
        public static IApplicationBuilder UseAPIRequestCultureMiddleWare(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<APIRequestCultureMiddleware>();
        }
    }
    /// <summary>
    /// API Request Culture Middleware
    /// </summary>
    public class APIRequestCultureMiddleware
    {
        private readonly RequestDelegate _next;

        public APIRequestCultureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var language = context.Request.Cookies[RequestHeaders.Language];
            if (string.IsNullOrWhiteSpace(language))
            {
                language = SupportedCultures.Arabic;
            }

            var culture = new CultureInfo(language);
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
    }
}

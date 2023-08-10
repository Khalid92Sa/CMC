using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using CMC.Kernel.Core.Constants;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CMC.Kernel.Host.Base.ServiceRegistration
{
    public class RouteValueRequestCultureProvider : RequestCultureProvider
    {
        /// <summary>
        /// To Determine Provider Culture
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            string cultureCode = null;

            if (httpContext.Request.Path.HasValue && httpContext.Request.Path.Value == "/")
                cultureCode = this.GetDefaultCultureCode();

            else if (httpContext.Request.Path.HasValue && httpContext.Request.Path.Value.Length >= 3 && httpContext.Request.Path.Value[0] == '/' && httpContext.Request.Path.Value[3] == '/')
            {
                cultureCode = httpContext.Request.Path.Value.Substring(1, 2);
                if (!this.CheckCultureCode(cultureCode))
                    cultureCode = this.GetDefaultCultureCode();

                httpContext.Response.Cookies.Append(
                    CookieNames.SelectedLanguage,
                    cultureCode,
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                    );
            }
            else cultureCode = this.GetDefaultCultureCode();
            ProviderCultureResult requestCulture = new ProviderCultureResult(cultureCode);
            return Task.FromResult(requestCulture);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetDefaultCultureCode()
        {
            return this.Options.DefaultRequestCulture.Culture.TwoLetterISOLanguageName;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        private bool CheckCultureCode(string cultureCode)
        {
            return this.Options.SupportedCultures.Select(c => c.TwoLetterISOLanguageName).Contains(cultureCode);
        }
    }
}

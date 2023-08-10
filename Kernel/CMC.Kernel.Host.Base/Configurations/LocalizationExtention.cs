using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CMC.Kernel.Core.Constants;
using System.Collections.Generic;
using System.Globalization;

namespace CMC.Kernel.Host.Base.Configurations
{
    public static class LocalizationExtention
    {
        public static void UseLocalizationForPresnetationExtention(this IApplicationBuilder app)
        {
            PrepareLocalizationSettings(app);
            app.UseRequestCultureMiddleWare();
        }

        public static void UseLocalizationForAPIExtention(this IApplicationBuilder app)
        {
            PrepareLocalizationSettings(app);
            app.UseAPIRequestCultureMiddleWare();
        }

        private static void PrepareLocalizationSettings(IApplicationBuilder app)
        {
            var englishUs = new CultureInfo(SupportedCultures.EnglishUs);
            englishUs.NumberFormat.NumberDecimalSeparator = ".";
            englishUs.NumberFormat.CurrencyDecimalSeparator = ".";
            var arabic = new CultureInfo(SupportedCultures.Arabic);
            arabic.NumberFormat.NumberDecimalSeparator = ".";
            arabic.NumberFormat.CurrencyDecimalSeparator = ".";

            IList<CultureInfo> supportedCultures = new List<CultureInfo>
            {
                englishUs,
                arabic,
            };
            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(culture: SupportedCultures.Arabic, uiCulture: SupportedCultures.Arabic),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };
            app.UseRequestLocalization(localizationOptions);

            var requestProvider = new RouteDataRequestCultureProvider();
            localizationOptions.RequestCultureProviders.Insert(0, requestProvider);
            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            locOptions.Value.SupportedCultures = locOptions.Value.SupportedUICultures = supportedCultures;
            locOptions.Value.DefaultRequestCulture = new RequestCulture(culture: SupportedCultures.Arabic, uiCulture: SupportedCultures.Arabic);
            app.UseRequestLocalization(locOptions.Value);

        }
    }
}

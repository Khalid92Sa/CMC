using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CMC.Kernel.Core.Configurations;
using CMC.Kernel.Host.Base;
using CMC.Kernel.Host.Base.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMC.Presentation.Web
{
    public class Startup : HostConfigurator<Configuration>
    {
        public Startup(IWebHostEnvironment env) : base(env) { }
        public static IHttpContextAccessor _httpContext { get { return new HttpContextAccessor(); } }
        protected override void ConfigureAdditionalServices(IServiceCollection services, Configuration config)
        {
            base.ConfigureAdditionalServices(services, config);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHttpContextAccessor();
        }

        protected override void ConfigureAdditional(IApplicationBuilder app, IWebHostEnvironment env)
        {
            base.ConfigureAdditional(app, env);
            app.UseSession();
            app.UseLocalizationForPresnetationExtention();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{culture=ar}/{controller=Home}/{action=Index}/{id?}",
                    defaults: new { culture = "ar", controller = "Home", action = "Index" });
            });
        }
    }
}

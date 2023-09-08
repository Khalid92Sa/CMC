using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using CMC.Kernel.Core.Configurations;
using CMC.Kernel.Host.Base;
using CMC.Kernel.Host.Base.Configurations;
using AutoMapper;
using CMC.Kernel.Domain.Entities.Identity;
using CMC.Presentation.Application.DTOs.Identity;
using CMC.Kernel.Core.Helpers;
using Microsoft.Diagnostics.Tracing;
using System;

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
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddSession(option =>
            {
                option.IdleTimeout = TimeSpan.FromMinutes(40);
                option.Cookie.HttpOnly = true;
                option.Cookie.IsEssential = true;
            });
        }

        protected override void ConfigureAdditional(IApplicationBuilder app, IWebHostEnvironment env)
        {
            base.ConfigureAdditional(app, env);
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseLocalizationForPresnetationExtention();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }


        protected override void CreateAutoMapperMaps(IMapperConfigurationExpression config)
        {
            //Mapping User
            config.CreateMap<UserDTO, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.UserName) ? Security.Encrypt(src.UserName) : src.UserName))
                .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.EmailAddress) ? Security.Encrypt(src.EmailAddress) : src.EmailAddress))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.PhoneNumber) ? Security.Encrypt(src.PhoneNumber) : src.PhoneNumber))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.Now))
                .ReverseMap()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.UserName) ? Security.Decrypt(src.UserName) : src.UserName))
                .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.EmailAddress) ? Security.Decrypt(src.EmailAddress) : src.EmailAddress))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.PhoneNumber) ? Security.Decrypt(src.PhoneNumber) : src.PhoneNumber));
        }
    }
}

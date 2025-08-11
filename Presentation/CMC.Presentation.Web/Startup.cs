using AutoMapper;
using CMC.Kernel.Core.Configurations;
using CMC.Kernel.Core.Helpers;
using CMC.Kernel.Domain.Entities.Identity;
using CMC.Kernel.Host.Base;
using CMC.Kernel.Host.Base.Configurations;
using CMC.Presentation.Application.DTOs.Identity;
using CMC.Presentation.Application.Services.Competitions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            services.AddSingleton<ICompetitionUpdateQueue, CompetitionUpdateQueue>();
            services.AddHostedService<CompetitionUpdateQueue>(provider => (CompetitionUpdateQueue)provider.GetService<ICompetitionUpdateQueue>());
            services.AddHttpContextAccessor();
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddLogging();
            services.AddAuthentication(IISDefaults.AuthenticationScheme);
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

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
                app.UseHsts();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Users}/{action=Login}/{id?}",
                    defaults: new { controller = "Users", action = "Login" });
            });
        }


        protected override void CreateAutoMapperMaps(IMapperConfigurationExpression config)
        {
            //Mapping User
            config.CreateMap<UserDTO, User>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Name) ? Security.Encrypt(src.Name) : src.Name))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.UserName) ? Security.Encrypt(src.UserName) : src.UserName))
                .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.EmailAddress) ? Security.Encrypt(src.EmailAddress) : src.EmailAddress))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.PhoneNumber) ? Security.Encrypt(src.PhoneNumber) : src.PhoneNumber))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.Now))
                .ReverseMap()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Name) ? Security.Decrypt(src.Name) : src.Name))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.UserName) ? Security.Decrypt(src.UserName) : src.UserName))
                .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.EmailAddress) ? Security.Decrypt(src.EmailAddress) : src.EmailAddress))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.PhoneNumber) ? Security.Decrypt(src.PhoneNumber) : src.PhoneNumber))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.UserName) ? Security.Decrypt(src.UserName) : src.UserName));
        }
    }
}

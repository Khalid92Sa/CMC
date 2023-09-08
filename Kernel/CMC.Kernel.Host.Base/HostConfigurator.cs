using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CMC.Kernel.Core;
using CMC.Kernel.Core.Common;
using CMC.Kernel.Core.Configurations;
using CMC.Kernel.Core.Http;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Host.Base.Middlewares;
using CMC.Kernel.Host.Base.ServiceRegistration;
using CMC.Kernel.Infrastructure.Caching;
using CMC.Kernel.Infrastructure.Logging;
using CMC.Kernel.Infrastructure.Persistence.Repositories;
using CMC.Kernel.Infrastructure.Persistence.Services;
using CMC.Kernel.Infrastructure.Persistence.UnitOfWork;
using Serilog;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CMC.Kernel.Host.Base
{
    public class HostConfigurator : HostConfigurator<Configuration>
    {
        public HostConfigurator(IWebHostEnvironment env) : base(env)
        {
        }
    }
    /// <summary>
    /// Host Configurator
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    public class HostConfigurator<TConfiguration> where TConfiguration : Configuration
    {
        /// <summary>
        /// enviroment  
        /// </summary>
        private readonly IWebHostEnvironment _env;
        /// <summary>
        ///  Host Configurator
        /// </summary>
        /// <param name="env"></param>
        public HostConfigurator(IWebHostEnvironment env)
        {
            _env = env;
        }
        /// <summary>
        /// Configure Services
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            var config = BuildConfiguration();
            services.AddSingleton<Configuration>(config);
            services.AddSingleton(Log.Logger);

            services.AddTransient<IRestClient, RestClient>();
            services.AddHttpClient<RestClient>();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IHttpLogRepository, HttpLogRepository>();
            services.AddScoped<ICrossCuttingDependencies, CrossCuttingDependencies>();

            services.AddDbContext<IUnitOfWork, UnitOfWork>(options => options.UseSqlServer(config.ConnectionStrings.Default, providerOptions => providerOptions.CommandTimeout(60)).EnableSensitiveDataLogging(true), ServiceLifetime.Transient);
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddFluentValidationRegistration();
            services.AddCorsPolicyRegistration();
            services.AddLocalizationExtention();
            services.AddSession();
            services.AddControllers();
            services.RegisterRunningModuleDependencies();
            services.AddApplicationLoggers(config);
            services.AddCaching();
            services.AddMediatRRegistration();
            services.AddMvcCore()
                    .AddApiExplorer();
            services.AddApiVersioningRegistration();
            ConfigureAdditionalServices(services, config);
            services.AddAutoMapper(CreateAutoMapperMaps);
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddScoped<ILookupsService, LookupsService>();
        }
        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="config"></param>
        /// <param name="provider"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, Configuration config, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            if (env.IsProduction())
            {
                app.UseHsts();
                ConfigureProductionEnvironment(app, env);
            }

            app.UseStaticFilesMiddleware();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseCorsMiddleware();


            ConfigureAdditional(app, env);
            ConfigureAdditional(app, env, provider, config);
        }
        /// <summary>
        /// Build Configuration
        /// </summary>
        /// <returns></returns>
        private TConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder();
            builder
                .SetBasePath(_env.ContentRootPath)
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sharedsettings.json"), true, true)
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sharedsettings.Development.json"), true, true)
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), true, true)
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.{Environment}.json"), true, true)
                .AddEnvironmentVariables();
            var configRoot = builder.Build();
            return configRoot.Get<TConfiguration>();
        }
        /// <summary>
        /// Extract Resource Name From Relative Path
        /// </summary>
        /// <param name="tagSelector"></param>
        /// <returns></returns>
        private static string ExtractResourceNameFromRelativePath(ApiDescription tagSelector)
        {
            return Regex.Match(tagSelector.RelativePath, @"api\/(v\d.?/)?(?'resource_name'.*?)(\/|$)").Groups["resource_name"].Value;
        }
        /// <summary>
        /// Get Root Path
        /// </summary>
        /// <returns></returns>
        private string GetRootPath()
        {
            var exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var appRoot = exePath.Replace("file:\\", "");
            return appRoot;
        }
        /// <summary>
        /// Configure Additional Services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        protected virtual void ConfigureAdditionalServices(IServiceCollection services, Configuration config) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        protected virtual void ConfigureAdditional(IApplicationBuilder app, IWebHostEnvironment env) { }
        /// <summary>
        /// Configure Additional
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="provider"></param>
        protected virtual void ConfigureAdditional(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider, Configuration config) { }
        /// <summary>
        /// Configure Production Environment
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        protected virtual void ConfigureProductionEnvironment(IApplicationBuilder app, IWebHostEnvironment env) { }
        /// <summary>
        /// Create Auto Mapper Maps
        /// </summary>
        /// <param name="config"></param>
        protected virtual void CreateAutoMapperMaps(IMapperConfigurationExpression config) { }
    }
}

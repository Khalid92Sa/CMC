using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CMC.Kernel.Host.Base.Configurations
{
    /// <summary>
    /// To define swagger options 
    /// </summary>
    public class ConfigureSwaggerOptions
        : IConfigureNamedOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider provider;
        /// <summary>
        /// defult constructor for the class 
        /// </summary>
        /// <param name="provider"></param>
        public ConfigureSwaggerOptions(
            IApiVersionDescriptionProvider provider)
        {
            this.provider = provider;
        }
        /// <summary>
        /// Configure swagger for every API version discovered
        /// </summary>
        /// <param name="options"></param>
        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    CreateVersionInfo(description));
            }
        }
        /// <summary>
        /// Configure swagger for every API version discovered with names 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="options"></param>
        public void Configure(string name, SwaggerGenOptions options)
        {
            Configure(options);
        }
        /// <summary>
        /// add swagger document for every API version discovered 
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        private OpenApiInfo CreateVersionInfo(
                ApiVersionDescription description)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && x.FullName.StartsWith("CMC", StringComparison.OrdinalIgnoreCase)).ToList();
            var title = GetTitle(assemblies);

            var info = new OpenApiInfo()
            {
                Title = title,
                Version = description.ApiVersion.ToString()
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
        /// <summary>
        /// Get the Title of the Assemblies 
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        private string GetTitle(List<Assembly> assemblies)
        {
            if (assemblies.Any(a => a.FullName.Contains("CMC")))
            {
                return "CMC App API";
            }
            return "CMC App API";
        }
    }

}

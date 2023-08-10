using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using CMC.Kernel.Host.Base.Configurations;

namespace CMC.Kernel.Host.Base.ServiceRegistration
{
    public static class SwaggerRegistration
    {
        /// <summary>
        /// Register Swagger 
        /// </summary>
        /// <param name="services"></param>
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                  new OpenApiSecurityScheme
                  {
                    Reference = new OpenApiReference
                    {
                      Type = ReferenceType.SecurityScheme,
                      Id = "Bearer"
                    }
                   },
                   new string[] { }
                 }
                });
            });
            services.ConfigureOptions<ConfigureSwaggerOptions>();

            services.ConfigureSwaggerGen(options =>
            {
                options.CustomSchemaIds(x => x.FullName);
            });
        }
    }
}

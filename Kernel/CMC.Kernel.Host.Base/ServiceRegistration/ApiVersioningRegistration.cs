using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CMC.Kernel.Host.Base.ServiceRegistration
{
    public static class ApiVersioningRegistration
    {
        /// <summary>
        /// Registering the version of the API 
        /// </summary>
        /// <param name="services"></param>
        public static void AddApiVersioningRegistration(this IServiceCollection services)
        {
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            });

            services.AddVersionedApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });
        }
    }

}

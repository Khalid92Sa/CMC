using Microsoft.Extensions.DependencyInjection;

namespace CMC.Kernel.Host.Base.ServiceRegistration
{
    public static class CorsPolicyRegistration
    {
        /// <summary>
        /// Regestrion of Cors Policy
        /// </summary>
        /// <param name="service"></param>
        public static void AddCorsPolicyRegistration(this IServiceCollection service)
        {
            service.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed(origin => true)
                        .AllowCredentials());
            });
        }
    }
}

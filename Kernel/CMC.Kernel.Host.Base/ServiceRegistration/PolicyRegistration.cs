using Microsoft.Extensions.DependencyInjection;

namespace CMC.Kernel.Host.Base.ServiceRegistration
{
    public static class PolicyRegistration
    {
        /// <summary>
        /// To Add Policy and Authorization 
        /// </summary>
        /// <param name="services"></param>
        public static void AddPolicy(this IServiceCollection services)
        {
            services.AddAuthorization(config =>
            {
            });
        }
    }
}

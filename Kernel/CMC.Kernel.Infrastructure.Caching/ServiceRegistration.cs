using Microsoft.Extensions.DependencyInjection;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Infrastructure.Caching.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Infrastructure.Caching
{
    /// <summary>
    /// Service Registration
    /// </summary>
    public static class ServiceRegistration
    {
        /// <summary>
        /// Add Redis Caching
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCaching(this IServiceCollection services)
        {
            services.AddScoped<ICacheRepository, CacheRepository>();
            return services;
        }
    }
}

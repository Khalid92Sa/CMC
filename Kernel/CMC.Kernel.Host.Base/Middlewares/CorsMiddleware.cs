using Microsoft.AspNetCore.Builder;

namespace CMC.Kernel.Host.Base.Middlewares
{
    public static class CorsMiddleware
    {
        /// <summary>
        /// Used for API's security  
        /// </summary>
        /// <param name="app"></param>
        public static void UseCorsMiddleware(this IApplicationBuilder app)
        {
            app.UseCors(x => x
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                 .SetIsOriginAllowed(origin => true)
                 .AllowCredentials());
        }
    }
}

using Microsoft.AspNetCore.Builder;

namespace CMC.Kernel.Host.Base.Middlewares
{
    public static class StaticFilesMiddleware
    {
        /// <summary>
        /// Used for Static Files in the system 
        /// </summary>
        /// <param name="app"></param>
        public static void UseStaticFilesMiddleware(this IApplicationBuilder app)
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/yaml"
            });
        }
    }
}

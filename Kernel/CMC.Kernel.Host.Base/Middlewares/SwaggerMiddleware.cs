using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace CMC.Kernel.Host.Base.Middlewares
{
    public static class SwaggerMiddleware
    {
        /// <summary>
        /// To Allow the using of swagger in order to test and use API's
        /// </summary>
        /// <param name="app"></param>
        /// <param name="provider"></param>
        public static void UseSwagger(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            app.UseSwagger(options =>
            {

            });
            // Define swagger UI options 
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
                options.RoutePrefix = string.Empty;
            });
        }
    }
}

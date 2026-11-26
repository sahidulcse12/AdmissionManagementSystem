using Microsoft.OpenApi.Models;

namespace Admission.Api.OpenApi;

public static class SwaggerSetup
{
    public static IServiceCollection AddAdmissionSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "School Admission API",
                Version = "v1",
                Description =
                    "First-come-first-served school admission. Each shift is its own class. " +
                    "Student identity comes from the X-Student-Id header (17-digit birth registration number). " +
                    $"Optional API version: header {ApiVersioningSetup.HeaderName} or query ?{ApiVersioningSetup.QueryName}= (default 1.0).",
            });
        });
        return services;
    }

    // Swagger UI is always on (also in Docker) so reviewers can try the API at /swagger.
    public static WebApplication UseAdmissionSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(o =>
        {
            o.SwaggerEndpoint("/swagger/v1/swagger.json", "School Admission API v1");
            o.RoutePrefix = "swagger";
            o.DocumentTitle = "School Admission API";
        });
        return app;
    }
}

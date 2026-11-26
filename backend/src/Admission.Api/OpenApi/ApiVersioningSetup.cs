using Asp.Versioning;

namespace Admission.Api.OpenApi;

// API versioning without changing the contract paths (/classes, /holds/{id}, ...).
// Clients may send "X-Api-Version: 1.0" or "?api-version=1.0"; when they send nothing,
// version 1.0 is assumed, so existing clients and the assignment's tests keep working.
public static class ApiVersioningSetup
{
    public const string HeaderName = "X-Api-Version";
    public const string QueryName = "api-version";
    public const string SupportedVersionsHeader = "api-supported-versions";
    public static readonly ApiVersion V1 = new(1, 0);

    public static IServiceCollection AddAdmissionApiVersioning(this IServiceCollection services)
    {
        services
            .AddApiVersioning(o =>
            {
                o.DefaultApiVersion = V1;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true;
                o.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader(HeaderName),
                    new QueryStringApiVersionReader(QueryName));
            })
            .AddMvc()
            .AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.AssumeDefaultVersionWhenUnspecified = true;
            });
        return services;
    }
}

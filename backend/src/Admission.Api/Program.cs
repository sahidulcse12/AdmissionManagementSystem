using System.Text.Json.Serialization;
using Admission.Api.Configuration;
using Admission.Api.Endpoints;
using Admission.Api.Middleware;
using Admission.Api.OpenApi;
using Admission.Application;
using Admission.Application.Holds;
using Admission.Infrastructure;
using Admission.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;

var builder = WebApplication.CreateBuilder(args);

var settings = AdmissionSettings.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(settings);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddApplication(new HoldOptions
{
    HoldDuration = TimeSpan.FromSeconds(settings.HoldDurationSeconds),
    SweepInterval = TimeSpan.FromSeconds(settings.ExpirySweepSeconds),
});
builder.Services.AddInfrastructure(settings.ConnectionString);

// Let malformed requests surface as exceptions so the middleware formats them uniformly.
builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddAdmissionApiVersioning();
builder.Services.AddAdmissionSwagger();

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    if (settings.CorsOrigins.Contains("*")) policy.AllowAnyOrigin();
    else policy.WithOrigins(settings.CorsOrigins);
    policy.AllowAnyHeader().AllowAnyMethod().WithExposedHeaders(ApiVersioningSetup.SupportedVersionsHeader);
}));

var app = builder.Build();

await DatabaseInitializer.MigrateAsync(app.Services, app.Logger);

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors();
app.UseAdmissionSwagger();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).ExcludeFromDescription();

// Paths stay exactly as in the assignment contract; the version comes from the
// X-Api-Version header or ?api-version= query and defaults to 1.0.
var v1 = app.MapVersionedApi();
v1.MapClassEndpoints();
v1.MapHoldEndpoints();

app.Run();

public partial class Program;

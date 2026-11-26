using System.Text.Json.Serialization;
using Admission.Api.Configuration;
using Admission.Api.Endpoints;
using Admission.Api.Middleware;
using Admission.Application;
using Admission.Infrastructure;
using Admission.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;

var builder = WebApplication.CreateBuilder(args);

var settings = AdmissionSettings.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(settings);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(settings.ConnectionString);

// Let malformed requests surface as exceptions so the middleware formats them uniformly.
builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    if (settings.CorsOrigins.Contains("*")) policy.AllowAnyOrigin();
    else policy.WithOrigins(settings.CorsOrigins);
    policy.AllowAnyHeader().AllowAnyMethod();
}));

var app = builder.Build();

await DatabaseInitializer.MigrateAsync(app.Services, app.Logger);

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapClassEndpoints();

app.Run();

/// <summary>Exposed for WebApplicationFactory in integration tests.</summary>
public partial class Program;

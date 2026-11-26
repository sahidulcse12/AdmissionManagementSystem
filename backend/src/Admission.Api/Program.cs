using System.Text.Json.Serialization;
using Admission.Api.Configuration;
using Admission.Api.Data;
using Admission.Api.Features.Classes;
using Admission.Api.Http;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var settings = AdmissionSettings.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(settings);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<AdmissionDbContext>(o => o.UseNpgsql(settings.ConnectionString));
builder.Services.AddScoped<ClassService>();

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

app.UseCors();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapClassEndpoints();

app.Run();

/// <summary>Exposed for WebApplicationFactory in integration tests.</summary>
public partial class Program;

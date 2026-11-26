using Admission.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

var settings = AdmissionSettings.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(settings);

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    if (settings.CorsOrigins.Contains("*")) policy.AllowAnyOrigin();
    else policy.WithOrigins(settings.CorsOrigins);
    policy.AllowAnyHeader().AllowAnyMethod();
}));

var app = builder.Build();

app.UseCors();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

/// <summary>Exposed for WebApplicationFactory in integration tests.</summary>
public partial class Program;

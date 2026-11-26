using Admission.Api.Configuration;
using Admission.Api.Controllers;
using Admission.Api.Middleware;
using Admission.Api.OpenApi;
using Admission.Application;
using Admission.Application.Holds;
using Admission.Infrastructure;
using Admission.Infrastructure.Persistence;

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

builder.Services.AddAdmissionControllers();
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
app.MapControllers();

app.Run();

public partial class Program;

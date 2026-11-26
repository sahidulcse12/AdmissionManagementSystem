using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Admission.Infrastructure.Persistence;

/// <summary>Applies pending EF Core migrations at startup, retrying while the database starts.</summary>
public static class DatabaseInitializer
{
    public static async Task MigrateAsync(IServiceProvider services, ILogger logger, CancellationToken ct = default)
    {
        const int maxAttempts = 30;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AdmissionDbContext>();
                await db.Database.MigrateAsync(ct);
                logger.LogInformation("Database migrations applied.");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && ex is not OperationCanceledException)
            {
                logger.LogWarning("Database not ready (attempt {Attempt}/{Max}): {Message}", attempt, maxAttempts, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(2), ct);
            }
        }
    }
}

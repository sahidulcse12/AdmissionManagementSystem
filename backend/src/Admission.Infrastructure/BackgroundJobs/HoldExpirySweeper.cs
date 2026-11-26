using Admission.Application.Abstractions;
using Admission.Application.Holds;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Admission.Infrastructure.BackgroundJobs;

/// <summary>
/// Periodically persists EXPIRED for overdue holds. Correctness does not depend on this job:
/// seat counts and hold status already treat overdue HELD holds as expired. The sweeper keeps the
/// stored status accurate for reporting and frees the per-student unique slot promptly.
/// </summary>
internal sealed class HoldExpirySweeper(
    IServiceScopeFactory scopes,
    HoldOptions options,
    TimeProvider clock,
    ILogger<HoldExpirySweeper> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.SweepInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var holds = scope.ServiceProvider.GetRequiredService<IHoldRepository>();
                var expired = await holds.ExpireOverdueAsync(clock.GetUtcNow(), stoppingToken);
                if (expired > 0) logger.LogInformation("Expired {Count} overdue hold(s).", expired);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Hold expiry sweep failed; will retry on next tick.");
            }
        }
    }
}

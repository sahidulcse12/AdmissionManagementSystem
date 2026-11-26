using Admission.Application.Abstractions;
using Admission.Application.Holds;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Admission.Infrastructure.BackgroundJobs;

internal sealed class HoldExpirySweeper(
    IServiceScopeFactory scopes,
    HoldOptions options,
    TimeProvider clock,
    ILogger<HoldExpirySweeper> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Wait for the next sweep. SuppressThrowing makes shutdown end the wait quietly instead of
            // throwing OperationCanceledException (which the debugger would otherwise break on).
            await Task.Delay(options.SweepInterval, stoppingToken)
                .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var holds = scope.ServiceProvider.GetRequiredService<IHoldRepository>();
                var expired = await holds.ExpireOverdueAsync(clock.GetUtcNow(), stoppingToken);
                if (expired > 0) logger.LogInformation("Expired {Count} overdue hold(s).", expired);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break; // Application is shutting down mid-sweep; nothing to do.
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Hold expiry sweep failed; will retry on next tick.");
            }
        }
    }
}

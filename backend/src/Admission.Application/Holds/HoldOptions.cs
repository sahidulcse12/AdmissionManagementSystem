namespace Admission.Application.Holds;

public sealed class HoldOptions
{
    /// <summary>How long a new hold reserves its seat (HOLD_DURATION_SECONDS).</summary>
    public TimeSpan HoldDuration { get; init; } = TimeSpan.FromSeconds(600);

    /// <summary>How often overdue holds are persisted as EXPIRED (EXPIRY_SWEEP_SECONDS).</summary>
    public TimeSpan SweepInterval { get; init; } = TimeSpan.FromSeconds(15);
}

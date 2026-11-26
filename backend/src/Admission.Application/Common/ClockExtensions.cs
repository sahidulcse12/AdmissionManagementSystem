namespace Admission.Application.Common;

public static class ClockExtensions
{
    private const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;

    /// <summary>
    /// Current UTC time truncated to microseconds — the precision PostgreSQL stores.
    /// Using it everywhere guarantees that values returned straight after a write are identical
    /// to values read back later (required for idempotent confirmation responses).
    /// </summary>
    public static DateTimeOffset Now(this TimeProvider clock)
    {
        var now = clock.GetUtcNow();
        return now.AddTicks(-(now.Ticks % TicksPerMicrosecond));
    }
}

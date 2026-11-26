namespace Admission.Application.Holds;

public sealed class HoldOptions
{
    public TimeSpan HoldDuration { get; init; } = TimeSpan.FromSeconds(600);

    public TimeSpan SweepInterval { get; init; } = TimeSpan.FromSeconds(15);
}

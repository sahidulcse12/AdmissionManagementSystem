namespace Admission.Domain.Holds;

/// <summary>A temporary reservation of one seat in a class for one student.</summary>
public sealed class Hold
{
    private Hold() { } // EF Core

    public Guid Id { get; private set; }
    public Guid ClassId { get; private set; }
    public string StudentId { get; private set; } = string.Empty;
    public HoldStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    public static Hold Place(Guid classId, string studentId, DateTimeOffset now, TimeSpan duration) => new()
    {
        Id = Guid.NewGuid(),
        ClassId = classId,
        StudentId = studentId,
        Status = HoldStatus.HELD,
        CreatedAt = now,
        ExpiresAt = now + duration,
    };

    /// <summary>
    /// Status as of <paramref name="now"/>. A HELD hold past its deadline is reported as EXPIRED
    /// even before the background sweeper has persisted that change.
    /// </summary>
    public HoldStatus StatusAt(DateTimeOffset now) =>
        Status == HoldStatus.HELD && ExpiresAt <= now ? HoldStatus.EXPIRED : Status;

    /// <summary>True when the hold occupies a seat at <paramref name="now"/>.</summary>
    public bool OccupiesSeatAt(DateTimeOffset now) =>
        StatusAt(now) is HoldStatus.HELD or HoldStatus.CONFIRMED;

    public bool IsOwnedBy(string studentId) => StudentId == studentId;
}

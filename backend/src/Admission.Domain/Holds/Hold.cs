using Admission.Domain.Common;

namespace Admission.Domain.Holds;

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

    public HoldStatus StatusAt(DateTimeOffset now) =>
        Status == HoldStatus.HELD && ExpiresAt <= now ? HoldStatus.EXPIRED : Status;

    public bool OccupiesSeatAt(DateTimeOffset now) =>
        StatusAt(now) is HoldStatus.HELD or HoldStatus.CONFIRMED;

    public bool IsOwnedBy(string studentId) => StudentId == studentId;

    public bool Confirm(DateTimeOffset now)
    {
        switch (StatusAt(now))
        {
            case HoldStatus.CONFIRMED:
                return false;
            case HoldStatus.CANCELLED:
                throw new DomainException("HOLD_CANCELLED", "This hold was cancelled and cannot be confirmed.",
                    DomainErrorKind.Conflict);
            case HoldStatus.EXPIRED:
                throw new DomainException("HOLD_EXPIRED", "This hold has expired and cannot be confirmed.",
                    DomainErrorKind.Gone);
            default:
                Status = HoldStatus.CONFIRMED;
                ConfirmedAt = now;
                return true;
        }
    }

    public bool Cancel(DateTimeOffset now)
    {
        switch (StatusAt(now))
        {
            case HoldStatus.CANCELLED:
                return false;
            case HoldStatus.CONFIRMED:
                throw new DomainException("HOLD_ALREADY_CONFIRMED",
                    "This admission is already confirmed and cannot be cancelled.", DomainErrorKind.Conflict);
            case HoldStatus.EXPIRED:
                throw new DomainException("HOLD_EXPIRED", "This hold has already expired.", DomainErrorKind.Gone);
            default:
                Status = HoldStatus.CANCELLED;
                CancelledAt = now;
                return true;
        }
    }
}

using Admission.Application.Abstractions;

namespace Admission.Application.Holds;

public sealed record HoldResponse(
    Guid Id,
    Guid ClassId,
    string ClassName,
    string Shift,
    string StudentId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? CancelledAt,
    int RemainingSeconds,
    DateTimeOffset ServerTime)
{
    public static HoldResponse From(HoldWithClass item, DateTimeOffset now)
    {
        var (hold, cls) = item;
        var status = hold.StatusAt(now);
        var remaining = status == Domain.Holds.HoldStatus.HELD
            ? (int)Math.Ceiling((hold.ExpiresAt - now).TotalSeconds)
            : 0;

        return new HoldResponse(
            hold.Id, cls.Id, cls.Name, cls.Shift.ToString(), hold.StudentId, status.ToString(),
            hold.CreatedAt, hold.ExpiresAt, hold.ConfirmedAt, hold.CancelledAt,
            Math.Max(0, remaining), now);
    }
}

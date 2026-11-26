using Admission.Application.Abstractions;
using Admission.Domain.Holds;

namespace Admission.Application.Holds;

/// <summary>
/// Hold as returned by every hold endpoint. Contains all contract fields
/// (<c>holdId, classId, className, studentId, status, expiresAt</c>) plus extra detail.
/// </summary>
public sealed record HoldResponse(
    Guid HoldId,
    Guid ClassId,
    string ClassName,
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
        var remaining = status == HoldStatus.HELD
            ? (int)Math.Ceiling((hold.ExpiresAt - now).TotalSeconds)
            : 0;

        return new HoldResponse(
            hold.Id, cls.Id, cls.Name, hold.StudentId, status.ToString(),
            hold.CreatedAt, hold.ExpiresAt, hold.ConfirmedAt, hold.CancelledAt,
            Math.Max(0, remaining), now);
    }
}

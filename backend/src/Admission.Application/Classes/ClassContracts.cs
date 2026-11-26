using Admission.Application.Abstractions;

namespace Admission.Application.Classes;

/// <summary>Request body of <c>POST /classes</c>: <c>{ "name": "...", "totalSeats": 120 }</c>.</summary>
public sealed record CreateClassRequest(string? Name, int? TotalSeats);

public sealed record ClassResponse(
    Guid Id,
    string Name,
    int TotalSeats,
    int HeldSeats,
    int ConfirmedSeats,
    int AvailableSeats,
    DateTimeOffset CreatedAt)
{
    public static ClassResponse From(ClassWithUsage u) => new(
        u.Class.Id,
        u.Class.Name,
        u.Class.TotalSeats,
        u.HeldSeats,
        u.ConfirmedSeats,
        Math.Max(0, u.Class.TotalSeats - u.HeldSeats - u.ConfirmedSeats),
        u.Class.CreatedAt);
}

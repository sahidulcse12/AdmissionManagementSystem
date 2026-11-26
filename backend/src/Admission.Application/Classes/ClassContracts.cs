using Admission.Application.Abstractions;

namespace Admission.Application.Classes;

public sealed record CreateClassRequest(string? Name, string? Shift, int? Capacity);

public sealed record ClassResponse(
    Guid Id,
    string Name,
    string Shift,
    int Capacity,
    int HeldSeats,
    int ConfirmedSeats,
    int AvailableSeats,
    DateTimeOffset CreatedAt)
{
    public static ClassResponse From(ClassWithUsage u) => new(
        u.Class.Id,
        u.Class.Name,
        u.Class.Shift.ToString(),
        u.Class.Capacity,
        u.HeldSeats,
        u.ConfirmedSeats,
        Math.Max(0, u.Class.Capacity - u.HeldSeats - u.ConfirmedSeats),
        u.Class.CreatedAt);
}

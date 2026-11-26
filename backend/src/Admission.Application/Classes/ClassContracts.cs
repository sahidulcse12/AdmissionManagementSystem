using Admission.Domain.Classes;

namespace Admission.Application.Classes;

public sealed record CreateClassRequest(string? Name, string? Shift, int? Capacity);

public sealed record ClassResponse(
    Guid Id,
    string Name,
    string Shift,
    int Capacity,
    DateTimeOffset CreatedAt)
{
    public static ClassResponse From(SchoolClass c) =>
        new(c.Id, c.Name, c.Shift.ToString(), c.Capacity, c.CreatedAt);
}

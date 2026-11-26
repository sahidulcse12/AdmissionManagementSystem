namespace Admission.Tests.Infrastructure;

// Test-side copies of API response shapes, so tests verify the wire contract rather than internal types.

public sealed record ClassDto(
    Guid Id,
    string Name,
    string Shift,
    int Capacity,
    DateTimeOffset CreatedAt);

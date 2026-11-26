namespace Admission.Tests.Infrastructure;

// Test-side copies of API response shapes, so tests verify the wire contract rather than internal types.

public sealed record ClassDto(
    Guid Id,
    string Name,
    string Shift,
    int Capacity,
    int HeldSeats,
    int ConfirmedSeats,
    int AvailableSeats,
    DateTimeOffset CreatedAt);

public sealed record HoldDto(
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
    DateTimeOffset ServerTime);

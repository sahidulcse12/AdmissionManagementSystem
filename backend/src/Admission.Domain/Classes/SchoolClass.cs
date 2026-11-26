using Admission.Domain.Common;

namespace Admission.Domain.Classes;

public sealed class SchoolClass
{
    public const int NameMaxLength = 100;
    public const int MinSeats = 1;
    public const int MaxSeats = 10_000;

    private SchoolClass() { } // EF Core

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public int TotalSeats { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static SchoolClass Create(string? name, int? totalSeats, DateTimeOffset now)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new DomainException("VALIDATION_ERROR", "Class name is required.");
        if (trimmed.Length > NameMaxLength)
            throw new DomainException("VALIDATION_ERROR", $"Class name must be at most {NameMaxLength} characters.");

        return new SchoolClass
        {
            Id = Guid.NewGuid(),
            Name = trimmed,
            NormalizedName = Normalize(trimmed),
            TotalSeats = ValidateSeats(totalSeats),
            CreatedAt = now,
        };
    }

    public static string Normalize(string name) => name.Trim().ToUpperInvariant();

    private static int ValidateSeats(int? totalSeats) =>
        totalSeats is { } value && value is >= MinSeats and <= MaxSeats
            ? value
            : throw new DomainException("VALIDATION_ERROR",
                $"totalSeats must be a whole number between {MinSeats} and {MaxSeats}.");
}

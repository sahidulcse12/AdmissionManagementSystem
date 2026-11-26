using Admission.Domain.Common;

namespace Admission.Domain.Classes;

/// <summary>A class offered in a specific shift with a fixed number of seats.</summary>
public sealed class SchoolClass
{
    public const int NameMaxLength = 100;
    public const int MinCapacity = 1;
    public const int MaxCapacity = 1000;

    private SchoolClass() { } // EF Core

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    /// <summary>Upper-cased name used for case-insensitive uniqueness per shift.</summary>
    public string NormalizedName { get; private set; } = string.Empty;

    public Shift Shift { get; private set; }
    public int Capacity { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static SchoolClass Create(string? name, string? shift, int? capacity, DateTimeOffset now)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new DomainException("VALIDATION_ERROR", "Name is required.");
        if (trimmed.Length > NameMaxLength)
            throw new DomainException("VALIDATION_ERROR", $"Name must be at most {NameMaxLength} characters.");

        return new SchoolClass
        {
            Id = Guid.NewGuid(),
            Name = trimmed,
            NormalizedName = Normalize(trimmed),
            Shift = ParseShift(shift),
            Capacity = ValidateCapacity(capacity),
            CreatedAt = now,
        };
    }

    public static string Normalize(string name) => name.Trim().ToUpperInvariant();

    public static Shift ParseShift(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)
            && Enum.TryParse<Shift>(value.Trim(), ignoreCase: true, out var shift)
            && Enum.IsDefined(shift)
            && !int.TryParse(value, out _))
            return shift;

        throw new DomainException("VALIDATION_ERROR",
            $"Shift must be one of: {string.Join(", ", Enum.GetNames<Shift>())}.");
    }

    private static int ValidateCapacity(int? capacity) =>
        capacity is { } value && value is >= MinCapacity and <= MaxCapacity
            ? value
            : throw new DomainException("VALIDATION_ERROR",
                $"Capacity must be between {MinCapacity} and {MaxCapacity}.");
}

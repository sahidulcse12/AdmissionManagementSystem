namespace Admission.Api.Domain;

/// <summary>A class offered in a specific shift with a fixed number of seats.</summary>
public sealed class SchoolClass
{
    public const int NameMaxLength = 100;
    public const int MinCapacity = 1;
    public const int MaxCapacity = 1000;

    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Shift Shift { get; set; }
    public int Capacity { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

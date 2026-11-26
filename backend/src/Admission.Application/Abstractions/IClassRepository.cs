using Admission.Domain.Classes;

namespace Admission.Application.Abstractions;

/// <summary>A class with its current seat usage.</summary>
public sealed record ClassWithUsage(SchoolClass Class, int HeldSeats, int ConfirmedSeats);

public interface IClassRepository
{
    void Add(SchoolClass schoolClass);

    Task<bool> ExistsAsync(string normalizedName, CancellationToken ct);

    Task<SchoolClass?> GetAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Loads a class with a row lock (<c>SELECT … FOR UPDATE</c>). Must run inside a transaction.
    /// Every operation that can take a seat locks the class first, so seat checks are serialised per class.
    /// </summary>
    Task<SchoolClass?> LockAsync(Guid id, CancellationToken ct);

    Task<ClassWithUsage?> GetWithUsageAsync(Guid id, DateTimeOffset now, CancellationToken ct);

    Task<IReadOnlyList<ClassWithUsage>> ListWithUsageAsync(DateTimeOffset now, CancellationToken ct);
}

using Admission.Domain.Classes;

namespace Admission.Application.Abstractions;

public sealed record ClassWithUsage(SchoolClass Class, int HeldSeats, int ConfirmedSeats);

public interface IClassRepository
{
    void Add(SchoolClass schoolClass);

    Task<bool> ExistsAsync(string normalizedName, CancellationToken ct);

    Task<SchoolClass?> GetAsync(Guid id, CancellationToken ct);

    Task<SchoolClass?> LockAsync(Guid id, CancellationToken ct);

    Task<ClassWithUsage?> GetWithUsageAsync(Guid id, DateTimeOffset now, CancellationToken ct);

    Task<IReadOnlyList<ClassWithUsage>> ListWithUsageAsync(DateTimeOffset now, CancellationToken ct);
}

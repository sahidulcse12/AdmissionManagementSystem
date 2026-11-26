using Admission.Domain.Classes;

namespace Admission.Application.Abstractions;

public interface IClassRepository
{
    void Add(SchoolClass schoolClass);

    Task<bool> ExistsAsync(string normalizedName, Shift shift, CancellationToken ct);

    Task<SchoolClass?> GetAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<SchoolClass>> ListAsync(CancellationToken ct);
}

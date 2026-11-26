using Admission.Domain.Classes;
using Admission.Domain.Holds;

namespace Admission.Application.Abstractions;

public sealed record HoldWithClass(Hold Hold, SchoolClass Class);

public interface IHoldRepository
{
    void Add(Hold hold);

    Task<HoldWithClass?> GetAsync(Guid id, CancellationToken ct);

    Task<Hold?> LockAsync(Guid id, CancellationToken ct);

    Task<int> ExpireOverdueAsync(DateTimeOffset now, CancellationToken ct);

    Task ExpireOverdueForStudentAsync(Guid classId, string studentId, DateTimeOffset now, CancellationToken ct);

    Task<Hold?> FindActiveAsync(Guid classId, string studentId, CancellationToken ct);

    Task<int> CountOccupiedSeatsAsync(Guid classId, DateTimeOffset now, CancellationToken ct);

    Task<IReadOnlyList<HoldWithClass>> ListByStudentAsync(string studentId, CancellationToken ct);

    Task<IReadOnlyList<HoldWithClass>> ListByClassAsync(Guid classId, CancellationToken ct);
}

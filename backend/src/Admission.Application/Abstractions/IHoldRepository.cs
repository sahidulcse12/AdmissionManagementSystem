using Admission.Domain.Classes;
using Admission.Domain.Holds;

namespace Admission.Application.Abstractions;

/// <summary>A hold together with the class it belongs to, for display.</summary>
public sealed record HoldWithClass(Hold Hold, SchoolClass Class);

public interface IHoldRepository
{
    void Add(Hold hold);

    Task<HoldWithClass?> GetAsync(Guid id, CancellationToken ct);

    /// <summary>Loads a hold with a row lock (<c>SELECT … FOR UPDATE</c>). Must run inside a transaction.</summary>
    Task<Hold?> LockAsync(Guid id, CancellationToken ct);

    /// <summary>Persists EXPIRED for every HELD hold past its deadline (background sweep).</summary>
    Task<int> ExpireOverdueAsync(DateTimeOffset now, CancellationToken ct);

    /// <summary>
    /// Persists EXPIRED for one student's overdue hold in a class (at most one row), so the
    /// per-student unique slot is free before a new hold is placed. Touching a single row keeps
    /// lock ordering trivial and avoids deadlocks with the background sweep.
    /// </summary>
    Task ExpireOverdueForStudentAsync(Guid classId, string studentId, DateTimeOffset now, CancellationToken ct);

    /// <summary>The student's HELD or CONFIRMED hold in the class, if any.</summary>
    Task<Hold?> FindActiveAsync(Guid classId, string studentId, CancellationToken ct);

    /// <summary>Number of seats currently occupied (unexpired HELD + CONFIRMED).</summary>
    Task<int> CountOccupiedSeatsAsync(Guid classId, DateTimeOffset now, CancellationToken ct);

    Task<IReadOnlyList<HoldWithClass>> ListByStudentAsync(string studentId, CancellationToken ct);

    Task<IReadOnlyList<HoldWithClass>> ListByClassAsync(Guid classId, CancellationToken ct);
}

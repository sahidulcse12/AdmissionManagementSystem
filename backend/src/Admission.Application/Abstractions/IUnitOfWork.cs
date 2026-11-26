namespace Admission.Application.Abstractions;

/// <summary>Transaction boundary over the persistence layer.</summary>
public interface IUnitOfWork
{
    /// <summary>Persists tracked changes. Throws <c>UniqueConstraintException</c> on unique violations.</summary>
    Task SaveChangesAsync(CancellationToken ct);

    /// <summary>
    /// Runs <paramref name="work"/> inside a database transaction and commits it.
    /// Row locks taken inside the work are held until commit or rollback.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct);
}

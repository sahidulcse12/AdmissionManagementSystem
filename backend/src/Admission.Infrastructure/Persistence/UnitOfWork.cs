using Admission.Application.Abstractions;
using Admission.Application.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Admission.Infrastructure.Persistence;

internal sealed class UnitOfWork(AdmissionDbContext db) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            throw new UniqueConstraintException(pg.ConstraintName, ex);
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var result = await work(ct);
        await tx.CommitAsync(ct);
        return result;
    }
}

using Admission.Application.Abstractions;
using Admission.Domain.Classes;
using Admission.Domain.Holds;
using Microsoft.EntityFrameworkCore;

namespace Admission.Infrastructure.Persistence.Repositories;

internal sealed class ClassRepository(AdmissionDbContext db) : IClassRepository
{
    public void Add(SchoolClass schoolClass) => db.Classes.Add(schoolClass);

    public Task<bool> ExistsAsync(string normalizedName, CancellationToken ct) =>
        db.Classes.AnyAsync(c => c.NormalizedName == normalizedName, ct);

    public Task<SchoolClass?> GetAsync(Guid id, CancellationToken ct) =>
        db.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<SchoolClass?> LockAsync(Guid id, CancellationToken ct) =>
        db.Classes
            .FromSqlInterpolated($"SELECT * FROM classes WHERE id = {id} FOR UPDATE")
            .FirstOrDefaultAsync(ct);

    public Task<ClassWithUsage?> GetWithUsageAsync(Guid id, DateTimeOffset now, CancellationToken ct) =>
        WithUsage(db.Classes.Where(c => c.Id == id), now).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<ClassWithUsage>> ListWithUsageAsync(DateTimeOffset now, CancellationToken ct) =>
        await WithUsage(db.Classes.OrderBy(c => c.Name), now).ToListAsync(ct);

    /// <summary>Seat usage is always computed from the holds table, never cached.</summary>
    private IQueryable<ClassWithUsage> WithUsage(IQueryable<SchoolClass> query, DateTimeOffset now) =>
        query.AsNoTracking().Select(c => new ClassWithUsage(
            c,
            db.Holds.Count(h => h.ClassId == c.Id && h.Status == HoldStatus.HELD && h.ExpiresAt > now),
            db.Holds.Count(h => h.ClassId == c.Id && h.Status == HoldStatus.CONFIRMED)));
}

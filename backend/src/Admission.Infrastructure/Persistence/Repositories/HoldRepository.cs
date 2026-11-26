using Admission.Application.Abstractions;
using Admission.Domain.Holds;
using Microsoft.EntityFrameworkCore;

namespace Admission.Infrastructure.Persistence.Repositories;

internal sealed class HoldRepository(AdmissionDbContext db) : IHoldRepository
{
    public void Add(Hold hold) => db.Holds.Add(hold);

    public Task<HoldWithClass?> GetAsync(Guid id, CancellationToken ct) =>
        WithClass(db.Holds.Where(h => h.Id == id)).FirstOrDefaultAsync(ct);

    public Task<Hold?> LockAsync(Guid id, CancellationToken ct) =>
        db.Holds
            .FromSqlInterpolated($"SELECT * FROM holds WHERE id = {id} FOR UPDATE")
            .FirstOrDefaultAsync(ct);

    public Task<int> ExpireOverdueAsync(DateTimeOffset now, CancellationToken ct) =>
        db.Holds
            .Where(h => h.Status == HoldStatus.HELD && h.ExpiresAt <= now)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.Status, HoldStatus.EXPIRED), ct);

    public Task ExpireOverdueForStudentAsync(Guid classId, string studentId, DateTimeOffset now, CancellationToken ct) =>
        db.Holds
            .Where(h => h.ClassId == classId && h.StudentId == studentId &&
                        h.Status == HoldStatus.HELD && h.ExpiresAt <= now)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.Status, HoldStatus.EXPIRED), ct);

    public Task<Hold?> FindActiveAsync(Guid classId, string studentId, CancellationToken ct) =>
        db.Holds.AsNoTracking().FirstOrDefaultAsync(h =>
            h.ClassId == classId && h.StudentId == studentId &&
            (h.Status == HoldStatus.HELD || h.Status == HoldStatus.CONFIRMED), ct);

    public Task<int> CountOccupiedSeatsAsync(Guid classId, DateTimeOffset now, CancellationToken ct) =>
        db.Holds.CountAsync(h => h.ClassId == classId &&
            (h.Status == HoldStatus.CONFIRMED || (h.Status == HoldStatus.HELD && h.ExpiresAt > now)), ct);

    public async Task<IReadOnlyList<HoldWithClass>> ListByStudentAsync(string studentId, CancellationToken ct) =>
        await WithClass(db.Holds.Where(h => h.StudentId == studentId))
            .OrderByDescending(x => x.Hold.CreatedAt).ToListAsync(ct);

    public async Task<IReadOnlyList<HoldWithClass>> ListByClassAsync(Guid classId, CancellationToken ct) =>
        await WithClass(db.Holds.Where(h => h.ClassId == classId))
            .OrderByDescending(x => x.Hold.CreatedAt).ToListAsync(ct);

    private IQueryable<HoldWithClass> WithClass(IQueryable<Hold> holds) =>
        holds.AsNoTracking().Join(db.Classes.AsNoTracking(), h => h.ClassId, c => c.Id, (h, c) => new HoldWithClass(h, c));
}

using Admission.Application.Abstractions;
using Admission.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Admission.Infrastructure.Persistence.Repositories;

internal sealed class ClassRepository(AdmissionDbContext db) : IClassRepository
{
    public void Add(SchoolClass schoolClass) => db.Classes.Add(schoolClass);

    public Task<bool> ExistsAsync(string normalizedName, Shift shift, CancellationToken ct) =>
        db.Classes.AnyAsync(c => c.NormalizedName == normalizedName && c.Shift == shift, ct);

    public Task<SchoolClass?> GetAsync(Guid id, CancellationToken ct) =>
        db.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<SchoolClass>> ListAsync(CancellationToken ct) =>
        await db.Classes.AsNoTracking().OrderBy(c => c.Name).ThenBy(c => c.Shift).ToListAsync(ct);
}

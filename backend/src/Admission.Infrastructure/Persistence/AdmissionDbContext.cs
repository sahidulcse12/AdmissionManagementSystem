using Admission.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Admission.Infrastructure.Persistence;

public sealed class AdmissionDbContext(DbContextOptions<AdmissionDbContext> options) : DbContext(options)
{
    public DbSet<SchoolClass> Classes => Set<SchoolClass>();

    protected override void OnModelCreating(ModelBuilder model) =>
        model.ApplyConfigurationsFromAssembly(typeof(AdmissionDbContext).Assembly);
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Admission.Infrastructure.Persistence;

/// <summary>Used only by `dotnet ef` to generate migrations; no database connection is opened.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AdmissionDbContext>
{
    public AdmissionDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AdmissionDbContext>()
            .UseNpgsql("Host=localhost;Database=admission_design")
            .Options);
}

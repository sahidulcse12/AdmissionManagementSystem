using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Admission.Api.Data;

/// <summary>Used only by `dotnet ef` to generate migrations; no database connection is opened.</summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AdmissionDbContext>
{
    public AdmissionDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AdmissionDbContext>()
            .UseNpgsql("Host=localhost;Database=admission_design")
            .Options);
}

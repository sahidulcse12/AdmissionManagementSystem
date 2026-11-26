using Admission.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Admission.Api.Data;

public sealed class AdmissionDbContext(DbContextOptions<AdmissionDbContext> options) : DbContext(options)
{
    public DbSet<SchoolClass> Classes => Set<SchoolClass>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<SchoolClass>(e =>
        {
            e.ToTable("classes");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.Name).HasColumnName("name").HasMaxLength(SchoolClass.NameMaxLength).IsRequired();
            e.Property(c => c.Shift).HasColumnName("shift").HasConversion<string>().HasMaxLength(16).IsRequired();
            e.Property(c => c.Capacity).HasColumnName("capacity");
            e.Property(c => c.CreatedAt).HasColumnName("created_at");
            e.HasIndex(c => new { c.Name, c.Shift }).IsUnique().HasDatabaseName("ux_classes_name_shift");
            e.ToTable(t => t.HasCheckConstraint("ck_classes_capacity_positive", "capacity > 0"));
        });
    }
}

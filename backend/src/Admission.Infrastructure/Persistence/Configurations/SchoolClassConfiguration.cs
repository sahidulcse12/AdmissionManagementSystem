using Admission.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admission.Infrastructure.Persistence.Configurations;

internal sealed class SchoolClassConfiguration : IEntityTypeConfiguration<SchoolClass>
{
    public void Configure(EntityTypeBuilder<SchoolClass> e)
    {
        e.ToTable("classes", t => t.HasCheckConstraint("ck_classes_total_seats_positive", "total_seats > 0"));
        e.HasKey(c => c.Id);
        e.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        e.Property(c => c.Name).HasColumnName("name").HasMaxLength(SchoolClass.NameMaxLength).IsRequired();
        e.Property(c => c.NormalizedName).HasColumnName("normalized_name").HasMaxLength(SchoolClass.NameMaxLength).IsRequired();
        e.Property(c => c.TotalSeats).HasColumnName("total_seats");
        e.Property(c => c.CreatedAt).HasColumnName("created_at");

        e.HasIndex(c => c.NormalizedName).IsUnique().HasDatabaseName(ConstraintNames.ClassName);
    }
}

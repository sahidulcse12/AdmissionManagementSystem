using Admission.Domain.Classes;
using Admission.Domain.Holds;
using Admission.Domain.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admission.Infrastructure.Persistence.Configurations;

internal sealed class HoldConfiguration : IEntityTypeConfiguration<Hold>
{
    public void Configure(EntityTypeBuilder<Hold> e)
    {
        e.ToTable("holds", t => t.HasCheckConstraint("ck_holds_status",
            "status IN ('HELD','CONFIRMED','EXPIRED','CANCELLED')"));
        e.HasKey(h => h.Id);
        e.Property(h => h.Id).HasColumnName("id").ValueGeneratedNever();
        e.Property(h => h.ClassId).HasColumnName("class_id");
        e.Property(h => h.StudentId).HasColumnName("student_id").HasMaxLength(StudentId.Length).IsRequired();
        e.Property(h => h.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16).IsRequired();
        e.Property(h => h.CreatedAt).HasColumnName("created_at");
        e.Property(h => h.ExpiresAt).HasColumnName("expires_at");
        e.Property(h => h.ConfirmedAt).HasColumnName("confirmed_at");
        e.Property(h => h.CancelledAt).HasColumnName("cancelled_at");

        e.HasOne<SchoolClass>().WithMany().HasForeignKey(h => h.ClassId).OnDelete(DeleteBehavior.Restrict);

        // Database-level guarantee: one active (HELD or CONFIRMED) hold per student per class.
        e.HasIndex(h => new { h.ClassId, h.StudentId })
            .IsUnique()
            .HasFilter("status IN ('HELD','CONFIRMED')")
            .HasDatabaseName(ConstraintNames.OneActiveHoldPerStudentPerClass);

        // Seat counting and expiry sweeps filter by class, status and deadline.
        e.HasIndex(h => new { h.ClassId, h.Status, h.ExpiresAt }).HasDatabaseName("ix_holds_class_status_expires");
        e.HasIndex(h => new { h.Status, h.ExpiresAt }).HasDatabaseName("ix_holds_status_expires");
        e.HasIndex(h => new { h.StudentId, h.CreatedAt }).HasDatabaseName("ix_holds_student_created");
    }
}

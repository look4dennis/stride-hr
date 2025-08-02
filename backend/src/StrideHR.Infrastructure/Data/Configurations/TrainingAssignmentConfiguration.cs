using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class TrainingAssignmentConfiguration : IEntityTypeConfiguration<TrainingAssignment>
{
    public void Configure(EntityTypeBuilder<TrainingAssignment> builder)
    {
        builder.ToTable("TrainingAssignments");

        builder.HasKey(ta => ta.Id);

        builder.Property(ta => ta.TrainingModuleId)
            .IsRequired();

        builder.Property(ta => ta.EmployeeId)
            .IsRequired();

        builder.Property(ta => ta.AssignedBy)
            .IsRequired();

        builder.Property(ta => ta.AssignedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ta => ta.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(TrainingAssignmentStatus.Assigned);

        builder.Property(ta => ta.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(ta => ta.TrainingModule)
            .WithMany(tm => tm.TrainingAssignments)
            .HasForeignKey(ta => ta.TrainingModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ta => ta.Employee)
            .WithMany()
            .HasForeignKey(ta => ta.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ta => ta.AssignedByEmployee)
            .WithMany()
            .HasForeignKey(ta => ta.AssignedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ta => ta.TrainingProgress)
            .WithOne(tp => tp.TrainingAssignment)
            .HasForeignKey<TrainingProgress>(tp => tp.TrainingAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ta => ta.TrainingModuleId);
        builder.HasIndex(ta => ta.EmployeeId);
        builder.HasIndex(ta => ta.AssignedBy);
        builder.HasIndex(ta => ta.Status);
        builder.HasIndex(ta => ta.AssignedAt);
        builder.HasIndex(ta => ta.DueDate);
        builder.HasIndex(ta => new { ta.EmployeeId, ta.TrainingModuleId }).IsUnique();
    }
}
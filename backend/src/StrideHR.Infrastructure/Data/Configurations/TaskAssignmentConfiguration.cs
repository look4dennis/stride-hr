using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class TaskAssignmentConfiguration : IEntityTypeConfiguration<TaskAssignment>
{
    public void Configure(EntityTypeBuilder<TaskAssignment> builder)
    {
        builder.ToTable("TaskAssignments");

        builder.HasKey(ta => ta.Id);

        builder.Property(ta => ta.Notes)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(ta => ta.Task)
            .WithMany(t => t.TaskAssignments)
            .HasForeignKey(ta => ta.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ta => ta.Employee)
            .WithMany(e => e.TaskAssignments)
            .HasForeignKey(ta => ta.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ta => ta.TaskId);
        builder.HasIndex(ta => ta.EmployeeId);
        builder.HasIndex(ta => new { ta.TaskId, ta.EmployeeId });
        builder.HasIndex(ta => ta.CompletedDate);
    }
}
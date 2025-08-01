using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ProjectAssignmentConfiguration : IEntityTypeConfiguration<ProjectAssignment>
{
    public void Configure(EntityTypeBuilder<ProjectAssignment> builder)
    {
        builder.ToTable("ProjectAssignments");

        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.Role)
            .HasMaxLength(100);

        builder.Property(pa => pa.HourlyRate)
            .HasColumnType("decimal(18,2)");

        // Relationships
        builder.HasOne(pa => pa.Project)
            .WithMany(p => p.ProjectAssignments)
            .HasForeignKey(pa => pa.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pa => pa.Employee)
            .WithMany(e => e.ProjectAssignments)
            .HasForeignKey(pa => pa.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pa => pa.ProjectId);
        builder.HasIndex(pa => pa.EmployeeId);
        builder.HasIndex(pa => new { pa.ProjectId, pa.EmployeeId });
        builder.HasIndex(pa => pa.IsTeamLead);
    }
}
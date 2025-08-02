using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ProjectAlertConfiguration : IEntityTypeConfiguration<ProjectAlert>
{
    public void Configure(EntityTypeBuilder<ProjectAlert> builder)
    {
        builder.ToTable("ProjectAlerts");

        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.ProjectId)
            .IsRequired();

        builder.Property(pa => pa.AlertType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(pa => pa.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(pa => pa.Severity)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(pa => pa.IsResolved)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pa => pa.ResolvedByEmployeeId)
            .IsRequired(false);

        builder.Property(pa => pa.ResolvedAt)
            .IsRequired(false);

        builder.Property(pa => pa.ResolutionNotes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(pa => pa.Project)
            .WithMany()
            .HasForeignKey(pa => pa.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pa => pa.ResolvedByEmployee)
            .WithMany()
            .HasForeignKey(pa => pa.ResolvedByEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(pa => pa.ProjectId);
        builder.HasIndex(pa => pa.AlertType);
        builder.HasIndex(pa => pa.Severity);
        builder.HasIndex(pa => pa.IsResolved);
        builder.HasIndex(pa => pa.CreatedAt);
    }
}
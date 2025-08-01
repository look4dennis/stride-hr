using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class DSRConfiguration : IEntityTypeConfiguration<DSR>
{
    public void Configure(EntityTypeBuilder<DSR> builder)
    {
        builder.ToTable("DSRs");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(d => d.HoursWorked)
            .HasColumnType("decimal(4,2)");

        builder.Property(d => d.Status)
            .HasConversion<int>();

        builder.Property(d => d.ReviewComments)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(d => d.Employee)
            .WithMany(e => e.DSRs)
            .HasForeignKey(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Project)
            .WithMany(p => p.DSRs)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.Task)
            .WithMany(t => t.DSRs)
            .HasForeignKey(d => d.TaskId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.Reviewer)
            .WithMany(e => e.ReviewedDSRs)
            .HasForeignKey(d => d.ReviewedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(d => d.EmployeeId);
        builder.HasIndex(d => d.ProjectId);
        builder.HasIndex(d => d.TaskId);
        builder.HasIndex(d => d.Date);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => new { d.EmployeeId, d.Date });
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class AssetAssignmentConfiguration : IEntityTypeConfiguration<AssetAssignment>
{
    public void Configure(EntityTypeBuilder<AssetAssignment> builder)
    {
        builder.ToTable("AssetAssignments");

        builder.HasKey(aa => aa.Id);

        builder.Property(aa => aa.AssignedCondition)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(aa => aa.ReturnedCondition)
            .HasConversion<int>();

        builder.Property(aa => aa.AssignmentNotes)
            .HasMaxLength(1000);

        builder.Property(aa => aa.ReturnNotes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(aa => aa.Asset)
            .WithMany(a => a.AssetAssignments)
            .HasForeignKey(aa => aa.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(aa => aa.Employee)
            .WithMany()
            .HasForeignKey(aa => aa.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(aa => aa.Project)
            .WithMany()
            .HasForeignKey(aa => aa.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(aa => aa.AssignedByEmployee)
            .WithMany()
            .HasForeignKey(aa => aa.AssignedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(aa => aa.ReturnedByEmployee)
            .WithMany()
            .HasForeignKey(aa => aa.ReturnedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(aa => aa.AssetId);
        builder.HasIndex(aa => aa.EmployeeId);
        builder.HasIndex(aa => aa.ProjectId);
        builder.HasIndex(aa => aa.IsActive);
        builder.HasIndex(aa => aa.AssignedDate);
        builder.HasIndex(aa => new { aa.AssetId, aa.IsActive });
    }
}
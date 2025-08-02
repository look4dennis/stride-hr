using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class AssetHandoverConfiguration : IEntityTypeConfiguration<AssetHandover>
{
    public void Configure(EntityTypeBuilder<AssetHandover> builder)
    {
        builder.ToTable("AssetHandovers");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.ReturnedCondition)
            .HasConversion<int>();

        builder.Property(h => h.HandoverNotes)
            .HasMaxLength(1000);

        builder.Property(h => h.DamageNotes)
            .HasMaxLength(1000);

        builder.Property(h => h.DamageCharges)
            .HasColumnType("decimal(18,2)");

        builder.Property(h => h.Currency)
            .HasMaxLength(3);

        // Relationships
        builder.HasOne(h => h.Asset)
            .WithMany(a => a.HandoverRecords)
            .HasForeignKey(h => h.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.Employee)
            .WithMany()
            .HasForeignKey(h => h.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.EmployeeExit)
            .WithMany()
            .HasForeignKey(h => h.EmployeeExitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.InitiatedByEmployee)
            .WithMany()
            .HasForeignKey(h => h.InitiatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.CompletedByEmployee)
            .WithMany()
            .HasForeignKey(h => h.CompletedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.ApprovedByEmployee)
            .WithMany()
            .HasForeignKey(h => h.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(h => h.AssetId);
        builder.HasIndex(h => h.EmployeeId);
        builder.HasIndex(h => h.EmployeeExitId);
        builder.HasIndex(h => h.Status);
        builder.HasIndex(h => h.InitiatedDate);
        builder.HasIndex(h => h.DueDate);
        builder.HasIndex(h => new { h.Status, h.DueDate });
        builder.HasIndex(h => new { h.AssetId, h.Status });
    }
}
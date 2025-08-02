using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class AssetMaintenanceConfiguration : IEntityTypeConfiguration<AssetMaintenance>
{
    public void Configure(EntityTypeBuilder<AssetMaintenance> builder)
    {
        builder.ToTable("AssetMaintenances");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Vendor)
            .HasMaxLength(200);

        builder.Property(m => m.Cost)
            .HasColumnType("decimal(18,2)");

        builder.Property(m => m.Currency)
            .HasMaxLength(3);

        builder.Property(m => m.WorkPerformed)
            .HasMaxLength(1000);

        builder.Property(m => m.PartsReplaced)
            .HasMaxLength(500);

        builder.Property(m => m.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(m => m.Asset)
            .WithMany(a => a.MaintenanceRecords)
            .HasForeignKey(m => m.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Technician)
            .WithMany()
            .HasForeignKey(m => m.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.RequestedByEmployee)
            .WithMany()
            .HasForeignKey(m => m.RequestedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(m => m.AssetId);
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.Type);
        builder.HasIndex(m => m.ScheduledDate);
        builder.HasIndex(m => m.TechnicianId);
        builder.HasIndex(m => m.RequestedBy);
        builder.HasIndex(m => new { m.Status, m.ScheduledDate });
    }
}
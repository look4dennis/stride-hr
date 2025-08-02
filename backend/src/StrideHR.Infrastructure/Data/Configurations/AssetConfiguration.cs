using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AssetTag)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Description)
            .HasMaxLength(1000);

        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.Brand)
            .HasMaxLength(100);

        builder.Property(a => a.Model)
            .HasMaxLength(100);

        builder.Property(a => a.SerialNumber)
            .HasMaxLength(100);

        builder.Property(a => a.PurchasePrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.PurchaseCurrency)
            .HasMaxLength(3);

        builder.Property(a => a.Vendor)
            .HasMaxLength(200);

        builder.Property(a => a.WarrantyDetails)
            .HasMaxLength(500);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.Condition)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.Location)
            .HasMaxLength(200);

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.Property(a => a.DepreciationRate)
            .HasColumnType("decimal(5,2)");

        builder.Property(a => a.CurrentValue)
            .HasColumnType("decimal(18,2)");

        // Relationships
        builder.HasOne(a => a.Branch)
            .WithMany()
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.AssetAssignments)
            .WithOne(aa => aa.Asset)
            .HasForeignKey(aa => aa.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.MaintenanceRecords)
            .WithOne(m => m.Asset)
            .HasForeignKey(m => m.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.HandoverRecords)
            .WithOne(h => h.Asset)
            .HasForeignKey(h => h.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.AssetTag)
            .IsUnique();

        builder.HasIndex(a => a.BranchId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.Type);
        builder.HasIndex(a => new { a.Brand, a.Model });
        builder.HasIndex(a => a.SerialNumber);
    }
}
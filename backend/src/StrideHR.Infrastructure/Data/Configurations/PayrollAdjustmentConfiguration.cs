using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class PayrollAdjustmentConfiguration : IEntityTypeConfiguration<PayrollAdjustment>
{
    public void Configure(EntityTypeBuilder<PayrollAdjustment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Type)
            .IsRequired();

        builder.Property(a => a.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.Amount)
            .HasPrecision(18, 2);

        builder.Property(a => a.Reason)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.AdjustedAt)
            .IsRequired();

        builder.Property(a => a.IsApproved)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(a => a.PayrollRecordId)
            .HasDatabaseName("IX_PayrollAdjustment_PayrollRecord");

        builder.HasIndex(a => a.Type)
            .HasDatabaseName("IX_PayrollAdjustment_Type");

        builder.HasIndex(a => a.IsApproved)
            .HasDatabaseName("IX_PayrollAdjustment_IsApproved");

        // Relationships
        builder.HasOne(a => a.PayrollRecord)
            .WithMany(p => p.PayrollAdjustments)
            .HasForeignKey(a => a.PayrollRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.AdjustedByEmployee)
            .WithMany()
            .HasForeignKey(a => a.AdjustedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.ApprovedByEmployee)
            .WithMany()
            .HasForeignKey(a => a.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
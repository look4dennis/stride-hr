using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Infrastructure.Data.Configurations;

public class PayslipGenerationConfiguration : IEntityTypeConfiguration<PayslipGeneration>
{
    public void Configure(EntityTypeBuilder<PayslipGeneration> builder)
    {
        builder.ToTable("PayslipGenerations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PayslipPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.PayslipFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(PayslipStatus.Generated);

        builder.Property(e => e.HRApprovalNotes)
            .HasMaxLength(1000);

        builder.Property(e => e.FinanceApprovalNotes)
            .HasMaxLength(1000);

        builder.Property(e => e.RegenerationReason)
            .HasMaxLength(500);

        builder.Property(e => e.Version)
            .HasDefaultValue(1);

        // Relationships
        builder.HasOne(e => e.PayrollRecord)
            .WithMany()
            .HasForeignKey(e => e.PayrollRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PayslipTemplate)
            .WithMany(t => t.PayslipGenerations)
            .HasForeignKey(e => e.PayslipTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.GeneratedByEmployee)
            .WithMany()
            .HasForeignKey(e => e.GeneratedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.HRApprovedByEmployee)
            .WithMany()
            .HasForeignKey(e => e.HRApprovedBy)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(e => e.FinanceApprovedByEmployee)
            .WithMany()
            .HasForeignKey(e => e.FinanceApprovedBy)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(e => e.ReleasedByEmployee)
            .WithMany()
            .HasForeignKey(e => e.ReleasedBy)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(e => e.PayrollRecordId)
            .HasDatabaseName("IX_PayslipGenerations_PayrollRecord");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_PayslipGenerations_Status");

        builder.HasIndex(e => e.GeneratedAt)
            .HasDatabaseName("IX_PayslipGenerations_GeneratedAt");

        builder.HasIndex(e => new { e.PayrollRecordId, e.Version })
            .IsUnique()
            .HasDatabaseName("IX_PayslipGenerations_PayrollRecord_Version");
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class PayslipApprovalHistoryConfiguration : IEntityTypeConfiguration<PayslipApprovalHistory>
{
    public void Configure(EntityTypeBuilder<PayslipApprovalHistory> builder)
    {
        builder.ToTable("PayslipApprovalHistories");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ApprovalLevel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Action)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.RejectionReason)
            .HasMaxLength(500);

        builder.Property(e => e.PreviousStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.NewStatus)
            .IsRequired()
            .HasConversion<int>();

        // Relationships
        builder.HasOne(e => e.PayslipGeneration)
            .WithMany(pg => pg.ApprovalHistory)
            .HasForeignKey(e => e.PayslipGenerationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ActionByEmployee)
            .WithMany()
            .HasForeignKey(e => e.ActionBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.PayslipGenerationId)
            .HasDatabaseName("IX_PayslipApprovalHistories_PayslipGeneration");

        builder.HasIndex(e => e.ActionAt)
            .HasDatabaseName("IX_PayslipApprovalHistories_ActionAt");

        builder.HasIndex(e => new { e.ApprovalLevel, e.Action })
            .HasDatabaseName("IX_PayslipApprovalHistories_Level_Action");
    }
}
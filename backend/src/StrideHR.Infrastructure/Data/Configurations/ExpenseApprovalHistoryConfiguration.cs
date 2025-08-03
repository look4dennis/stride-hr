using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ExpenseApprovalHistoryConfiguration : IEntityTypeConfiguration<ExpenseApprovalHistory>
{
    public void Configure(EntityTypeBuilder<ExpenseApprovalHistory> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ApprovalLevel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Action)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Comments)
            .HasMaxLength(1000);

        builder.Property(e => e.ApprovedAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.RejectionReason)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(e => e.ExpenseClaim)
            .WithMany(ec => ec.ApprovalHistory)
            .HasForeignKey(e => e.ExpenseClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Approver)
            .WithMany()
            .HasForeignKey(e => e.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
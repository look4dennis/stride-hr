using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ExpenseClaimConfiguration : IEntityTypeConfiguration<ExpenseClaim>
{
    public void Configure(EntityTypeBuilder<ExpenseClaim> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ClaimNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.ClaimNumber)
            .IsUnique();

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.RejectionReason)
            .HasMaxLength(500);

        builder.Property(e => e.AdvanceAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.ReimbursementReference)
            .HasMaxLength(100);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ApprovedByEmployee)
            .WithMany()
            .HasForeignKey(e => e.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ExpenseItems)
            .WithOne(ei => ei.ExpenseClaim)
            .HasForeignKey(ei => ei.ExpenseClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ApprovalHistory)
            .WithOne(ah => ah.ExpenseClaim)
            .HasForeignKey(ah => ah.ExpenseClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Documents)
            .WithOne(d => d.ExpenseClaim)
            .HasForeignKey(d => d.ExpenseClaimId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
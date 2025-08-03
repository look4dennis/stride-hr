using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ExpenseItemConfiguration : IEntityTypeConfiguration<ExpenseItem>
{
    public void Configure(EntityTypeBuilder<ExpenseItem> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.Vendor)
            .HasMaxLength(200);

        builder.Property(e => e.Location)
            .HasMaxLength(200);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        builder.Property(e => e.MileageDistance)
            .HasPrecision(18, 2);

        builder.Property(e => e.MileageRate)
            .HasPrecision(18, 2);

        // Relationships
        builder.HasOne(e => e.ExpenseClaim)
            .WithMany(ec => ec.ExpenseItems)
            .HasForeignKey(e => e.ExpenseClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ExpenseCategory)
            .WithMany(ec => ec.ExpenseItems)
            .HasForeignKey(e => e.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Documents)
            .WithOne(d => d.ExpenseItem)
            .HasForeignKey(d => d.ExpenseItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
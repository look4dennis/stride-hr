using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(e => new { e.Code, e.OrganizationId })
            .IsUnique();

        builder.Property(e => e.MaxAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.DailyLimit)
            .HasPrecision(18, 2);

        builder.Property(e => e.MonthlyLimit)
            .HasPrecision(18, 2);

        builder.Property(e => e.MileageRate)
            .HasPrecision(18, 2);

        builder.Property(e => e.PolicyDescription)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(e => e.Organization)
            .WithMany()
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ExpenseItems)
            .WithOne(ei => ei.ExpenseCategory)
            .HasForeignKey(ei => ei.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.PolicyRules)
            .WithOne(pr => pr.ExpenseCategory)
            .HasForeignKey(pr => pr.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
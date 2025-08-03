using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ExpensePolicyRuleConfiguration : IEntityTypeConfiguration<ExpensePolicyRule>
{
    public void Configure(EntityTypeBuilder<ExpensePolicyRule> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.RuleName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.RuleDescription)
            .HasMaxLength(500);

        builder.Property(e => e.RuleType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.RuleCondition)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.MaxAmount)
            .HasPrecision(18, 2);

        // Relationships
        builder.HasOne(e => e.ExpenseCategory)
            .WithMany(ec => ec.PolicyRules)
            .HasForeignKey(e => e.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class PayrollFormulaConfiguration : IEntityTypeConfiguration<PayrollFormula>
{
    public void Configure(EntityTypeBuilder<PayrollFormula> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.Description)
            .HasMaxLength(500);

        builder.Property(f => f.Type)
            .IsRequired();

        builder.Property(f => f.Formula)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(f => f.Variables)
            .HasColumnType("json")
            .HasDefaultValue("[]");

        builder.Property(f => f.IsActive)
            .HasDefaultValue(true);

        builder.Property(f => f.Priority)
            .HasDefaultValue(0);

        builder.Property(f => f.Conditions)
            .HasColumnType("json");

        builder.Property(f => f.Department)
            .HasMaxLength(100);

        builder.Property(f => f.Designation)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(f => f.Name)
            .IsUnique()
            .HasDatabaseName("IX_PayrollFormula_Name");

        builder.HasIndex(f => f.Type)
            .HasDatabaseName("IX_PayrollFormula_Type");

        builder.HasIndex(f => f.IsActive)
            .HasDatabaseName("IX_PayrollFormula_IsActive");

        builder.HasIndex(f => f.Priority)
            .HasDatabaseName("IX_PayrollFormula_Priority");

        // Relationships
        builder.HasOne(f => f.Organization)
            .WithMany()
            .HasForeignKey(f => f.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Branch)
            .WithMany()
            .HasForeignKey(f => f.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
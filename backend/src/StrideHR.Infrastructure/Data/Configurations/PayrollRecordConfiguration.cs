using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class PayrollRecordConfiguration : IEntityTypeConfiguration<PayrollRecord>
{
    public void Configure(EntityTypeBuilder<PayrollRecord> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PayrollPeriodStart)
            .IsRequired();

        builder.Property(p => p.PayrollPeriodEnd)
            .IsRequired();

        builder.Property(p => p.PayrollMonth)
            .IsRequired();

        builder.Property(p => p.PayrollYear)
            .IsRequired();

        // Salary components with precision
        builder.Property(p => p.BasicSalary)
            .HasPrecision(18, 2);

        builder.Property(p => p.GrossSalary)
            .HasPrecision(18, 2);

        builder.Property(p => p.NetSalary)
            .HasPrecision(18, 2);

        // Allowances
        builder.Property(p => p.HouseRentAllowance)
            .HasPrecision(18, 2);

        builder.Property(p => p.TransportAllowance)
            .HasPrecision(18, 2);

        builder.Property(p => p.MedicalAllowance)
            .HasPrecision(18, 2);

        builder.Property(p => p.FoodAllowance)
            .HasPrecision(18, 2);

        builder.Property(p => p.OtherAllowances)
            .HasPrecision(18, 2);

        builder.Property(p => p.TotalAllowances)
            .HasPrecision(18, 2);

        // Overtime
        builder.Property(p => p.OvertimeHours)
            .HasPrecision(8, 2);

        builder.Property(p => p.OvertimeRate)
            .HasPrecision(8, 2);

        builder.Property(p => p.OvertimeAmount)
            .HasPrecision(18, 2);

        // Deductions
        builder.Property(p => p.TaxDeduction)
            .HasPrecision(18, 2);

        builder.Property(p => p.ProvidentFund)
            .HasPrecision(18, 2);

        builder.Property(p => p.EmployeeStateInsurance)
            .HasPrecision(18, 2);

        builder.Property(p => p.ProfessionalTax)
            .HasPrecision(18, 2);

        builder.Property(p => p.LoanDeduction)
            .HasPrecision(18, 2);

        builder.Property(p => p.AdvanceDeduction)
            .HasPrecision(18, 2);

        builder.Property(p => p.OtherDeductions)
            .HasPrecision(18, 2);

        builder.Property(p => p.TotalDeductions)
            .HasPrecision(18, 2);

        builder.Property(p => p.LeaveDeduction)
            .HasPrecision(18, 2);

        // Currency and exchange rate
        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(p => p.ExchangeRate)
            .HasPrecision(18, 6);

        builder.Property(p => p.BaseCurrency)
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        // JSON fields
        builder.Property(p => p.CustomCalculations)
            .HasColumnType("json")
            .HasDefaultValue("{}");

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);

        builder.Property(p => p.PayslipPath)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(p => new { p.EmployeeId, p.PayrollYear, p.PayrollMonth })
            .IsUnique()
            .HasDatabaseName("IX_PayrollRecord_Employee_Period");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_PayrollRecord_Status");

        // Relationships
        builder.HasOne(p => p.Employee)
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ApprovedByEmployee)
            .WithMany()
            .HasForeignKey(p => p.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ProcessedByEmployee)
            .WithMany()
            .HasForeignKey(p => p.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.PayrollAdjustments)
            .WithOne(pa => pa.PayrollRecord)
            .HasForeignKey(pa => pa.PayrollRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
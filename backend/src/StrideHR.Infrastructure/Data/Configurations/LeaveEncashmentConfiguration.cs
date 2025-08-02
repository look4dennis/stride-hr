using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class LeaveEncashmentConfiguration : IEntityTypeConfiguration<LeaveEncashment>
{
    public void Configure(EntityTypeBuilder<LeaveEncashment> builder)
    {
        builder.ToTable("LeaveEncashments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EmployeeId)
            .IsRequired();

        builder.Property(e => e.LeavePolicyId)
            .IsRequired();

        builder.Property(e => e.Year)
            .IsRequired();

        builder.Property(e => e.EncashedDays)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.EncashmentRate)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.EncashmentAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.EncashmentDate)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.Reason)
            .HasMaxLength(1000);

        builder.Property(e => e.Comments)
            .HasMaxLength(2000);

        // Configure relationships with explicit foreign keys
        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.LeavePolicy)
            .WithMany()
            .HasForeignKey(e => e.LeavePolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ApprovedByEmployee)
            .WithMany()
            .HasForeignKey(e => e.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => new { e.EmployeeId, e.Year, e.LeavePolicyId })
            .HasDatabaseName("IX_LeaveEncashments_Employee_Year_Policy");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_LeaveEncashments_Status");
    }
}
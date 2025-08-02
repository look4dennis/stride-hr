using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.HasKey(lb => lb.Id);
        
        builder.Property(lb => lb.Year)
            .IsRequired();
            
        builder.Property(lb => lb.AllocatedDays)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
            
        builder.Property(lb => lb.UsedDays)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
            
        builder.Property(lb => lb.CarriedForwardDays)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
            
        builder.Property(lb => lb.EncashedDays)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
            
        // Computed column for RemainingDays
        builder.Property(lb => lb.RemainingDays)
            .HasComputedColumnSql("[AllocatedDays] + [CarriedForwardDays] - [UsedDays] - [EncashedDays]");
            
        // Relationships
        builder.HasOne(lb => lb.Employee)
            .WithMany(e => e.LeaveBalances)
            .HasForeignKey(lb => lb.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(lb => lb.LeavePolicy)
            .WithMany(lp => lp.LeaveBalances)
            .HasForeignKey(lb => lb.LeavePolicyId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Indexes
        builder.HasIndex(lb => new { lb.EmployeeId, lb.LeavePolicyId, lb.Year })
            .IsUnique();
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class LeavePolicyConfiguration : IEntityTypeConfiguration<LeavePolicy>
{
    public void Configure(EntityTypeBuilder<LeavePolicy> builder)
    {
        builder.HasKey(lp => lp.Id);
        
        builder.Property(lp => lp.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(lp => lp.Description)
            .HasMaxLength(500);
            
        builder.Property(lp => lp.LeaveType)
            .IsRequired()
            .HasConversion<int>();
            
        builder.Property(lp => lp.AnnualAllocation)
            .IsRequired();
            
        builder.Property(lp => lp.MaxConsecutiveDays)
            .IsRequired();
            
        builder.Property(lp => lp.MinAdvanceNoticeDays)
            .IsRequired();
            
        builder.Property(lp => lp.EncashmentRate)
            .HasColumnType("decimal(18,2)");
            
        // Relationships
        builder.HasOne(lp => lp.Branch)
            .WithMany(b => b.LeavePolicies)
            .HasForeignKey(lp => lp.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Indexes
        builder.HasIndex(lp => new { lp.BranchId, lp.LeaveType })
            .IsUnique();
    }
}
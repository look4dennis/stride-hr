using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.HasKey(lr => lr.Id);
        
        builder.Property(lr => lr.StartDate)
            .IsRequired()
            .HasColumnType("date");
            
        builder.Property(lr => lr.EndDate)
            .IsRequired()
            .HasColumnType("date");
            
        builder.Property(lr => lr.RequestedDays)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
            
        builder.Property(lr => lr.ApprovedDays)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
            
        builder.Property(lr => lr.Reason)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(lr => lr.Comments)
            .HasMaxLength(1000);
            
        builder.Property(lr => lr.Status)
            .IsRequired()
            .HasConversion<int>();
            
        builder.Property(lr => lr.RejectionReason)
            .HasMaxLength(1000);
            
        builder.Property(lr => lr.AttachmentPath)
            .HasMaxLength(500);
            
        // Relationships
        builder.HasOne(lr => lr.Employee)
            .WithMany(e => e.LeaveRequests)
            .HasForeignKey(lr => lr.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(lr => lr.LeavePolicy)
            .WithMany(lp => lp.LeaveRequests)
            .HasForeignKey(lr => lr.LeavePolicyId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(lr => lr.ApprovedByEmployee)
            .WithMany(e => e.ApprovedLeaveRequests)
            .HasForeignKey(lr => lr.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Indexes
        builder.HasIndex(lr => lr.EmployeeId);
        builder.HasIndex(lr => lr.Status);
        builder.HasIndex(lr => new { lr.StartDate, lr.EndDate });
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class LeaveApprovalHistoryConfiguration : IEntityTypeConfiguration<LeaveApprovalHistory>
{
    public void Configure(EntityTypeBuilder<LeaveApprovalHistory> builder)
    {
        builder.HasKey(lah => lah.Id);
        
        builder.Property(lah => lah.Level)
            .IsRequired()
            .HasConversion<int>();
            
        builder.Property(lah => lah.Action)
            .IsRequired()
            .HasConversion<int>();
            
        builder.Property(lah => lah.Comments)
            .HasMaxLength(1000);
            
        builder.Property(lah => lah.ActionDate)
            .IsRequired();
            
        // Relationships
        builder.HasOne(lah => lah.LeaveRequest)
            .WithMany(lr => lr.ApprovalHistory)
            .HasForeignKey(lah => lah.LeaveRequestId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(lah => lah.Approver)
            .WithMany(e => e.ApprovalHistory)
            .HasForeignKey(lah => lah.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(lah => lah.EscalatedTo)
            .WithMany(e => e.EscalatedApprovals)
            .HasForeignKey(lah => lah.EscalatedToId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Indexes
        builder.HasIndex(lah => lah.LeaveRequestId);
        builder.HasIndex(lah => lah.ApproverId);
        builder.HasIndex(lah => lah.ActionDate);
    }
}
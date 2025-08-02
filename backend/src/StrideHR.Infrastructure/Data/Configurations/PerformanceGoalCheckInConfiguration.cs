using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class PerformanceGoalCheckInConfiguration : IEntityTypeConfiguration<PerformanceGoalCheckIn>
{
    public void Configure(EntityTypeBuilder<PerformanceGoalCheckIn> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.ProgressPercentage)
            .HasPrecision(5, 2);
            
        builder.Property(c => c.EmployeeComments)
            .IsRequired()
            .HasMaxLength(1000);
            
        builder.Property(c => c.ManagerComments)
            .HasMaxLength(1000);
            
        builder.Property(c => c.Challenges)
            .HasMaxLength(1000);
            
        builder.Property(c => c.SupportNeeded)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(c => c.PerformanceGoal)
            .WithMany(g => g.CheckIns)
            .HasForeignKey(c => c.PerformanceGoalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Employee)
            .WithMany()
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Manager)
            .WithMany()
            .HasForeignKey(c => c.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
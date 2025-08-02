using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class PerformanceGoalConfiguration : IEntityTypeConfiguration<PerformanceGoal>
{
    public void Configure(EntityTypeBuilder<PerformanceGoal> builder)
    {
        builder.HasKey(g => g.Id);
        
        builder.Property(g => g.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(g => g.Description)
            .IsRequired()
            .HasMaxLength(1000);
            
        builder.Property(g => g.SuccessCriteria)
            .IsRequired()
            .HasMaxLength(1000);
            
        builder.Property(g => g.WeightPercentage)
            .IsRequired();
            
        builder.Property(g => g.ProgressPercentage)
            .HasPrecision(5, 2);
            
        builder.Property(g => g.Notes)
            .HasMaxLength(1000);
            
        builder.Property(g => g.ManagerComments)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(g => g.Employee)
            .WithMany()
            .HasForeignKey(g => g.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.Manager)
            .WithMany()
            .HasForeignKey(g => g.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(g => g.CheckIns)
            .WithOne(c => c.PerformanceGoal)
            .HasForeignKey(c => c.PerformanceGoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
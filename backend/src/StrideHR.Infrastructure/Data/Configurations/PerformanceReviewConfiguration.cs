using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class PerformanceReviewConfiguration : IEntityTypeConfiguration<PerformanceReview>
{
    public void Configure(EntityTypeBuilder<PerformanceReview> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.ReviewPeriod)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(r => r.OverallScore)
            .HasPrecision(5, 2);
            
        builder.Property(r => r.EmployeeSelfAssessment)
            .HasMaxLength(2000);
            
        builder.Property(r => r.ManagerComments)
            .HasMaxLength(2000);
            
        builder.Property(r => r.DevelopmentPlan)
            .HasMaxLength(2000);
            
        builder.Property(r => r.StrengthsIdentified)
            .HasMaxLength(1000);
            
        builder.Property(r => r.AreasForImprovement)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(r => r.Employee)
            .WithMany()
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Manager)
            .WithMany()
            .HasForeignKey(r => r.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ApprovedByEmployee)
            .WithMany()
            .HasForeignKey(r => r.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Feedbacks)
            .WithOne(f => f.PerformanceReview)
            .HasForeignKey(f => f.PerformanceReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Goals)
            .WithMany()
            .UsingEntity(j => j.ToTable("PerformanceReviewGoals"));
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class PerformanceImprovementPlanConfiguration : IEntityTypeConfiguration<PerformanceImprovementPlan>
{
    public void Configure(EntityTypeBuilder<PerformanceImprovementPlan> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(2000);
            
        builder.Property(p => p.PerformanceIssues)
            .IsRequired()
            .HasMaxLength(2000);
            
        builder.Property(p => p.ExpectedImprovements)
            .IsRequired()
            .HasMaxLength(2000);
            
        builder.Property(p => p.SupportProvided)
            .IsRequired()
            .HasMaxLength(2000);
            
        builder.Property(p => p.FinalOutcome)
            .HasMaxLength(2000);
            
        builder.Property(p => p.ManagerNotes)
            .HasMaxLength(2000);
            
        builder.Property(p => p.HRNotes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(p => p.Employee)
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Manager)
            .WithMany()
            .HasForeignKey(p => p.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.HR)
            .WithMany()
            .HasForeignKey(p => p.HRId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.PerformanceReview)
            .WithMany()
            .HasForeignKey(p => p.PerformanceReviewId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Goals)
            .WithOne(g => g.PIP)
            .HasForeignKey(g => g.PIPId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Reviews)
            .WithOne(r => r.PIP)
            .HasForeignKey(r => r.PIPId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
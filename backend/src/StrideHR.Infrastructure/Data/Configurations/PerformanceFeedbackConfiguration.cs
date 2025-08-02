using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class PerformanceFeedbackConfiguration : IEntityTypeConfiguration<PerformanceFeedback>
{
    public void Configure(EntityTypeBuilder<PerformanceFeedback> builder)
    {
        builder.HasKey(f => f.Id);
        
        builder.Property(f => f.CompetencyArea)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(f => f.Comments)
            .IsRequired()
            .HasMaxLength(2000);
            
        builder.Property(f => f.Strengths)
            .HasMaxLength(1000);
            
        builder.Property(f => f.AreasForImprovement)
            .HasMaxLength(1000);
            
        builder.Property(f => f.SpecificExamples)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(f => f.PerformanceReview)
            .WithMany(r => r.Feedbacks)
            .HasForeignKey(f => f.PerformanceReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Reviewee)
            .WithMany()
            .HasForeignKey(f => f.RevieweeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Reviewer)
            .WithMany()
            .HasForeignKey(f => f.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
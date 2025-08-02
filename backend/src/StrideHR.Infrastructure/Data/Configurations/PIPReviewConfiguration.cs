using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class PIPReviewConfiguration : IEntityTypeConfiguration<PIPReview>
{
    public void Configure(EntityTypeBuilder<PIPReview> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.ProgressSummary)
            .IsRequired()
            .HasMaxLength(2000);
            
        builder.Property(r => r.EmployeeFeedback)
            .IsRequired()
            .HasMaxLength(2000);
            
        builder.Property(r => r.ManagerFeedback)
            .IsRequired()
            .HasMaxLength(2000);
            
        builder.Property(r => r.ChallengesFaced)
            .HasMaxLength(1000);
            
        builder.Property(r => r.SupportProvided)
            .HasMaxLength(1000);
            
        builder.Property(r => r.NextSteps)
            .HasMaxLength(1000);
            
        builder.Property(r => r.RecommendedActions)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(r => r.PIP)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.PIPId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ReviewedByEmployee)
            .WithMany()
            .HasForeignKey(r => r.ReviewedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
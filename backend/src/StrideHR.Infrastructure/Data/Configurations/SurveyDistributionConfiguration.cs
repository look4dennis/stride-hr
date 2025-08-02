using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class SurveyDistributionConfiguration : IEntityTypeConfiguration<SurveyDistribution>
{
    public void Configure(EntityTypeBuilder<SurveyDistribution> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.TargetRole)
            .HasMaxLength(100);

        builder.Property(d => d.TargetCriteria)
            .HasMaxLength(2000); // JSON for complex targeting

        builder.Property(d => d.InvitationMessage)
            .HasMaxLength(1000);

        builder.Property(d => d.AccessToken)
            .HasMaxLength(200);

        // Relationships
        builder.HasOne(d => d.Survey)
            .WithMany(s => s.Distributions)
            .HasForeignKey(d => d.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.TargetEmployee)
            .WithMany()
            .HasForeignKey(d => d.TargetEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.TargetBranch)
            .WithMany()
            .HasForeignKey(d => d.TargetBranchId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(d => d.SurveyId);
        builder.HasIndex(d => d.TargetEmployeeId);
        builder.HasIndex(d => d.TargetBranchId);
        builder.HasIndex(d => d.TargetRole);
        builder.HasIndex(d => d.SentAt);
        builder.HasIndex(d => d.CompletedAt);
        builder.HasIndex(d => d.IsActive);
    }
}
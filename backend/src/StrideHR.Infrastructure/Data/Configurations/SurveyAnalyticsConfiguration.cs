using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class SurveyAnalyticsConfiguration : IEntityTypeConfiguration<SurveyAnalytics>
{
    public void Configure(EntityTypeBuilder<SurveyAnalytics> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.MetricType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.MetricValue)
            .IsRequired()
            .HasMaxLength(4000); // JSON data

        builder.Property(a => a.Segment)
            .HasMaxLength(100);

        builder.Property(a => a.SegmentValue)
            .HasMaxLength(200);

        builder.Property(a => a.SentimentScore)
            .HasConversion<int>();

        builder.Property(a => a.Keywords)
            .HasMaxLength(2000); // JSON array

        builder.Property(a => a.Themes)
            .HasMaxLength(2000); // JSON array

        // Relationships
        builder.HasOne(a => a.Survey)
            .WithMany(s => s.Analytics)
            .HasForeignKey(a => a.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(a => a.SurveyId);
        builder.HasIndex(a => a.QuestionId);
        builder.HasIndex(a => a.MetricType);
        builder.HasIndex(a => a.Segment);
        builder.HasIndex(a => a.CalculatedAt);
        builder.HasIndex(a => a.SentimentScore);

        // Composite index for efficient querying
        builder.HasIndex(a => new { a.SurveyId, a.MetricType, a.Segment });
    }
}
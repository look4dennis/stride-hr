using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class SurveyAnswerConfiguration : IEntityTypeConfiguration<SurveyAnswer>
{
    public void Configure(EntityTypeBuilder<SurveyAnswer> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.TextAnswer)
            .HasMaxLength(4000);

        builder.Property(a => a.OtherAnswer)
            .HasMaxLength(1000);

        builder.Property(a => a.MultipleSelections)
            .HasMaxLength(2000); // JSON array

        // Relationships
        builder.HasOne(a => a.Response)
            .WithMany(r => r.Answers)
            .HasForeignKey(a => a.ResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.SelectedOption)
            .WithMany(o => o.Answers)
            .HasForeignKey(a => a.SelectedOptionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(a => a.ResponseId);
        builder.HasIndex(a => a.QuestionId);
        builder.HasIndex(a => a.SelectedOptionId);
        builder.HasIndex(a => a.IsSkipped);

        // Unique constraint to prevent duplicate answers for the same question in a response
        builder.HasIndex(a => new { a.ResponseId, a.QuestionId })
            .IsUnique();
    }
}
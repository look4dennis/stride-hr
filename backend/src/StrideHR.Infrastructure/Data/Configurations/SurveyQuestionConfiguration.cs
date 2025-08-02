using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class SurveyQuestionConfiguration : IEntityTypeConfiguration<SurveyQuestion>
{
    public void Configure(EntityTypeBuilder<SurveyQuestion> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.QuestionText)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(q => q.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(q => q.HelpText)
            .HasMaxLength(500);

        builder.Property(q => q.ValidationRules)
            .HasMaxLength(1000);

        builder.Property(q => q.PlaceholderText)
            .HasMaxLength(200);

        builder.Property(q => q.ConditionalLogic)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(q => q.Survey)
            .WithMany(s => s.Questions)
            .HasForeignKey(q => q.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(q => q.SurveyId);
        builder.HasIndex(q => q.OrderIndex);
        builder.HasIndex(q => q.Type);
        builder.HasIndex(q => q.IsActive);
    }
}
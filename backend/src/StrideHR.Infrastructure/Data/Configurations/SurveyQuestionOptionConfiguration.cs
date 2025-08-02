using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class SurveyQuestionOptionConfiguration : IEntityTypeConfiguration<SurveyQuestionOption>
{
    public void Configure(EntityTypeBuilder<SurveyQuestionOption> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OptionText)
            .IsRequired()
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Answers)
            .WithOne(a => a.SelectedOption)
            .HasForeignKey(a => a.SelectedOptionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(o => o.QuestionId);
        builder.HasIndex(o => o.OrderIndex);
        builder.HasIndex(o => o.IsActive);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.ToTable("Assessments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TrainingModuleId)
            .IsRequired();

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Description)
            .HasMaxLength(1000);

        builder.Property(a => a.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.TimeLimit)
            .IsRequired();

        builder.Property(a => a.PassingScore)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(a => a.MaxAttempts)
            .HasDefaultValue(3);

        builder.Property(a => a.RetakeWaitingPeriodHours)
            .HasDefaultValue(24);

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true);

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(a => a.TrainingModule)
            .WithMany(tm => tm.Assessments)
            .HasForeignKey(a => a.TrainingModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.CreatedByEmployee)
            .WithMany()
            .HasForeignKey(a => a.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Questions)
            .WithOne(q => q.Assessment)
            .HasForeignKey(q => q.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Attempts)
            .WithOne(att => att.Assessment)
            .HasForeignKey(att => att.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.TrainingModuleId);
        builder.HasIndex(a => a.Type);
        builder.HasIndex(a => a.IsActive);
        builder.HasIndex(a => a.CreatedBy);
        builder.HasIndex(a => a.CreatedAt);
    }
}

public class AssessmentQuestionConfiguration : IEntityTypeConfiguration<AssessmentQuestion>
{
    public void Configure(EntityTypeBuilder<AssessmentQuestion> builder)
    {
        builder.ToTable("AssessmentQuestions");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.AssessmentId)
            .IsRequired();

        builder.Property(q => q.QuestionText)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(q => q.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(q => q.Options)
            .HasConversion(
                v => string.Join("|", v),
                v => v.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasColumnType("TEXT");

        builder.Property(q => q.CorrectAnswers)
            .HasConversion(
                v => string.Join("|", v),
                v => v.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasColumnType("TEXT");

        builder.Property(q => q.Points)
            .HasPrecision(5, 2)
            .HasDefaultValue(1);

        builder.Property(q => q.IsActive)
            .HasDefaultValue(true);

        builder.Property(q => q.Explanation)
            .HasColumnType("TEXT");

        // Relationships
        builder.HasOne(q => q.Assessment)
            .WithMany(a => a.Questions)
            .HasForeignKey(q => q.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(q => q.Answers)
            .WithOne(ans => ans.AssessmentQuestion)
            .HasForeignKey(ans => ans.AssessmentQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(q => q.AssessmentId);
        builder.HasIndex(q => q.Type);
        builder.HasIndex(q => q.OrderIndex);
        builder.HasIndex(q => q.IsActive);
    }
}
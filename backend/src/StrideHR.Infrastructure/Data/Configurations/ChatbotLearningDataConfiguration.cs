using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ChatbotLearningDataConfiguration : IEntityTypeConfiguration<ChatbotLearningData>
{
    public void Configure(EntityTypeBuilder<ChatbotLearningData> builder)
    {
        builder.HasKey(ld => ld.Id);

        builder.Property(ld => ld.UserInput)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(ld => ld.BotResponse)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(ld => ld.Intent)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ld => ld.ConfidenceScore)
            .HasPrecision(5, 4);

        builder.Property(ld => ld.UserFeedback)
            .HasMaxLength(1000);

        builder.Property(ld => ld.CorrectResponse)
            .HasMaxLength(2000);

        builder.Property(ld => ld.SessionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(ld => ld.Intent);

        builder.HasIndex(ld => ld.EmployeeId);

        builder.HasIndex(ld => ld.SessionId);

        builder.HasIndex(ld => ld.InteractionDate);

        builder.HasIndex(ld => ld.IsTrainingData);

        builder.HasIndex(ld => ld.WasHelpful);

        // Relationships
        builder.HasOne(ld => ld.Employee)
            .WithMany()
            .HasForeignKey(ld => ld.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
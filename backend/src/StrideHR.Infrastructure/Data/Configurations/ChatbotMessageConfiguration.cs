using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ChatbotMessageConfiguration : IEntityTypeConfiguration<ChatbotMessage>
{
    public void Configure(EntityTypeBuilder<ChatbotMessage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.Intent)
            .HasMaxLength(100);

        builder.Property(m => m.Entities)
            .HasColumnType("json");

        builder.Property(m => m.ActionType)
            .HasMaxLength(100);

        builder.Property(m => m.ActionData)
            .HasColumnType("json");

        builder.Property(m => m.ConfidenceScore)
            .HasPrecision(5, 4);

        builder.HasIndex(m => m.ConversationId);

        builder.HasIndex(m => m.Timestamp);

        builder.HasIndex(m => m.Intent);

        builder.HasIndex(m => m.Sender);

        // Relationships
        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ChatbotConversationConfiguration : IEntityTypeConfiguration<ChatbotConversation>
{
    public void Configure(EntityTypeBuilder<ChatbotConversation> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.SessionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Topic)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.EscalationReason)
            .HasMaxLength(500);

        builder.Property(c => c.FeedbackComments)
            .HasMaxLength(1000);

        builder.HasIndex(c => c.SessionId)
            .IsUnique();

        builder.HasIndex(c => c.EmployeeId);

        builder.HasIndex(c => c.Status);

        builder.HasIndex(c => c.StartedAt);

        // Relationships
        builder.HasOne(c => c.Employee)
            .WithMany()
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.EscalatedToEmployee)
            .WithMany()
            .HasForeignKey(c => c.EscalatedToEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
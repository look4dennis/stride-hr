using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ChatbotKnowledgeBaseFeedbackConfiguration : IEntityTypeConfiguration<ChatbotKnowledgeBaseFeedback>
{
    public void Configure(EntityTypeBuilder<ChatbotKnowledgeBaseFeedback> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Comments)
            .HasMaxLength(1000);

        builder.HasIndex(f => f.KnowledgeBaseId);

        builder.HasIndex(f => f.EmployeeId);

        builder.HasIndex(f => f.ProvidedAt);

        // Relationships
        builder.HasOne(f => f.KnowledgeBase)
            .WithMany(kb => kb.Feedback)
            .HasForeignKey(f => f.KnowledgeBaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Employee)
            .WithMany()
            .HasForeignKey(f => f.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
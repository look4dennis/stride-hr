using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ChatbotKnowledgeBaseConfiguration : IEntityTypeConfiguration<ChatbotKnowledgeBase>
{
    public void Configure(EntityTypeBuilder<ChatbotKnowledgeBase> builder)
    {
        builder.HasKey(kb => kb.Id);

        builder.Property(kb => kb.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(kb => kb.Content)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(kb => kb.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(kb => kb.Keywords)
            .HasColumnType("json");

        builder.Property(kb => kb.Tags)
            .HasColumnType("json");

        builder.Property(kb => kb.RelatedArticleIds)
            .HasColumnType("json");

        builder.HasIndex(kb => kb.Category);

        builder.HasIndex(kb => kb.Status);

        builder.HasIndex(kb => kb.Priority);

        builder.HasIndex(kb => kb.LastUpdated);

        // Relationships
        builder.HasOne(kb => kb.UpdatedByEmployee)
            .WithMany()
            .HasForeignKey(kb => kb.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(kb => kb.Feedback)
            .WithOne(f => f.KnowledgeBase)
            .HasForeignKey(f => f.KnowledgeBaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class KnowledgeBaseDocumentViewConfiguration : IEntityTypeConfiguration<KnowledgeBaseDocumentView>
{
    public void Configure(EntityTypeBuilder<KnowledgeBaseDocumentView> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(v => v.UserAgent)
            .HasMaxLength(500);

        builder.HasIndex(v => v.DocumentId);
        builder.HasIndex(v => v.ViewedBy);
        builder.HasIndex(v => v.ViewedAt);
        builder.HasIndex(v => v.IsUniqueView);

        // Relationships
        builder.HasOne(v => v.Document)
            .WithMany(d => d.Views)
            .HasForeignKey(v => v.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.ViewedByEmployee)
            .WithMany()
            .HasForeignKey(v => v.ViewedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
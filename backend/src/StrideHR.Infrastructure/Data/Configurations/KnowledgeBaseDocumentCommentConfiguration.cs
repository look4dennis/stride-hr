using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class KnowledgeBaseDocumentCommentConfiguration : IEntityTypeConfiguration<KnowledgeBaseDocumentComment>
{
    public void Configure(EntityTypeBuilder<KnowledgeBaseDocumentComment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasIndex(c => c.DocumentId);
        builder.HasIndex(c => c.AuthorId);
        builder.HasIndex(c => c.ParentCommentId);
        builder.HasIndex(c => c.PostedAt);
        builder.HasIndex(c => c.IsInternal);
        builder.HasIndex(c => c.IsDeleted);

        // Relationships
        builder.HasOne(c => c.Document)
            .WithMany(d => d.Comments)
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
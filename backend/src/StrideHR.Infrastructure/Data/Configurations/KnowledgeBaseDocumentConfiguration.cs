using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class KnowledgeBaseDocumentConfiguration : IEntityTypeConfiguration<KnowledgeBaseDocument>
{
    public void Configure(EntityTypeBuilder<KnowledgeBaseDocument> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Content)
            .IsRequired()
            .HasColumnType("LONGTEXT");

        builder.Property(d => d.Summary)
            .HasMaxLength(500);

        builder.Property(d => d.Tags)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .HasMaxLength(1000);

        builder.Property(d => d.Keywords)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .HasMaxLength(1000);

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(d => d.ReviewComments)
            .HasMaxLength(1000);

        builder.Property(d => d.MetaDescription)
            .HasMaxLength(300);

        builder.Property(d => d.ThumbnailUrl)
            .HasMaxLength(500);

        builder.HasIndex(d => d.Title);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.AuthorId);
        builder.HasIndex(d => d.CategoryId);
        builder.HasIndex(d => d.CreatedAt);
        builder.HasIndex(d => d.UpdatedAt);
        builder.HasIndex(d => d.PublishedAt);
        builder.HasIndex(d => d.ExpiryDate);
        builder.HasIndex(d => d.IsCurrentVersion);

        // Relationships
        builder.HasOne(d => d.Category)
            .WithMany(c => c.Documents)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Author)
            .WithMany()
            .HasForeignKey(d => d.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Reviewer)
            .WithMany()
            .HasForeignKey(d => d.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.ParentDocument)
            .WithMany(d => d.ChildVersions)
            .HasForeignKey(d => d.ParentDocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Approvals)
            .WithOne(a => a.Document)
            .HasForeignKey(a => a.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Attachments)
            .WithOne(a => a.Document)
            .HasForeignKey(a => a.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Views)
            .WithOne(v => v.Document)
            .HasForeignKey(v => v.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Comments)
            .WithOne(c => c.Document)
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
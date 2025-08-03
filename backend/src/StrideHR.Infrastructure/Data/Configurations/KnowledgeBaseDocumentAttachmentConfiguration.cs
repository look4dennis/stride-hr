using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class KnowledgeBaseDocumentAttachmentConfiguration : IEntityTypeConfiguration<KnowledgeBaseDocumentAttachment>
{
    public void Configure(EntityTypeBuilder<KnowledgeBaseDocumentAttachment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.HasIndex(a => a.DocumentId);
        builder.HasIndex(a => a.UploadedBy);
        builder.HasIndex(a => a.UploadedAt);

        // Relationships
        builder.HasOne(a => a.Document)
            .WithMany(d => d.Attachments)
            .HasForeignKey(a => a.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.UploadedByEmployee)
            .WithMany()
            .HasForeignKey(a => a.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
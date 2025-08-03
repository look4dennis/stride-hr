using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class KnowledgeBaseDocumentApprovalConfiguration : IEntityTypeConfiguration<KnowledgeBaseDocumentApproval>
{
    public void Configure(EntityTypeBuilder<KnowledgeBaseDocumentApproval> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.Comments)
            .HasMaxLength(1000);

        builder.Property(a => a.Level)
            .IsRequired()
            .HasConversion<string>();

        builder.HasIndex(a => a.DocumentId);
        builder.HasIndex(a => a.ApproverId);
        builder.HasIndex(a => a.ActionDate);
        builder.HasIndex(a => a.Level);

        // Relationships
        builder.HasOne(a => a.Document)
            .WithMany(d => d.Approvals)
            .HasForeignKey(a => a.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Approver)
            .WithMany()
            .HasForeignKey(a => a.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
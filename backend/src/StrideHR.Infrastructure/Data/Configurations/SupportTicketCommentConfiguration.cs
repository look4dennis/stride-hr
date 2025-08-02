using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class SupportTicketCommentConfiguration : IEntityTypeConfiguration<SupportTicketComment>
{
    public void Configure(EntityTypeBuilder<SupportTicketComment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Comment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.AttachmentPath)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.SupportTicket)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.SupportTicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Author)
            .WithMany()
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.SupportTicketId);
        builder.HasIndex(x => x.AuthorId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.IsInternal);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TicketNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(x => x.TicketNumber)
            .IsUnique();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Category)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Resolution)
            .HasMaxLength(2000);

        builder.Property(x => x.RemoteAccessDetails)
            .HasMaxLength(1000);

        builder.Property(x => x.AttachmentPath)
            .HasMaxLength(500);

        builder.Property(x => x.FeedbackComments)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Requester)
            .WithMany()
            .HasForeignKey(x => x.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssignedTo)
            .WithMany()
            .HasForeignKey(x => x.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Asset)
            .WithMany()
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Comments)
            .WithOne(x => x.SupportTicket)
            .HasForeignKey(x => x.SupportTicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StatusHistory)
            .WithOne(x => x.SupportTicket)
            .HasForeignKey(x => x.SupportTicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.RequesterId);
        builder.HasIndex(x => x.AssignedToId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.Priority);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.AssetId);
    }
}
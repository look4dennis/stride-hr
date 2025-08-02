using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class SupportTicketStatusHistoryConfiguration : IEntityTypeConfiguration<SupportTicketStatusHistory>
{
    public void Configure(EntityTypeBuilder<SupportTicketStatusHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FromStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.ToStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.ChangedAt)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(x => x.SupportTicket)
            .WithMany(x => x.StatusHistory)
            .HasForeignKey(x => x.SupportTicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ChangedBy)
            .WithMany()
            .HasForeignKey(x => x.ChangedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.SupportTicketId);
        builder.HasIndex(x => x.ChangedById);
        builder.HasIndex(x => x.ChangedAt);
    }
}
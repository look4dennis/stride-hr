using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class DocumentApprovalConfiguration : IEntityTypeConfiguration<DocumentApproval>
{
    public void Configure(EntityTypeBuilder<DocumentApproval> builder)
    {
        builder.ToTable("DocumentApprovals");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Level)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Action)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Comments)
            .HasMaxLength(1000);

        builder.Property(e => e.EscalationReason)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(e => e.GeneratedDocument)
            .WithMany(d => d.Approvals)
            .HasForeignKey(e => e.GeneratedDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Approver)
            .WithMany()
            .HasForeignKey(e => e.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.EscalatedToEmployee)
            .WithMany()
            .HasForeignKey(e => e.EscalatedTo)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.GeneratedDocumentId);
        builder.HasIndex(e => e.ApproverId);
        builder.HasIndex(e => e.Level);
        builder.HasIndex(e => e.Action);
        builder.HasIndex(e => e.ActionDate);
        builder.HasIndex(e => e.ApprovalOrder);
        builder.HasIndex(e => e.IsOverdue);
    }
}
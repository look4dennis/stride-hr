using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class GrievanceConfiguration : IEntityTypeConfiguration<Grievance>
{
    public void Configure(EntityTypeBuilder<Grievance> builder)
    {
        builder.ToTable("Grievances");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.GrievanceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(g => g.GrievanceNumber)
            .IsUnique();

        builder.Property(g => g.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(g => g.Category)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(g => g.Priority)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(g => g.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(g => g.CurrentEscalationLevel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(g => g.Resolution)
            .HasMaxLength(2000);

        builder.Property(g => g.ResolutionNotes)
            .HasMaxLength(1000);

        builder.Property(g => g.AttachmentPath)
            .HasMaxLength(500);

        builder.Property(g => g.InvestigationNotes)
            .HasMaxLength(2000);

        builder.Property(g => g.FeedbackComments)
            .HasMaxLength(1000);

        builder.Property(g => g.EscalationReason)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(g => g.SubmittedBy)
            .WithMany()
            .HasForeignKey(g => g.SubmittedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.AssignedTo)
            .WithMany()
            .HasForeignKey(g => g.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(g => g.ResolvedBy)
            .WithMany()
            .HasForeignKey(g => g.ResolvedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(g => g.EscalatedBy)
            .WithMany()
            .HasForeignKey(g => g.EscalatedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(g => g.Comments)
            .WithOne(c => c.Grievance)
            .HasForeignKey(c => c.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.StatusHistory)
            .WithOne(sh => sh.Grievance)
            .HasForeignKey(sh => sh.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.Escalations)
            .WithOne(e => e.Grievance)
            .HasForeignKey(e => e.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.FollowUps)
            .WithOne(f => f.Grievance)
            .HasForeignKey(f => f.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class GrievanceFollowUpConfiguration : IEntityTypeConfiguration<GrievanceFollowUp>
{
    public void Configure(EntityTypeBuilder<GrievanceFollowUp> builder)
    {
        builder.ToTable("GrievanceFollowUps");

        builder.HasKey(gf => gf.Id);

        builder.Property(gf => gf.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(gf => gf.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(gf => gf.CompletionNotes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(gf => gf.Grievance)
            .WithMany(g => g.FollowUps)
            .HasForeignKey(gf => gf.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gf => gf.ScheduledBy)
            .WithMany()
            .HasForeignKey(gf => gf.ScheduledById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(gf => gf.CompletedBy)
            .WithMany()
            .HasForeignKey(gf => gf.CompletedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
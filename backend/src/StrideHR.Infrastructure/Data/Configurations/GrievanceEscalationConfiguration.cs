using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class GrievanceEscalationConfiguration : IEntityTypeConfiguration<GrievanceEscalation>
{
    public void Configure(EntityTypeBuilder<GrievanceEscalation> builder)
    {
        builder.ToTable("GrievanceEscalations");

        builder.HasKey(ge => ge.Id);

        builder.Property(ge => ge.FromLevel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(ge => ge.ToLevel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(ge => ge.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ge => ge.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(ge => ge.Grievance)
            .WithMany(g => g.Escalations)
            .HasForeignKey(ge => ge.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ge => ge.EscalatedBy)
            .WithMany()
            .HasForeignKey(ge => ge.EscalatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ge => ge.EscalatedTo)
            .WithMany()
            .HasForeignKey(ge => ge.EscalatedToId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class GrievanceStatusHistoryConfiguration : IEntityTypeConfiguration<GrievanceStatusHistory>
{
    public void Configure(EntityTypeBuilder<GrievanceStatusHistory> builder)
    {
        builder.ToTable("GrievanceStatusHistories");

        builder.HasKey(gsh => gsh.Id);

        builder.Property(gsh => gsh.FromStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(gsh => gsh.ToStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(gsh => gsh.Reason)
            .HasMaxLength(500);

        builder.Property(gsh => gsh.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(gsh => gsh.Grievance)
            .WithMany(g => g.StatusHistory)
            .HasForeignKey(gsh => gsh.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gsh => gsh.ChangedBy)
            .WithMany()
            .HasForeignKey(gsh => gsh.ChangedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
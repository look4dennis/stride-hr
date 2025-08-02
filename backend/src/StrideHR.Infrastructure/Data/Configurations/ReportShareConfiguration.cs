using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ReportShareConfiguration : IEntityTypeConfiguration<ReportShare>
{
    public void Configure(EntityTypeBuilder<ReportShare> builder)
    {
        builder.ToTable("ReportShares");

        builder.HasKey(rs => rs.Id);

        builder.Property(rs => rs.Permission)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(rs => rs.SharedAt)
            .IsRequired();

        builder.Property(rs => rs.CreatedAt)
            .IsRequired();

        builder.Property(rs => rs.UpdatedAt);

        // Relationships
        builder.HasOne(rs => rs.Report)
            .WithMany(r => r.ReportShares)
            .HasForeignKey(rs => rs.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rs => rs.SharedWithEmployee)
            .WithMany()
            .HasForeignKey(rs => rs.SharedWith)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rs => rs.SharedByEmployee)
            .WithMany()
            .HasForeignKey(rs => rs.SharedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(rs => rs.ReportId);
        builder.HasIndex(rs => rs.SharedWith);
        builder.HasIndex(rs => rs.SharedBy);
        builder.HasIndex(rs => rs.IsActive);
        builder.HasIndex(rs => rs.ExpiresAt);

        // Unique constraint to prevent duplicate shares
        builder.HasIndex(rs => new { rs.ReportId, rs.SharedWith })
            .IsUnique();
    }
}
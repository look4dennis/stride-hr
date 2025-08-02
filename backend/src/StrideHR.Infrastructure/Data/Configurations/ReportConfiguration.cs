using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.DataSource)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Configuration)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(r => r.Filters)
            .HasColumnType("TEXT");

        builder.Property(r => r.Columns)
            .HasColumnType("TEXT");

        builder.Property(r => r.ChartConfiguration)
            .HasColumnType("TEXT");

        builder.Property(r => r.ScheduleCron)
            .HasMaxLength(100);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt);

        // Relationships
        builder.HasOne(r => r.CreatedByEmployee)
            .WithMany()
            .HasForeignKey(r => r.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Branch)
            .WithMany()
            .HasForeignKey(r => r.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.ReportExecutions)
            .WithOne(re => re.Report)
            .HasForeignKey(re => re.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.ReportSchedules)
            .WithOne(rs => rs.Report)
            .HasForeignKey(rs => rs.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.ReportShares)
            .WithOne(rs => rs.Report)
            .HasForeignKey(rs => rs.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.CreatedBy);
        builder.HasIndex(r => r.BranchId);
        builder.HasIndex(r => r.Type);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.IsPublic);
        builder.HasIndex(r => r.CreatedAt);
    }
}
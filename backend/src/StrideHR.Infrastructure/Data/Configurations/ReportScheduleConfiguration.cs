using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ReportScheduleConfiguration : IEntityTypeConfiguration<ReportSchedule>
{
    public void Configure(EntityTypeBuilder<ReportSchedule> builder)
    {
        builder.ToTable("ReportSchedules");

        builder.HasKey(rs => rs.Id);

        builder.Property(rs => rs.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(rs => rs.CronExpression)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rs => rs.Parameters)
            .HasColumnType("TEXT");

        builder.Property(rs => rs.Recipients)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(rs => rs.ExportFormat)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(rs => rs.EmailSubject)
            .HasMaxLength(200);

        builder.Property(rs => rs.EmailBody)
            .HasColumnType("TEXT");

        builder.Property(rs => rs.CreatedAt)
            .IsRequired();

        builder.Property(rs => rs.UpdatedAt);

        // Relationships
        builder.HasOne(rs => rs.Report)
            .WithMany(r => r.ReportSchedules)
            .HasForeignKey(rs => rs.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rs => rs.CreatedByEmployee)
            .WithMany()
            .HasForeignKey(rs => rs.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(rs => rs.ReportId);
        builder.HasIndex(rs => rs.CreatedBy);
        builder.HasIndex(rs => rs.IsActive);
        builder.HasIndex(rs => rs.NextRunTime);
    }
}
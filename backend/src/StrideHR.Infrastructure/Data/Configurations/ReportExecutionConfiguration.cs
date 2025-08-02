using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ReportExecutionConfiguration : IEntityTypeConfiguration<ReportExecution>
{
    public void Configure(EntityTypeBuilder<ReportExecution> builder)
    {
        builder.ToTable("ReportExecutions");

        builder.HasKey(re => re.Id);

        builder.Property(re => re.ExecutedAt)
            .IsRequired();

        builder.Property(re => re.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(re => re.Parameters)
            .HasColumnType("TEXT");

        builder.Property(re => re.ResultData)
            .HasColumnType("LONGTEXT");

        builder.Property(re => re.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(re => re.ExecutionTime)
            .IsRequired();

        builder.Property(re => re.ExportFormat)
            .HasMaxLength(50);

        builder.Property(re => re.ExportPath)
            .HasMaxLength(500);

        builder.Property(re => re.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(re => re.Report)
            .WithMany(r => r.ReportExecutions)
            .HasForeignKey(re => re.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(re => re.ExecutedByEmployee)
            .WithMany()
            .HasForeignKey(re => re.ExecutedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(re => re.ReportId);
        builder.HasIndex(re => re.ExecutedBy);
        builder.HasIndex(re => re.ExecutedAt);
        builder.HasIndex(re => re.Status);
    }
}
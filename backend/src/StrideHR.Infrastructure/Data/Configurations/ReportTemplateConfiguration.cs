using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ReportTemplateConfiguration : IEntityTypeConfiguration<ReportTemplate>
{
    public void Configure(EntityTypeBuilder<ReportTemplate> builder)
    {
        builder.ToTable("ReportTemplates");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(rt => rt.Description)
            .HasMaxLength(1000);

        builder.Property(rt => rt.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(rt => rt.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rt => rt.DataSource)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rt => rt.Configuration)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(rt => rt.DefaultFilters)
            .HasColumnType("TEXT");

        builder.Property(rt => rt.DefaultColumns)
            .HasColumnType("TEXT");

        builder.Property(rt => rt.DefaultChartConfiguration)
            .HasColumnType("TEXT");

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.UpdatedAt);

        // Relationships
        builder.HasOne(rt => rt.CreatedByEmployee)
            .WithMany()
            .HasForeignKey(rt => rt.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(rt => rt.Type);
        builder.HasIndex(rt => rt.Category);
        builder.HasIndex(rt => rt.IsSystemTemplate);
        builder.HasIndex(rt => rt.IsActive);
        builder.HasIndex(rt => rt.CreatedBy);
    }
}
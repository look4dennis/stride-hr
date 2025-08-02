using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class PayslipTemplateConfiguration : IEntityTypeConfiguration<PayslipTemplate>
{
    public void Configure(EntityTypeBuilder<PayslipTemplate> builder)
    {
        builder.ToTable("PayslipTemplates");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.TemplateConfig)
            .IsRequired()
            .HasColumnType("json");

        builder.Property(e => e.HeaderText)
            .HasMaxLength(200);

        builder.Property(e => e.HeaderColor)
            .HasMaxLength(7)
            .HasDefaultValue("#3b82f6");

        builder.Property(e => e.FooterText)
            .HasMaxLength(500);

        builder.Property(e => e.VisibleFields)
            .IsRequired()
            .HasColumnType("json");

        builder.Property(e => e.FieldLabels)
            .IsRequired()
            .HasColumnType("json");

        builder.Property(e => e.PrimaryColor)
            .HasMaxLength(7)
            .HasDefaultValue("#3b82f6");

        builder.Property(e => e.SecondaryColor)
            .HasMaxLength(7)
            .HasDefaultValue("#6b7280");

        builder.Property(e => e.FontFamily)
            .HasMaxLength(50)
            .HasDefaultValue("Inter");

        builder.Property(e => e.FontSize)
            .HasDefaultValue(12);

        // Relationships
        builder.HasOne(e => e.Organization)
            .WithMany()
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(e => e.CreatedByEmployee)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.LastModifiedByEmployee)
            .WithMany()
            .HasForeignKey(e => e.LastModifiedBy)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(e => new { e.OrganizationId, e.Name })
            .IsUnique()
            .HasDatabaseName("IX_PayslipTemplates_Organization_Name");

        builder.HasIndex(e => new { e.OrganizationId, e.BranchId, e.IsDefault })
            .HasDatabaseName("IX_PayslipTemplates_Default");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("IX_PayslipTemplates_Active");
    }
}
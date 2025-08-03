using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;
using System.Text.Json;

namespace StrideHR.Infrastructure.Data.Configurations;

public class DocumentTemplateConfiguration : IEntityTypeConfiguration<DocumentTemplate>
{
    public void Configure(EntityTypeBuilder<DocumentTemplate> builder)
    {
        builder.ToTable("DocumentTemplates");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Content)
            .IsRequired()
            .HasColumnType("LONGTEXT");

        builder.Property(e => e.MergeFields)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions)null!) ?? Array.Empty<string>())
            .HasColumnType("JSON");

        builder.Property(e => e.Category)
            .HasMaxLength(100);

        builder.Property(e => e.Settings)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!) ?? new Dictionary<string, object>())
            .HasColumnType("JSON");

        builder.Property(e => e.RequiredFields)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions)null!) ?? Array.Empty<string>())
            .HasColumnType("JSON");

        builder.Property(e => e.OptionalFields)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions)null!) ?? Array.Empty<string>())
            .HasColumnType("JSON");

        builder.Property(e => e.ApprovalWorkflow)
            .HasMaxLength(500);

        builder.Property(e => e.PreviewImageUrl)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(e => e.CreatedByEmployee)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.LastModifiedByEmployee)
            .WithMany()
            .HasForeignKey(e => e.LastModifiedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.GeneratedDocuments)
            .WithOne(d => d.DocumentTemplate)
            .HasForeignKey(d => d.DocumentTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Versions)
            .WithOne(v => v.DocumentTemplate)
            .HasForeignKey(v => v.DocumentTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.CreatedBy);
    }
}
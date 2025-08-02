using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;
using System.Text.Json;

namespace StrideHR.Infrastructure.Data.Configurations;

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.ToTable("EmailTemplates");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.HtmlBody)
            .IsRequired()
            .HasColumnType("LONGTEXT");

        builder.Property(e => e.TextBody)
            .HasColumnType("LONGTEXT");

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.RequiredParameters)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>())
            .HasColumnType("JSON");

        builder.Property(e => e.DefaultParameters)
            .HasConversion(
                v => v != null ? JsonSerializer.Serialize(v, (JsonSerializerOptions)null!) : null,
                v => v != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!) : null)
            .HasColumnType("JSON");

        builder.Property(e => e.PreviewData)
            .HasColumnType("TEXT");

        builder.HasIndex(e => e.Name)
            .IsUnique();

        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.IsGlobal);

        // Relationships
        builder.HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.EmailLogs)
            .WithOne(l => l.EmailTemplate)
            .HasForeignKey(l => l.EmailTemplateId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;
using System.Text.Json;

namespace StrideHR.Infrastructure.Data.Configurations;

public class GeneratedDocumentConfiguration : IEntityTypeConfiguration<GeneratedDocument>
{
    public void Configure(EntityTypeBuilder<GeneratedDocument> builder)
    {
        builder.ToTable("GeneratedDocuments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DocumentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Content)
            .IsRequired()
            .HasColumnType("LONGTEXT");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.FilePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.FileHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.MergeData)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!) ?? new Dictionary<string, object>())
            .HasColumnType("JSON");

        builder.Property(e => e.SignatureWorkflow)
            .HasMaxLength(500);

        builder.Property(e => e.SignedBy)
            .HasMaxLength(200);

        builder.Property(e => e.SignatureHash)
            .HasMaxLength(256);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(e => e.DocumentTemplate)
            .WithMany(t => t.GeneratedDocuments)
            .HasForeignKey(e => e.DocumentTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.GeneratedByEmployee)
            .WithMany()
            .HasForeignKey(e => e.GeneratedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Signatures)
            .WithOne(s => s.GeneratedDocument)
            .HasForeignKey(s => s.GeneratedDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Approvals)
            .WithOne(a => a.GeneratedDocument)
            .HasForeignKey(a => a.GeneratedDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.AuditLogs)
            .WithOne(l => l.GeneratedDocument)
            .HasForeignKey(l => l.GeneratedDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.DocumentNumber).IsUnique();
        builder.HasIndex(e => e.DocumentTemplateId);
        builder.HasIndex(e => e.EmployeeId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.GeneratedAt);
        builder.HasIndex(e => e.ExpiryDate);
        builder.HasIndex(e => e.RequiresSignature);
        builder.HasIndex(e => e.IsDigitallySigned);
    }
}
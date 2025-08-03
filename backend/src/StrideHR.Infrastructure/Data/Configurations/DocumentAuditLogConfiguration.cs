using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;
using System.Text.Json;

namespace StrideHR.Infrastructure.Data.Configurations;

public class DocumentAuditLogConfiguration : IEntityTypeConfiguration<DocumentAuditLog>
{
    public void Configure(EntityTypeBuilder<DocumentAuditLog> builder)
    {
        builder.ToTable("DocumentAuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.GeneratedDocumentId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Details)
            .HasMaxLength(2000);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.Timestamp)
            .IsRequired();

        // Configure the Metadata property as JSON
        builder.Property(x => x.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!) ?? new Dictionary<string, object>())
            .HasColumnType("json");

        // Configure relationships
        builder.HasOne(x => x.GeneratedDocument)
            .WithMany()
            .HasForeignKey(x => x.GeneratedDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.GeneratedDocumentId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Action);
        builder.HasIndex(x => x.Timestamp);
    }
}
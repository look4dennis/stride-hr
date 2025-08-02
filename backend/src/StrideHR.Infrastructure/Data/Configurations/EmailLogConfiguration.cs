using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;
using System.Text.Json;

namespace StrideHR.Infrastructure.Data.Configurations;

public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.ToTable("EmailLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ToEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.ToName)
            .HasMaxLength(255);

        builder.Property(e => e.CcEmails)
            .HasMaxLength(1000);

        builder.Property(e => e.BccEmails)
            .HasMaxLength(1000);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.HtmlBody)
            .IsRequired()
            .HasColumnType("LONGTEXT");

        builder.Property(e => e.TextBody)
            .HasColumnType("LONGTEXT");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(e => e.ExternalId)
            .HasMaxLength(100);

        builder.Property(e => e.Priority)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Metadata)
            .HasConversion(
                v => v != null ? JsonSerializer.Serialize(v, (JsonSerializerOptions)null!) : null,
                v => v != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!) : null)
            .HasColumnType("JSON");

        builder.Property(e => e.CampaignId)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(e => e.ToEmail);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ExternalId);
        builder.HasIndex(e => e.CampaignId);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.SentAt);
        builder.HasIndex(e => e.Priority);

        // Relationships
        builder.HasOne(e => e.EmailTemplate)
            .WithMany(t => t.EmailLogs)
            .HasForeignKey(e => e.EmailTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
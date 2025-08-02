using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;
using System.Text.Json;

namespace StrideHR.Infrastructure.Data.Configurations;

public class EmailCampaignConfiguration : IEntityTypeConfiguration<EmailCampaign>
{
    public void Configure(EntityTypeBuilder<EmailCampaign> builder)
    {
        builder.ToTable("EmailCampaigns");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.TargetAudience)
            .HasMaxLength(500);

        builder.Property(e => e.TargetUserIds)
            .HasConversion(
                v => v != null ? JsonSerializer.Serialize(v, (JsonSerializerOptions)null!) : null,
                v => v != null ? JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions)null!) : null)
            .HasColumnType("JSON");

        builder.Property(e => e.TargetBranchIds)
            .HasConversion(
                v => v != null ? JsonSerializer.Serialize(v, (JsonSerializerOptions)null!) : null,
                v => v != null ? JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions)null!) : null)
            .HasColumnType("JSON");

        builder.Property(e => e.TargetRoles)
            .HasConversion(
                v => v != null ? JsonSerializer.Serialize(v, (JsonSerializerOptions)null!) : null,
                v => v != null ? JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) : null)
            .HasColumnType("JSON");

        builder.Property(e => e.Parameters)
            .HasConversion(
                v => v != null ? JsonSerializer.Serialize(v, (JsonSerializerOptions)null!) : null,
                v => v != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!) : null)
            .HasColumnType("JSON");

        // Indexes
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.CreatedBy);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.ScheduledAt);

        // Relationships
        builder.HasOne(e => e.EmailTemplate)
            .WithMany()
            .HasForeignKey(e => e.EmailTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.EmailLogs)
            .WithOne()
            .HasForeignKey(l => l.CampaignId)
            .HasPrincipalKey(e => e.Id)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
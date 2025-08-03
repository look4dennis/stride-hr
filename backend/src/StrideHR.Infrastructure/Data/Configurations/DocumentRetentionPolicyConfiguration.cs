using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;
using System.Text.Json;

namespace StrideHR.Infrastructure.Data.Configurations;

public class DocumentRetentionPolicyConfiguration : IEntityTypeConfiguration<DocumentRetentionPolicy>
{
    public void Configure(EntityTypeBuilder<DocumentRetentionPolicy> builder)
    {
        builder.ToTable("DocumentRetentionPolicies");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.DocumentType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.ApprovalRoles)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions)null!) ?? Array.Empty<string>())
            .HasColumnType("JSON");

        builder.Property(e => e.LegalBasis)
            .HasMaxLength(500);

        builder.Property(e => e.ComplianceNotes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(e => e.CreatedByEmployee)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Executions)
            .WithOne(ex => ex.DocumentRetentionPolicy)
            .HasForeignKey(ex => ex.DocumentRetentionPolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.DocumentType);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.NextReviewDate);
    }
}
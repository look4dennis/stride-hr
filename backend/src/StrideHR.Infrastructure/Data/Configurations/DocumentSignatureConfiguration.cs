using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class DocumentSignatureConfiguration : IEntityTypeConfiguration<DocumentSignature>
{
    public void Configure(EntityTypeBuilder<DocumentSignature> builder)
    {
        builder.ToTable("DocumentSignatures");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SignerRole)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.SignatureType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.SignatureData)
            .IsRequired()
            .HasColumnType("LONGTEXT");

        builder.Property(e => e.SignatureHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.IpAddress)
            .IsRequired()
            .HasMaxLength(45);

        builder.Property(e => e.UserAgent)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Location)
            .HasMaxLength(200);

        builder.Property(e => e.Action)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Comments)
            .HasMaxLength(1000);

        builder.Property(e => e.InvalidationReason)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(e => e.GeneratedDocument)
            .WithMany(d => d.Signatures)
            .HasForeignKey(e => e.GeneratedDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Signer)
            .WithMany()
            .HasForeignKey(e => e.SignerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.GeneratedDocumentId);
        builder.HasIndex(e => e.SignerId);
        builder.HasIndex(e => e.SignedAt);
        builder.HasIndex(e => e.SignatureOrder);
        builder.HasIndex(e => e.IsValid);
    }
}
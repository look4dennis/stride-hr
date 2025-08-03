using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ExpenseDocumentConfiguration : IEntityTypeConfiguration<ExpenseDocument>
{
    public void Configure(EntityTypeBuilder<ExpenseDocument> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.DocumentType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(e => e.ExpenseClaim)
            .WithMany(ec => ec.Documents)
            .HasForeignKey(e => e.ExpenseClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ExpenseItem)
            .WithMany(ei => ei.Documents)
            .HasForeignKey(e => e.ExpenseItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.UploadedByEmployee)
            .WithMany()
            .HasForeignKey(e => e.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
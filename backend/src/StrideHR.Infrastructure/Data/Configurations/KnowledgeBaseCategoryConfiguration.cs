using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class KnowledgeBaseCategoryConfiguration : IEntityTypeConfiguration<KnowledgeBaseCategory>
{
    public void Configure(EntityTypeBuilder<KnowledgeBaseCategory> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.IconUrl)
            .HasMaxLength(500);

        builder.Property(c => c.Color)
            .HasMaxLength(7)
            .HasDefaultValue("#007bff");

        builder.Property(c => c.Slug)
            .HasMaxLength(150);

        builder.Property(c => c.MetaDescription)
            .HasMaxLength(300);

        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.Slug)
            .IsUnique()
            .HasFilter("Slug IS NOT NULL");
        builder.HasIndex(c => c.ParentCategoryId);
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.SortOrder);

        // Relationships
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Documents)
            .WithOne(d => d.Category)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
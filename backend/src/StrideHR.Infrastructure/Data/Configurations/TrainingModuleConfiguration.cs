using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;
using System.Text.Json;

namespace StrideHR.Infrastructure.Data.Configurations;

public class TrainingModuleConfiguration : IEntityTypeConfiguration<TrainingModule>
{
    public void Configure(EntityTypeBuilder<TrainingModule> builder)
    {
        builder.ToTable("TrainingModules");

        builder.HasKey(tm => tm.Id);

        builder.Property(tm => tm.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(tm => tm.Description)
            .HasMaxLength(1000);

        builder.Property(tm => tm.Content)
            .HasColumnType("TEXT");

        builder.Property(tm => tm.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(tm => tm.Level)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(tm => tm.EstimatedDurationMinutes)
            .IsRequired();

        builder.Property(tm => tm.IsMandatory)
            .HasDefaultValue(false);

        builder.Property(tm => tm.IsActive)
            .HasDefaultValue(true);

        builder.Property(tm => tm.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // JSON conversion for lists
        builder.Property(tm => tm.PrerequisiteModuleIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions)null!) ?? new List<int>())
            .HasColumnType("JSON");

        builder.Property(tm => tm.ContentFiles)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>())
            .HasColumnType("JSON");

        // Relationships
        builder.HasOne(tm => tm.CreatedByEmployee)
            .WithMany()
            .HasForeignKey(tm => tm.CreatedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(tm => tm.TrainingAssignments)
            .WithOne(ta => ta.TrainingModule)
            .HasForeignKey(ta => ta.TrainingModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(tm => tm.TrainingProgresses)
            .WithOne(tp => tp.TrainingModule)
            .HasForeignKey(tp => tp.TrainingModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(tm => tm.Assessments)
            .WithOne(a => a.TrainingModule)
            .HasForeignKey(a => a.TrainingModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(tm => tm.Certifications)
            .WithOne(c => c.TrainingModule)
            .HasForeignKey(c => c.TrainingModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(tm => tm.Title);
        builder.HasIndex(tm => tm.Type);
        builder.HasIndex(tm => tm.Level);
        builder.HasIndex(tm => tm.IsActive);
        builder.HasIndex(tm => tm.IsMandatory);
        builder.HasIndex(tm => tm.CreatedByEmployeeId);
        builder.HasIndex(tm => tm.CreatedAt);
    }
}
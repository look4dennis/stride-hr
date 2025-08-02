using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;
using System.Text.Json;

namespace StrideHR.Infrastructure.Data.Configurations;

public class TrainingProgressConfiguration : IEntityTypeConfiguration<TrainingProgress>
{
    public void Configure(EntityTypeBuilder<TrainingProgress> builder)
    {
        builder.ToTable("TrainingProgress");

        builder.HasKey(tp => tp.Id);

        builder.Property(tp => tp.TrainingAssignmentId)
            .IsRequired();

        builder.Property(tp => tp.EmployeeId)
            .IsRequired();

        builder.Property(tp => tp.TrainingModuleId)
            .IsRequired();

        builder.Property(tp => tp.ProgressPercentage)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(tp => tp.TimeSpentMinutes)
            .IsRequired();

        builder.Property(tp => tp.LastAccessedAt)
            .IsRequired(false);

        builder.Property(tp => tp.StartedAt)
            .IsRequired(false);

        builder.Property(tp => tp.CompletedAt)
            .IsRequired(false);

        builder.Property(tp => tp.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Configure the ProgressData dictionary as JSON
        builder.Property(tp => tp.ProgressData)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!) ?? new Dictionary<string, object>())
            .HasColumnType("longtext") // Use longtext for MySQL compatibility and InMemory testing
            .IsRequired();

        // Configure relationships
        builder.HasOne(tp => tp.TrainingAssignment)
            .WithOne(ta => ta.TrainingProgress)
            .HasForeignKey<TrainingProgress>(tp => tp.TrainingAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tp => tp.Employee)
            .WithMany(e => e.TrainingProgress)
            .HasForeignKey(tp => tp.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tp => tp.TrainingModule)
            .WithMany(tm => tm.TrainingProgresses)
            .HasForeignKey(tp => tp.TrainingModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(tp => tp.AssessmentAttempts)
            .WithOne(aa => aa.TrainingProgress)
            .HasForeignKey(aa => aa.TrainingProgressId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(tp => new { tp.TrainingAssignmentId, tp.EmployeeId })
            .IsUnique();

        builder.HasIndex(tp => tp.EmployeeId);
        builder.HasIndex(tp => tp.TrainingModuleId);
        builder.HasIndex(tp => tp.Status);
        builder.HasIndex(tp => tp.CompletedAt);
    }
}
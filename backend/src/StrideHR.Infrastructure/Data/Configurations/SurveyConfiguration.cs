using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class SurveyConfiguration : IEntityTypeConfiguration<Survey>
{
    public void Configure(EntityTypeBuilder<Survey> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.Instructions)
            .HasMaxLength(2000);

        builder.Property(s => s.ThankYouMessage)
            .HasMaxLength(1000);

        builder.Property(s => s.Tags)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(s => s.CreatedByEmployee)
            .WithMany()
            .HasForeignKey(s => s.CreatedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Branch)
            .WithMany()
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Questions)
            .WithOne(q => q.Survey)
            .HasForeignKey(q => q.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Responses)
            .WithOne(r => r.Survey)
            .HasForeignKey(r => r.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Distributions)
            .WithOne(d => d.Survey)
            .HasForeignKey(d => d.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Analytics)
            .WithOne(a => a.Survey)
            .HasForeignKey(a => a.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.Type);
        builder.HasIndex(s => s.CreatedByEmployeeId);
        builder.HasIndex(s => s.BranchId);
        builder.HasIndex(s => s.IsGlobal);
    }
}
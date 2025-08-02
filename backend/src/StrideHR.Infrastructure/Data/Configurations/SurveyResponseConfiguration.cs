using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class SurveyResponseConfiguration : IEntityTypeConfiguration<SurveyResponse>
{
    public void Configure(EntityTypeBuilder<SurveyResponse> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.AnonymousId)
            .HasMaxLength(100);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(r => r.UserAgent)
            .HasMaxLength(500);

        builder.Property(r => r.DeviceInfo)
            .HasMaxLength(200);

        builder.Property(r => r.Location)
            .HasMaxLength(200);

        builder.Property(r => r.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(r => r.Survey)
            .WithMany(s => s.Responses)
            .HasForeignKey(r => r.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.RespondentEmployee)
            .WithMany()
            .HasForeignKey(r => r.RespondentEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Answers)
            .WithOne(a => a.Response)
            .HasForeignKey(a => a.ResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.SurveyId);
        builder.HasIndex(r => r.RespondentEmployeeId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.AnonymousId);
        builder.HasIndex(r => r.CompletedAt);
    }
}
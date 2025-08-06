using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
    public void Configure(EntityTypeBuilder<Certification> builder)
    {
        builder.ToTable("Certifications");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TrainingModuleId)
            .IsRequired();

        builder.Property(c => c.EmployeeId)
            .IsRequired();

        builder.Property(c => c.CertificationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.CertificationNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.IssuedDate);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(CertificationStatus.Active);

        builder.Property(c => c.CertificateFilePath)
            .HasMaxLength(500);

        builder.Property(c => c.Score)
            .HasPrecision(5, 2);

        builder.Property(c => c.Notes)
            .HasMaxLength(1000);

        builder.Property(c => c.IsExternalCertification)
            .HasDefaultValue(false);

        builder.Property(c => c.ExternalProvider)
            .HasMaxLength(200);

        builder.Property(c => c.VerificationUrl)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(c => c.TrainingModule)
            .WithMany(tm => tm.Certifications)
            .HasForeignKey(c => c.TrainingModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Employee)
            .WithMany()
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.IssuedByEmployee)
            .WithMany()
            .HasForeignKey(c => c.IssuedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(c => c.TrainingModuleId);
        builder.HasIndex(c => c.EmployeeId);
        builder.HasIndex(c => c.CertificationNumber).IsUnique();
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.IssuedDate);
        builder.HasIndex(c => c.ExpiryDate);
        builder.HasIndex(c => c.IsExternalCertification);
    }
}
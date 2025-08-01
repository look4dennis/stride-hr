using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(o => o.Email)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(o => o.Phone)
            .HasMaxLength(20);
            
        builder.Property(o => o.Address)
            .HasMaxLength(500);
            
        builder.Property(o => o.Logo)
            .HasMaxLength(500);

        builder.Property(o => o.Website)
            .HasMaxLength(200);

        builder.Property(o => o.TaxId)
            .HasMaxLength(50);

        builder.Property(o => o.RegistrationNumber)
            .HasMaxLength(50);

        builder.Property(o => o.OvertimeRate)
            .HasPrecision(5, 2);

        builder.Property(o => o.ConfigurationSettings)
            .HasColumnType("json");

        // Relationships
        builder.HasMany(o => o.Branches)
            .WithOne(b => b.Organization)
            .HasForeignKey(b => b.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Shifts)
            .WithOne(s => s.Organization)
            .HasForeignKey(s => s.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
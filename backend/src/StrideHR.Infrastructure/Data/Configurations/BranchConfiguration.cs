using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(b => b.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.CountryCode)
            .IsRequired()
            .HasMaxLength(3);
            
        builder.Property(b => b.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(b => b.CurrencySymbol)
            .IsRequired()
            .HasMaxLength(5);
            
        builder.Property(b => b.TimeZone)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(b => b.Address)
            .HasMaxLength(500);

        builder.Property(b => b.City)
            .HasMaxLength(100);

        builder.Property(b => b.State)
            .HasMaxLength(100);

        builder.Property(b => b.PostalCode)
            .HasMaxLength(20);

        builder.Property(b => b.Phone)
            .HasMaxLength(20);

        builder.Property(b => b.Email)
            .HasMaxLength(100);

        builder.Property(b => b.LocalHolidays)
            .HasColumnType("json");

        builder.Property(b => b.ComplianceSettings)
            .HasColumnType("json");

        // Relationships
        builder.HasOne(b => b.Organization)
            .WithMany(o => o.Branches)
            .HasForeignKey(b => b.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Employees)
            .WithOne(e => e.Branch)
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
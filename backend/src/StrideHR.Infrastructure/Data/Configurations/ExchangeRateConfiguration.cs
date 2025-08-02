using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.FromCurrency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(r => r.ToCurrency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(r => r.Rate)
            .HasPrecision(18, 6);

        builder.Property(r => r.EffectiveDate)
            .IsRequired();

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);

        builder.Property(r => r.Source)
            .HasMaxLength(100);

        builder.Property(r => r.LastUpdated)
            .IsRequired();

        // Indexes
        builder.HasIndex(r => new { r.FromCurrency, r.ToCurrency, r.EffectiveDate })
            .HasDatabaseName("IX_ExchangeRate_Currencies_Date");

        builder.HasIndex(r => r.IsActive)
            .HasDatabaseName("IX_ExchangeRate_IsActive");

        builder.HasIndex(r => r.EffectiveDate)
            .HasDatabaseName("IX_ExchangeRate_EffectiveDate");
    }
}
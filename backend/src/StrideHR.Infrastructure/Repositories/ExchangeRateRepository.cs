using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ExchangeRateRepository : Repository<ExchangeRate>, IExchangeRateRepository
{
    public ExchangeRateRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<ExchangeRate?> GetLatestRateAsync(string fromCurrency, string toCurrency)
    {
        return await _context.ExchangeRates
            .Where(r => r.FromCurrency == fromCurrency && 
                       r.ToCurrency == toCurrency && 
                       r.IsActive &&
                       (r.ExpiryDate == null || r.ExpiryDate > DateTime.UtcNow))
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ExchangeRate>> GetRatesForDateAsync(DateTime date)
    {
        return await _context.ExchangeRates
            .Where(r => r.IsActive &&
                       r.EffectiveDate <= date &&
                       (r.ExpiryDate == null || r.ExpiryDate > date))
            .ToListAsync();
    }

    public async Task<List<ExchangeRate>> GetActiveRatesAsync()
    {
        return await _context.ExchangeRates
            .Where(r => r.IsActive &&
                       (r.ExpiryDate == null || r.ExpiryDate > DateTime.UtcNow))
            .OrderBy(r => r.FromCurrency)
            .ThenBy(r => r.ToCurrency)
            .ThenByDescending(r => r.EffectiveDate)
            .ToListAsync();
    }
}
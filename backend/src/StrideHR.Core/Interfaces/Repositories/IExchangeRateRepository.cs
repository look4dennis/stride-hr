using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IExchangeRateRepository : IRepository<ExchangeRate>
{
    Task<ExchangeRate?> GetLatestRateAsync(string fromCurrency, string toCurrency);
    Task<List<ExchangeRate>> GetRatesForDateAsync(DateTime date);
    Task<List<ExchangeRate>> GetActiveRatesAsync();
}
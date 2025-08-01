namespace StrideHR.Core.Interfaces.Services;

public interface ICurrencyService
{
    Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency);
    Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);
    Task<IEnumerable<string>> GetSupportedCurrenciesAsync();
    Task<string> GetCurrencySymbolAsync(string currencyCode);
    Task<bool> IsCurrencySupportedAsync(string currencyCode);
}
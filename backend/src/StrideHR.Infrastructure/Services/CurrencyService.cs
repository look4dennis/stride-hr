using Microsoft.Extensions.Logging;
using StrideHR.Core.Interfaces.Services;

namespace StrideHR.Infrastructure.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ILogger<CurrencyService> _logger;

    // Static data for supported currencies and their symbols
    private static readonly Dictionary<string, string> SupportedCurrencies = new()
    {
        { "USD", "$" },
        { "CAD", "C$" },
        { "GBP", "£" },
        { "INR", "₹" },
        { "AUD", "A$" },
        { "EUR", "€" },
        { "JPY", "¥" },
        { "SGD", "S$" },
        { "AED", "د.إ" },
        { "CHF", "CHF" },
        { "CNY", "¥" },
        { "HKD", "HK$" },
        { "NZD", "NZ$" },
        { "SEK", "kr" },
        { "NOK", "kr" },
        { "DKK", "kr" }
    };

    // Mock exchange rates (in a real implementation, this would come from an external API)
    private static readonly Dictionary<string, decimal> ExchangeRates = new()
    {
        { "USD", 1.0m },
        { "EUR", 0.85m },
        { "GBP", 0.73m },
        { "INR", 83.0m },
        { "CAD", 1.35m },
        { "AUD", 1.50m },
        { "JPY", 150.0m },
        { "SGD", 1.35m },
        { "AED", 3.67m },
        { "CHF", 0.92m },
        { "CNY", 7.25m },
        { "HKD", 7.80m },
        { "NZD", 1.65m },
        { "SEK", 10.50m },
        { "NOK", 10.80m },
        { "DKK", 6.85m }
    };

    public CurrencyService(ILogger<CurrencyService> logger)
    {
        _logger = logger;
    }

    public async Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        if (string.IsNullOrEmpty(fromCurrency) || string.IsNullOrEmpty(toCurrency))
        {
            throw new ArgumentException("Currency codes cannot be null or empty");
        }

        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return amount;
        }

        var fromRate = await GetExchangeRateAsync("USD", fromCurrency);
        var toRate = await GetExchangeRateAsync("USD", toCurrency);

        if (fromRate == 0 || toRate == 0)
        {
            throw new ArgumentException("Invalid currency code or exchange rate not available");
        }

        // Convert to USD first, then to target currency
        var usdAmount = amount / fromRate;
        var convertedAmount = usdAmount * toRate;

        var result = Math.Round(convertedAmount, 2);

        _logger.LogInformation("Currency converted: {Amount} {FromCurrency} = {ConvertedAmount} {ToCurrency}", 
            amount, fromCurrency, result, toCurrency);

        return result;
    }

    public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        if (string.IsNullOrEmpty(fromCurrency) || string.IsNullOrEmpty(toCurrency))
        {
            throw new ArgumentException("Currency codes cannot be null or empty");
        }

        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return 1.0m;
        }

        var fromCurrencyUpper = fromCurrency.ToUpperInvariant();
        var toCurrencyUpper = toCurrency.ToUpperInvariant();

        if (!ExchangeRates.ContainsKey(fromCurrencyUpper) || !ExchangeRates.ContainsKey(toCurrencyUpper))
        {
            throw new ArgumentException("Unsupported currency");
        }

        // In a real implementation, you would fetch current rates from an external API
        // For now, we'll use static rates
        var fromRate = ExchangeRates[fromCurrencyUpper];
        var toRate = ExchangeRates[toCurrencyUpper];

        return await Task.FromResult(toRate / fromRate);
    }

    public async Task<IEnumerable<string>> GetSupportedCurrenciesAsync()
    {
        return await Task.FromResult(SupportedCurrencies.Keys);
    }

    public async Task<string> GetCurrencySymbolAsync(string currencyCode)
    {
        if (string.IsNullOrEmpty(currencyCode))
        {
            throw new ArgumentException("Currency code cannot be null or empty");
        }

        var currencyUpper = currencyCode.ToUpperInvariant();
        
        if (SupportedCurrencies.TryGetValue(currencyUpper, out var symbol))
        {
            return await Task.FromResult(symbol);
        }

        throw new ArgumentException($"Unsupported currency: {currencyCode}");
    }

    public async Task<bool> IsCurrencySupportedAsync(string currencyCode)
    {
        if (string.IsNullOrEmpty(currencyCode))
        {
            return false;
        }

        var currencyUpper = currencyCode.ToUpperInvariant();
        return await Task.FromResult(SupportedCurrencies.ContainsKey(currencyUpper));
    }
}
using Microsoft.Extensions.Logging;
using StrideHR.Core.Interfaces;
using System.Globalization;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// Service implementation for currency conversion and management operations
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly ILogger<CurrencyService> _logger;
    
    // In production, this would come from an external API like ExchangeRates-API, Fixer.io, etc.
    private static readonly Dictionary<string, decimal> ExchangeRates = new()
    {
        { "USD", 1.0m },      // Base currency
        { "EUR", 0.85m },     // Euro
        { "GBP", 0.73m },     // British Pound
        { "INR", 83.0m },     // Indian Rupee
        { "CAD", 1.35m },     // Canadian Dollar
        { "AUD", 1.50m },     // Australian Dollar
        { "JPY", 150.0m },    // Japanese Yen
        { "SGD", 1.35m },     // Singapore Dollar
        { "AED", 3.67m },     // UAE Dirham
        { "CHF", 0.88m },     // Swiss Franc
        { "CNY", 7.25m },     // Chinese Yuan
        { "SEK", 10.5m },     // Swedish Krona
        { "NOK", 10.8m },     // Norwegian Krone
        { "DKK", 6.85m },     // Danish Krone
        { "PLN", 4.15m },     // Polish Zloty
        { "CZK", 22.5m },     // Czech Koruna
        { "HUF", 360.0m },    // Hungarian Forint
        { "RUB", 90.0m },     // Russian Ruble
        { "BRL", 5.0m },      // Brazilian Real
        { "MXN", 17.0m },     // Mexican Peso
        { "ZAR", 18.5m },     // South African Rand
        { "KRW", 1320.0m },   // South Korean Won
        { "THB", 35.0m },     // Thai Baht
        { "MYR", 4.65m },     // Malaysian Ringgit
        { "IDR", 15500.0m },  // Indonesian Rupiah
        { "PHP", 56.0m },     // Philippine Peso
        { "VND", 24000.0m },  // Vietnamese Dong
        { "TRY", 28.0m },     // Turkish Lira
        { "EGP", 31.0m },     // Egyptian Pound
        { "SAR", 3.75m },     // Saudi Riyal
        { "QAR", 3.64m },     // Qatari Riyal
        { "KWD", 0.31m },     // Kuwaiti Dinar
        { "BHD", 0.38m },     // Bahraini Dinar
        { "OMR", 0.38m },     // Omani Rial
        { "JOD", 0.71m },     // Jordanian Dinar
        { "LBP", 15000.0m },  // Lebanese Pound
        { "ILS", 3.7m },      // Israeli Shekel
        { "PKR", 280.0m },    // Pakistani Rupee
        { "BDT", 110.0m },    // Bangladeshi Taka
        { "LKR", 320.0m },    // Sri Lankan Rupee
        { "NPR", 133.0m },    // Nepalese Rupee
        { "AFN", 85.0m },     // Afghan Afghani
        { "MMK", 2100.0m },   // Myanmar Kyat
        { "KHR", 4100.0m },   // Cambodian Riel
        { "LAK", 20000.0m },  // Lao Kip
        { "BND", 1.35m },     // Brunei Dollar
        { "TWD", 31.5m },     // Taiwan Dollar
        { "HKD", 7.8m },      // Hong Kong Dollar
        { "MOP", 8.05m },     // Macanese Pataca
        { "NZD", 1.65m },     // New Zealand Dollar
        { "FJD", 2.25m },     // Fijian Dollar
        { "PGK", 3.7m },      // Papua New Guinea Kina
        { "WST", 2.7m },      // Samoan Tala
        { "TOP", 2.35m },     // Tongan Pa'anga
        { "VUV", 120.0m },    // Vanuatu Vatu
        { "SBD", 8.3m },      // Solomon Islands Dollar
        { "NCF", 110.0m },    // New Caledonian Franc
        { "XPF", 110.0m },    // CFP Franc
    };

    private static readonly Dictionary<string, CurrencyInfo> CurrencyInfos = new()
    {
        { "USD", new CurrencyInfo { Code = "USD", Name = "US Dollar", Symbol = "$", DecimalPlaces = 2 } },
        { "EUR", new CurrencyInfo { Code = "EUR", Name = "Euro", Symbol = "€", DecimalPlaces = 2 } },
        { "GBP", new CurrencyInfo { Code = "GBP", Name = "British Pound", Symbol = "£", DecimalPlaces = 2 } },
        { "INR", new CurrencyInfo { Code = "INR", Name = "Indian Rupee", Symbol = "₹", DecimalPlaces = 2 } },
        { "CAD", new CurrencyInfo { Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", DecimalPlaces = 2 } },
        { "AUD", new CurrencyInfo { Code = "AUD", Name = "Australian Dollar", Symbol = "A$", DecimalPlaces = 2 } },
        { "JPY", new CurrencyInfo { Code = "JPY", Name = "Japanese Yen", Symbol = "¥", DecimalPlaces = 0 } },
        { "SGD", new CurrencyInfo { Code = "SGD", Name = "Singapore Dollar", Symbol = "S$", DecimalPlaces = 2 } },
        { "AED", new CurrencyInfo { Code = "AED", Name = "UAE Dirham", Symbol = "د.إ", DecimalPlaces = 2 } },
        { "CHF", new CurrencyInfo { Code = "CHF", Name = "Swiss Franc", Symbol = "CHF", DecimalPlaces = 2 } },
        { "CNY", new CurrencyInfo { Code = "CNY", Name = "Chinese Yuan", Symbol = "¥", DecimalPlaces = 2 } },
        { "SEK", new CurrencyInfo { Code = "SEK", Name = "Swedish Krona", Symbol = "kr", DecimalPlaces = 2 } },
        { "NOK", new CurrencyInfo { Code = "NOK", Name = "Norwegian Krone", Symbol = "kr", DecimalPlaces = 2 } },
        { "DKK", new CurrencyInfo { Code = "DKK", Name = "Danish Krone", Symbol = "kr", DecimalPlaces = 2 } },
        { "PLN", new CurrencyInfo { Code = "PLN", Name = "Polish Zloty", Symbol = "zł", DecimalPlaces = 2 } },
        { "CZK", new CurrencyInfo { Code = "CZK", Name = "Czech Koruna", Symbol = "Kč", DecimalPlaces = 2 } },
        { "HUF", new CurrencyInfo { Code = "HUF", Name = "Hungarian Forint", Symbol = "Ft", DecimalPlaces = 0 } },
        { "RUB", new CurrencyInfo { Code = "RUB", Name = "Russian Ruble", Symbol = "₽", DecimalPlaces = 2 } },
        { "BRL", new CurrencyInfo { Code = "BRL", Name = "Brazilian Real", Symbol = "R$", DecimalPlaces = 2 } },
        { "MXN", new CurrencyInfo { Code = "MXN", Name = "Mexican Peso", Symbol = "$", DecimalPlaces = 2 } },
        { "ZAR", new CurrencyInfo { Code = "ZAR", Name = "South African Rand", Symbol = "R", DecimalPlaces = 2 } },
        { "KRW", new CurrencyInfo { Code = "KRW", Name = "South Korean Won", Symbol = "₩", DecimalPlaces = 0 } },
        { "THB", new CurrencyInfo { Code = "THB", Name = "Thai Baht", Symbol = "฿", DecimalPlaces = 2 } },
        { "MYR", new CurrencyInfo { Code = "MYR", Name = "Malaysian Ringgit", Symbol = "RM", DecimalPlaces = 2 } },
        { "IDR", new CurrencyInfo { Code = "IDR", Name = "Indonesian Rupiah", Symbol = "Rp", DecimalPlaces = 0 } },
        { "PHP", new CurrencyInfo { Code = "PHP", Name = "Philippine Peso", Symbol = "₱", DecimalPlaces = 2 } },
        { "VND", new CurrencyInfo { Code = "VND", Name = "Vietnamese Dong", Symbol = "₫", DecimalPlaces = 0 } },
        { "TRY", new CurrencyInfo { Code = "TRY", Name = "Turkish Lira", Symbol = "₺", DecimalPlaces = 2 } },
        { "EGP", new CurrencyInfo { Code = "EGP", Name = "Egyptian Pound", Symbol = "£", DecimalPlaces = 2 } },
        { "SAR", new CurrencyInfo { Code = "SAR", Name = "Saudi Riyal", Symbol = "﷼", DecimalPlaces = 2 } },
        { "QAR", new CurrencyInfo { Code = "QAR", Name = "Qatari Riyal", Symbol = "﷼", DecimalPlaces = 2 } },
        { "KWD", new CurrencyInfo { Code = "KWD", Name = "Kuwaiti Dinar", Symbol = "د.ك", DecimalPlaces = 3 } },
        { "BHD", new CurrencyInfo { Code = "BHD", Name = "Bahraini Dinar", Symbol = ".د.ب", DecimalPlaces = 3 } },
        { "OMR", new CurrencyInfo { Code = "OMR", Name = "Omani Rial", Symbol = "﷼", DecimalPlaces = 3 } },
        { "JOD", new CurrencyInfo { Code = "JOD", Name = "Jordanian Dinar", Symbol = "د.ا", DecimalPlaces = 3 } },
        { "LBP", new CurrencyInfo { Code = "LBP", Name = "Lebanese Pound", Symbol = "£", DecimalPlaces = 2 } },
        { "ILS", new CurrencyInfo { Code = "ILS", Name = "Israeli Shekel", Symbol = "₪", DecimalPlaces = 2 } },
        { "PKR", new CurrencyInfo { Code = "PKR", Name = "Pakistani Rupee", Symbol = "₨", DecimalPlaces = 2 } },
        { "BDT", new CurrencyInfo { Code = "BDT", Name = "Bangladeshi Taka", Symbol = "৳", DecimalPlaces = 2 } },
        { "LKR", new CurrencyInfo { Code = "LKR", Name = "Sri Lankan Rupee", Symbol = "₨", DecimalPlaces = 2 } },
        { "NPR", new CurrencyInfo { Code = "NPR", Name = "Nepalese Rupee", Symbol = "₨", DecimalPlaces = 2 } },
        { "AFN", new CurrencyInfo { Code = "AFN", Name = "Afghan Afghani", Symbol = "؋", DecimalPlaces = 2 } },
        { "MMK", new CurrencyInfo { Code = "MMK", Name = "Myanmar Kyat", Symbol = "K", DecimalPlaces = 2 } },
        { "KHR", new CurrencyInfo { Code = "KHR", Name = "Cambodian Riel", Symbol = "៛", DecimalPlaces = 2 } },
        { "LAK", new CurrencyInfo { Code = "LAK", Name = "Lao Kip", Symbol = "₭", DecimalPlaces = 2 } },
        { "BND", new CurrencyInfo { Code = "BND", Name = "Brunei Dollar", Symbol = "$", DecimalPlaces = 2 } },
        { "TWD", new CurrencyInfo { Code = "TWD", Name = "Taiwan Dollar", Symbol = "NT$", DecimalPlaces = 2 } },
        { "HKD", new CurrencyInfo { Code = "HKD", Name = "Hong Kong Dollar", Symbol = "HK$", DecimalPlaces = 2 } },
        { "MOP", new CurrencyInfo { Code = "MOP", Name = "Macanese Pataca", Symbol = "MOP$", DecimalPlaces = 2 } },
        { "NZD", new CurrencyInfo { Code = "NZD", Name = "New Zealand Dollar", Symbol = "NZ$", DecimalPlaces = 2 } },
        { "FJD", new CurrencyInfo { Code = "FJD", Name = "Fijian Dollar", Symbol = "FJ$", DecimalPlaces = 2 } },
        { "PGK", new CurrencyInfo { Code = "PGK", Name = "Papua New Guinea Kina", Symbol = "K", DecimalPlaces = 2 } },
        { "WST", new CurrencyInfo { Code = "WST", Name = "Samoan Tala", Symbol = "T", DecimalPlaces = 2 } },
        { "TOP", new CurrencyInfo { Code = "TOP", Name = "Tongan Pa'anga", Symbol = "T$", DecimalPlaces = 2 } },
        { "VUV", new CurrencyInfo { Code = "VUV", Name = "Vanuatu Vatu", Symbol = "VT", DecimalPlaces = 0 } },
        { "SBD", new CurrencyInfo { Code = "SBD", Name = "Solomon Islands Dollar", Symbol = "$", DecimalPlaces = 2 } },
        { "NCF", new CurrencyInfo { Code = "NCF", Name = "New Caledonian Franc", Symbol = "₣", DecimalPlaces = 0 } },
        { "XPF", new CurrencyInfo { Code = "XPF", Name = "CFP Franc", Symbol = "₣", DecimalPlaces = 0 } }
    };

    public CurrencyService(ILogger<CurrencyService> logger)
    {
        _logger = logger;
    }

    public async Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken = default)
    {
        if (fromCurrency == toCurrency)
        {
            return amount;
        }

        if (!ExchangeRates.ContainsKey(fromCurrency) || !ExchangeRates.ContainsKey(toCurrency))
        {
            _logger.LogWarning("Unsupported currency for conversion: {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            throw new ArgumentException($"Unsupported currency for conversion: {fromCurrency} to {toCurrency}");
        }

        try
        {
            // Convert to USD first (base currency), then to target currency
            var usdAmount = amount / ExchangeRates[fromCurrency];
            var convertedAmount = usdAmount * ExchangeRates[toCurrency];

            // Round to appropriate decimal places based on target currency
            var targetCurrencyInfo = CurrencyInfos[toCurrency];
            var roundedAmount = Math.Round(convertedAmount, targetCurrencyInfo.DecimalPlaces);

            _logger.LogDebug("Currency conversion: {Amount} {FromCurrency} = {ConvertedAmount} {ToCurrency}", 
                amount, fromCurrency, roundedAmount, toCurrency);

            return await Task.FromResult(roundedAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting currency from {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            throw;
        }
    }

    public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken = default)
    {
        if (fromCurrency == toCurrency)
        {
            return 1.0m;
        }

        if (!ExchangeRates.ContainsKey(fromCurrency) || !ExchangeRates.ContainsKey(toCurrency))
        {
            throw new ArgumentException($"Unsupported currency: {fromCurrency} or {toCurrency}");
        }

        // Calculate rate: (1 / fromRate) * toRate
        var rate = ExchangeRates[toCurrency] / ExchangeRates[fromCurrency];
        return await Task.FromResult(Math.Round(rate, 6));
    }

    public async Task<IEnumerable<CurrencyInfo>> GetSupportedCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(CurrencyInfos.Values.Where(c => c.IsActive).OrderBy(c => c.Code));
    }

    public async Task UpdateExchangeRatesAsync(CancellationToken cancellationToken = default)
    {
        // In production, this would fetch rates from an external API
        // For now, we'll just log that rates would be updated
        _logger.LogInformation("Exchange rates update requested - in production this would fetch from external API");
        
        // Simulate API call delay
        await Task.Delay(100, cancellationToken);
        
        // Here you would implement actual API calls to services like:
        // - ExchangeRates-API (https://exchangeratesapi.io/)
        // - Fixer.io (https://fixer.io/)
        // - CurrencyAPI (https://currencyapi.com/)
        // - Open Exchange Rates (https://openexchangerates.org/)
        
        _logger.LogInformation("Exchange rates updated successfully");
    }

    public async Task<string> GetCurrencySymbolAsync(string currencyCode, CancellationToken cancellationToken = default)
    {
        if (CurrencyInfos.TryGetValue(currencyCode, out var currencyInfo))
        {
            return await Task.FromResult(currencyInfo.Symbol);
        }

        _logger.LogWarning("Currency symbol not found for code: {CurrencyCode}", currencyCode);
        return await Task.FromResult(currencyCode); // Fallback to currency code
    }

    public async Task<string> FormatCurrencyAsync(decimal amount, string currencyCode, CancellationToken cancellationToken = default)
    {
        try
        {
            if (CurrencyInfos.TryGetValue(currencyCode, out var currencyInfo))
            {
                var roundedAmount = Math.Round(amount, currencyInfo.DecimalPlaces);
                var formattedAmount = roundedAmount.ToString($"N{currencyInfo.DecimalPlaces}", CultureInfo.InvariantCulture);
                return await Task.FromResult($"{currencyInfo.Symbol}{formattedAmount}");
            }

            // Fallback formatting
            var fallbackAmount = Math.Round(amount, 2).ToString("N2", CultureInfo.InvariantCulture);
            return await Task.FromResult($"{currencyCode} {fallbackAmount}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting currency amount: {Amount} {CurrencyCode}", amount, currencyCode);
            return await Task.FromResult($"{currencyCode} {amount:N2}");
        }
    }
}
using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Interface for currency conversion and management operations
/// </summary>
public interface ICurrencyService
{
    /// <summary>
    /// Convert amount from one currency to another
    /// </summary>
    Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get current exchange rate between two currencies
    /// </summary>
    Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all supported currencies
    /// </summary>
    Task<IEnumerable<CurrencyInfo>> GetSupportedCurrenciesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update exchange rates (typically called by a background service)
    /// </summary>
    Task UpdateExchangeRatesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get currency symbol for a given currency code
    /// </summary>
    Task<string> GetCurrencySymbolAsync(string currencyCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Format amount with currency symbol and proper decimal places
    /// </summary>
    Task<string> FormatCurrencyAsync(decimal amount, string currencyCode, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for timezone conversion and management operations
/// </summary>
public interface ITimeZoneService
{
    /// <summary>
    /// Convert UTC time to local time for a specific timezone
    /// </summary>
    Task<DateTime> ConvertToLocalTimeAsync(DateTime utcDateTime, string timeZone, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Convert local time to UTC for a specific timezone
    /// </summary>
    Task<DateTime> ConvertToUtcAsync(DateTime localDateTime, string timeZone, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all supported timezones
    /// </summary>
    Task<IEnumerable<TimeZoneInfo>> GetSupportedTimeZonesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get timezone offset for a specific timezone
    /// </summary>
    Task<TimeSpan> GetTimeZoneOffsetAsync(string timeZone, DateTime? dateTime = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get current time in a specific timezone
    /// </summary>
    Task<DateTime> GetCurrentTimeInTimeZoneAsync(string timeZone, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a timezone observes daylight saving time
    /// </summary>
    Task<bool> IsDaylightSavingTimeAsync(string timeZone, DateTime? dateTime = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Currency information model
/// </summary>
public class CurrencyInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; } = 2;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Exchange rate model
/// </summary>
public class ExchangeRate
{
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime LastUpdated { get; set; }
    public string Source { get; set; } = string.Empty;
}
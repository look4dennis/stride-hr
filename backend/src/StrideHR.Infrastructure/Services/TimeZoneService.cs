using Microsoft.Extensions.Logging;
using StrideHR.Core.Interfaces.Services;

namespace StrideHR.Infrastructure.Services;

public class TimeZoneService : ITimeZoneService
{
    private readonly ILogger<TimeZoneService> _logger;

    // Supported time zones with their display names
    private static readonly Dictionary<string, string> SupportedTimeZones = new()
    {
        { "UTC", "Coordinated Universal Time" },
        { "America/New_York", "Eastern Time (US & Canada)" },
        { "America/Chicago", "Central Time (US & Canada)" },
        { "America/Denver", "Mountain Time (US & Canada)" },
        { "America/Los_Angeles", "Pacific Time (US & Canada)" },
        { "America/Toronto", "Eastern Time (Canada)" },
        { "America/Vancouver", "Pacific Time (Canada)" },
        { "Europe/London", "Greenwich Mean Time (London)" },
        { "Europe/Paris", "Central European Time (Paris)" },
        { "Europe/Berlin", "Central European Time (Berlin)" },
        { "Europe/Rome", "Central European Time (Rome)" },
        { "Europe/Madrid", "Central European Time (Madrid)" },
        { "Asia/Kolkata", "India Standard Time" },
        { "Asia/Tokyo", "Japan Standard Time" },
        { "Asia/Singapore", "Singapore Standard Time" },
        { "Asia/Dubai", "Gulf Standard Time" },
        { "Asia/Shanghai", "China Standard Time" },
        { "Asia/Hong_Kong", "Hong Kong Time" },
        { "Australia/Sydney", "Australian Eastern Time" },
        { "Australia/Melbourne", "Australian Eastern Time" },
        { "Australia/Perth", "Australian Western Time" },
        { "Pacific/Auckland", "New Zealand Standard Time" }
    };

    public TimeZoneService(ILogger<TimeZoneService> logger)
    {
        _logger = logger;
    }

    public async Task<DateTime> ConvertTimeZoneAsync(DateTime dateTime, string fromTimeZone, string toTimeZone)
    {
        if (string.IsNullOrEmpty(fromTimeZone) || string.IsNullOrEmpty(toTimeZone))
        {
            throw new ArgumentException("Time zone IDs cannot be null or empty");
        }

        if (fromTimeZone.Equals(toTimeZone, StringComparison.OrdinalIgnoreCase))
        {
            return dateTime;
        }

        try
        {
            TimeZoneInfo fromTz, toTz;

            // Handle special case for UTC
            if (fromTimeZone.Equals("UTC", StringComparison.OrdinalIgnoreCase))
            {
                fromTz = TimeZoneInfo.Utc;
            }
            else
            {
                fromTz = TimeZoneInfo.FindSystemTimeZoneById(GetSystemTimeZoneId(fromTimeZone));
            }

            if (toTimeZone.Equals("UTC", StringComparison.OrdinalIgnoreCase))
            {
                toTz = TimeZoneInfo.Utc;
            }
            else
            {
                toTz = TimeZoneInfo.FindSystemTimeZoneById(GetSystemTimeZoneId(toTimeZone));
            }

            // Convert to UTC first, then to target timezone
            var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, fromTz);
            var convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, toTz);

            _logger.LogDebug("Time converted from {FromTimeZone} to {ToTimeZone}: {OriginalTime} -> {ConvertedTime}",
                fromTimeZone, toTimeZone, dateTime, convertedDateTime);

            return await Task.FromResult(convertedDateTime);
        }
        catch (TimeZoneNotFoundException ex)
        {
            _logger.LogError(ex, "Invalid timezone: {FromTimeZone} or {ToTimeZone}", fromTimeZone, toTimeZone);
            throw new ArgumentException($"Invalid timezone: {fromTimeZone} or {toTimeZone}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting time from {FromTimeZone} to {ToTimeZone}", fromTimeZone, toTimeZone);
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetSupportedTimeZonesAsync()
    {
        return await Task.FromResult(SupportedTimeZones.Keys);
    }

    public async Task<string> GetTimeZoneDisplayNameAsync(string timeZoneId)
    {
        if (string.IsNullOrEmpty(timeZoneId))
        {
            throw new ArgumentException("Time zone ID cannot be null or empty");
        }

        if (SupportedTimeZones.TryGetValue(timeZoneId, out var displayName))
        {
            return await Task.FromResult(displayName);
        }

        throw new ArgumentException($"Unsupported time zone: {timeZoneId}");
    }

    public async Task<bool> IsTimeZoneSupportedAsync(string timeZoneId)
    {
        if (string.IsNullOrEmpty(timeZoneId))
        {
            return false;
        }

        return await Task.FromResult(SupportedTimeZones.ContainsKey(timeZoneId));
    }

    public async Task<DateTime> GetCurrentTimeInTimeZoneAsync(string timeZoneId)
    {
        if (string.IsNullOrEmpty(timeZoneId))
        {
            throw new ArgumentException("Time zone ID cannot be null or empty");
        }

        var utcNow = DateTime.UtcNow;
        return await ConvertTimeZoneAsync(utcNow, "UTC", timeZoneId);
    }

    public async Task<TimeSpan> GetTimeZoneOffsetAsync(string timeZoneId)
    {
        if (string.IsNullOrEmpty(timeZoneId))
        {
            throw new ArgumentException("Time zone ID cannot be null or empty");
        }

        try
        {
            TimeZoneInfo timeZone;

            if (timeZoneId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
            {
                timeZone = TimeZoneInfo.Utc;
            }
            else
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(GetSystemTimeZoneId(timeZoneId));
            }

            var offset = timeZone.GetUtcOffset(DateTime.UtcNow);
            return await Task.FromResult(offset);
        }
        catch (TimeZoneNotFoundException ex)
        {
            _logger.LogError(ex, "Invalid timezone: {TimeZoneId}", timeZoneId);
            throw new ArgumentException($"Invalid timezone: {timeZoneId}");
        }
    }

    private static string GetSystemTimeZoneId(string timeZoneId)
    {
        // Map IANA time zone IDs to Windows time zone IDs when running on Windows
        if (OperatingSystem.IsWindows())
        {
            return timeZoneId switch
            {
                "America/New_York" => "Eastern Standard Time",
                "America/Chicago" => "Central Standard Time",
                "America/Denver" => "Mountain Standard Time",
                "America/Los_Angeles" => "Pacific Standard Time",
                "America/Toronto" => "Eastern Standard Time",
                "America/Vancouver" => "Pacific Standard Time",
                "Europe/London" => "GMT Standard Time",
                "Europe/Paris" => "Romance Standard Time",
                "Europe/Berlin" => "W. Europe Standard Time",
                "Europe/Rome" => "W. Europe Standard Time",
                "Europe/Madrid" => "Romance Standard Time",
                "Asia/Kolkata" => "India Standard Time",
                "Asia/Tokyo" => "Tokyo Standard Time",
                "Asia/Singapore" => "Singapore Standard Time",
                "Asia/Dubai" => "Arabian Standard Time",
                "Asia/Shanghai" => "China Standard Time",
                "Asia/Hong_Kong" => "China Standard Time",
                "Australia/Sydney" => "AUS Eastern Standard Time",
                "Australia/Melbourne" => "AUS Eastern Standard Time",
                "Australia/Perth" => "W. Australia Standard Time",
                "Pacific/Auckland" => "New Zealand Standard Time",
                _ => timeZoneId
            };
        }

        return timeZoneId;
    }
}
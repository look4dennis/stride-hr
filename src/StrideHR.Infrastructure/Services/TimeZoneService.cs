using Microsoft.Extensions.Logging;
using StrideHR.Core.Interfaces;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// Service implementation for timezone conversion and management operations
/// </summary>
public class TimeZoneService : ITimeZoneService
{
    private readonly ILogger<TimeZoneService> _logger;

    // Common timezone mappings for better cross-platform compatibility
    private static readonly Dictionary<string, string> TimeZoneMappings = new()
    {
        // UTC and GMT
        { "UTC", "UTC" },
        { "GMT", "GMT Standard Time" },
        
        // North America
        { "America/New_York", "Eastern Standard Time" },
        { "America/Chicago", "Central Standard Time" },
        { "America/Denver", "Mountain Standard Time" },
        { "America/Los_Angeles", "Pacific Standard Time" },
        { "America/Phoenix", "US Mountain Standard Time" },
        { "America/Anchorage", "Alaskan Standard Time" },
        { "America/Honolulu", "Hawaiian Standard Time" },
        { "America/Toronto", "Eastern Standard Time" },
        { "America/Vancouver", "Pacific Standard Time" },
        { "America/Montreal", "Eastern Standard Time" },
        { "America/Mexico_City", "Central Standard Time (Mexico)" },
        
        // Europe
        { "Europe/London", "GMT Standard Time" },
        { "Europe/Paris", "W. Europe Standard Time" },
        { "Europe/Berlin", "W. Europe Standard Time" },
        { "Europe/Rome", "W. Europe Standard Time" },
        { "Europe/Madrid", "W. Europe Standard Time" },
        { "Europe/Amsterdam", "W. Europe Standard Time" },
        { "Europe/Brussels", "W. Europe Standard Time" },
        { "Europe/Vienna", "W. Europe Standard Time" },
        { "Europe/Zurich", "W. Europe Standard Time" },
        { "Europe/Stockholm", "W. Europe Standard Time" },
        { "Europe/Oslo", "W. Europe Standard Time" },
        { "Europe/Copenhagen", "W. Europe Standard Time" },
        { "Europe/Helsinki", "FLE Standard Time" },
        { "Europe/Warsaw", "Central European Standard Time" },
        { "Europe/Prague", "Central European Standard Time" },
        { "Europe/Budapest", "Central European Standard Time" },
        { "Europe/Bucharest", "GTB Standard Time" },
        { "Europe/Athens", "GTB Standard Time" },
        { "Europe/Istanbul", "Turkey Standard Time" },
        { "Europe/Moscow", "Russian Standard Time" },
        
        // Asia
        { "Asia/Tokyo", "Tokyo Standard Time" },
        { "Asia/Seoul", "Korea Standard Time" },
        { "Asia/Shanghai", "China Standard Time" },
        { "Asia/Hong_Kong", "China Standard Time" },
        { "Asia/Singapore", "Singapore Standard Time" },
        { "Asia/Bangkok", "SE Asia Standard Time" },
        { "Asia/Jakarta", "SE Asia Standard Time" },
        { "Asia/Manila", "Singapore Standard Time" },
        { "Asia/Kuala_Lumpur", "Singapore Standard Time" },
        { "Asia/Kolkata", "India Standard Time" },
        { "Asia/Mumbai", "India Standard Time" },
        { "Asia/Delhi", "India Standard Time" },
        { "Asia/Karachi", "Pakistan Standard Time" },
        { "Asia/Dhaka", "Bangladesh Standard Time" },
        { "Asia/Colombo", "Sri Lanka Standard Time" },
        { "Asia/Kathmandu", "Nepal Standard Time" },
        { "Asia/Dubai", "Arabian Standard Time" },
        { "Asia/Riyadh", "Arab Standard Time" },
        { "Asia/Kuwait", "Arab Standard Time" },
        { "Asia/Qatar", "Arab Standard Time" },
        { "Asia/Bahrain", "Arab Standard Time" },
        { "Asia/Muscat", "Arabian Standard Time" },
        { "Asia/Tehran", "Iran Standard Time" },
        { "Asia/Baghdad", "Arabic Standard Time" },
        { "Asia/Jerusalem", "Israel Standard Time" },
        { "Asia/Beirut", "Middle East Standard Time" },
        { "Asia/Damascus", "Syria Standard Time" },
        { "Asia/Amman", "Jordan Standard Time" },
        { "Asia/Yerevan", "Caucasus Standard Time" },
        { "Asia/Baku", "Azerbaijan Standard Time" },
        { "Asia/Tbilisi", "Georgian Standard Time" },
        { "Asia/Almaty", "Central Asia Standard Time" },
        { "Asia/Tashkent", "West Asia Standard Time" },
        { "Asia/Ashgabat", "West Asia Standard Time" },
        { "Asia/Dushanbe", "West Asia Standard Time" },
        { "Asia/Bishkek", "Central Asia Standard Time" },
        { "Asia/Kabul", "Afghanistan Standard Time" },
        { "Asia/Islamabad", "Pakistan Standard Time" },
        { "Asia/Ulaanbaatar", "Ulaanbaatar Standard Time" },
        { "Asia/Irkutsk", "North Asia East Standard Time" },
        { "Asia/Vladivostok", "Vladivostok Standard Time" },
        { "Asia/Yakutsk", "Yakutsk Standard Time" },
        { "Asia/Magadan", "Magadan Standard Time" },
        { "Asia/Kamchatka", "Kamchatka Standard Time" },
        
        // Australia and Oceania
        { "Australia/Sydney", "AUS Eastern Standard Time" },
        { "Australia/Melbourne", "AUS Eastern Standard Time" },
        { "Australia/Brisbane", "E. Australia Standard Time" },
        { "Australia/Adelaide", "Cen. Australia Standard Time" },
        { "Australia/Perth", "W. Australia Standard Time" },
        { "Australia/Darwin", "AUS Central Standard Time" },
        { "Australia/Hobart", "Tasmania Standard Time" },
        { "Pacific/Auckland", "New Zealand Standard Time" },
        { "Pacific/Fiji", "Fiji Standard Time" },
        { "Pacific/Honolulu", "Hawaiian Standard Time" },
        { "Pacific/Guam", "West Pacific Standard Time" },
        { "Pacific/Port_Moresby", "West Pacific Standard Time" },
        { "Pacific/Noumea", "Central Pacific Standard Time" },
        { "Pacific/Tahiti", "Hawaiian Standard Time" },
        { "Pacific/Marquesas", "Marquesas Standard Time" },
        { "Pacific/Gambier", "Hawaiian Standard Time" },
        { "Pacific/Easter", "Easter Island Standard Time" },
        
        // Africa
        { "Africa/Cairo", "Egypt Standard Time" },
        { "Africa/Johannesburg", "South Africa Standard Time" },
        { "Africa/Lagos", "W. Central Africa Standard Time" },
        { "Africa/Nairobi", "E. Africa Standard Time" },
        { "Africa/Casablanca", "Morocco Standard Time" },
        { "Africa/Algiers", "W. Central Africa Standard Time" },
        { "Africa/Tunis", "W. Central Africa Standard Time" },
        { "Africa/Tripoli", "Libya Standard Time" },
        { "Africa/Khartoum", "Sudan Standard Time" },
        { "Africa/Addis_Ababa", "E. Africa Standard Time" },
        { "Africa/Dar_es_Salaam", "E. Africa Standard Time" },
        { "Africa/Kampala", "E. Africa Standard Time" },
        { "Africa/Kigali", "E. Africa Standard Time" },
        { "Africa/Bujumbura", "E. Africa Standard Time" },
        { "Africa/Lusaka", "South Africa Standard Time" },
        { "Africa/Harare", "South Africa Standard Time" },
        { "Africa/Maputo", "South Africa Standard Time" },
        { "Africa/Windhoek", "Namibia Standard Time" },
        { "Africa/Gaborone", "South Africa Standard Time" },
        { "Africa/Maseru", "South Africa Standard Time" },
        { "Africa/Mbabane", "South Africa Standard Time" },
        
        // South America
        { "America/Sao_Paulo", "E. South America Standard Time" },
        { "America/Buenos_Aires", "Argentina Standard Time" },
        { "America/Santiago", "Pacific SA Standard Time" },
        { "America/Lima", "SA Pacific Standard Time" },
        { "America/Bogota", "SA Pacific Standard Time" },
        { "America/Caracas", "Venezuela Standard Time" },
        { "America/La_Paz", "SA Western Standard Time" },
        { "America/Asuncion", "Paraguay Standard Time" },
        { "America/Montevideo", "Montevideo Standard Time" },
        { "America/Guyana", "SA Western Standard Time" },
        { "America/Suriname", "SA Eastern Standard Time" },
        { "America/Cayenne", "SA Eastern Standard Time" }
    };

    public TimeZoneService(ILogger<TimeZoneService> logger)
    {
        _logger = logger;
    }

    public async Task<DateTime> ConvertToLocalTimeAsync(DateTime utcDateTime, string timeZone, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeZoneInfo = GetTimeZoneInfo(timeZone);
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
            
            _logger.LogDebug("Converted UTC time {UtcTime} to local time {LocalTime} for timezone {TimeZone}", 
                utcDateTime, localTime, timeZone);
            
            return await Task.FromResult(localTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting UTC time to local time for timezone {TimeZone}", timeZone);
            return await Task.FromResult(utcDateTime); // Return UTC as fallback
        }
    }

    public async Task<DateTime> ConvertToUtcAsync(DateTime localDateTime, string timeZone, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeZoneInfo = GetTimeZoneInfo(timeZone);
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZoneInfo);
            
            _logger.LogDebug("Converted local time {LocalTime} to UTC time {UtcTime} for timezone {TimeZone}", 
                localDateTime, utcTime, timeZone);
            
            return await Task.FromResult(utcTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting local time to UTC for timezone {TimeZone}", timeZone);
            return await Task.FromResult(localDateTime); // Return original time as fallback
        }
    }

    public async Task<IEnumerable<TimeZoneInfo>> GetSupportedTimeZonesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var supportedTimeZones = new List<TimeZoneInfo>();
            
            foreach (var mapping in TimeZoneMappings)
            {
                try
                {
                    var timeZoneInfo = GetTimeZoneInfo(mapping.Key);
                    if (!supportedTimeZones.Any(tz => tz.Id == timeZoneInfo.Id))
                    {
                        supportedTimeZones.Add(timeZoneInfo);
                    }
                }
                catch
                {
                    // Skip unsupported timezones
                    continue;
                }
            }
            
            return await Task.FromResult(supportedTimeZones.OrderBy(tz => tz.BaseUtcOffset).ThenBy(tz => tz.DisplayName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported timezones");
            return await Task.FromResult(Enumerable.Empty<TimeZoneInfo>());
        }
    }

    public async Task<TimeSpan> GetTimeZoneOffsetAsync(string timeZone, DateTime? dateTime = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeZoneInfo = GetTimeZoneInfo(timeZone);
            var targetDateTime = dateTime ?? DateTime.UtcNow;
            var offset = timeZoneInfo.GetUtcOffset(targetDateTime);
            
            return await Task.FromResult(offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting timezone offset for {TimeZone}", timeZone);
            return await Task.FromResult(TimeSpan.Zero);
        }
    }

    public async Task<DateTime> GetCurrentTimeInTimeZoneAsync(string timeZone, CancellationToken cancellationToken = default)
    {
        try
        {
            var utcNow = DateTime.UtcNow;
            return await ConvertToLocalTimeAsync(utcNow, timeZone, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current time for timezone {TimeZone}", timeZone);
            return await Task.FromResult(DateTime.UtcNow);
        }
    }

    public async Task<bool> IsDaylightSavingTimeAsync(string timeZone, DateTime? dateTime = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeZoneInfo = GetTimeZoneInfo(timeZone);
            var targetDateTime = dateTime ?? DateTime.UtcNow;
            var isDst = timeZoneInfo.IsDaylightSavingTime(targetDateTime);
            
            return await Task.FromResult(isDst);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking daylight saving time for timezone {TimeZone}", timeZone);
            return await Task.FromResult(false);
        }
    }

    private TimeZoneInfo GetTimeZoneInfo(string timeZone)
    {
        try
        {
            // First try to get timezone directly
            return TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            // Try to find mapped timezone
            if (TimeZoneMappings.TryGetValue(timeZone, out var mappedTimeZone))
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(mappedTimeZone);
                }
                catch (TimeZoneNotFoundException)
                {
                    _logger.LogWarning("Mapped timezone {MappedTimeZone} not found for {TimeZone}", mappedTimeZone, timeZone);
                }
            }
            
            // Try common variations
            var variations = new[]
            {
                timeZone.Replace("_", " "),
                timeZone.Replace("/", " "),
                timeZone.Replace("_", "/"),
                $"{timeZone} Standard Time"
            };
            
            foreach (var variation in variations)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(variation);
                }
                catch (TimeZoneNotFoundException)
                {
                    continue;
                }
            }
            
            _logger.LogWarning("TimeZone {TimeZone} not found, falling back to UTC", timeZone);
            return TimeZoneInfo.Utc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting timezone info for {TimeZone}", timeZone);
            return TimeZoneInfo.Utc;
        }
    }
}
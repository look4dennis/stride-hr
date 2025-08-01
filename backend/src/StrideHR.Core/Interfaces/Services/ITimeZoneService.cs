namespace StrideHR.Core.Interfaces.Services;

public interface ITimeZoneService
{
    Task<DateTime> ConvertTimeZoneAsync(DateTime dateTime, string fromTimeZone, string toTimeZone);
    Task<IEnumerable<string>> GetSupportedTimeZonesAsync();
    Task<string> GetTimeZoneDisplayNameAsync(string timeZoneId);
    Task<bool> IsTimeZoneSupportedAsync(string timeZoneId);
    Task<DateTime> GetCurrentTimeInTimeZoneAsync(string timeZoneId);
    Task<TimeSpan> GetTimeZoneOffsetAsync(string timeZoneId);
}
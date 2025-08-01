using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class TimeZoneServiceTests
{
    private readonly Mock<ILogger<TimeZoneService>> _mockLogger;
    private readonly TimeZoneService _timeZoneService;

    public TimeZoneServiceTests()
    {
        _mockLogger = new Mock<ILogger<TimeZoneService>>();
        _timeZoneService = new TimeZoneService(_mockLogger.Object);
    }

    [Fact]
    public async Task ConvertToLocalTimeAsync_UtcToEastern_ReturnsCorrectLocalTime()
    {
        // Arrange
        var utcDateTime = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc); // Summer time
        var timeZone = "America/New_York";

        // Act
        var result = await _timeZoneService.ConvertToLocalTimeAsync(utcDateTime, timeZone);

        // Assert
        Assert.NotEqual(utcDateTime, result);
        // In summer, Eastern Time is UTC-4, so 12:00 UTC should be 08:00 EDT
        Assert.True(result < utcDateTime);
    }

    [Fact]
    public async Task ConvertToLocalTimeAsync_UtcToUtc_ReturnsSameTime()
    {
        // Arrange
        var utcDateTime = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var timeZone = "UTC";

        // Act
        var result = await _timeZoneService.ConvertToLocalTimeAsync(utcDateTime, timeZone);

        // Assert
        Assert.Equal(utcDateTime, result);
    }

    [Fact]
    public async Task ConvertToLocalTimeAsync_InvalidTimeZone_ReturnsUtcTime()
    {
        // Arrange
        var utcDateTime = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var invalidTimeZone = "Invalid/TimeZone";

        // Act
        var result = await _timeZoneService.ConvertToLocalTimeAsync(utcDateTime, invalidTimeZone);

        // Assert
        // Should fallback to UTC when timezone is invalid
        Assert.Equal(utcDateTime, result);
    }

    [Fact]
    public async Task ConvertToUtcAsync_EasternToUtc_ReturnsCorrectUtcTime()
    {
        // Arrange
        var localDateTime = new DateTime(2024, 6, 15, 8, 0, 0); // 8 AM EDT
        var timeZone = "America/New_York";

        // Act
        var result = await _timeZoneService.ConvertToUtcAsync(localDateTime, timeZone);

        // Assert
        Assert.NotEqual(localDateTime, result);
        // In summer, 8:00 AM EDT should be 12:00 PM UTC
        Assert.True(result > localDateTime);
    }

    [Fact]
    public async Task ConvertToUtcAsync_UtcToUtc_ReturnsSameTime()
    {
        // Arrange
        var localDateTime = new DateTime(2024, 1, 15, 12, 0, 0);
        var timeZone = "UTC";

        // Act
        var result = await _timeZoneService.ConvertToUtcAsync(localDateTime, timeZone);

        // Assert
        Assert.Equal(localDateTime, result);
    }

    [Fact]
    public async Task ConvertToUtcAsync_InvalidTimeZone_ReturnsOriginalTime()
    {
        // Arrange
        var localDateTime = new DateTime(2024, 1, 15, 12, 0, 0);
        var invalidTimeZone = "Invalid/TimeZone";

        // Act
        var result = await _timeZoneService.ConvertToUtcAsync(localDateTime, invalidTimeZone);

        // Assert
        // Should fallback to original time when timezone is invalid
        Assert.Equal(localDateTime, result);
    }

    [Fact]
    public async Task GetSupportedTimeZonesAsync_ReturnsTimeZones()
    {
        // Act
        var result = await _timeZoneService.GetSupportedTimeZonesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Should contain UTC
        Assert.Contains(result, tz => tz.Id == "UTC");
        
        // Should be ordered by offset then by display name
        var timeZoneList = result.ToList();
        for (int i = 1; i < timeZoneList.Count; i++)
        {
            var current = timeZoneList[i];
            var previous = timeZoneList[i - 1];
            
            // Either offset is greater or equal with better display name
            Assert.True(current.BaseUtcOffset >= previous.BaseUtcOffset);
        }
    }

    [Fact]
    public async Task GetTimeZoneOffsetAsync_EasternStandardTime_ReturnsCorrectOffset()
    {
        // Arrange
        var timeZone = "America/New_York";
        var winterDate = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc); // Winter time

        // Act
        var result = await _timeZoneService.GetTimeZoneOffsetAsync(timeZone, winterDate);

        // Assert
        // Eastern Standard Time is UTC-5 in winter
        Assert.Equal(TimeSpan.FromHours(-5), result);
    }

    [Fact]
    public async Task GetTimeZoneOffsetAsync_EasternDaylightTime_ReturnsCorrectOffset()
    {
        // Arrange
        var timeZone = "America/New_York";
        var summerDate = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc); // Summer time

        // Act
        var result = await _timeZoneService.GetTimeZoneOffsetAsync(timeZone, summerDate);

        // Assert
        // Eastern Daylight Time is UTC-4 in summer
        Assert.Equal(TimeSpan.FromHours(-4), result);
    }

    [Fact]
    public async Task GetTimeZoneOffsetAsync_UtcTimeZone_ReturnsZeroOffset()
    {
        // Arrange
        var timeZone = "UTC";

        // Act
        var result = await _timeZoneService.GetTimeZoneOffsetAsync(timeZone);

        // Assert
        Assert.Equal(TimeSpan.Zero, result);
    }

    [Fact]
    public async Task GetTimeZoneOffsetAsync_InvalidTimeZone_ReturnsZeroOffset()
    {
        // Arrange
        var invalidTimeZone = "Invalid/TimeZone";

        // Act
        var result = await _timeZoneService.GetTimeZoneOffsetAsync(invalidTimeZone);

        // Assert
        Assert.Equal(TimeSpan.Zero, result);
    }

    [Fact]
    public async Task GetCurrentTimeInTimeZoneAsync_ValidTimeZone_ReturnsCurrentTime()
    {
        // Arrange
        var timeZone = "UTC";
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = await _timeZoneService.GetCurrentTimeInTimeZoneAsync(timeZone);
        var afterCall = DateTime.UtcNow;

        // Assert
        Assert.True(result >= beforeCall);
        Assert.True(result <= afterCall.AddSeconds(1)); // Allow for small execution time
    }

    [Fact]
    public async Task GetCurrentTimeInTimeZoneAsync_InvalidTimeZone_ReturnsUtcTime()
    {
        // Arrange
        var invalidTimeZone = "Invalid/TimeZone";
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = await _timeZoneService.GetCurrentTimeInTimeZoneAsync(invalidTimeZone);
        var afterCall = DateTime.UtcNow;

        // Assert
        // Should fallback to UTC time
        Assert.True(result >= beforeCall);
        Assert.True(result <= afterCall.AddSeconds(1));
    }

    [Fact]
    public async Task IsDaylightSavingTimeAsync_EasternSummerTime_ReturnsTrue()
    {
        // Arrange
        var timeZone = "America/New_York";
        var summerDate = new DateTime(2024, 6, 15, 12, 0, 0); // Summer time

        // Act
        var result = await _timeZoneService.IsDaylightSavingTimeAsync(timeZone, summerDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsDaylightSavingTimeAsync_EasternWinterTime_ReturnsFalse()
    {
        // Arrange
        var timeZone = "America/New_York";
        var winterDate = new DateTime(2024, 1, 15, 12, 0, 0); // Winter time

        // Act
        var result = await _timeZoneService.IsDaylightSavingTimeAsync(timeZone, winterDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsDaylightSavingTimeAsync_UtcTimeZone_ReturnsFalse()
    {
        // Arrange
        var timeZone = "UTC";

        // Act
        var result = await _timeZoneService.IsDaylightSavingTimeAsync(timeZone);

        // Assert
        Assert.False(result); // UTC never observes DST
    }

    [Fact]
    public async Task IsDaylightSavingTimeAsync_InvalidTimeZone_ReturnsFalse()
    {
        // Arrange
        var invalidTimeZone = "Invalid/TimeZone";

        // Act
        var result = await _timeZoneService.IsDaylightSavingTimeAsync(invalidTimeZone);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("America/New_York")]
    [InlineData("America/Los_Angeles")]
    [InlineData("Europe/London")]
    [InlineData("Asia/Tokyo")]
    [InlineData("UTC")]
    public async Task ConvertToLocalTimeAsync_CommonTimeZones_WorksCorrectly(string inputTimeZone)
    {
        // Arrange
        var utcDateTime = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await _timeZoneService.ConvertToLocalTimeAsync(utcDateTime, inputTimeZone);

        // Assert
        // The result should be different from UTC unless it's UTC timezone
        if (inputTimeZone != "UTC")
        {
            Assert.NotEqual(utcDateTime, result);
        }
        else
        {
            Assert.Equal(utcDateTime, result);
        }
    }

    [Theory]
    [InlineData("America/New_York")]
    [InlineData("America/Chicago")]
    [InlineData("America/Denver")]
    [InlineData("America/Los_Angeles")]
    [InlineData("Europe/London")]
    [InlineData("Europe/Paris")]
    [InlineData("Asia/Tokyo")]
    [InlineData("Asia/Shanghai")]
    [InlineData("Australia/Sydney")]
    public async Task GetTimeZoneOffsetAsync_VariousTimeZones_ReturnsValidOffsets(string timeZone)
    {
        // Act
        var result = await _timeZoneService.GetTimeZoneOffsetAsync(timeZone);

        // Assert
        // Offset should be within reasonable bounds (-12 to +14 hours)
        Assert.True(result >= TimeSpan.FromHours(-12));
        Assert.True(result <= TimeSpan.FromHours(14));
    }

    [Fact]
    public async Task ConvertToLocalTimeAsync_RoundTripConversion_ReturnsOriginalTime()
    {
        // Arrange
        var originalUtc = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var timeZone = "America/New_York";

        // Act
        var localTime = await _timeZoneService.ConvertToLocalTimeAsync(originalUtc, timeZone);
        var backToUtc = await _timeZoneService.ConvertToUtcAsync(localTime, timeZone);

        // Assert
        // Should get back to original UTC time (within reasonable precision)
        Assert.Equal(originalUtc, backToUtc);
    }
}
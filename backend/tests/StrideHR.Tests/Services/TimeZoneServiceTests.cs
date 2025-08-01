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
    public async Task ConvertTimeZoneAsync_SameTimeZone_ReturnsOriginalTime()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 1, 12, 0, 0);
        var timeZone = "UTC";

        // Act
        var result = await _timeZoneService.ConvertTimeZoneAsync(dateTime, timeZone, timeZone);

        // Assert
        Assert.Equal(dateTime, result);
    }

    [Fact]
    public async Task ConvertTimeZoneAsync_UTCToEST_ReturnsConvertedTime()
    {
        // Arrange
        var utcDateTime = new DateTime(2024, 6, 1, 12, 0, 0); // Summer time
        var fromTimeZone = "UTC";
        var toTimeZone = "America/New_York";

        // Act
        var result = await _timeZoneService.ConvertTimeZoneAsync(utcDateTime, fromTimeZone, toTimeZone);

        // Assert
        Assert.NotEqual(utcDateTime, result);
        // In summer, EST is UTC-4, so 12:00 UTC should be 08:00 EST
        Assert.True(result.Hour < utcDateTime.Hour);
    }

    [Fact]
    public async Task ConvertTimeZoneAsync_InvalidFromTimeZone_ThrowsArgumentException()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 1, 12, 0, 0);
        var fromTimeZone = "Invalid/TimeZone";
        var toTimeZone = "UTC";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _timeZoneService.ConvertTimeZoneAsync(dateTime, fromTimeZone, toTimeZone));
    }

    [Fact]
    public async Task ConvertTimeZoneAsync_InvalidToTimeZone_ThrowsArgumentException()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 1, 12, 0, 0);
        var fromTimeZone = "UTC";
        var toTimeZone = "Invalid/TimeZone";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _timeZoneService.ConvertTimeZoneAsync(dateTime, fromTimeZone, toTimeZone));
    }

    [Fact]
    public async Task ConvertTimeZoneAsync_NullFromTimeZone_ThrowsArgumentException()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 1, 12, 0, 0);
        string fromTimeZone = null!;
        var toTimeZone = "UTC";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _timeZoneService.ConvertTimeZoneAsync(dateTime, fromTimeZone, toTimeZone));
    }

    [Fact]
    public async Task ConvertTimeZoneAsync_EmptyToTimeZone_ThrowsArgumentException()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 1, 12, 0, 0);
        var fromTimeZone = "UTC";
        var toTimeZone = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _timeZoneService.ConvertTimeZoneAsync(dateTime, fromTimeZone, toTimeZone));
    }

    [Fact]
    public async Task GetSupportedTimeZonesAsync_ReturnsExpectedTimeZones()
    {
        // Act
        var result = await _timeZoneService.GetSupportedTimeZonesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("UTC", result);
        Assert.Contains("America/New_York", result);
        Assert.Contains("America/Chicago", result);
        Assert.Contains("America/Los_Angeles", result);
        Assert.Contains("Europe/London", result);
        Assert.Contains("Europe/Paris", result);
        Assert.Contains("Asia/Kolkata", result);
        Assert.Contains("Asia/Tokyo", result);
        Assert.Contains("Asia/Singapore", result);
        Assert.Contains("Australia/Sydney", result);
    }

    [Fact]
    public async Task GetTimeZoneDisplayNameAsync_UTC_ReturnsCorrectDisplayName()
    {
        // Arrange
        var timeZoneId = "UTC";

        // Act
        var result = await _timeZoneService.GetTimeZoneDisplayNameAsync(timeZoneId);

        // Assert
        Assert.Equal("Coordinated Universal Time", result);
    }

    [Fact]
    public async Task GetTimeZoneDisplayNameAsync_NewYork_ReturnsCorrectDisplayName()
    {
        // Arrange
        var timeZoneId = "America/New_York";

        // Act
        var result = await _timeZoneService.GetTimeZoneDisplayNameAsync(timeZoneId);

        // Assert
        Assert.Equal("Eastern Time (US & Canada)", result);
    }

    [Fact]
    public async Task GetTimeZoneDisplayNameAsync_InvalidTimeZone_ThrowsArgumentException()
    {
        // Arrange
        var timeZoneId = "Invalid/TimeZone";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _timeZoneService.GetTimeZoneDisplayNameAsync(timeZoneId));
    }

    [Fact]
    public async Task GetTimeZoneDisplayNameAsync_NullTimeZone_ThrowsArgumentException()
    {
        // Arrange
        string timeZoneId = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _timeZoneService.GetTimeZoneDisplayNameAsync(timeZoneId));
    }

    [Fact]
    public async Task IsTimeZoneSupportedAsync_ValidTimeZone_ReturnsTrue()
    {
        // Arrange
        var timeZoneId = "UTC";

        // Act
        var result = await _timeZoneService.IsTimeZoneSupportedAsync(timeZoneId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsTimeZoneSupportedAsync_InvalidTimeZone_ReturnsFalse()
    {
        // Arrange
        var timeZoneId = "Invalid/TimeZone";

        // Act
        var result = await _timeZoneService.IsTimeZoneSupportedAsync(timeZoneId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsTimeZoneSupportedAsync_NullTimeZone_ReturnsFalse()
    {
        // Arrange
        string timeZoneId = null!;

        // Act
        var result = await _timeZoneService.IsTimeZoneSupportedAsync(timeZoneId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsTimeZoneSupportedAsync_EmptyTimeZone_ReturnsFalse()
    {
        // Arrange
        var timeZoneId = "";

        // Act
        var result = await _timeZoneService.IsTimeZoneSupportedAsync(timeZoneId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetCurrentTimeInTimeZoneAsync_UTC_ReturnsCurrentUTCTime()
    {
        // Arrange
        var timeZoneId = "UTC";
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = await _timeZoneService.GetCurrentTimeInTimeZoneAsync(timeZoneId);

        // Assert
        var afterCall = DateTime.UtcNow;
        Assert.True(result >= beforeCall && result <= afterCall.AddSeconds(1));
    }

    [Fact]
    public async Task GetCurrentTimeInTimeZoneAsync_InvalidTimeZone_ThrowsArgumentException()
    {
        // Arrange
        var timeZoneId = "Invalid/TimeZone";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _timeZoneService.GetCurrentTimeInTimeZoneAsync(timeZoneId));
    }

    [Fact]
    public async Task GetTimeZoneOffsetAsync_UTC_ReturnsZeroOffset()
    {
        // Arrange
        var timeZoneId = "UTC";

        // Act
        var result = await _timeZoneService.GetTimeZoneOffsetAsync(timeZoneId);

        // Assert
        Assert.Equal(TimeSpan.Zero, result);
    }

    [Fact]
    public async Task GetTimeZoneOffsetAsync_InvalidTimeZone_ThrowsArgumentException()
    {
        // Arrange
        var timeZoneId = "Invalid/TimeZone";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _timeZoneService.GetTimeZoneOffsetAsync(timeZoneId));
    }

    [Fact]
    public async Task GetTimeZoneOffsetAsync_NullTimeZone_ThrowsArgumentException()
    {
        // Arrange
        string timeZoneId = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _timeZoneService.GetTimeZoneOffsetAsync(timeZoneId));
    }

    [Theory]
    [InlineData("UTC")]
    [InlineData("America/New_York")]
    [InlineData("Europe/London")]
    [InlineData("Asia/Kolkata")]
    public async Task GetTimeZoneOffsetAsync_ValidTimeZones_ReturnsValidOffset(string timeZoneId)
    {
        // Act
        var result = await _timeZoneService.GetTimeZoneOffsetAsync(timeZoneId);

        // Assert
        Assert.True(result >= TimeSpan.FromHours(-12) && result <= TimeSpan.FromHours(14));
    }
}
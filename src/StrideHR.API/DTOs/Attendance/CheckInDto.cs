using System.ComponentModel.DataAnnotations;

namespace StrideHR.API.DTOs.Attendance;

/// <summary>
/// DTO for check-in request
/// </summary>
public class CheckInDto
{
    /// <summary>
    /// Check-in location (GPS coordinates or office location)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// IP address of the device
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Device information
    /// </summary>
    [MaxLength(500)]
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Weather information at check-in
    /// </summary>
    public Dictionary<string, object>? WeatherInfo { get; set; }
}
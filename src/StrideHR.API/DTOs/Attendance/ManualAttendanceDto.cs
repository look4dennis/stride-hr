using System.ComponentModel.DataAnnotations;
using StrideHR.Core.Entities;

namespace StrideHR.API.DTOs.Attendance;

/// <summary>
/// DTO for manual attendance entry
/// </summary>
public class ManualAttendanceDto
{
    /// <summary>
    /// Employee ID
    /// </summary>
    [Required]
    public int EmployeeId { get; set; }

    /// <summary>
    /// Attendance date
    /// </summary>
    [Required]
    public DateTime Date { get; set; }

    /// <summary>
    /// Check-in time
    /// </summary>
    public DateTime? CheckInTime { get; set; }

    /// <summary>
    /// Check-out time
    /// </summary>
    public DateTime? CheckOutTime { get; set; }

    /// <summary>
    /// Attendance status
    /// </summary>
    [Required]
    public AttendanceStatus Status { get; set; }

    /// <summary>
    /// Location
    /// </summary>
    [MaxLength(200)]
    public string? Location { get; set; }

    /// <summary>
    /// Reason for manual entry
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}
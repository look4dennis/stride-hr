using System.ComponentModel.DataAnnotations;
using StrideHR.Core.Entities;

namespace StrideHR.API.DTOs.Attendance;

/// <summary>
/// DTO for starting a break
/// </summary>
public class StartBreakDto
{
    /// <summary>
    /// Type of break
    /// </summary>
    [Required]
    public BreakType Type { get; set; }

    /// <summary>
    /// Break location
    /// </summary>
    [MaxLength(200)]
    public string? Location { get; set; }

    /// <summary>
    /// Reason for the break
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
}
using System.ComponentModel.DataAnnotations;
using StrideHR.Core.Entities;

namespace StrideHR.API.DTOs.Attendance;

/// <summary>
/// DTO for requesting attendance correction
/// </summary>
public class AttendanceCorrectionRequestDto
{
    /// <summary>
    /// Type of correction
    /// </summary>
    [Required]
    public CorrectionType Type { get; set; }

    /// <summary>
    /// Original value (before correction)
    /// </summary>
    [MaxLength(200)]
    public string? OriginalValue { get; set; }

    /// <summary>
    /// Corrected value (after correction)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string CorrectedValue { get; set; } = string.Empty;

    /// <summary>
    /// Reason for correction
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// DTO for attendance correction response
/// </summary>
public class AttendanceCorrectionDto
{
    public int Id { get; set; }
    public int AttendanceRecordId { get; set; }
    public int RequestedBy { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public CorrectionType Type { get; set; }
    public string? OriginalValue { get; set; }
    public string CorrectedValue { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public CorrectionStatus Status { get; set; }
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalComments { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for approving/rejecting correction
/// </summary>
public class CorrectionApprovalDto
{
    /// <summary>
    /// Approval comments
    /// </summary>
    [MaxLength(500)]
    public string? Comments { get; set; }
}
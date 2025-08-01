using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Attendance;

public class AttendanceCorrectionRequest
{
    public DateTime? CheckInTime { get; set; }
    
    public DateTime? CheckOutTime { get; set; }
    
    [Required]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string Reason { get; set; } = string.Empty;
    
    public string? Notes { get; set; }
}
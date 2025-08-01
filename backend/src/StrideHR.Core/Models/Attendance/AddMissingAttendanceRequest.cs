using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Attendance;

public class AddMissingAttendanceRequest
{
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public DateTime CheckInTime { get; set; }
    
    public DateTime? CheckOutTime { get; set; }
    
    [Required]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string Reason { get; set; } = string.Empty;
    
    public string? Notes { get; set; }
}
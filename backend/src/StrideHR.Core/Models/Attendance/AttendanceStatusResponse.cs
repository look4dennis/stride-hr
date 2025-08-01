using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Attendance;

public class AttendanceStatusResponse
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public AttendanceStatus Status { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public TimeSpan? TotalWorkingHours { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public bool IsLate { get; set; }
    public TimeSpan? LateBy { get; set; }
    public string? CurrentLocation { get; set; }
    public BreakType? CurrentBreakType { get; set; }
    public DateTime? BreakStartTime { get; set; }
}
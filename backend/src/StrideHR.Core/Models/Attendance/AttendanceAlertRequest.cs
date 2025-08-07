using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Attendance;

public class AttendanceAlertRequest
{
    public int EmployeeId { get; set; }
    public int BranchId { get; set; }
    public AttendanceAlertType AlertType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; } = AlertSeverity.Medium;
    public int ThresholdMinutes { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
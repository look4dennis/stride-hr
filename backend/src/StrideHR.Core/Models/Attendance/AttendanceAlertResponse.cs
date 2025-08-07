using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Attendance;

public class AttendanceAlertResponse
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public AttendanceAlertType AlertType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string AlertMessage { get; set; } = string.Empty;
    public DateTime AlertDate { get; set; }
    public AlertSeverity Severity { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public int ReadBy { get; set; }
    public string? ReadByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Attendance;

public class AttendanceAlertRequest
{
    public AttendanceAlertType AlertType { get; set; }
    public int? EmployeeId { get; set; }
    public int? BranchId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? ThresholdMinutes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AttendanceAlertResponse
{
    public int Id { get; set; }
    public AttendanceAlertType AlertType { get; set; }
    public string AlertMessage { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public enum AttendanceAlertType
{
    LateArrival,
    EarlyDeparture,
    MissedCheckIn,
    MissedCheckOut,
    ExcessiveBreakTime,
    ConsecutiveAbsences,
    LowAttendancePercentage,
    OvertimeThreshold,
    UnusualWorkingHours
}
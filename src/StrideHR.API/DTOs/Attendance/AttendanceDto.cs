using StrideHR.Core.Entities;

namespace StrideHR.API.DTOs.Attendance;

/// <summary>
/// DTO for attendance record response
/// </summary>
public class AttendanceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public DateTime? CheckInTimeLocal { get; set; }
    public DateTime? CheckOutTimeLocal { get; set; }
    public TimeSpan? TotalWorkingHours { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public TimeSpan? OvertimeHours { get; set; }
    public TimeSpan? ProductiveHours { get; set; }
    public TimeSpan? IdleTime { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? CheckInLocation { get; set; }
    public string? CheckOutLocation { get; set; }
    public TimeSpan? LateArrivalDuration { get; set; }
    public TimeSpan? EarlyDepartureDuration { get; set; }
    public bool IsManualEntry { get; set; }
    public string? ManualEntryReason { get; set; }
    public string? Notes { get; set; }
    public List<BreakRecordDto> BreakRecords { get; set; } = new();
    public List<AttendanceCorrectionDto> Corrections { get; set; } = new();
}

/// <summary>
/// DTO for break record
/// </summary>
public class BreakRecordDto
{
    public int Id { get; set; }
    public BreakType Type { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime StartTimeLocal { get; set; }
    public DateTime? EndTimeLocal { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? Location { get; set; }
    public string? Reason { get; set; }
    public bool IsPaid { get; set; }
    public int? MaxAllowedMinutes { get; set; }
    public bool IsExceeding { get; set; }
    public TimeSpan? ExceededDuration { get; set; }
    public BreakApprovalStatus ApprovalStatus { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for attendance status summary
/// </summary>
public class AttendanceStatusSummaryDto
{
    public DateTime Date { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int OnBreakCount { get; set; }
    public int OnLeaveCount { get; set; }
    public decimal AttendancePercentage { get; set; }
    public List<EmployeeAttendanceStatusDto> EmployeeStatuses { get; set; } = new();
}

/// <summary>
/// DTO for employee attendance status
/// </summary>
public class EmployeeAttendanceStatusDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public AttendanceStatus Status { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public TimeSpan? WorkingHours { get; set; }
    public BreakType? CurrentBreakType { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public bool IsLate { get; set; }
    public TimeSpan? LateBy { get; set; }
}
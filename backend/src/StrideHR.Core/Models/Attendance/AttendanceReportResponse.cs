namespace StrideHR.Core.Models.Attendance;

public class AttendanceReportResponse
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public int TotalEmployees { get; set; }
    public List<AttendanceReportItem> Items { get; set; } = new();
    public AttendanceReportSummary Summary { get; set; } = new();
}

public class AttendanceReportItem
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public int EarlyDepartures { get; set; }
    public TimeSpan TotalWorkingHours { get; set; }
    public TimeSpan TotalOvertimeHours { get; set; }
    public TimeSpan TotalBreakTime { get; set; }
    public double AttendancePercentage { get; set; }
    public List<AttendanceDetailItem> Details { get; set; } = new();
}

public class AttendanceDetailItem
{
    public DateTime Date { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public TimeSpan? WorkingHours { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public TimeSpan? OvertimeHours { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsLate { get; set; }
    public TimeSpan? LateBy { get; set; }
    public bool IsEarlyOut { get; set; }
    public TimeSpan? EarlyOutBy { get; set; }
    public string? Notes { get; set; }
}

public class AttendanceReportSummary
{
    public int TotalEmployees { get; set; }
    public int TotalWorkingDays { get; set; }
    public double AverageAttendancePercentage { get; set; }
    public int TotalPresentDays { get; set; }
    public int TotalAbsentDays { get; set; }
    public int TotalLateDays { get; set; }
    public int TotalEarlyDepartures { get; set; }
    public TimeSpan TotalWorkingHours { get; set; }
    public TimeSpan TotalOvertimeHours { get; set; }
    public TimeSpan AverageWorkingHoursPerDay { get; set; }
    public TimeSpan AverageOvertimePerDay { get; set; }
}
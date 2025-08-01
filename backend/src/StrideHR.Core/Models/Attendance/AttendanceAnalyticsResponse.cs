namespace StrideHR.Core.Models.Attendance;

public class AttendanceAnalyticsResponse
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public TimeSpan AverageWorkingHours { get; set; }
    public int TotalWorkingDays { get; set; }
    public int LateCount { get; set; }
    public TimeSpan TotalOvertimeHours { get; set; }
    public double AttendancePercentage { get; set; }
    public TimeSpan TotalBreakTime { get; set; }
}
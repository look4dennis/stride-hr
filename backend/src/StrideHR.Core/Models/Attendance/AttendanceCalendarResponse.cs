using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Attendance;

public class AttendanceCalendarResponse
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<AttendanceCalendarDay> Days { get; set; } = new();
    public AttendanceCalendarSummary Summary { get; set; } = new();
}

public class AttendanceCalendarDay
{
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public TimeSpan? WorkingHours { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public TimeSpan? OvertimeHours { get; set; }
    public bool IsLate { get; set; }
    public TimeSpan? LateBy { get; set; }
    public bool IsEarlyOut { get; set; }
    public TimeSpan? EarlyOutBy { get; set; }
    public bool IsWeekend { get; set; }
    public bool IsHoliday { get; set; }
    public string? HolidayName { get; set; }
    public string? Notes { get; set; }
    public List<AttendanceCalendarBreak> Breaks { get; set; } = new();
}

public class AttendanceCalendarBreak
{
    public BreakType Type { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan? Duration { get; set; }
}

public class AttendanceCalendarSummary
{
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public int EarlyDepartures { get; set; }
    public int Weekends { get; set; }
    public int Holidays { get; set; }
    public TimeSpan TotalWorkingHours { get; set; }
    public TimeSpan TotalOvertimeHours { get; set; }
    public double AttendancePercentage { get; set; }
}
using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class AttendanceRecord : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public TimeSpan? TotalWorkingHours { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public TimeSpan? OvertimeHours { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Absent;
    public string? Location { get; set; }
    public string? Notes { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual ICollection<BreakRecord> BreakRecords { get; set; } = new List<BreakRecord>();
}

public class BreakRecord : BaseEntity
{
    public int AttendanceRecordId { get; set; }
    public BreakType Type { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan? Duration { get; set; }
    
    // Navigation Properties
    public virtual AttendanceRecord AttendanceRecord { get; set; } = null!;
}
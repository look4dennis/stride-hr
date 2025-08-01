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
    public string? CheckInLocation { get; set; }
    public string? CheckOutLocation { get; set; }
    public double? CheckInLatitude { get; set; }
    public double? CheckInLongitude { get; set; }
    public double? CheckOutLatitude { get; set; }
    public double? CheckOutLongitude { get; set; }
    public string? CheckInTimeZone { get; set; }
    public string? CheckOutTimeZone { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public bool IsLate { get; set; } = false;
    public TimeSpan? LateBy { get; set; }
    public bool IsEarlyOut { get; set; } = false;
    public TimeSpan? EarlyOutBy { get; set; }
    public int? ShiftId { get; set; }
    public string? Notes { get; set; }
    public string? CorrectionReason { get; set; }
    public int? CorrectedBy { get; set; }
    public DateTime? CorrectedAt { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Shift? Shift { get; set; }
    public virtual Employee? CorrectedByEmployee { get; set; }
    public virtual ICollection<BreakRecord> BreakRecords { get; set; } = new List<BreakRecord>();
}
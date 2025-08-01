using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

/// <summary>
/// Attendance record entity for comprehensive time tracking
/// </summary>
public class AttendanceRecord : AuditableEntity
{
    public int EmployeeId { get; set; }
    
    /// <summary>
    /// Attendance date
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Check-in time (UTC)
    /// </summary>
    public DateTime? CheckInTime { get; set; }
    
    /// <summary>
    /// Check-out time (UTC)
    /// </summary>
    public DateTime? CheckOutTime { get; set; }
    
    /// <summary>
    /// Check-in time in employee's local timezone
    /// </summary>
    public DateTime? CheckInTimeLocal { get; set; }
    
    /// <summary>
    /// Check-out time in employee's local timezone
    /// </summary>
    public DateTime? CheckOutTimeLocal { get; set; }
    
    /// <summary>
    /// Total working hours for the day
    /// </summary>
    public TimeSpan? TotalWorkingHours { get; set; }
    
    /// <summary>
    /// Total break duration
    /// </summary>
    public TimeSpan? BreakDuration { get; set; }
    
    /// <summary>
    /// Overtime hours
    /// </summary>
    public TimeSpan? OvertimeHours { get; set; }
    
    /// <summary>
    /// Productive hours (working hours minus idle time)
    /// </summary>
    public TimeSpan? ProductiveHours { get; set; }
    
    /// <summary>
    /// Idle time duration
    /// </summary>
    public TimeSpan? IdleTime { get; set; }
    
    /// <summary>
    /// Attendance status
    /// </summary>
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    
    /// <summary>
    /// Check-in location (GPS coordinates or office location)
    /// </summary>
    [MaxLength(200)]
    public string? CheckInLocation { get; set; }
    
    /// <summary>
    /// Check-out location (GPS coordinates or office location)
    /// </summary>
    [MaxLength(200)]
    public string? CheckOutLocation { get; set; }
    
    /// <summary>
    /// Check-in IP address
    /// </summary>
    [MaxLength(45)]
    public string? CheckInIpAddress { get; set; }
    
    /// <summary>
    /// Check-out IP address
    /// </summary>
    [MaxLength(45)]
    public string? CheckOutIpAddress { get; set; }
    
    /// <summary>
    /// Device information used for check-in
    /// </summary>
    [MaxLength(500)]
    public string? CheckInDevice { get; set; }
    
    /// <summary>
    /// Device information used for check-out
    /// </summary>
    [MaxLength(500)]
    public string? CheckOutDevice { get; set; }
    
    /// <summary>
    /// Shift ID if employee is assigned to a specific shift
    /// </summary>
    public int? ShiftId { get; set; }
    
    /// <summary>
    /// Expected check-in time based on shift or working hours
    /// </summary>
    public DateTime? ExpectedCheckInTime { get; set; }
    
    /// <summary>
    /// Expected check-out time based on shift or working hours
    /// </summary>
    public DateTime? ExpectedCheckOutTime { get; set; }
    
    /// <summary>
    /// Late arrival duration (if applicable)
    /// </summary>
    public TimeSpan? LateArrivalDuration { get; set; }
    
    /// <summary>
    /// Early departure duration (if applicable)
    /// </summary>
    public TimeSpan? EarlyDepartureDuration { get; set; }
    
    /// <summary>
    /// Is this a manual attendance entry (corrected by HR)
    /// </summary>
    public bool IsManualEntry { get; set; } = false;
    
    /// <summary>
    /// Manual entry reason
    /// </summary>
    [MaxLength(500)]
    public string? ManualEntryReason { get; set; }
    
    /// <summary>
    /// Who made the manual entry
    /// </summary>
    public int? ManualEntryBy { get; set; }
    
    /// <summary>
    /// Additional notes
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Attendance correction requests
    /// </summary>
    public string? CorrectionRequests { get; set; }
    
    /// <summary>
    /// Weather information at check-in (stored as JSON)
    /// </summary>
    public string? WeatherInfo { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Shift? Shift { get; set; }
    public virtual Employee? ManualEntryByEmployee { get; set; }
    public virtual ICollection<BreakRecord> BreakRecords { get; set; } = new List<BreakRecord>();
    public virtual ICollection<AttendanceCorrection> AttendanceCorrections { get; set; } = new List<AttendanceCorrection>();
    
    /// <summary>
    /// Calculate total working hours excluding breaks
    /// </summary>
    public TimeSpan CalculateWorkingHours()
    {
        if (CheckInTime == null || CheckOutTime == null)
            return TimeSpan.Zero;
            
        var totalTime = CheckOutTime.Value - CheckInTime.Value;
        var breakTime = BreakDuration ?? TimeSpan.Zero;
        
        return totalTime > breakTime ? totalTime - breakTime : TimeSpan.Zero;
    }
    
    /// <summary>
    /// Check if employee is currently on break
    /// </summary>
    public bool IsOnBreak()
    {
        return BreakRecords.Any(b => b.EndTime == null);
    }
    
    /// <summary>
    /// Get current break type if on break
    /// </summary>
    public BreakType? CurrentBreakType()
    {
        var activeBreak = BreakRecords.FirstOrDefault(b => b.EndTime == null);
        return activeBreak?.Type;
    }
}

/// <summary>
/// Break record entity for detailed break tracking
/// </summary>
public class BreakRecord : BaseEntity
{
    public int AttendanceRecordId { get; set; }
    
    /// <summary>
    /// Break type
    /// </summary>
    public BreakType Type { get; set; }
    
    /// <summary>
    /// Break start time (UTC)
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Break end time (UTC)
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// Break start time in local timezone
    /// </summary>
    public DateTime StartTimeLocal { get; set; }
    
    /// <summary>
    /// Break end time in local timezone
    /// </summary>
    public DateTime? EndTimeLocal { get; set; }
    
    /// <summary>
    /// Break duration
    /// </summary>
    public TimeSpan? Duration { get; set; }
    
    /// <summary>
    /// Break location
    /// </summary>
    [MaxLength(200)]
    public string? Location { get; set; }
    
    /// <summary>
    /// Break reason/notes
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    /// <summary>
    /// Is this break paid or unpaid
    /// </summary>
    public bool IsPaid { get; set; } = true;
    
    /// <summary>
    /// Maximum allowed duration for this break type (in minutes)
    /// </summary>
    public int? MaxAllowedMinutes { get; set; }
    
    /// <summary>
    /// Is this break exceeding the allowed duration
    /// </summary>
    public bool IsExceeding { get; set; } = false;
    
    /// <summary>
    /// Exceeded duration (if applicable)
    /// </summary>
    public TimeSpan? ExceededDuration { get; set; }
    
    /// <summary>
    /// Break approval status (for extended breaks)
    /// </summary>
    public BreakApprovalStatus ApprovalStatus { get; set; } = BreakApprovalStatus.NotRequired;
    
    /// <summary>
    /// Approved by (for extended breaks)
    /// </summary>
    public int? ApprovedBy { get; set; }
    
    /// <summary>
    /// Approval timestamp
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
    
    // Navigation Properties
    public virtual AttendanceRecord AttendanceRecord { get; set; } = null!;
    public virtual Employee? ApprovedByEmployee { get; set; }
    
    /// <summary>
    /// Calculate break duration
    /// </summary>
    public TimeSpan CalculateDuration()
    {
        if (EndTime == null)
            return DateTime.UtcNow - StartTime;
            
        return EndTime.Value - StartTime;
    }
    
    /// <summary>
    /// Check if break is currently active
    /// </summary>
    public bool IsActive => EndTime == null;
}

/// <summary>
/// Attendance status enumeration
/// </summary>
public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    Late = 3,
    OnBreak = 4,
    HalfDay = 5,
    OnLeave = 6
}

/// <summary>
/// Break type enumeration
/// </summary>
public enum BreakType
{
    Tea = 1,
    Lunch = 2,
    Personal = 3,
    Meeting = 4,
    Prayer = 5,
    Medical = 6,
    Emergency = 7,
    Other = 8
}

/// <summary>
/// Break approval status enumeration
/// </summary>
public enum BreakApprovalStatus
{
    NotRequired = 1,
    Pending = 2,
    Approved = 3,
    Rejected = 4
}

/// <summary>
/// Attendance correction entity for HR corrections
/// </summary>
public class AttendanceCorrection : AuditableEntity
{
    public int AttendanceRecordId { get; set; }
    public int RequestedBy { get; set; }
    
    /// <summary>
    /// Correction type
    /// </summary>
    public CorrectionType Type { get; set; }
    
    /// <summary>
    /// Original value (before correction)
    /// </summary>
    [MaxLength(200)]
    public string? OriginalValue { get; set; }
    
    /// <summary>
    /// Corrected value (after correction)
    /// </summary>
    [MaxLength(200)]
    public string? CorrectedValue { get; set; }
    
    /// <summary>
    /// Reason for correction
    /// </summary>
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Correction status
    /// </summary>
    public CorrectionStatus Status { get; set; } = CorrectionStatus.Pending;
    
    /// <summary>
    /// Approved/rejected by
    /// </summary>
    public int? ApprovedBy { get; set; }
    
    /// <summary>
    /// Approval/rejection timestamp
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
    
    /// <summary>
    /// Approval/rejection comments
    /// </summary>
    [MaxLength(500)]
    public string? ApprovalComments { get; set; }
    
    // Navigation Properties
    public virtual AttendanceRecord AttendanceRecord { get; set; } = null!;
    public virtual Employee RequestedByEmployee { get; set; } = null!;
    public virtual Employee? ApprovedByEmployee { get; set; }
}

/// <summary>
/// Correction type enumeration
/// </summary>
public enum CorrectionType
{
    CheckInTime = 1,
    CheckOutTime = 2,
    BreakDuration = 3,
    WorkingHours = 4,
    AttendanceStatus = 5,
    Location = 6
}

/// <summary>
/// Correction status enumeration
/// </summary>
public enum CorrectionStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}
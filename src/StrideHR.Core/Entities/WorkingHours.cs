using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

/// <summary>
/// Working hours configuration entity for branches and employees
/// </summary>
public class WorkingHours : AuditableEntity
{
    public int? BranchId { get; set; }
    public int? EmployeeId { get; set; }
    
    /// <summary>
    /// Configuration name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Working hours type
    /// </summary>
    public WorkingHoursType Type { get; set; } = WorkingHoursType.Standard;
    
    /// <summary>
    /// Monday working hours
    /// </summary>
    public string? Monday { get; set; }
    
    /// <summary>
    /// Tuesday working hours
    /// </summary>
    public string? Tuesday { get; set; }
    
    /// <summary>
    /// Wednesday working hours
    /// </summary>
    public string? Wednesday { get; set; }
    
    /// <summary>
    /// Thursday working hours
    /// </summary>
    public string? Thursday { get; set; }
    
    /// <summary>
    /// Friday working hours
    /// </summary>
    public string? Friday { get; set; }
    
    /// <summary>
    /// Saturday working hours
    /// </summary>
    public string? Saturday { get; set; }
    
    /// <summary>
    /// Sunday working hours
    /// </summary>
    public string? Sunday { get; set; }
    
    /// <summary>
    /// Total weekly working hours
    /// </summary>
    public decimal WeeklyHours { get; set; } = 40.0m;
    
    /// <summary>
    /// Timezone identifier (e.g., "America/New_York")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TimeZone { get; set; } = "UTC";
    
    /// <summary>
    /// Break duration in minutes
    /// </summary>
    public int BreakDurationMinutes { get; set; } = 60;
    
    /// <summary>
    /// Overtime threshold in hours
    /// </summary>
    public decimal OvertimeThreshold { get; set; } = 8.0m;
    
    /// <summary>
    /// Grace period for late arrival in minutes
    /// </summary>
    public int GracePeriodMinutes { get; set; } = 15;
    
    /// <summary>
    /// Is flexible working hours enabled
    /// </summary>
    public bool IsFlexible { get; set; } = false;
    
    /// <summary>
    /// Flexible hours core time start (if flexible)
    /// </summary>
    public TimeSpan? FlexibleCoreStart { get; set; }
    
    /// <summary>
    /// Flexible hours core time end (if flexible)
    /// </summary>
    public TimeSpan? FlexibleCoreEnd { get; set; }
    
    /// <summary>
    /// Minimum working hours per day for flexible schedule
    /// </summary>
    public decimal? FlexibleMinHours { get; set; }
    
    /// <summary>
    /// Maximum working hours per day for flexible schedule
    /// </summary>
    public decimal? FlexibleMaxHours { get; set; }
    
    /// <summary>
    /// Is this configuration currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Effective start date
    /// </summary>
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Effective end date (null for ongoing)
    /// </summary>
    public DateTime? EffectiveTo { get; set; }
    
    /// <summary>
    /// Holiday calendar configuration (stored as JSON)
    /// </summary>
    public string? HolidayCalendar { get; set; }
    
    // Navigation Properties
    public virtual Branch? Branch { get; set; }
    public virtual Employee? Employee { get; set; }
}

/// <summary>
/// Working hours type enumeration
/// </summary>
public enum WorkingHoursType
{
    Standard = 1,
    Flexible = 2,
    Shift = 3,
    Remote = 4,
    Hybrid = 5,
    PartTime = 6,
    Compressed = 7
}

/// <summary>
/// Holiday entity for managing organizational holidays
/// </summary>
public class Holiday : AuditableEntity
{
    public int? BranchId { get; set; }
    
    /// <summary>
    /// Holiday name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Holiday date
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Holiday type
    /// </summary>
    public HolidayType Type { get; set; } = HolidayType.National;
    
    /// <summary>
    /// Is this a recurring holiday
    /// </summary>
    public bool IsRecurring { get; set; } = false;
    
    /// <summary>
    /// Recurrence pattern (if recurring)
    /// </summary>
    [MaxLength(100)]
    public string? RecurrencePattern { get; set; }
    
    /// <summary>
    /// Holiday description
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Is this holiday optional (employees can choose to work)
    /// </summary>
    public bool IsOptional { get; set; } = false;
    
    /// <summary>
    /// Compensation type for working on this holiday
    /// </summary>
    public HolidayCompensation CompensationType { get; set; } = HolidayCompensation.DoubleTime;
    
    /// <summary>
    /// Is this holiday currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Branch? Branch { get; set; }
}

/// <summary>
/// Holiday type enumeration
/// </summary>
public enum HolidayType
{
    National = 1,
    Regional = 2,
    Religious = 3,
    Company = 4,
    Personal = 5
}

/// <summary>
/// Holiday compensation enumeration
/// </summary>
public enum HolidayCompensation
{
    None = 1,
    RegularTime = 2,
    TimeAndHalf = 3,
    DoubleTime = 4,
    CompensatoryOff = 5
}
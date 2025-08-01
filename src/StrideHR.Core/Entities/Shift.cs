using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

/// <summary>
/// Shift entity representing different work shifts in the organization
/// </summary>
public class Shift : AuditableEntity
{
    public int OrganizationId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Shift start time
    /// </summary>
    public TimeSpan StartTime { get; set; }
    
    /// <summary>
    /// Shift end time
    /// </summary>
    public TimeSpan EndTime { get; set; }
    
    /// <summary>
    /// Total working hours for this shift
    /// </summary>
    public decimal WorkingHours { get; set; }
    
    /// <summary>
    /// Break duration in minutes
    /// </summary>
    public int BreakDurationMinutes { get; set; } = 60;
    
    /// <summary>
    /// Shift type (Day, Night, Rotating, etc.)
    /// </summary>
    public ShiftType Type { get; set; } = ShiftType.Day;
    
    /// <summary>
    /// Days of the week this shift applies to (stored as JSON array)
    /// </summary>
    public string? WorkingDays { get; set; }
    
    /// <summary>
    /// Overtime rate multiplier for this shift
    /// </summary>
    public decimal OvertimeRate { get; set; } = 1.5m;
    
    /// <summary>
    /// Shift allowance amount (if any)
    /// </summary>
    public decimal ShiftAllowance { get; set; } = 0;
    
    /// <summary>
    /// Is this shift currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Color code for shift display in UI
    /// </summary>
    [MaxLength(7)]
    public string? ColorCode { get; set; }
    
    // Navigation Properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
}

/// <summary>
/// Shift assignment entity linking employees to shifts
/// </summary>
public class ShiftAssignment : AuditableEntity
{
    public int EmployeeId { get; set; }
    public int ShiftId { get; set; }
    
    /// <summary>
    /// Assignment start date
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Assignment end date (null for ongoing assignments)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Is this assignment currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Assignment notes
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Shift Shift { get; set; } = null!;
}

/// <summary>
/// Shift type enumeration
/// </summary>
public enum ShiftType
{
    Day = 1,
    Night = 2,
    Evening = 3,
    Rotating = 4,
    Flexible = 5,
    Split = 6
}
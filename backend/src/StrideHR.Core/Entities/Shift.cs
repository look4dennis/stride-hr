using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class Shift : BaseEntity
{
    public int OrganizationId { get; set; }
    public int? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public ShiftType Type { get; set; } = ShiftType.Day;
    public TimeSpan? BreakDuration { get; set; }
    public TimeSpan? GracePeriod { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan WorkingHours { get; set; } = TimeSpan.FromHours(8);
    public bool IsFlexible { get; set; } = false;
    public TimeSpan? FlexibilityWindow { get; set; }
    public string? TimeZone { get; set; }
    public string WorkingDays { get; set; } = "1,2,3,4,5"; // JSON array of working days (1=Monday, 7=Sunday)
    public decimal OvertimeMultiplier { get; set; } = 1.5m;
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual Branch? Branch { get; set; }
    public virtual ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
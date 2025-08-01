namespace StrideHR.Core.Entities;

public class AttendancePolicy : BaseEntity
{
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan StandardWorkingHours { get; set; }
    public TimeSpan GracePeriod { get; set; }
    public TimeSpan MaxBreakDuration { get; set; }
    public int MaxBreaksPerDay { get; set; }
    public bool RequireLocationTracking { get; set; } = false;
    public double LocationRadius { get; set; } = 100;
    public decimal OvertimeRate { get; set; }
    public TimeSpan MinimumOvertimeHours { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Branch Branch { get; set; } = null!;
}
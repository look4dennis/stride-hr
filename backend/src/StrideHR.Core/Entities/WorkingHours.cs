using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class WorkingHours : BaseEntity
{
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan MondayStart { get; set; }
    public TimeSpan MondayEnd { get; set; }
    public bool IsMondayWorking { get; set; } = true;
    public TimeSpan TuesdayStart { get; set; }
    public TimeSpan TuesdayEnd { get; set; }
    public bool IsTuesdayWorking { get; set; } = true;
    public TimeSpan WednesdayStart { get; set; }
    public TimeSpan WednesdayEnd { get; set; }
    public bool IsWednesdayWorking { get; set; } = true;
    public TimeSpan ThursdayStart { get; set; }
    public TimeSpan ThursdayEnd { get; set; }
    public bool IsThursdayWorking { get; set; } = true;
    public TimeSpan FridayStart { get; set; }
    public TimeSpan FridayEnd { get; set; }
    public bool IsFridayWorking { get; set; } = true;
    public TimeSpan SaturdayStart { get; set; }
    public TimeSpan SaturdayEnd { get; set; }
    public bool IsSaturdayWorking { get; set; } = false;
    public TimeSpan SundayStart { get; set; }
    public TimeSpan SundayEnd { get; set; }
    public bool IsSundayWorking { get; set; } = false;
    public TimeSpan TotalWeeklyHours { get; set; } = TimeSpan.FromHours(40);
    public TimeSpan DailyBreakDuration { get; set; } = TimeSpan.FromHours(1);
    public string TimeZone { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Branch Branch { get; set; } = null!;
}

public class Holiday : BaseEntity
{
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool IsRecurring { get; set; } = false;
    public HolidayType Type { get; set; } = HolidayType.National;
    public bool IsOptional { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Branch Branch { get; set; } = null!;
}

public class AttendancePolicy : BaseEntity
{
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan GracePeriodForLate { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan MaximumLateAllowed { get; set; } = TimeSpan.FromMinutes(30);
    public int MaxLateCountPerMonth { get; set; } = 3;
    public bool RequireLocationTracking { get; set; } = true;
    public double LocationRadius { get; set; } = 100; // meters
    public bool AllowRemoteCheckIn { get; set; } = false;
    public bool RequireManagerApprovalForCorrection { get; set; } = true;
    public TimeSpan MinimumWorkingHours { get; set; } = TimeSpan.FromHours(8);
    public TimeSpan OvertimeThreshold { get; set; } = TimeSpan.FromHours(8);
    public decimal OvertimeRate { get; set; } = 1.5m;
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Branch Branch { get; set; } = null!;
}
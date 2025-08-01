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
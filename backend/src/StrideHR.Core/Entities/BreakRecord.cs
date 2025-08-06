using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class BreakRecord : BaseEntity
{
    public int AttendanceRecordId { get; set; }
    public DateTime BreakStartTime { get; set; }
    public DateTime? BreakEndTime { get; set; }
    public string BreakType { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public TimeSpan? Duration { get; set; }
    public bool IsPaid { get; set; } = true;
    
    // Additional properties referenced in code
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public BreakType Type { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual AttendanceRecord AttendanceRecord { get; set; } = null!;
}
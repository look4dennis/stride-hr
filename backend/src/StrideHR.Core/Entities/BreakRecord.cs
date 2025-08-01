using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class BreakRecord : BaseEntity
{
    public int AttendanceRecordId { get; set; }
    public BreakType Type { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? Location { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? TimeZone { get; set; }
    public string? Notes { get; set; }
    
    // Navigation Properties
    public virtual AttendanceRecord AttendanceRecord { get; set; } = null!;
    
    // Helper properties
    public bool IsActive => EndTime == null;
}
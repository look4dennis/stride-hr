using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Asset;

public class AssetMaintenanceDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string AssetTag { get; set; } = string.Empty;
    public MaintenanceType Type { get; set; }
    public MaintenanceStatus Status { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Vendor { get; set; }
    public decimal? Cost { get; set; }
    public string? Currency { get; set; }
    public string? WorkPerformed { get; set; }
    public string? PartsReplaced { get; set; }
    public string? Notes { get; set; }
    public string? TechnicianName { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public DateTime? NextMaintenanceDate { get; set; }
    public string? DocumentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Calculated properties
    public bool IsOverdue => Status == MaintenanceStatus.Scheduled && ScheduledDate < DateTime.UtcNow;
    public int? DaysUntilDue => Status == MaintenanceStatus.Scheduled ? (int?)(ScheduledDate - DateTime.UtcNow).TotalDays : null;
}
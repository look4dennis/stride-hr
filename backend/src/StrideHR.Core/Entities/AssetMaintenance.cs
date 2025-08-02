using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class AssetMaintenance : BaseEntity
{
    public int AssetId { get; set; }
    public MaintenanceType Type { get; set; }
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Scheduled;
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
    public int? TechnicianId { get; set; }
    public int RequestedBy { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? DocumentUrl { get; set; }

    // Navigation Properties
    public virtual Asset Asset { get; set; } = null!;
    public virtual Employee? Technician { get; set; }
    public virtual Employee RequestedByEmployee { get; set; } = null!;
}
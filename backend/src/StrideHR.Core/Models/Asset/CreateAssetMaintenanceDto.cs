using StrideHR.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Asset;

public class CreateAssetMaintenanceDto
{
    [Required]
    public int AssetId { get; set; }
    
    [Required]
    public MaintenanceType Type { get; set; }
    
    [Required]
    public DateTime ScheduledDate { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Vendor { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? Cost { get; set; }
    
    [StringLength(3)]
    public string? Currency { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    public int? TechnicianId { get; set; }
    
    [Required]
    public int RequestedBy { get; set; }
    
    public DateTime? NextMaintenanceDate { get; set; }
}

public class UpdateAssetMaintenanceDto
{
    public MaintenanceStatus Status { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? CompletedDate { get; set; }
    
    [StringLength(1000)]
    public string? WorkPerformed { get; set; }
    
    [StringLength(500)]
    public string? PartsReplaced { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    public int? TechnicianId { get; set; }
    
    public DateTime? NextMaintenanceDate { get; set; }
    
    public string? DocumentUrl { get; set; }
}
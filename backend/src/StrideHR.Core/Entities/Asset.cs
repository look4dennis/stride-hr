using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class Asset : BaseEntity
{
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssetType Type { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public string? PurchaseCurrency { get; set; }
    public string? Vendor { get; set; }
    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public string? WarrantyDetails { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.Available;
    public AssetCondition Condition { get; set; } = AssetCondition.Excellent;
    public string? Location { get; set; }
    public int BranchId { get; set; }
    public string? Notes { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? DepreciationRate { get; set; }
    public decimal? CurrentValue { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }

    // Navigation Properties
    public virtual Branch Branch { get; set; } = null!;
    public virtual ICollection<AssetAssignment> AssetAssignments { get; set; } = new List<AssetAssignment>();
    public virtual ICollection<AssetMaintenance> MaintenanceRecords { get; set; } = new List<AssetMaintenance>();
    public virtual ICollection<AssetHandover> HandoverRecords { get; set; } = new List<AssetHandover>();
}
using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Asset;

public class AssetDto
{
    public int Id { get; set; }
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
    public AssetStatus Status { get; set; }
    public AssetCondition Condition { get; set; }
    public string? Location { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? Notes { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? DepreciationRate { get; set; }
    public decimal? CurrentValue { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Assignment Information
    public string? AssignedToEmployee { get; set; }
    public string? AssignedToProject { get; set; }
    public DateTime? AssignedDate { get; set; }
    
    // Warranty Status
    public bool IsUnderWarranty => WarrantyEndDate.HasValue && WarrantyEndDate.Value > DateTime.UtcNow;
    public int? WarrantyDaysRemaining => IsUnderWarranty ? (int?)(WarrantyEndDate!.Value - DateTime.UtcNow).TotalDays : null;
}
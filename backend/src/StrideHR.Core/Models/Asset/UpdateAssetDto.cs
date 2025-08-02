using StrideHR.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Asset;

public class UpdateAssetDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public AssetType Type { get; set; }
    
    [StringLength(100)]
    public string? Brand { get; set; }
    
    [StringLength(100)]
    public string? Model { get; set; }
    
    [StringLength(100)]
    public string? SerialNumber { get; set; }
    
    public DateTime? PurchaseDate { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? PurchasePrice { get; set; }
    
    [StringLength(3)]
    public string? PurchaseCurrency { get; set; }
    
    [StringLength(200)]
    public string? Vendor { get; set; }
    
    public DateTime? WarrantyStartDate { get; set; }
    
    public DateTime? WarrantyEndDate { get; set; }
    
    [StringLength(500)]
    public string? WarrantyDetails { get; set; }
    
    public AssetStatus Status { get; set; }
    
    public AssetCondition Condition { get; set; }
    
    [StringLength(200)]
    public string? Location { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    public string? ImageUrl { get; set; }
    
    [Range(0, 100)]
    public decimal? DepreciationRate { get; set; }
    
    public DateTime? NextMaintenanceDate { get; set; }
}
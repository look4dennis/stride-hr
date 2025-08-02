using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Asset;

public class AssetSearchCriteria
{
    public string? SearchTerm { get; set; }
    public AssetType? Type { get; set; }
    public AssetStatus? Status { get; set; }
    public AssetCondition? Condition { get; set; }
    public int? BranchId { get; set; }
    public int? AssignedToEmployeeId { get; set; }
    public int? AssignedToProjectId { get; set; }
    public bool? IsUnderWarranty { get; set; }
    public bool? RequiresMaintenance { get; set; }
    public DateTime? PurchaseDateFrom { get; set; }
    public DateTime? PurchaseDateTo { get; set; }
    public decimal? PurchasePriceFrom { get; set; }
    public decimal? PurchasePriceTo { get; set; }
    public string? Vendor { get; set; }
    public string? Brand { get; set; }
    public string? Location { get; set; }
    
    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    // Sorting
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}
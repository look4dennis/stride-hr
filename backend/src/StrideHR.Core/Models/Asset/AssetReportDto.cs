using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Asset;

public class AssetReportDto
{
    public int TotalAssets { get; set; }
    public int AvailableAssets { get; set; }
    public int AssignedAssets { get; set; }
    public int AssetsInMaintenance { get; set; }
    public int RetiredAssets { get; set; }
    public decimal TotalAssetValue { get; set; }
    public decimal TotalMaintenanceCost { get; set; }
    public int AssetsUnderWarranty { get; set; }
    public int AssetsRequiringMaintenance { get; set; }
    public int OverdueMaintenanceCount { get; set; }
    public int PendingHandovers { get; set; }
    public int OverdueHandovers { get; set; }
    
    public List<AssetTypeStatistics> AssetTypeBreakdown { get; set; } = new();
    public List<AssetStatusStatistics> AssetStatusBreakdown { get; set; } = new();
    public List<BranchAssetStatistics> BranchBreakdown { get; set; } = new();
    public List<MaintenanceCostTrend> MaintenanceCostTrends { get; set; } = new();
}

public class AssetTypeStatistics
{
    public AssetType Type { get; set; }
    public int Count { get; set; }
    public decimal TotalValue { get; set; }
    public int Available { get; set; }
    public int Assigned { get; set; }
    public int InMaintenance { get; set; }
}

public class AssetStatusStatistics
{
    public AssetStatus Status { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class BranchAssetStatistics
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int TotalAssets { get; set; }
    public decimal TotalValue { get; set; }
    public int AssignedAssets { get; set; }
    public int AvailableAssets { get; set; }
}

public class MaintenanceCostTrend
{
    public DateTime Month { get; set; }
    public decimal Cost { get; set; }
    public int MaintenanceCount { get; set; }
}
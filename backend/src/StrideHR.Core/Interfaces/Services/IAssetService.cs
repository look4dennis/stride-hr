using StrideHR.Core.Models.Asset;

namespace StrideHR.Core.Interfaces.Services;

public interface IAssetService
{
    Task<AssetDto> CreateAssetAsync(CreateAssetDto createAssetDto);
    Task<AssetDto> UpdateAssetAsync(int id, UpdateAssetDto updateAssetDto);
    Task<AssetDto?> GetAssetByIdAsync(int id);
    Task<AssetDto?> GetAssetByTagAsync(string assetTag);
    Task<IEnumerable<AssetDto>> SearchAssetsAsync(AssetSearchCriteria criteria);
    Task<int> GetTotalAssetsCountAsync(AssetSearchCriteria criteria);
    Task<IEnumerable<AssetDto>> GetAssetsByBranchAsync(int branchId);
    Task<bool> DeleteAssetAsync(int id);
    Task<bool> IsAssetTagUniqueAsync(string assetTag, int? excludeId = null);
    Task<AssetReportDto> GenerateAssetReportAsync(int? branchId = null);
    Task<IEnumerable<AssetDto>> GetAssetsRequiringMaintenanceAsync();
    Task<IEnumerable<AssetDto>> GetAssetsUnderWarrantyAsync();
    Task<IEnumerable<AssetDto>> GetExpiredWarrantyAssetsAsync();
    Task<decimal> CalculateCurrentValueAsync(int assetId);
}
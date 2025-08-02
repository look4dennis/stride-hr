using StrideHR.Core.Entities;
using StrideHR.Core.Models.Asset;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IAssetRepository : IRepository<Asset>
{
    Task<Asset?> GetByAssetTagAsync(string assetTag);
    Task<bool> IsAssetTagUniqueAsync(string assetTag, int? excludeId = null);
    Task<IEnumerable<Asset>> SearchAssetsAsync(AssetSearchCriteria criteria);
    Task<int> GetTotalAssetsCountAsync(AssetSearchCriteria criteria);
    Task<IEnumerable<Asset>> GetAssetsByBranchAsync(int branchId);
    Task<IEnumerable<Asset>> GetAssetsRequiringMaintenanceAsync();
    Task<IEnumerable<Asset>> GetAssetsUnderWarrantyAsync();
    Task<IEnumerable<Asset>> GetExpiredWarrantyAssetsAsync();
    Task<AssetReportDto> GenerateAssetReportAsync(int? branchId = null);
    Task<Asset?> GetAssetWithDetailsAsync(int id);
}
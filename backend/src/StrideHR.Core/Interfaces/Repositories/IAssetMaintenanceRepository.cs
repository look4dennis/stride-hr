using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IAssetMaintenanceRepository : IRepository<AssetMaintenance>
{
    Task<IEnumerable<AssetMaintenance>> GetMaintenanceByAssetIdAsync(int assetId);
    Task<IEnumerable<AssetMaintenance>> GetScheduledMaintenanceAsync();
    Task<IEnumerable<AssetMaintenance>> GetOverdueMaintenanceAsync();
    Task<IEnumerable<AssetMaintenance>> GetMaintenanceByStatusAsync(MaintenanceStatus status);
    Task<IEnumerable<AssetMaintenance>> GetMaintenanceByTechnicianAsync(int technicianId);
    Task<IEnumerable<AssetMaintenance>> GetMaintenanceByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<decimal> GetMaintenanceCostByAssetAsync(int assetId);
    Task<decimal> GetMaintenanceCostByPeriodAsync(DateTime startDate, DateTime endDate, int? branchId = null);
    Task<AssetMaintenance?> GetLastMaintenanceByAssetAsync(int assetId);
}
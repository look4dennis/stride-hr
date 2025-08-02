using StrideHR.Core.Models.Asset;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Services;

public interface IAssetMaintenanceService
{
    Task<AssetMaintenanceDto> CreateMaintenanceAsync(CreateAssetMaintenanceDto maintenanceDto);
    Task<AssetMaintenanceDto> UpdateMaintenanceAsync(int id, UpdateAssetMaintenanceDto updateDto);
    Task<AssetMaintenanceDto?> GetMaintenanceByIdAsync(int id);
    Task<IEnumerable<AssetMaintenanceDto>> GetMaintenanceByAssetIdAsync(int assetId);
    Task<IEnumerable<AssetMaintenanceDto>> GetScheduledMaintenanceAsync();
    Task<IEnumerable<AssetMaintenanceDto>> GetOverdueMaintenanceAsync();
    Task<IEnumerable<AssetMaintenanceDto>> GetMaintenanceByStatusAsync(MaintenanceStatus status);
    Task<IEnumerable<AssetMaintenanceDto>> GetMaintenanceByTechnicianAsync(int technicianId);
    Task<IEnumerable<AssetMaintenanceDto>> GetMaintenanceByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> DeleteMaintenanceAsync(int id);
    Task<decimal> GetMaintenanceCostByAssetAsync(int assetId);
    Task<decimal> GetMaintenanceCostByPeriodAsync(DateTime startDate, DateTime endDate, int? branchId = null);
    Task<AssetMaintenanceDto> StartMaintenanceAsync(int id, int technicianId);
    Task<AssetMaintenanceDto> CompleteMaintenanceAsync(int id, UpdateAssetMaintenanceDto completionDto);
}
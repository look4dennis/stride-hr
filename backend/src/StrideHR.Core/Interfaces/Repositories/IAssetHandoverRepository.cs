using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IAssetHandoverRepository : IRepository<AssetHandover>
{
    Task<IEnumerable<AssetHandover>> GetHandoversByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<AssetHandover>> GetHandoversByAssetIdAsync(int assetId);
    Task<IEnumerable<AssetHandover>> GetHandoversByStatusAsync(HandoverStatus status);
    Task<IEnumerable<AssetHandover>> GetPendingHandoversAsync();
    Task<IEnumerable<AssetHandover>> GetOverdueHandoversAsync();
    Task<IEnumerable<AssetHandover>> GetHandoversByEmployeeExitAsync(int employeeExitId);
    Task<bool> HasPendingHandoverAsync(int assetId);
    Task<IEnumerable<AssetHandover>> GetHandoversRequiringApprovalAsync();
}
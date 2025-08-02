using StrideHR.Core.Models.Asset;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Services;

public interface IAssetHandoverService
{
    Task<AssetHandoverDto> InitiateHandoverAsync(CreateAssetHandoverDto handoverDto);
    Task<AssetHandoverDto> CompleteHandoverAsync(int id, CompleteAssetHandoverDto completionDto);
    Task<AssetHandoverDto> ApproveHandoverAsync(int id, ApproveAssetHandoverDto approvalDto);
    Task<AssetHandoverDto?> GetHandoverByIdAsync(int id);
    Task<IEnumerable<AssetHandoverDto>> GetHandoversByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<AssetHandoverDto>> GetHandoversByAssetIdAsync(int assetId);
    Task<IEnumerable<AssetHandoverDto>> GetHandoversByStatusAsync(HandoverStatus status);
    Task<IEnumerable<AssetHandoverDto>> GetPendingHandoversAsync();
    Task<IEnumerable<AssetHandoverDto>> GetOverdueHandoversAsync();
    Task<IEnumerable<AssetHandoverDto>> GetHandoversByEmployeeExitAsync(int employeeExitId);
    Task<IEnumerable<AssetHandoverDto>> GetHandoversRequiringApprovalAsync();
    Task<bool> CancelHandoverAsync(int id, int cancelledBy);
    Task<IEnumerable<AssetHandoverDto>> InitiateEmployeeExitHandoversAsync(int employeeId, int employeeExitId, int initiatedBy);
}
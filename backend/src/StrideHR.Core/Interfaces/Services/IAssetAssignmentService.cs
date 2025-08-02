using StrideHR.Core.Models.Asset;

namespace StrideHR.Core.Interfaces.Services;

public interface IAssetAssignmentService
{
    Task<AssetAssignmentDto> AssignAssetToEmployeeAsync(CreateAssetAssignmentDto assignmentDto);
    Task<AssetAssignmentDto> AssignAssetToProjectAsync(CreateAssetAssignmentDto assignmentDto);
    Task<AssetAssignmentDto> ReturnAssetAsync(int assignmentId, ReturnAssetDto returnDto);
    Task<AssetAssignmentDto?> GetAssignmentByIdAsync(int id);
    Task<AssetAssignmentDto?> GetActiveAssignmentByAssetIdAsync(int assetId);
    Task<IEnumerable<AssetAssignmentDto>> GetAssignmentsByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<AssetAssignmentDto>> GetAssignmentsByProjectIdAsync(int projectId);
    Task<IEnumerable<AssetAssignmentDto>> GetAssignmentHistoryByAssetIdAsync(int assetId);
    Task<IEnumerable<AssetAssignmentDto>> GetActiveAssignmentsAsync();
    Task<IEnumerable<AssetAssignmentDto>> GetOverdueReturnsAsync();
    Task<bool> CanAssignAssetAsync(int assetId);
}
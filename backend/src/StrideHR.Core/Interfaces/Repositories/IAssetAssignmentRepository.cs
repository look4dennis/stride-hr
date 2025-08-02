using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IAssetAssignmentRepository : IRepository<AssetAssignment>
{
    Task<AssetAssignment?> GetActiveAssignmentByAssetIdAsync(int assetId);
    Task<IEnumerable<AssetAssignment>> GetAssignmentsByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<AssetAssignment>> GetAssignmentsByProjectIdAsync(int projectId);
    Task<IEnumerable<AssetAssignment>> GetAssignmentHistoryByAssetIdAsync(int assetId);
    Task<bool> HasActiveAssignmentAsync(int assetId);
    Task<IEnumerable<AssetAssignment>> GetActiveAssignmentsAsync();
    Task<IEnumerable<AssetAssignment>> GetOverdueReturnsAsync();
}
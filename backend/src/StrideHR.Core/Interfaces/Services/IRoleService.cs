using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Services;

public interface IRoleService
{
    Task<Role?> GetRoleByIdAsync(int id);
    Task<Role?> GetRoleByNameAsync(string name);
    Task<List<Role>> GetAllRolesAsync();
    Task<List<Role>> GetActiveRolesAsync();
    Task<Role> CreateRoleAsync(string name, string description, int hierarchyLevel);
    Task<bool> UpdateRoleAsync(int id, string name, string description, int hierarchyLevel);
    Task<bool> DeleteRoleAsync(int id);
    Task<bool> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds);
    Task<bool> RemovePermissionsFromRoleAsync(int roleId, List<int> permissionIds);
    Task<List<Permission>> GetRolePermissionsAsync(int roleId);
    Task<List<string>> GetRolePermissionNamesAsync(int roleId);
    Task<int> GetMaxHierarchyLevelAsync(List<string> roleNames);
    Task<bool> AssignRoleToEmployeeAsync(int employeeId, int roleId, DateTime? expiryDate = null);
    Task<bool> RemoveRoleFromEmployeeAsync(int employeeId, int roleId);
    Task<List<Role>> GetEmployeeRolesAsync(int employeeId);
    Task<bool> HasPermissionAsync(int userId, string permission);
    Task<bool> CanAccessBranchAsync(int userId, int branchId);
}
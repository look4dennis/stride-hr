using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
    Task<List<Role>> GetActiveRolesAsync();
    Task<Role?> GetWithPermissionsAsync(int id);
    Task<List<Role>> GetRolesByNamesAsync(List<string> names);
    Task<List<Permission>> GetRolePermissionsAsync(int roleId);
    Task<bool> AssignPermissionsAsync(int roleId, List<int> permissionIds);
    Task<bool> RemovePermissionsAsync(int roleId, List<int> permissionIds);
    Task<bool> AssignRoleToEmployeeAsync(int employeeId, int roleId, DateTime? expiryDate = null);
    Task<bool> RemoveRoleFromEmployeeAsync(int employeeId, int roleId);
    Task<List<Role>> GetEmployeeRolesAsync(int employeeId);
}

public interface IPermissionRepository : IRepository<Permission>
{
    Task<Permission?> GetByNameAsync(string name);
    Task<List<Permission>> GetByModuleAsync(string module);
    Task<List<Permission>> GetByIdsAsync(List<int> ids);
    Task<bool> ExistsAsync(string name);
}
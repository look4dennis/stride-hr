using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IBranchRepository : IRepository<Branch>
{
    Task<List<Branch>> GetByOrganizationIdAsync(int organizationId);
    Task<Branch?> GetByNameAsync(string name);
    Task<List<Branch>> GetActiveAsync();
    Task<bool> ExistsAsync(string name, int? excludeId = null);
}
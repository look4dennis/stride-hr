using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IPayslipTemplateRepository : IRepository<PayslipTemplate>
{
    Task<List<PayslipTemplate>> GetByOrganizationAsync(int organizationId);
    Task<List<PayslipTemplate>> GetByBranchAsync(int branchId);
    Task<PayslipTemplate?> GetDefaultTemplateAsync(int organizationId, int? branchId = null);
    Task<PayslipTemplate?> GetActiveTemplateByNameAsync(int organizationId, string name);
    Task<bool> SetAsDefaultAsync(int templateId, int organizationId, int? branchId = null);
}
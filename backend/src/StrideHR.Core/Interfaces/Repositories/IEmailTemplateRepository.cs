using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IEmailTemplateRepository : IRepository<EmailTemplate>
{
    Task<EmailTemplate?> GetByNameAsync(string name);
    Task<List<EmailTemplate>> GetByTypeAsync(EmailTemplateType type);
    Task<List<EmailTemplate>> GetByCategoryAsync(string category);
    Task<List<EmailTemplate>> GetActiveTemplatesAsync();
    Task<List<EmailTemplate>> GetGlobalTemplatesAsync();
    Task<List<EmailTemplate>> GetBranchTemplatesAsync(int branchId);
    Task<bool> ExistsAsync(string name, int? excludeId = null);
    Task<int> GetUsageCountAsync(int templateId);
    Task<List<EmailTemplate>> SearchAsync(string searchTerm, int? branchId = null);
}
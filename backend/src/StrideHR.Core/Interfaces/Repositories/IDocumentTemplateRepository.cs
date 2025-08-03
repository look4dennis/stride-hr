using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IDocumentTemplateRepository : IRepository<DocumentTemplate>
{
    Task<IEnumerable<DocumentTemplate>> GetActiveTemplatesAsync();
    Task<IEnumerable<DocumentTemplate>> GetTemplatesByTypeAsync(DocumentType type);
    Task<IEnumerable<DocumentTemplate>> GetTemplatesByCategoryAsync(string category);
    Task<DocumentTemplate?> GetTemplateWithVersionsAsync(int id);
    Task<IEnumerable<string>> GetAvailableCategoriesAsync();
    Task<int> GetUsageCountAsync(int templateId);
    Task<bool> IsTemplateNameUniqueAsync(string name, int? excludeId = null);
}
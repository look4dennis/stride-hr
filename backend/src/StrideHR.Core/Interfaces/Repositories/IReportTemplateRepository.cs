using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IReportTemplateRepository : IRepository<ReportTemplate>
{
    Task<IEnumerable<ReportTemplate>> GetActiveTemplatesAsync();
    Task<IEnumerable<ReportTemplate>> GetTemplatesByTypeAsync(ReportType type);
    Task<IEnumerable<ReportTemplate>> GetTemplatesByCategoryAsync(string category);
    Task<IEnumerable<ReportTemplate>> GetSystemTemplatesAsync();
    Task<IEnumerable<ReportTemplate>> GetUserTemplatesAsync(int userId);
}
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IReportRepository : IRepository<Report>
{
    Task<IEnumerable<Report>> GetReportsByUserAsync(int userId, int? branchId = null);
    Task<IEnumerable<Report>> GetPublicReportsAsync(int? branchId = null);
    Task<IEnumerable<Report>> GetSharedReportsAsync(int userId);
    Task<Report?> GetReportWithExecutionsAsync(int reportId);
    Task<IEnumerable<Report>> GetScheduledReportsAsync();
    Task<IEnumerable<Report>> GetReportsByTypeAsync(ReportType type, int? branchId = null);
    Task<bool> HasPermissionAsync(int reportId, int userId, ReportPermission permission);
}
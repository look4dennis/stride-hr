using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models;

namespace StrideHR.Core.Interfaces.Services;

public interface IReportBuilderService
{
    Task<Report> CreateReportAsync(string name, string description, ReportType type, ReportBuilderConfiguration configuration, int userId, int? branchId = null);
    Task<Report> UpdateReportAsync(int reportId, string name, string description, ReportBuilderConfiguration configuration, int userId);
    Task<bool> DeleteReportAsync(int reportId, int userId);
    Task<Report?> GetReportAsync(int reportId, int userId);
    Task<IEnumerable<Report>> GetUserReportsAsync(int userId, int? branchId = null);
    Task<IEnumerable<Report>> GetPublicReportsAsync(int? branchId = null);
    Task<IEnumerable<Report>> GetSharedReportsAsync(int userId);
    Task<bool> ShareReportAsync(int reportId, int sharedWithUserId, ReportPermission permission, int sharedByUserId, DateTime? expiresAt = null);
    Task<bool> RevokeReportShareAsync(int reportId, int userId, int revokedByUserId);
    Task<ReportExecutionResult> ExecuteReportAsync(int reportId, int userId, Dictionary<string, object>? parameters = null);
    Task<ReportExecutionResult> PreviewReportAsync(ReportBuilderConfiguration configuration, int userId, int limit = 100);
    Task<IEnumerable<ReportDataSource>> GetAvailableDataSourcesAsync(int userId);
    Task<ReportDataSource> GetDataSourceSchemaAsync(string dataSourceName, int userId);
}
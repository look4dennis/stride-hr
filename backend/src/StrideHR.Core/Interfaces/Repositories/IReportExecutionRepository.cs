using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IReportExecutionRepository : IRepository<ReportExecution>
{
    Task<IEnumerable<ReportExecution>> GetExecutionsByReportAsync(int reportId, int limit = 50);
    Task<IEnumerable<ReportExecution>> GetExecutionsByUserAsync(int userId, int limit = 50);
    Task<IEnumerable<ReportExecution>> GetFailedExecutionsAsync();
    Task<ReportExecution?> GetLatestExecutionAsync(int reportId);
    Task<IEnumerable<ReportExecution>> GetExecutionsByStatusAsync(ReportExecutionStatus status);
}
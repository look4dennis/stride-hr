using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Services;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<List<AuditLog>> GetAuditLogsAsync(AuditLogFilter filter);
    Task<List<AuditLog>> GetUserAuditLogsAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<AuditLog>> GetSecurityAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> GetAuditLogCountAsync(AuditLogFilter filter);
}
using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Services;

public interface IAuditLogService
{
    Task LogEventAsync(string eventType, string description, int? userId = null, object? additionalData = null);
    Task LogSecurityEventAsync(string eventType, string description, int? userId = null, string? ipAddress = null, object? additionalData = null);
    Task<List<AuditLog>> GetAuditLogsAsync(AuditLogFilter filter);
    Task<List<AuditLog>> GetUserAuditLogsAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<AuditLog>> GetSecurityAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null);
}

public class AuditLogFilter
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? EventType { get; set; }
    public int? UserId { get; set; }
    public string? IpAddress { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
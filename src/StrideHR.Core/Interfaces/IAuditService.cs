using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Interface for audit logging service
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Log authentication event
    /// </summary>
    Task LogAuthenticationEventAsync(int? userId, string action, bool isSuccess, string? details = null, string? ipAddress = null, string? userAgent = null);
    
    /// <summary>
    /// Log security event
    /// </summary>
    Task LogSecurityEventAsync(int? userId, string action, AuditSeverity severity, string? details = null, string? ipAddress = null, string? userAgent = null);
    
    /// <summary>
    /// Log data access event
    /// </summary>
    Task LogDataAccessAsync(int? userId, string entityName, int? entityId, string action, string? details = null);
    
    /// <summary>
    /// Log data modification event
    /// </summary>
    Task LogDataModificationAsync(int? userId, string entityName, int? entityId, string action, object? oldValues = null, object? newValues = null);
    
    /// <summary>
    /// Log general audit event
    /// </summary>
    Task LogAuditEventAsync(AuditLog auditLog);
    
    /// <summary>
    /// Log a simple audit event with entity, action and details
    /// </summary>
    Task LogAsync(string entityName, int? entityId, string action, string details, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get audit logs for user
    /// </summary>
    Task<List<AuditLog>> GetUserAuditLogsAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Get audit logs for entity
    /// </summary>
    Task<List<AuditLog>> GetEntityAuditLogsAsync(string entityName, int entityId, DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Get security audit logs
    /// </summary>
    Task<List<AuditLog>> GetSecurityAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, AuditSeverity? severity = null);
}
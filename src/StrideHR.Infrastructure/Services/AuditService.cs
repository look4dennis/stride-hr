using System.Text.Json;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// Audit logging service implementation
/// </summary>
public class AuditService : IAuditService
{
    private readonly IRepository<AuditLog> _auditRepository;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IRepository<AuditLog> auditRepository, ILogger<AuditService> logger)
    {
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task LogAuthenticationEventAsync(int? userId, string action, bool isSuccess, string? details = null, string? ipAddress = null, string? userAgent = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Severity = isSuccess ? AuditSeverity.Information : AuditSeverity.Warning,
            Category = AuditCategory.Authentication,
            IsSuccess = isSuccess,
            CreatedAt = DateTime.UtcNow
        };

        await LogAuditEventAsync(auditLog);
    }

    public async Task LogSecurityEventAsync(int? userId, string action, AuditSeverity severity, string? details = null, string? ipAddress = null, string? userAgent = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Severity = severity,
            Category = AuditCategory.Security,
            IsSuccess = severity != AuditSeverity.Error && severity != AuditSeverity.Critical,
            CreatedAt = DateTime.UtcNow
        };

        await LogAuditEventAsync(auditLog);
    }

    public async Task LogDataAccessAsync(int? userId, string entityName, int? entityId, string action, string? details = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details,
            Severity = AuditSeverity.Information,
            Category = AuditCategory.DataAccess,
            IsSuccess = true,
            CreatedAt = DateTime.UtcNow
        };

        await LogAuditEventAsync(auditLog);
    }

    public async Task LogDataModificationAsync(int? userId, string entityName, int? entityId, string action, object? oldValues = null, object? newValues = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            Severity = AuditSeverity.Information,
            Category = AuditCategory.DataModification,
            IsSuccess = true,
            CreatedAt = DateTime.UtcNow
        };

        await LogAuditEventAsync(auditLog);
    }

    public async Task LogAuditEventAsync(AuditLog auditLog)
    {
        try
        {
            // Set default values if not provided
            auditLog.CreatedAt = auditLog.CreatedAt == default ? DateTime.UtcNow : auditLog.CreatedAt;
            auditLog.RequestId ??= Guid.NewGuid().ToString();

            await _auditRepository.AddAsync(auditLog);
            await _auditRepository.SaveChangesAsync();

            // Log to application logger as well for immediate visibility
            var logLevel = auditLog.Severity switch
            {
                AuditSeverity.Information => LogLevel.Information,
                AuditSeverity.Warning => LogLevel.Warning,
                AuditSeverity.Error => LogLevel.Error,
                AuditSeverity.Critical => LogLevel.Critical,
                _ => LogLevel.Information
            };

            _logger.Log(logLevel, "Audit Event: {Action} by User {UserId} - {Details}", 
                auditLog.Action, auditLog.UserId, auditLog.Details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit event: {Action} by User {UserId}", 
                auditLog.Action, auditLog.UserId);
            
            // Don't throw exception to avoid breaking the main operation
            // Audit logging should be non-blocking
        }
    }

    public async Task LogAsync(string entityName, int? entityId, string action, string details, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            Details = details,
            Severity = AuditSeverity.Information,
            Category = AuditCategory.DataModification,
            IsSuccess = true,
            CreatedAt = DateTime.UtcNow,
            RequestId = Guid.NewGuid().ToString()
        };

        await LogAuditEventAsync(auditLog);
    }

    public async Task<List<AuditLog>> GetUserAuditLogsAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = await _auditRepository.GetAllAsync();
        
        var filteredLogs = query.Where(log => log.UserId == userId);

        if (fromDate.HasValue)
        {
            filteredLogs = filteredLogs.Where(log => log.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            filteredLogs = filteredLogs.Where(log => log.CreatedAt <= toDate.Value);
        }

        return filteredLogs.OrderByDescending(log => log.CreatedAt).ToList();
    }

    public async Task<List<AuditLog>> GetEntityAuditLogsAsync(string entityName, int entityId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = await _auditRepository.GetAllAsync();
        
        var filteredLogs = query.Where(log => 
            log.EntityName == entityName && log.EntityId == entityId);

        if (fromDate.HasValue)
        {
            filteredLogs = filteredLogs.Where(log => log.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            filteredLogs = filteredLogs.Where(log => log.CreatedAt <= toDate.Value);
        }

        return filteredLogs.OrderByDescending(log => log.CreatedAt).ToList();
    }

    public async Task<List<AuditLog>> GetSecurityAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, AuditSeverity? severity = null)
    {
        var query = await _auditRepository.GetAllAsync();
        
        var filteredLogs = query.Where(log => 
            log.Category == AuditCategory.Security || 
            log.Category == AuditCategory.Authentication ||
            log.Category == AuditCategory.Authorization);

        if (fromDate.HasValue)
        {
            filteredLogs = filteredLogs.Where(log => log.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            filteredLogs = filteredLogs.Where(log => log.CreatedAt <= toDate.Value);
        }

        if (severity.HasValue)
        {
            filteredLogs = filteredLogs.Where(log => log.Severity == severity.Value);
        }

        return filteredLogs.OrderByDescending(log => log.CreatedAt).ToList();
    }
}
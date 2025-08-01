using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using System.Security.Claims;
using System.Text.Json;

namespace StrideHR.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IAuditLogRepository auditLogRepository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogEventAsync(string eventType, string description, int? userId = null, object? additionalData = null)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var currentUserId = userId ?? GetCurrentUserId();
            var currentUserName = GetCurrentUserName();

            var auditLog = new AuditLog
            {
                EventType = eventType,
                Description = description,
                UserId = currentUserId,
                UserName = currentUserName,
                IpAddress = GetClientIpAddress(),
                UserAgent = httpContext?.Request.Headers["User-Agent"].ToString(),
                AdditionalData = additionalData != null ? JsonSerializer.Serialize(additionalData) : null,
                Timestamp = DateTime.UtcNow,
                IsSecurityEvent = IsSecurityEvent(eventType)
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _auditLogRepository.SaveChangesAsync();

            _logger.LogInformation("Audit log created: {EventType} - {Description}", eventType, description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log for event: {EventType}", eventType);
        }
    }

    public async Task LogSecurityEventAsync(string eventType, string description, int? userId = null, string? ipAddress = null, object? additionalData = null)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var currentUserId = userId ?? GetCurrentUserId();
            var currentUserName = GetCurrentUserName();

            var auditLog = new AuditLog
            {
                EventType = eventType,
                Description = description,
                UserId = currentUserId,
                UserName = currentUserName,
                IpAddress = ipAddress ?? GetClientIpAddress(),
                UserAgent = httpContext?.Request.Headers["User-Agent"].ToString(),
                AdditionalData = additionalData != null ? JsonSerializer.Serialize(additionalData) : null,
                Timestamp = DateTime.UtcNow,
                IsSecurityEvent = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _auditLogRepository.SaveChangesAsync();

            _logger.LogWarning("Security audit log created: {EventType} - {Description}", eventType, description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create security audit log for event: {EventType}", eventType);
        }
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync(AuditLogFilter filter)
    {
        return await _auditLogRepository.GetAuditLogsAsync(filter);
    }

    public async Task<List<AuditLog>> GetUserAuditLogsAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        return await _auditLogRepository.GetUserAuditLogsAsync(userId, fromDate, toDate);
    }

    public async Task<List<AuditLog>> GetSecurityAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        return await _auditLogRepository.GetSecurityAuditLogsAsync(fromDate, toDate);
    }

    private int? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
        }
        return null;
    }

    private string? GetCurrentUserName()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            return httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        }
        return null;
    }

    private string GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return "Unknown";

        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        // Check for forwarded IP (when behind proxy/load balancer)
        if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        }
        else if (httpContext.Request.Headers.ContainsKey("X-Real-IP"))
        {
            ipAddress = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        }

        return ipAddress ?? "Unknown";
    }

    private static bool IsSecurityEvent(string eventType)
    {
        var securityEvents = new[]
        {
            "User.Login",
            "User.LoginFailed",
            "User.Logout",
            "User.PasswordChanged",
            "User.PasswordResetRequested",
            "User.AccountLocked",
            "User.AccountUnlocked",
            "User.Deactivated",
            "User.Activated",
            "User.ForcePasswordChange",
            "User.SessionTerminated",
            "User.AllSessionsTerminated",
            "Role.Assigned",
            "Role.Removed",
            "Permission.Granted",
            "Permission.Revoked",
            "Security.UnauthorizedAccess",
            "Security.InvalidToken",
            "Security.SuspiciousActivity"
        };

        return securityEvents.Any(se => eventType.StartsWith(se, StringComparison.OrdinalIgnoreCase));
    }
}
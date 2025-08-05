using Microsoft.Extensions.Logging;
using StrideHR.Core.Interfaces.Services;

namespace StrideHR.Infrastructure.Services;

public class SecurityEventService : ISecurityEventService
{
    private readonly ILogger<SecurityEventService> _logger;
    private readonly IAuditLogService _auditLogService;

    public SecurityEventService(ILogger<SecurityEventService> logger, IAuditLogService auditLogService)
    {
        _logger = logger;
        _auditLogService = auditLogService;
    }

    public async Task LogAuthenticationSuccessAsync(string userId, string employeeId, string ipAddress, string userAgent)
    {
        var logMessage = "Authentication successful for User {UserId}, Employee {EmployeeId} from {IpAddress} using {UserAgent}";
        _logger.LogInformation(logMessage, userId, employeeId, ipAddress, userAgent);

        await _auditLogService.LogSecurityEventAsync(
            eventType: "Authentication",
            description: $"Successful authentication for Employee {employeeId} from {ipAddress}",
            userId: int.TryParse(userId, out var uid) ? uid : null,
            ipAddress: ipAddress,
            additionalData: new { EmployeeId = employeeId, UserAgent = userAgent }
        );
    }

    public async Task LogAuthenticationFailureAsync(string reason, string ipAddress, string userAgent, string? attemptedUserId = null)
    {
        var logMessage = "Authentication failed: {Reason} from {IpAddress} using {UserAgent}";
        if (!string.IsNullOrEmpty(attemptedUserId))
        {
            logMessage += " for attempted User {AttemptedUserId}";
            _logger.LogWarning(logMessage, reason, ipAddress, userAgent, attemptedUserId);
        }
        else
        {
            _logger.LogWarning(logMessage, reason, ipAddress, userAgent);
        }

        await _auditLogService.LogSecurityEventAsync(
            eventType: "AuthenticationFailure",
            description: $"Authentication failed: {reason} from {ipAddress}",
            userId: int.TryParse(attemptedUserId ?? "", out var uid) ? uid : null,
            ipAddress: ipAddress,
            additionalData: new { Reason = reason, UserAgent = userAgent, AttemptedUserId = attemptedUserId }
        );
    }

    public async Task LogAuthorizationFailureAsync(string userId, string employeeId, string resource, string action, string ipAddress, string userAgent)
    {
        var logMessage = "Authorization failed for User {UserId}, Employee {EmployeeId} attempting {Action} on {Resource} from {IpAddress} using {UserAgent}";
        _logger.LogWarning(logMessage, userId, employeeId, action, resource, ipAddress, userAgent);

        await _auditLogService.LogSecurityEventAsync(
            eventType: "AuthorizationFailure",
            description: $"Authorization failed: User {userId} (Employee {employeeId}) attempted {action} on {resource}",
            userId: int.TryParse(userId, out var uid) ? uid : null,
            ipAddress: ipAddress,
            additionalData: new { EmployeeId = employeeId, Resource = resource, Action = action, UserAgent = userAgent }
        );
    }

    public async Task LogTokenValidationFailureAsync(string reason, string ipAddress, string userAgent, string? tokenInfo = null)
    {
        var logMessage = "Token validation failed: {Reason} from {IpAddress} using {UserAgent}";
        if (!string.IsNullOrEmpty(tokenInfo))
        {
            logMessage += " - Token info: {TokenInfo}";
            _logger.LogWarning(logMessage, reason, ipAddress, userAgent, tokenInfo);
        }
        else
        {
            _logger.LogWarning(logMessage, reason, ipAddress, userAgent);
        }

        await _auditLogService.LogSecurityEventAsync(
            eventType: "TokenValidationFailure",
            description: $"Token validation failed: {reason} from {ipAddress}. Token info: {tokenInfo ?? "N/A"}",
            userId: null,
            ipAddress: ipAddress,
            additionalData: new { Reason = reason, TokenInfo = tokenInfo, UserAgent = userAgent }
        );
    }

    public async Task LogSuspiciousActivityAsync(string activity, string userId, string ipAddress, string userAgent, string details)
    {
        var logMessage = "Suspicious activity detected: {Activity} by User {UserId} from {IpAddress} using {UserAgent} - {Details}";
        _logger.LogWarning(logMessage, activity, userId, ipAddress, userAgent, details);

        await _auditLogService.LogSecurityEventAsync(
            eventType: "SuspiciousActivity",
            description: $"Suspicious activity: {activity} - {details}",
            userId: int.TryParse(userId, out var uid) ? uid : null,
            ipAddress: ipAddress,
            additionalData: new { Activity = activity, Details = details, UserAgent = userAgent }
        );
    }
}
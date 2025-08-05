namespace StrideHR.Core.Interfaces.Services;

public interface ISecurityEventService
{
    Task LogAuthenticationSuccessAsync(string userId, string employeeId, string ipAddress, string userAgent);
    Task LogAuthenticationFailureAsync(string reason, string ipAddress, string userAgent, string? attemptedUserId = null);
    Task LogAuthorizationFailureAsync(string userId, string employeeId, string resource, string action, string ipAddress, string userAgent);
    Task LogTokenValidationFailureAsync(string reason, string ipAddress, string userAgent, string? tokenInfo = null);
    Task LogSuspiciousActivityAsync(string activity, string userId, string ipAddress, string userAgent, string details);
}
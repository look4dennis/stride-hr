using System.Security.Claims;
using StrideHR.Core.Interfaces;

namespace StrideHR.API.Middleware;

/// <summary>
/// JWT middleware for token validation and user context setup
/// </summary>
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IJwtService jwtService, IUserRepository userRepository)
    {
        var token = ExtractTokenFromRequest(context.Request);
        
        if (!string.IsNullOrEmpty(token))
        {
            await AttachUserToContextAsync(context, token, jwtService, userRepository);
        }

        await _next(context);
    }

    private string? ExtractTokenFromRequest(HttpRequest request)
    {
        // Check Authorization header
        var authHeader = request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        // Check query parameter (for SignalR connections)
        var tokenFromQuery = request.Query["access_token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(tokenFromQuery))
        {
            return tokenFromQuery;
        }

        return null;
    }

    private async Task AttachUserToContextAsync(HttpContext context, string token, IJwtService jwtService, IUserRepository userRepository)
    {
        try
        {
            var principal = jwtService.ValidateToken(token);
            if (principal == null)
                return;

            var userIdClaim = principal.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return;

            // Get user from database to ensure they still exist and are active
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null || user.Status != Core.Entities.UserStatus.Active)
                return;

            // Set the user principal
            context.User = principal;

            // Add user information to context items for easy access
            context.Items["UserId"] = userId;
            context.Items["User"] = user;
            context.Items["UserEmail"] = user.Email;
            context.Items["EmployeeId"] = user.EmployeeId;

            _logger.LogDebug("User {UserId} authenticated successfully", userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to attach user to context for token");
            // Don't throw - just continue without authentication
        }
    }
}
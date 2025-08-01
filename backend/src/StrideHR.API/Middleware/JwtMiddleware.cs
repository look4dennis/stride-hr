using Microsoft.Extensions.Options;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace StrideHR.API.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(RequestDelegate next, IOptions<JwtSettings> jwtSettings, ILogger<JwtMiddleware> logger)
    {
        _next = next;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IJwtService jwtService)
    {
        var token = ExtractTokenFromRequest(context.Request);

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var principal = jwtService.ValidateToken(token);
                if (principal != null)
                {
                    context.User = principal;
                    
                    // Add user info to context items for easy access
                    var userInfo = await jwtService.GetUserInfoFromTokenAsync(token);
                    if (userInfo != null)
                    {
                        context.Items["User"] = userInfo;
                        context.Items["UserId"] = userInfo.Id;
                        context.Items["EmployeeId"] = userInfo.EmployeeId;
                        context.Items["BranchId"] = userInfo.BranchId;
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid JWT token provided");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating JWT token");
            }
        }

        await _next(context);
    }

    private static string? ExtractTokenFromRequest(HttpRequest request)
    {
        // Check Authorization header
        var authHeader = request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            return authHeader["Bearer ".Length..].Trim();
        }

        // Check query parameter (for SignalR connections)
        if (request.Query.ContainsKey("access_token"))
        {
            return request.Query["access_token"];
        }

        return null;
    }
}
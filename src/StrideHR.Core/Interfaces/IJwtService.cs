using System.Security.Claims;
using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Interface for JWT token service
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate JWT access token
    /// </summary>
    string GenerateAccessToken(User user, List<string> roles, List<string> permissions);
    
    /// <summary>
    /// Generate refresh token
    /// </summary>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validate JWT token and extract claims
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);
    
    /// <summary>
    /// Get token expiration date
    /// </summary>
    DateTime GetTokenExpiration(string token);
    
    /// <summary>
    /// Extract user ID from token
    /// </summary>
    int? GetUserIdFromToken(string token);
    
    /// <summary>
    /// Extract claims from token
    /// </summary>
    Dictionary<string, string> GetClaimsFromToken(string token);
    
    /// <summary>
    /// Check if token is expired
    /// </summary>
    bool IsTokenExpired(string token);
}
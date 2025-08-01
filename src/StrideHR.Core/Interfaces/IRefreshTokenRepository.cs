using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Interface for RefreshToken repository
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>
    /// Get refresh token by token value
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(string token);
    
    /// <summary>
    /// Get active refresh tokens for user
    /// </summary>
    Task<List<RefreshToken>> GetActiveTokensForUserAsync(int userId);
    
    /// <summary>
    /// Get all refresh tokens for user
    /// </summary>
    Task<List<RefreshToken>> GetTokensForUserAsync(int userId);
    
    /// <summary>
    /// Revoke token
    /// </summary>
    Task RevokeTokenAsync(string token, int? revokedBy = null, string? reason = null);
    
    /// <summary>
    /// Revoke all tokens for user
    /// </summary>
    Task RevokeAllTokensForUserAsync(int userId, int? revokedBy = null, string? reason = null);
    
    /// <summary>
    /// Clean up expired tokens
    /// </summary>
    Task CleanupExpiredTokensAsync();
    
    /// <summary>
    /// Get expired tokens
    /// </summary>
    Task<List<RefreshToken>> GetExpiredTokensAsync();
    
    /// <summary>
    /// Replace token (for token rotation)
    /// </summary>
    Task<RefreshToken> ReplaceTokenAsync(string oldToken, string newToken, int userId, string? ipAddress = null, string? userAgent = null);
}
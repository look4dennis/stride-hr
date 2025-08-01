using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

/// <summary>
/// RefreshToken repository implementation
/// </summary>
public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .Include(rt => rt.User)
                .ThenInclude(u => u.Employee)
            .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsDeleted);
    }

    public async Task<List<RefreshToken>> GetActiveTokensForUserAsync(int userId)
    {
        return await _dbSet
            .Where(rt => 
                rt.UserId == userId && 
                !rt.IsRevoked && 
                rt.ExpiryDate > DateTime.UtcNow && 
                !rt.IsDeleted)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<RefreshToken>> GetTokensForUserAsync(int userId)
    {
        return await _dbSet
            .Where(rt => rt.UserId == userId && !rt.IsDeleted)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();
    }

    public async Task RevokeTokenAsync(string token, int? revokedBy = null, string? reason = null)
    {
        var refreshToken = await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token);
        if (refreshToken != null && !refreshToken.IsRevoked)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedBy = revokedBy;
            refreshToken.RevocationReason = reason;
            refreshToken.UpdatedAt = DateTime.UtcNow;
            
            _dbSet.Update(refreshToken);
        }
    }

    public async Task RevokeAllTokensForUserAsync(int userId, int? revokedBy = null, string? reason = null)
    {
        var activeTokens = await GetActiveTokensForUserAsync(userId);
        
        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedBy = revokedBy;
            token.RevocationReason = reason ?? "All tokens revoked";
            token.UpdatedAt = DateTime.UtcNow;
        }
        
        if (activeTokens.Any())
        {
            _dbSet.UpdateRange(activeTokens);
        }
    }

    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = await GetExpiredTokensAsync();
        
        if (expiredTokens.Any())
        {
            // Soft delete expired tokens
            foreach (var token in expiredTokens)
            {
                token.IsDeleted = true;
                token.DeletedAt = DateTime.UtcNow;
                token.DeletedBy = "System";
            }
            
            _dbSet.UpdateRange(expiredTokens);
        }
    }

    public async Task<List<RefreshToken>> GetExpiredTokensAsync()
    {
        return await _dbSet
            .Where(rt => 
                rt.ExpiryDate <= DateTime.UtcNow && 
                !rt.IsDeleted)
            .ToListAsync();
    }

    public async Task<RefreshToken> ReplaceTokenAsync(string oldToken, string newToken, int userId, string? ipAddress = null, string? userAgent = null)
    {
        // Revoke the old token
        await RevokeTokenAsync(oldToken, userId, "Token replaced");
        
        // Create new token
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = newToken,
            ExpiryDate = DateTime.UtcNow.AddDays(30), // Default 30 days
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };
        
        // Set the replacement reference
        var oldRefreshToken = await _dbSet.FirstOrDefaultAsync(rt => rt.Token == oldToken);
        if (oldRefreshToken != null)
        {
            oldRefreshToken.ReplacedByToken = newToken;
            _dbSet.Update(oldRefreshToken);
        }
        
        await _dbSet.AddAsync(refreshToken);
        return refreshToken;
    }

    public override async Task<IEnumerable<RefreshToken>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(rt => rt.User)
            .Where(rt => !rt.IsDeleted)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public override async Task<RefreshToken?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Id == id && !rt.IsDeleted, cancellationToken);
    }
}
using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && !u.IsDeleted);
    }

    public async Task<User?> GetByEmployeeIdAsync(string employeeId)
    {
        return await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Employee != null && u.Employee.EmployeeId == employeeId && !u.IsDeleted);
    }

    public async Task<User?> GetWithEmployeeAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.Employee)
            .ThenInclude(e => e.Branch)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
    }

    public async Task<User?> GetWithEmployeeByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Employee)
            .ThenInclude(e => e.Branch)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
    }

    public async Task<User?> GetWithRolesAndPermissionsAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.Employee)
            .ThenInclude(e => e.EmployeeRoles)
            .ThenInclude(er => er.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
    }

    public async Task<List<string>> GetUserRolesAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .ThenInclude(e => e.EmployeeRoles)
            .ThenInclude(er => er.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user?.Employee?.EmployeeRoles == null)
            return new List<string>();

        return user.Employee.EmployeeRoles
            .Where(er => er.IsActive && er.Role.IsActive && (er.ExpiryDate == null || er.ExpiryDate > DateTime.UtcNow))
            .Select(er => er.Role.Name)
            .ToList();
    }

    public async Task<List<string>> GetUserPermissionsAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .ThenInclude(e => e.EmployeeRoles)
            .ThenInclude(er => er.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user?.Employee?.EmployeeRoles == null)
            return new List<string>();

        return user.Employee.EmployeeRoles
            .Where(er => er.IsActive && er.Role.IsActive && (er.ExpiryDate == null || er.ExpiryDate > DateTime.UtcNow))
            .SelectMany(er => er.Role.RolePermissions)
            .Select(rp => $"{rp.Permission.Module}.{rp.Permission.Action}.{rp.Permission.Resource}")
            .Distinct()
            .ToList();
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
    }

    public async Task<bool> IsUsernameExistsAsync(string username)
    {
        return await _context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower() && !u.IsDeleted);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsDeleted);
    }

    public async Task<List<RefreshToken>> GetUserRefreshTokensAsync(int userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsDeleted)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token, string? ipAddress = null)
    {
        var refreshToken = await GetRefreshTokenAsync(token);
        if (refreshToken == null || !refreshToken.IsActive)
            return false;

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.UpdatedAt = DateTime.UtcNow;

        _context.RefreshTokens.Update(refreshToken);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> RevokeAllRefreshTokensAsync(int userId)
    {
        var refreshTokens = await GetUserRefreshTokensAsync(userId);
        var activeTokens = refreshTokens.Where(rt => rt.IsActive).ToList();

        if (!activeTokens.Any())
            return true;

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.UpdatedAt = DateTime.UtcNow;
        }

        _context.RefreshTokens.UpdateRange(activeTokens);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<UserSession?> GetUserSessionAsync(int userId, string sessionId)
    {
        return await _context.UserSessions
            .FirstOrDefaultAsync(us => us.UserId == userId && us.SessionId == sessionId && !us.IsDeleted);
    }

    public async Task<List<UserSession>> GetActiveUserSessionsAsync(int userId)
    {
        return await _context.UserSessions
            .Where(us => us.UserId == userId && us.IsActive && !us.IsDeleted)
            .OrderByDescending(us => us.LoginTime)
            .ToListAsync();
    }

    public async Task<bool> CreateUserSessionAsync(UserSession session)
    {
        await _context.UserSessions.AddAsync(session);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> EndUserSessionAsync(int userId, string sessionId)
    {
        var session = await GetUserSessionAsync(userId, sessionId);
        if (session == null || !session.IsActive)
            return false;

        session.IsActive = false;
        session.LogoutTime = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;

        _context.UserSessions.Update(session);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> EndAllUserSessionsAsync(int userId)
    {
        var sessions = await GetActiveUserSessionsAsync(userId);
        if (!sessions.Any())
            return true;

        foreach (var session in sessions)
        {
            session.IsActive = false;
            session.LogoutTime = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;
        }

        _context.UserSessions.UpdateRange(sessions);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<RefreshToken> AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        return refreshToken;
    }

    public async Task<List<User>> GetByBranchIdAsync(int branchId)
    {
        return await _context.Users
            .Include(u => u.Employee)
            .Where(u => u.Employee != null && u.Employee.BranchId == branchId && !u.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<User>> GetByRoleAsync(string role)
    {
        return await _context.Users
            .Include(u => u.Employee)
            .ThenInclude(e => e.EmployeeRoles)
            .ThenInclude(er => er.Role)
            .Where(u => u.Employee != null && 
                       u.Employee.EmployeeRoles.Any(er => er.Role.Name == role && er.IsActive) && 
                       !u.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string email, int? excludeId = null)
    {
        var query = _context.Users.Where(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
        
        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await GetByEmailAsync(email);
        if (user == null || !user.IsActive)
            return null;

        // Here you would verify the password hash
        // For now, returning the user if found
        return user;
    }
}
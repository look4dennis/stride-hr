using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByEmployeeIdAsync(string employeeId);
    Task<List<User>> GetByBranchIdAsync(int branchId);
    Task<List<User>> GetByRoleAsync(string role);
    Task<bool> ExistsAsync(string email, int? excludeId = null);
    Task<User?> ValidateCredentialsAsync(string email, string password);
    
    // Additional methods needed by existing services
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetWithEmployeeAsync(int userId);
    Task<User?> GetWithEmployeeByEmailAsync(string email);
    Task<User?> GetWithRolesAndPermissionsAsync(int userId);
    Task<List<string>> GetUserRolesAsync(int userId);
    Task<List<string>> GetUserPermissionsAsync(int userId);
    Task<bool> IsEmailExistsAsync(string email);
    Task<bool> IsUsernameExistsAsync(string username);
    
    // Refresh token methods
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task<List<RefreshToken>> GetUserRefreshTokensAsync(int userId);
    Task<bool> RevokeRefreshTokenAsync(string token, string? ipAddress = null);
    Task<bool> RevokeAllRefreshTokensAsync(int userId);
    Task<RefreshToken> AddRefreshTokenAsync(RefreshToken refreshToken);
    
    // User session methods
    Task<UserSession?> GetUserSessionAsync(int userId, string sessionId);
    Task<List<UserSession>> GetActiveUserSessionsAsync(int userId);
    Task<bool> CreateUserSessionAsync(UserSession session);
    Task<bool> EndUserSessionAsync(int userId, string sessionId);
    Task<bool> EndAllUserSessionsAsync(int userId);
}
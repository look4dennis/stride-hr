using StrideHR.Core.Entities;
using StrideHR.Core.Models.Authentication;

namespace StrideHR.Core.Interfaces.Services;

public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(LoginRequest request);
    Task<RefreshTokenResult> RefreshTokenAsync(RefreshTokenRequest request);
    Task<bool> LogoutAsync(int userId, string? ipAddress = null);
    Task<bool> LogoutAllSessionsAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<bool> ResetPasswordConfirmAsync(ResetPasswordConfirmRequest request);
    Task<bool> ValidateTokenAsync(string token);
    Task<UserInfo?> GetUserFromTokenAsync(string token);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, string? ipAddress = null);
    Task<List<UserSession>> GetActiveSessionsAsync(int userId);
    Task<bool> RevokeSessionAsync(int userId, string sessionId);
}
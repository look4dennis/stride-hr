using StrideHR.Core.Entities;
using StrideHR.Core.Models.Authentication;
using System.Security.Claims;

namespace StrideHR.Core.Interfaces.Services;

public interface IJwtService
{
    string GenerateToken(User user, Employee employee, List<string> roles, List<string> permissions);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    ClaimsPrincipal? ValidateToken(string token);
    Task<UserInfo?> GetUserInfoFromTokenAsync(string token);
    bool IsTokenExpired(string token);
    DateTime GetTokenExpiration(string token);
    bool ValidateTokenStructure(string token);
    string ExtractEmployeeId(string token);
    string ExtractOrganizationId(string token);
    string ExtractBranchId(string token);
}
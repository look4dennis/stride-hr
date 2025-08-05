using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Authentication;
using StrideHR.Core.Models.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace StrideHR.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IEmployeeService _employeeService;

    public JwtService(IOptions<JwtSettings> jwtSettings, IEmployeeService employeeService)
    {
        _jwtSettings = jwtSettings.Value;
        _employeeService = employeeService;
    }

    public string GenerateToken(User user, Employee employee, List<string> roles, List<string> permissions)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("EmployeeId", employee.Id.ToString()),
            new("EmployeeCode", employee.EmployeeId),
            new("FullName", $"{employee.FirstName} {employee.LastName}"),
            new("BranchId", employee.BranchId.ToString()),
            new("OrganizationId", employee.Branch?.OrganizationId.ToString() ?? "0"),
            new("Department", employee.Department ?? string.Empty),
            new("Designation", employee.Designation ?? string.Empty),
            new("IsFirstLogin", user.IsFirstLogin.ToString()),
            new("ForcePasswordChange", user.ForcePasswordChange.ToString()),
            new("IsTwoFactorEnabled", user.IsTwoFactorEnabled.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add roles
        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role));
        }

        // Add permissions
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var now = DateTime.UtcNow;
        var expires = now.AddHours(_jwtSettings.ExpirationHours);
        
        // Handle edge case for expired tokens in tests
        if (expires <= now)
        {
            now = expires.AddMinutes(-1);
        }
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = now,
            Expires = expires,
            IssuedAt = now,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[64];
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = _jwtSettings.ValidateAudience,
            ValidateIssuer = _jwtSettings.ValidateIssuer,
            ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.SecretKey)),
            ValidateLifetime = false, // Don't validate lifetime for expired token
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewMinutes)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            
            if (validatedToken is not JwtSecurityToken jwtToken || 
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = _jwtSettings.ValidateAudience,
            ValidateIssuer = _jwtSettings.ValidateIssuer,
            ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
            ValidateLifetime = _jwtSettings.ValidateLifetime,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.SecretKey)),
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewMinutes)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public async Task<UserInfo?> GetUserInfoFromTokenAsync(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null)
            return null;

        // Validate token structure and extract required claims
        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return null;

        var employeeIdClaim = principal.FindFirst("EmployeeId")?.Value;
        if (!int.TryParse(employeeIdClaim, out var employeeId))
            return null;

        var branchIdClaim = principal.FindFirst("BranchId")?.Value;
        if (!int.TryParse(branchIdClaim, out var branchId))
            return null;

        var organizationIdClaim = principal.FindFirst("OrganizationId")?.Value;
        if (!int.TryParse(organizationIdClaim, out var organizationId))
            organizationId = 0; // Default fallback

        return new UserInfo
        {
            Id = userId,
            EmployeeId = employeeId,
            Username = principal.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ?? string.Empty,
            Email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? string.Empty,
            FullName = principal.FindFirst("FullName")?.Value ?? string.Empty,
            BranchId = branchId,
            OrganizationId = organizationId,
            Roles = principal.FindAll("role").Select(c => c.Value).ToList(),
            Permissions = principal.FindAll("permission").Select(c => c.Value).ToList(),
            IsFirstLogin = bool.TryParse(principal.FindFirst("IsFirstLogin")?.Value, out var isFirstLogin) && isFirstLogin,
            ForcePasswordChange = bool.TryParse(principal.FindFirst("ForcePasswordChange")?.Value, out var forcePasswordChange) && forcePasswordChange,
            IsTwoFactorEnabled = bool.TryParse(principal.FindFirst("IsTwoFactorEnabled")?.Value, out var isTwoFactorEnabled) && isTwoFactorEnabled
        };
    }

    public bool IsTokenExpired(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }

    public DateTime GetTokenExpiration(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo;
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    public bool ValidateTokenStructure(string token)
    {
        try
        {
            var principal = ValidateToken(token);
            if (principal == null)
                return false;

            // Validate required claims are present
            var requiredClaims = new[]
            {
                JwtRegisteredClaimNames.Sub,
                JwtRegisteredClaimNames.UniqueName,
                JwtRegisteredClaimNames.Email,
                "EmployeeId",
                "BranchId",
                "OrganizationId"
            };

            foreach (var claimType in requiredClaims)
            {
                var claim = principal.FindFirst(claimType);
                if (claim == null || string.IsNullOrEmpty(claim.Value))
                    return false;
            }

            // Validate numeric claims can be parsed
            var numericClaims = new[] { JwtRegisteredClaimNames.Sub, "EmployeeId", "BranchId", "OrganizationId" };
            foreach (var claimType in numericClaims)
            {
                var claimValue = principal.FindFirst(claimType)?.Value;
                if (!int.TryParse(claimValue, out _))
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string ExtractEmployeeId(string token)
    {
        try
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst("EmployeeId")?.Value ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public string ExtractOrganizationId(string token)
    {
        try
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst("OrganizationId")?.Value ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public string ExtractBranchId(string token)
    {
        try
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst("BranchId")?.Value ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
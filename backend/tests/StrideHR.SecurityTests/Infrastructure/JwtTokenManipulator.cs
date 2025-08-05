using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StrideHR.SecurityTests.Infrastructure;

public class JwtTokenManipulator
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenManipulator(string secretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256", 
                              string issuer = "test-issuer", 
                              string audience = "test-audience")
    {
        _secretKey = secretKey;
        _issuer = issuer;
        _audience = audience;
    }

    public string CreateValidToken(string employeeId = "test-employee-id", 
                                  string organizationId = "test-org-id", 
                                  string branchId = "test-branch-id", 
                                  string[] roles = null,
                                  DateTime? expiry = null)
    {
        roles ??= new[] { "Employee" };
        expiry ??= DateTime.UtcNow.AddHours(1);

        var key = Encoding.ASCII.GetBytes(_secretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, employeeId),
            new("employeeId", employeeId),
            new("organizationId", organizationId),
            new("branchId", branchId),
            new(ClaimTypes.Email, "test@example.com"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiry,
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string CreateExpiredToken(string employeeId = "test-employee-id")
    {
        return CreateValidToken(employeeId, expiry: DateTime.UtcNow.AddHours(-1));
    }

    public string CreateTokenWithInvalidSignature(string employeeId = "test-employee-id")
    {
        var validToken = CreateValidToken(employeeId);
        var parts = validToken.Split('.');
        
        // Corrupt the signature
        var corruptedSignature = parts[2].Replace('a', 'b').Replace('A', 'B');
        return $"{parts[0]}.{parts[1]}.{corruptedSignature}";
    }

    public string CreateTokenWithMissingClaims(string employeeId = "test-employee-id")
    {
        var key = Encoding.ASCII.GetBytes(_secretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        // Create token with minimal claims (missing required claims)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, employeeId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            // Missing employeeId, organizationId, branchId claims
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string CreateTokenWithElevatedPrivileges(string employeeId = "test-employee-id")
    {
        return CreateValidToken(employeeId, roles: new[] { "Admin", "SuperAdmin", "SystemAdmin" });
    }

    public string CreateTokenWithDifferentOrganization(string employeeId = "test-employee-id", 
                                                      string targetOrganizationId = "different-org-id")
    {
        return CreateValidToken(employeeId, organizationId: targetOrganizationId);
    }

    public string CreateTokenWithDifferentBranch(string employeeId = "test-employee-id", 
                                                string targetBranchId = "different-branch-id")
    {
        return CreateValidToken(employeeId, branchId: targetBranchId);
    }

    public string CreateMalformedToken()
    {
        return "malformed.jwt.token";
    }

    public string CreateTokenWithNoneAlgorithm(string employeeId = "test-employee-id")
    {
        // Create a token with "none" algorithm (security vulnerability)
        var header = new { alg = "none", typ = "JWT" };
        var payload = new 
        { 
            sub = employeeId,
            employeeId = employeeId,
            organizationId = "test-org-id",
            branchId = "test-branch-id",
            exp = new DateTimeOffset(DateTime.UtcNow.AddHours(1)).ToUnixTimeSeconds(),
            iss = _issuer,
            aud = _audience
        };

        var headerJson = System.Text.Json.JsonSerializer.Serialize(header);
        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);

        var headerBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(headerJson))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        return $"{headerBase64}.{payloadBase64}.";
    }

    public TokenValidationResult ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            return new TokenValidationResult
            {
                IsValid = true,
                Principal = principal,
                Token = validatedToken as JwtSecurityToken
            };
        }
        catch (Exception ex)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                Error = ex.Message
            };
        }
    }
}

public class TokenValidationResult
{
    public bool IsValid { get; set; }
    public ClaimsPrincipal? Principal { get; set; }
    public JwtSecurityToken? Token { get; set; }
    public string? Error { get; set; }
}
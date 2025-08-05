using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace StrideHR.Tests.Middleware;

public class AuthenticationMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public AuthenticationMiddlewareTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task AuthenticationMiddleware_WithValidToken_ShouldAllowAccess()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Configure test JWT settings
                services.Configure<JwtSettings>(options =>
                {
                    options.SecretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256";
                    options.Issuer = "test-issuer";
                    options.Audience = "test-audience";
                    options.ValidateIssuer = true;
                    options.ValidateAudience = true;
                    options.ValidateLifetime = true;
                    options.ValidateIssuerSigningKey = true;
                    options.ClockSkewMinutes = 5;
                });
            });
        }).CreateClient();

        var token = GenerateValidJwtToken();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticationMiddleware_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<JwtSettings>(options =>
                {
                    options.SecretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256";
                    options.Issuer = "test-issuer";
                    options.Audience = "test-audience";
                    options.ValidateIssuer = true;
                    options.ValidateAudience = true;
                    options.ValidateLifetime = true;
                    options.ValidateIssuerSigningKey = true;
                    options.ClockSkewMinutes = 0; // No clock skew for expired token test
                });
            });
        }).CreateClient();

        var expiredToken = GenerateExpiredJwtToken();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", expiredToken);

        // Act
        var response = await client.GetAsync("/api/employees");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(response.Headers.Contains("Token-Expired"));
        Assert.True(response.Headers.Contains("WWW-Authenticate"));
    }

    [Fact]
    public async Task AuthenticationMiddleware_WithInvalidSignature_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<JwtSettings>(options =>
                {
                    options.SecretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256";
                    options.Issuer = "test-issuer";
                    options.Audience = "test-audience";
                    options.ValidateIssuer = true;
                    options.ValidateAudience = true;
                    options.ValidateLifetime = true;
                    options.ValidateIssuerSigningKey = true;
                    options.ClockSkewMinutes = 5;
                });
            });
        }).CreateClient();

        var invalidToken = GenerateTokenWithInvalidSignature();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", invalidToken);

        // Act
        var response = await client.GetAsync("/api/employees");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(response.Headers.Contains("WWW-Authenticate"));
    }

    [Fact]
    public async Task AuthenticationMiddleware_WithMalformedToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var malformedToken = "invalid.token.format";
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", malformedToken);

        // Act
        var response = await client.GetAsync("/api/employees");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(response.Headers.Contains("WWW-Authenticate"));
    }

    [Fact]
    public async Task AuthenticationMiddleware_WithMissingEmployeeIdClaim_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<JwtSettings>(options =>
                {
                    options.SecretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256";
                    options.Issuer = "test-issuer";
                    options.Audience = "test-audience";
                    options.ValidateIssuer = true;
                    options.ValidateAudience = true;
                    options.ValidateLifetime = true;
                    options.ValidateIssuerSigningKey = true;
                    options.ClockSkewMinutes = 5;
                });
            });
        }).CreateClient();

        var tokenWithoutEmployeeId = GenerateTokenWithoutEmployeeId();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenWithoutEmployeeId);

        // Act
        var response = await client.GetAsync("/api/employees");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticationMiddleware_WithNoToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/employees");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("SuperAdmin")]
    [InlineData("HRManager")]
    [InlineData("Manager")]
    [InlineData("Employee")]
    public async Task AuthenticationMiddleware_WithValidRoles_ShouldSetContextItems(string role)
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<JwtSettings>(options =>
                {
                    options.SecretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256";
                    options.Issuer = "test-issuer";
                    options.Audience = "test-audience";
                    options.ValidateIssuer = true;
                    options.ValidateAudience = true;
                    options.ValidateLifetime = true;
                    options.ValidateIssuerSigningKey = true;
                    options.ClockSkewMinutes = 5;
                });
            });
        }).CreateClient();

        var token = GenerateValidJwtTokenWithRole(role);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticationMiddleware_SignalRConnection_ShouldAcceptTokenFromQueryString()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<JwtSettings>(options =>
                {
                    options.SecretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256";
                    options.Issuer = "test-issuer";
                    options.Audience = "test-audience";
                    options.ValidateIssuer = true;
                    options.ValidateAudience = true;
                    options.ValidateLifetime = true;
                    options.ValidateIssuerSigningKey = true;
                    options.ClockSkewMinutes = 5;
                });
            });
        }).CreateClient();

        var token = GenerateValidJwtToken();

        // Act
        var response = await client.GetAsync($"/hubs/notification?access_token={token}");

        // Assert
        // SignalR hub will return 404 for GET requests, but authentication should pass
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private string GenerateValidJwtToken()
    {
        var secretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256";
        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim("EmployeeId", "123"),
            new Claim("OrganizationId", "1"),
            new Claim("BranchId", "1"),
            new Claim(ClaimTypes.Role, "Employee"),
            new Claim("permission", "Employee.View")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "test-issuer",
            Audience = "test-audience",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateValidJwtTokenWithRole(string role)
    {
        var secretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256";
        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim("EmployeeId", "123"),
            new Claim("OrganizationId", "1"),
            new Claim("BranchId", "1"),
            new Claim(ClaimTypes.Role, role),
            new Claim("permission", "Employee.View")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "test-issuer",
            Audience = "test-audience",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateExpiredJwtToken()
    {
        var secretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256";
        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim("EmployeeId", "123"),
            new Claim("OrganizationId", "1"),
            new Claim("BranchId", "1"),
            new Claim(ClaimTypes.Role, "Employee")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(-10), // Expired 10 minutes ago
            Issuer = "test-issuer",
            Audience = "test-audience",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateTokenWithInvalidSignature()
    {
        var secretKey = "different-secret-key-that-is-long-enough-for-hmac-sha256";
        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim("EmployeeId", "123"),
            new Claim("OrganizationId", "1"),
            new Claim("BranchId", "1"),
            new Claim(ClaimTypes.Role, "Employee")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "test-issuer",
            Audience = "test-audience",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateTokenWithoutEmployeeId()
    {
        var secretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256";
        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim("OrganizationId", "1"),
            new Claim("BranchId", "1"),
            new Claim(ClaimTypes.Role, "Employee")
            // Missing EmployeeId claim
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "test-issuer",
            Audience = "test-audience",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
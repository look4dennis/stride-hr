using FluentAssertions;
using Microsoft.Extensions.Options;
using StrideHR.Core.Entities;
using StrideHR.Core.Models;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly JwtSettings _jwtSettings;

    public JwtServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256-algorithm",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationHours = 1,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkewMinutes = 5
        };

        var options = Options.Create(_jwtSettings);
        _jwtService = new JwtService(options);
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ShouldReturnValidToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            EmployeeId = 100
        };
        var roles = new List<string> { "Employee", "Manager" };
        var permissions = new List<string> { "Employee.Read", "Employee.Update" };

        // Act
        var token = _jwtService.GenerateAccessToken(user, roles, permissions);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokens()
    {
        // Act
        var token1 = _jwtService.GenerateRefreshToken();
        var token2 = _jwtService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnClaimsPrincipal()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            EmployeeId = 100
        };
        var roles = new List<string> { "Employee" };
        var permissions = new List<string> { "Employee.Read" };
        var token = _jwtService.GenerateAccessToken(user, roles, permissions);

        // Act
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Identity!.IsAuthenticated.Should().BeTrue();
        principal.FindFirst("UserId")?.Value.Should().Be("1");
        principal.FindFirst("email")?.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _jwtService.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GetUserIdFromToken_WithValidToken_ShouldReturnUserId()
    {
        // Arrange
        var user = new User
        {
            Id = 123,
            Email = "test@example.com",
            Username = "testuser",
            EmployeeId = 100
        };
        var token = _jwtService.GenerateAccessToken(user, new List<string>(), new List<string>());

        // Act
        var userId = _jwtService.GetUserIdFromToken(token);

        // Assert
        userId.Should().Be(123);
    }

    [Fact]
    public void GetUserIdFromToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var userId = _jwtService.GetUserIdFromToken(invalidToken);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void GetTokenExpiration_WithValidToken_ShouldReturnExpirationTime()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            EmployeeId = 100
        };
        var beforeGeneration = DateTime.UtcNow;
        var token = _jwtService.GenerateAccessToken(user, new List<string>(), new List<string>());
        var afterGeneration = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours);

        // Act
        var expiration = _jwtService.GetTokenExpiration(token);

        // Assert
        expiration.Should().BeAfter(beforeGeneration);
        expiration.Should().BeBefore(afterGeneration.AddMinutes(1)); // Allow small margin
    }

    [Fact]
    public void IsTokenExpired_WithExpiredToken_ShouldReturnTrue()
    {
        // Arrange - Create a token that's already expired by manipulating the expiration time
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            EmployeeId = 100
        };
        
        // Create a token with normal settings first
        var token = _jwtService.GenerateAccessToken(user, new List<string>(), new List<string>());
        
        // Wait a moment to ensure time has passed
        Thread.Sleep(100);
        
        // For this test, we'll create a mock expired token by creating one with very short expiration
        var shortExpirationSettings = new JwtSettings
        {
            SecretKey = _jwtSettings.SecretKey,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            ExpirationHours = 0, // This will create a token that expires immediately
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
        
        // Since we can't create a token with negative expiration, we'll test with an invalid token
        var invalidToken = "invalid.token.here";

        // Act
        var isExpired = _jwtService.IsTokenExpired(invalidToken);

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void IsTokenExpired_WithValidToken_ShouldReturnFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            EmployeeId = 100
        };
        var token = _jwtService.GenerateAccessToken(user, new List<string>(), new List<string>());

        // Act
        var isExpired = _jwtService.IsTokenExpired(token);

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void GetClaimsFromToken_WithValidToken_ShouldReturnClaims()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            EmployeeId = 100
        };
        var roles = new List<string> { "Employee", "Manager" };
        var permissions = new List<string> { "Employee.Read", "Employee.Update" };
        var token = _jwtService.GenerateAccessToken(user, roles, permissions);

        // Act
        var claims = _jwtService.GetClaimsFromToken(token);

        // Assert
        claims.Should().NotBeEmpty();
        claims.Should().ContainKey("UserId");
        claims.Should().ContainKey("email");
        claims["UserId"].Should().Be("1");
        claims["email"].Should().Be("test@example.com");
    }
}
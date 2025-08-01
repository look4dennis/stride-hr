using Microsoft.Extensions.Options;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Configuration;
using StrideHR.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace StrideHR.Tests.Services;

public class JwtServiceTests
{
    private readonly Mock<IEmployeeService> _mockEmployeeService;
    private readonly JwtService _jwtService;
    private readonly JwtSettings _jwtSettings;

    public JwtServiceTests()
    {
        _mockEmployeeService = new Mock<IEmployeeService>();
        
        _jwtSettings = new JwtSettings
        {
            SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
            Issuer = "StrideHR",
            Audience = "StrideHR-Users",
            ExpirationHours = 24,
            RefreshTokenExpirationDays = 7
        };

        var options = Options.Create(_jwtSettings);
        _jwtService = new JwtService(options, _mockEmployeeService.Object);
    }

    [Fact]
    public void GenerateToken_ValidInput_ReturnsValidJwtToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            IsFirstLogin = false,
            ForcePasswordChange = false,
            IsTwoFactorEnabled = false
        };

        var employee = new Employee
        {
            Id = 1,
            EmployeeId = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            BranchId = 1,
            Department = "IT",
            Designation = "Developer"
        };

        var roles = new List<string> { "Employee", "Developer" };
        var permissions = new List<string> { "Employee.View", "Project.Create" };

        // Act
        var token = _jwtService.GenerateToken(user, employee, roles, permissions);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Verify token structure
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.Equal(_jwtSettings.Issuer, jwtToken.Issuer);
        Assert.Contains(_jwtSettings.Audience, jwtToken.Audiences);
        Assert.True(jwtToken.ValidTo > DateTime.UtcNow);

        // Verify claims
        var nameIdentifierClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.NotNull(nameIdentifierClaim);
        Assert.Equal(user.Id.ToString(), nameIdentifierClaim.Value);
        
        var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName);
        Assert.NotNull(nameClaim);
        Assert.Equal(user.Username, nameClaim.Value);
        
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        Assert.NotNull(emailClaim);
        Assert.Equal(user.Email, emailClaim.Value);
        
        var employeeIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "EmployeeId");
        Assert.NotNull(employeeIdClaim);
        Assert.Equal(employee.Id.ToString(), employeeIdClaim.Value);
        
        var employeeCodeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "EmployeeCode");
        Assert.NotNull(employeeCodeClaim);
        Assert.Equal(employee.EmployeeId, employeeCodeClaim.Value);

        // Verify roles
        var roleClaims = jwtToken.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();
        Assert.Equal(roles.Count, roleClaims.Count);
        Assert.Contains("Employee", roleClaims);
        Assert.Contains("Developer", roleClaims);

        // Verify permissions
        var permissionClaims = jwtToken.Claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
        Assert.Equal(permissions.Count, permissionClaims.Count);
        Assert.Contains("Employee.View", permissionClaims);
        Assert.Contains("Project.Create", permissionClaims);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsValidBase64String()
    {
        // Act
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Assert
        Assert.NotNull(refreshToken);
        Assert.NotEmpty(refreshToken);

        // Verify it's a valid base64 string
        var bytes = Convert.FromBase64String(refreshToken);
        Assert.Equal(64, bytes.Length);
    }

    [Fact]
    public void ValidateToken_ValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        var employee = new Employee
        {
            Id = 1,
            EmployeeId = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            BranchId = 1,
            Department = "IT",
            Designation = "Developer"
        };

        var roles = new List<string> { "Employee" };
        var permissions = new List<string> { "Employee.View" };

        var token = _jwtService.GenerateToken(user, employee, roles, permissions);

        // Act
        var principal = _jwtService.ValidateToken(token);

        // Assert
        Assert.NotNull(principal);
        Assert.Equal(user.Id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(user.Username, principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal(user.Email, principal.FindFirst(ClaimTypes.Email)?.Value);
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _jwtService.ValidateToken(invalidToken);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void ValidateToken_ExpiredToken_ReturnsNull()
    {
        // Arrange
        var expiredJwtSettings = new JwtSettings
        {
            SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
            Issuer = "StrideHR",
            Audience = "StrideHR-Users",
            ExpirationHours = -1 // Expired token
        };

        var expiredOptions = Options.Create(expiredJwtSettings);
        var expiredJwtService = new JwtService(expiredOptions, _mockEmployeeService.Object);

        var user = new User { Id = 1, Username = "testuser", Email = "test@example.com" };
        var employee = new Employee { Id = 1, EmployeeId = "EMP001", FirstName = "John", LastName = "Doe", BranchId = 1, Department = "IT", Designation = "Developer" };
        var roles = new List<string> { "Employee" };
        var permissions = new List<string> { "Employee.View" };

        var expiredToken = expiredJwtService.GenerateToken(user, employee, roles, permissions);

        // Act
        var principal = _jwtService.ValidateToken(expiredToken);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ExpiredToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var expiredJwtSettings = new JwtSettings
        {
            SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
            Issuer = "StrideHR",
            Audience = "StrideHR-Users",
            ExpirationHours = -1 // Expired token
        };

        var expiredOptions = Options.Create(expiredJwtSettings);
        var expiredJwtService = new JwtService(expiredOptions, _mockEmployeeService.Object);

        var user = new User { Id = 1, Username = "testuser", Email = "test@example.com" };
        var employee = new Employee { Id = 1, EmployeeId = "EMP001", FirstName = "John", LastName = "Doe", BranchId = 1, Department = "IT", Designation = "Developer" };
        var roles = new List<string> { "Employee" };
        var permissions = new List<string> { "Employee.View" };

        var expiredToken = expiredJwtService.GenerateToken(user, employee, roles, permissions);

        // Act
        var principal = _jwtService.GetPrincipalFromExpiredToken(expiredToken);

        // Assert
        Assert.NotNull(principal);
        Assert.Equal(user.Id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }

    [Fact]
    public void IsTokenExpired_ExpiredToken_ReturnsTrue()
    {
        // Arrange
        var expiredJwtSettings = new JwtSettings
        {
            SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
            Issuer = "StrideHR",
            Audience = "StrideHR-Users",
            ExpirationHours = -1 // Expired token
        };

        var expiredOptions = Options.Create(expiredJwtSettings);
        var expiredJwtService = new JwtService(expiredOptions, _mockEmployeeService.Object);

        var user = new User { Id = 1, Username = "testuser", Email = "test@example.com" };
        var employee = new Employee { Id = 1, EmployeeId = "EMP001", FirstName = "John", LastName = "Doe", BranchId = 1, Department = "IT", Designation = "Developer" };
        var roles = new List<string> { "Employee" };
        var permissions = new List<string> { "Employee.View" };

        var expiredToken = expiredJwtService.GenerateToken(user, employee, roles, permissions);

        // Act
        var isExpired = _jwtService.IsTokenExpired(expiredToken);

        // Assert
        Assert.True(isExpired);
    }

    [Fact]
    public void IsTokenExpired_ValidToken_ReturnsFalse()
    {
        // Arrange
        var user = new User { Id = 1, Username = "testuser", Email = "test@example.com" };
        var employee = new Employee { Id = 1, EmployeeId = "EMP001", FirstName = "John", LastName = "Doe", BranchId = 1, Department = "IT", Designation = "Developer" };
        var roles = new List<string> { "Employee" };
        var permissions = new List<string> { "Employee.View" };

        var token = _jwtService.GenerateToken(user, employee, roles, permissions);

        // Act
        var isExpired = _jwtService.IsTokenExpired(token);

        // Assert
        Assert.False(isExpired);
    }

    [Fact]
    public void GetTokenExpiration_ValidToken_ReturnsCorrectExpiration()
    {
        // Arrange
        var user = new User { Id = 1, Username = "testuser", Email = "test@example.com" };
        var employee = new Employee { Id = 1, EmployeeId = "EMP001", FirstName = "John", LastName = "Doe", BranchId = 1, Department = "IT", Designation = "Developer" };
        var roles = new List<string> { "Employee" };
        var permissions = new List<string> { "Employee.View" };

        var beforeGeneration = DateTime.UtcNow;
        var token = _jwtService.GenerateToken(user, employee, roles, permissions);
        var afterGeneration = DateTime.UtcNow;

        // Act
        var expiration = _jwtService.GetTokenExpiration(token);

        // Assert - Add small buffer for timing precision
        var expectedMinExpiration = beforeGeneration.AddHours(_jwtSettings.ExpirationHours).AddSeconds(-1);
        var expectedMaxExpiration = afterGeneration.AddHours(_jwtSettings.ExpirationHours).AddSeconds(1);

        Assert.True(expiration >= expectedMinExpiration, $"Expiration {expiration} should be >= {expectedMinExpiration}");
        Assert.True(expiration <= expectedMaxExpiration, $"Expiration {expiration} should be <= {expectedMaxExpiration}");
    }
}
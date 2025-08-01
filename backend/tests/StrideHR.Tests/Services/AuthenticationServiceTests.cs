using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Authentication;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<ILogger<AuthenticationService>> _mockLogger;
    private readonly AuthenticationService _authenticationService;

    public AuthenticationServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockLogger = new Mock<ILogger<AuthenticationService>>();

        _authenticationService = new AuthenticationService(
            _mockUserRepository.Object,
            _mockJwtService.Object,
            _mockPasswordService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsSuccessResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123",
            IpAddress = "127.0.0.1"
        };

        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            IsActive = true,
            FailedLoginAttempts = 0,
            Employee = new Employee
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BranchId = 1,
                Branch = new Branch { Id = 1, Name = "Main Branch" }
            }
        };

        var roles = new List<string> { "Employee" };
        var permissions = new List<string> { "Employee.View" };

        _mockUserRepository.Setup(x => x.GetWithEmployeeByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);
        _mockUserRepository.Setup(x => x.GetUserRolesAsync(user.Id))
            .ReturnsAsync(roles);
        _mockUserRepository.Setup(x => x.GetUserPermissionsAsync(user.Id))
            .ReturnsAsync(permissions);
        _mockJwtService.Setup(x => x.GenerateToken(user, user.Employee, roles, permissions))
            .Returns("jwt-token");
        _mockJwtService.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");
        _mockUserRepository.Setup(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken());
        _mockUserRepository.Setup(x => x.CreateUserSessionAsync(It.IsAny<UserSession>()))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.UpdateAsync(user))
            .Returns(Task.CompletedTask);
        _mockUserRepository.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _authenticationService.AuthenticateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.NotNull(result.User);
        Assert.Equal(user.Email, result.User.Email);
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidEmail_ReturnsFailureResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        _mockUserRepository.Setup(x => x.GetWithEmployeeByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authenticationService.AuthenticateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Message);
        Assert.Contains("Invalid credentials", result.Errors);
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidPassword_ReturnsFailureResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            IsActive = true,
            FailedLoginAttempts = 0,
            Employee = new Employee
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BranchId = 1,
                Branch = new Branch { Id = 1, Name = "Main Branch" }
            }
        };

        _mockUserRepository.Setup(x => x.GetWithEmployeeByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(false);
        _mockUserRepository.Setup(x => x.UpdateAsync(user))
            .Returns(Task.CompletedTask);
        _mockUserRepository.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _authenticationService.AuthenticateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Message);
        Assert.Contains("Invalid credentials", result.Errors);
        Assert.Equal(1, user.FailedLoginAttempts);
    }

    [Fact]
    public async Task AuthenticateAsync_InactiveUser_ReturnsFailureResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            IsActive = false,
            Employee = new Employee
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BranchId = 1,
                Branch = new Branch { Id = 1, Name = "Main Branch" }
            }
        };

        _mockUserRepository.Setup(x => x.GetWithEmployeeByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authenticationService.AuthenticateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Account is inactive", result.Message);
        Assert.Contains("Your account has been deactivated", result.Errors);
    }

    [Fact]
    public async Task AuthenticateAsync_LockedAccount_ReturnsFailureResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            IsActive = true,
            LockedUntil = DateTime.UtcNow.AddMinutes(30),
            Employee = new Employee
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BranchId = 1,
                Branch = new Branch { Id = 1, Name = "Main Branch" }
            }
        };

        _mockUserRepository.Setup(x => x.GetWithEmployeeByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authenticationService.AuthenticateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Account is temporarily locked", result.Message);
        Assert.Contains("Account is locked until", result.Errors[0]);
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "oldpassword",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        var user = new User
        {
            Id = userId,
            PasswordHash = "oldhash"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(x => x.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            .Returns(true);
        _mockPasswordService.Setup(x => x.IsPasswordStrong(request.NewPassword))
            .Returns(true);
        _mockPasswordService.Setup(x => x.HashPassword(request.NewPassword))
            .Returns("newhash");
        _mockUserRepository.Setup(x => x.UpdateAsync(user))
            .Returns(Task.CompletedTask);
        _mockUserRepository.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.RevokeAllRefreshTokensAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _authenticationService.ChangePasswordAsync(userId, request);

        // Assert
        Assert.True(result);
        Assert.Equal("newhash", user.PasswordHash);
        Assert.False(user.ForcePasswordChange);
        Assert.False(user.IsFirstLogin);
    }

    [Fact]
    public async Task ChangePasswordAsync_InvalidCurrentPassword_ReturnsFalse()
    {
        // Arrange
        var userId = 1;
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "wrongpassword",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        var user = new User
        {
            Id = userId,
            PasswordHash = "oldhash"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(x => x.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _authenticationService.ChangePasswordAsync(userId, request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateTokenAsync_ValidToken_ReturnsTrue()
    {
        // Arrange
        var token = "valid-token";

        _mockJwtService.Setup(x => x.ValidateToken(token))
            .Returns(new System.Security.Claims.ClaimsPrincipal());

        // Act
        var result = await _authenticationService.ValidateTokenAsync(token);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateTokenAsync_InvalidToken_ReturnsFalse()
    {
        // Arrange
        var token = "invalid-token";

        _mockJwtService.Setup(x => x.ValidateToken(token))
            .Returns((System.Security.Claims.ClaimsPrincipal?)null);

        // Act
        var result = await _authenticationService.ValidateTokenAsync(token);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task LogoutAsync_ValidUser_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var ipAddress = "127.0.0.1";

        _mockUserRepository.Setup(x => x.RevokeAllRefreshTokensAsync(userId))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.EndAllUserSessionsAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _authenticationService.LogoutAsync(userId, ipAddress);

        // Assert
        Assert.True(result);
        _mockUserRepository.Verify(x => x.RevokeAllRefreshTokensAsync(userId), Times.Once);
        _mockUserRepository.Verify(x => x.EndAllUserSessionsAsync(userId), Times.Once);
    }
}
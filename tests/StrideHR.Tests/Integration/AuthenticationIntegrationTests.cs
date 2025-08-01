using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Models;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Integration;

public class AuthenticationIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IAuthenticationService _authenticationService;
    private readonly IUserService _userService;
    private readonly IPasswordService _passwordService;

    public AuthenticationIntegrationTests()
    {
        var services = new ServiceCollection();
        
        // Configure settings
        var jwtSettings = new JwtSettings
        {
            SecretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256-algorithm",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationHours = 1
        };
        
        var securitySettings = new SecuritySettings
        {
            MaxFailedLoginAttempts = 3,
            LockoutDurationMinutes = 30,
            PasswordMinLength = 8,
            RequireUppercase = true,
            RequireLowercase = true,
            RequireDigits = true,
            RequireSpecialCharacters = true
        };

        services.Configure<JwtSettings>(options =>
        {
            options.SecretKey = jwtSettings.SecretKey;
            options.Issuer = jwtSettings.Issuer;
            options.Audience = jwtSettings.Audience;
            options.ExpirationHours = jwtSettings.ExpirationHours;
        });

        services.Configure<SecuritySettings>(options =>
        {
            options.MaxFailedLoginAttempts = securitySettings.MaxFailedLoginAttempts;
            options.LockoutDurationMinutes = securitySettings.LockoutDurationMinutes;
            options.PasswordMinLength = securitySettings.PasswordMinLength;
            options.RequireUppercase = securitySettings.RequireUppercase;
            options.RequireLowercase = securitySettings.RequireLowercase;
            options.RequireDigits = securitySettings.RequireDigits;
            options.RequireSpecialCharacters = securitySettings.RequireSpecialCharacters;
        });

        // Add logging
        services.AddLogging(builder => builder.AddConsole());

        // Add services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAuditService, MockAuditService>();
        services.AddScoped<IUserRepository, MockUserRepository>();
        services.AddScoped<IRefreshTokenRepository, MockRefreshTokenRepository>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();

        _serviceProvider = services.BuildServiceProvider();
        _authenticationService = _serviceProvider.GetRequiredService<IAuthenticationService>();
        _userService = _serviceProvider.GetRequiredService<IUserService>();
        _passwordService = _serviceProvider.GetRequiredService<IPasswordService>();
    }

    [Fact]
    public async Task AuthenticationWorkflow_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var email = "test@example.com";
        var password = "TestPassword123!";
        var ipAddress = "127.0.0.1";
        var userAgent = "Test User Agent";

        // Create a test user
        var user = await CreateTestUserAsync(email, password);

        // Act
        var result = await _authenticationService.AuthenticateAsync(email, password, ipAddress, userAgent);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be(email);
    }

    [Fact]
    public async Task AuthenticationWorkflow_WithInvalidPassword_ShouldFail()
    {
        // Arrange
        var email = "test@example.com";
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword123!";
        var ipAddress = "127.0.0.1";
        var userAgent = "Test User Agent";

        // Create a test user
        await CreateTestUserAsync(email, password);

        // Act
        var result = await _authenticationService.AuthenticateAsync(email, wrongPassword, ipAddress, userAgent);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Token.Should().BeNull();
        result.RefreshToken.Should().BeNull();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshTokenWorkflow_WithValidToken_ShouldSucceed()
    {
        // Arrange
        var email = "test@example.com";
        var password = "TestPassword123!";
        var ipAddress = "127.0.0.1";
        var userAgent = "Test User Agent";

        // Create a test user and authenticate
        await CreateTestUserAsync(email, password);
        var authResult = await _authenticationService.AuthenticateAsync(email, password, ipAddress, userAgent);

        // Act
        var refreshResult = await _authenticationService.RefreshTokenAsync(
            authResult.RefreshToken!, ipAddress, userAgent);

        // Assert
        refreshResult.Should().NotBeNull();
        refreshResult.Success.Should().BeTrue();
        refreshResult.Token.Should().NotBeNullOrEmpty();
        refreshResult.RefreshToken.Should().NotBeNullOrEmpty();
        refreshResult.Token.Should().NotBe(authResult.Token);
    }

    [Fact]
    public async Task PasswordChangeWorkflow_WithValidCurrentPassword_ShouldSucceed()
    {
        // Arrange
        var email = "test@example.com";
        var currentPassword = "TestPassword123!";
        var newPassword = "NewPassword456!";

        // Create a test user
        var user = await CreateTestUserAsync(email, currentPassword);

        // Act
        var result = await _authenticationService.ChangePasswordAsync(user.Id, currentPassword, newPassword);

        // Assert
        result.Should().BeTrue();

        // Verify new password works
        var authResult = await _authenticationService.AuthenticateAsync(email, newPassword, "127.0.0.1", "Test");
        authResult.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UserManagement_CreateAndManageUser_ShouldWork()
    {
        // Arrange
        var employeeId = 1;
        var email = "newuser@example.com";
        var username = "newuser";

        // Act - Create user
        var user = await _userService.CreateUserAsync(employeeId, email, username);

        // Assert - User created
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.Username.Should().Be(username);
        user.Status.Should().Be(UserStatus.Active);

        // Act - Lock user
        var lockResult = await _userService.LockUserAsync(user.Id, TimeSpan.FromMinutes(30), "Test lock");

        // Assert - User locked
        lockResult.Should().BeTrue();

        // Act - Unlock user
        var unlockResult = await _userService.UnlockUserAsync(user.Id);

        // Assert - User unlocked
        unlockResult.Should().BeTrue();
    }

    private Task<User> CreateTestUserAsync(string email, string password)
    {
        var (hash, salt) = _passwordService.HashPassword(password);
        
        var user = new User
        {
            Id = 1,
            EmployeeId = 1,
            Email = email,
            Username = email.Split('@')[0],
            PasswordHash = hash,
            PasswordSalt = salt,
            Status = UserStatus.Active,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            Employee = new Employee
            {
                Id = 1,
                EmployeeId = "EMP001",
                FirstName = "Test",
                LastName = "User",
                Email = email,
                BranchId = 1,
                Status = EmployeeStatus.Active,
                Branch = new Branch
                {
                    Id = 1,
                    Name = "Test Branch",
                    Country = "US",
                    Currency = "USD",
                    TimeZone = "UTC"
                }
            }
        };

        // Add to mock repository
        var userRepository = _serviceProvider.GetRequiredService<IUserRepository>() as MockUserRepository;
        userRepository?.AddUser(user);

        return Task.FromResult(user);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}

// Mock implementations for testing
public class MockAuditService : IAuditService
{
    public Task LogAuthenticationEventAsync(int? userId, string action, bool isSuccess, string? details = null, string? ipAddress = null, string? userAgent = null)
        => Task.CompletedTask;

    public Task LogSecurityEventAsync(int? userId, string action, AuditSeverity severity, string? details = null, string? ipAddress = null, string? userAgent = null)
        => Task.CompletedTask;

    public Task LogDataAccessAsync(int? userId, string entityName, int? entityId, string action, string? details = null)
        => Task.CompletedTask;

    public Task LogDataModificationAsync(int? userId, string entityName, int? entityId, string action, object? oldValues = null, object? newValues = null)
        => Task.CompletedTask;

    public Task LogAuditEventAsync(AuditLog auditLog)
        => Task.CompletedTask;

    public Task LogAsync(string entityName, int? entityId, string action, string details, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<List<AuditLog>> GetUserAuditLogsAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null)
        => Task.FromResult(new List<AuditLog>());

    public Task<List<AuditLog>> GetEntityAuditLogsAsync(string entityName, int entityId, DateTime? fromDate = null, DateTime? toDate = null)
        => Task.FromResult(new List<AuditLog>());

    public Task<List<AuditLog>> GetSecurityAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, AuditSeverity? severity = null)
        => Task.FromResult(new List<AuditLog>());
}

public class MockUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public void AddUser(User user) => _users.Add(user);

    public Task<User?> GetByEmailAsync(string email)
        => Task.FromResult(_users.FirstOrDefault(u => u.Email == email));

    public Task<User?> GetByUsernameAsync(string username)
        => Task.FromResult(_users.FirstOrDefault(u => u.Username == username));

    public Task<User?> GetWithEmployeeAsync(int userId)
        => Task.FromResult(_users.FirstOrDefault(u => u.Id == userId));

    public Task<User?> GetWithRolesAsync(int userId)
        => Task.FromResult(_users.FirstOrDefault(u => u.Id == userId));

    public Task<User?> GetByPasswordResetTokenAsync(string resetToken)
        => Task.FromResult(_users.FirstOrDefault(u => u.PasswordResetToken == resetToken));

    public Task<User?> GetByEmailVerificationTokenAsync(string verificationToken)
        => Task.FromResult(_users.FirstOrDefault(u => u.EmailVerificationToken == verificationToken));

    public Task<bool> EmailExistsAsync(string email)
        => Task.FromResult(_users.Any(u => u.Email == email));

    public Task<bool> UsernameExistsAsync(string username)
        => Task.FromResult(_users.Any(u => u.Username == username));

    public Task<List<User>> GetByStatusAsync(UserStatus status)
        => Task.FromResult(_users.Where(u => u.Status == status).ToList());

    public Task<List<User>> GetLockedUsersAsync()
        => Task.FromResult(_users.Where(u => u.Status == UserStatus.Locked).ToList());

    public Task UpdateLastLoginAsync(int userId, string ipAddress)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = ipAddress;
        }
        return Task.CompletedTask;
    }

    public Task IncrementFailedLoginAttemptsAsync(int userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.FailedLoginAttempts++;
        }
        return Task.CompletedTask;
    }

    public Task ResetFailedLoginAttemptsAsync(int userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.FailedLoginAttempts = 0;
        }
        return Task.CompletedTask;
    }

    public Task LockUserAsync(int userId, DateTime lockUntil)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.Status = UserStatus.Locked;
            user.LockedUntil = lockUntil;
        }
        return Task.CompletedTask;
    }

    public Task UnlockUserAsync(int userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.Status = UserStatus.Active;
            user.LockedUntil = null;
        }
        return Task.CompletedTask;
    }

    // IRepository implementation
    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_users.AsEnumerable());

    public Task<IEnumerable<User>> FindAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(_users.Where(predicate.Compile()));

    public Task<User?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(_users.FirstOrDefault(predicate.Compile()));

    public Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(_users.Any(predicate.Compile()));

    public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<User, bool>>? predicate = null, CancellationToken cancellationToken = default)
        => Task.FromResult(predicate == null ? _users.Count : _users.Count(predicate.Compile()));

    public Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, System.Linq.Expressions.Expression<Func<User, bool>>? predicate = null, System.Linq.Expressions.Expression<Func<User, object>>? orderBy = null, bool ascending = true, CancellationToken cancellationToken = default)
    {
        var query = predicate == null ? _users : _users.Where(predicate.Compile());
        var totalCount = query.Count();
        var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        return Task.FromResult((items, totalCount));
    }

    public Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        entity.Id = _users.Count + 1;
        _users.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<IEnumerable<User>> AddRangeAsync(IEnumerable<User> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            entity.Id = _users.Count + 1;
            _users.Add(entity);
        }
        return Task.FromResult(entities);
    }

    public Task<User> UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        var existing = _users.FirstOrDefault(u => u.Id == entity.Id);
        if (existing != null)
        {
            var index = _users.IndexOf(existing);
            _users[index] = entity;
        }
        return Task.FromResult(entity);
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            _users.Remove(user);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User entity, CancellationToken cancellationToken = default)
    {
        _users.Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<User> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            _users.Remove(entity);
        }
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(int id, string? deletedBy = null, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedBy = deletedBy;
        }
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(User entity, string? deletedBy = null, CancellationToken cancellationToken = default)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = deletedBy;
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(1);
}

public class MockRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly List<RefreshToken> _tokens = new();

    public Task<RefreshToken?> GetByTokenAsync(string token)
        => Task.FromResult(_tokens.FirstOrDefault(t => t.Token == token));

    public Task<List<RefreshToken>> GetActiveTokensForUserAsync(int userId)
        => Task.FromResult(_tokens.Where(t => t.UserId == userId && t.IsActive).ToList());

    public Task<List<RefreshToken>> GetTokensForUserAsync(int userId)
        => Task.FromResult(_tokens.Where(t => t.UserId == userId).ToList());

    public Task RevokeTokenAsync(string token, int? revokedBy = null, string? reason = null)
    {
        var refreshToken = _tokens.FirstOrDefault(t => t.Token == token);
        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task RevokeAllTokensForUserAsync(int userId, int? revokedBy = null, string? reason = null)
    {
        var userTokens = _tokens.Where(t => t.UserId == userId).ToList();
        foreach (var token in userTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task CleanupExpiredTokensAsync()
        => Task.CompletedTask;

    public Task<List<RefreshToken>> GetExpiredTokensAsync()
        => Task.FromResult(_tokens.Where(t => t.IsExpired).ToList());

    public Task<RefreshToken> ReplaceTokenAsync(string oldToken, string newToken, int userId, string? ipAddress = null, string? userAgent = null)
    {
        RevokeTokenAsync(oldToken);
        var refreshToken = new RefreshToken
        {
            Id = _tokens.Count + 1,
            UserId = userId,
            Token = newToken,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };
        _tokens.Add(refreshToken);
        return Task.FromResult(refreshToken);
    }

    // IRepository implementation
    public Task<RefreshToken?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_tokens.FirstOrDefault(t => t.Id == id));

    public Task<IEnumerable<RefreshToken>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_tokens.AsEnumerable());

    public Task<IEnumerable<RefreshToken>> FindAsync(System.Linq.Expressions.Expression<Func<RefreshToken, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(_tokens.Where(predicate.Compile()));

    public Task<RefreshToken?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<RefreshToken, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(_tokens.FirstOrDefault(predicate.Compile()));

    public Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<RefreshToken, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(_tokens.Any(predicate.Compile()));

    public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<RefreshToken, bool>>? predicate = null, CancellationToken cancellationToken = default)
        => Task.FromResult(predicate == null ? _tokens.Count : _tokens.Count(predicate.Compile()));

    public Task<(IEnumerable<RefreshToken> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, System.Linq.Expressions.Expression<Func<RefreshToken, bool>>? predicate = null, System.Linq.Expressions.Expression<Func<RefreshToken, object>>? orderBy = null, bool ascending = true, CancellationToken cancellationToken = default)
    {
        var query = predicate == null ? _tokens : _tokens.Where(predicate.Compile());
        var totalCount = query.Count();
        var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        return Task.FromResult((items, totalCount));
    }

    public Task<RefreshToken> AddAsync(RefreshToken entity, CancellationToken cancellationToken = default)
    {
        entity.Id = _tokens.Count + 1;
        _tokens.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<IEnumerable<RefreshToken>> AddRangeAsync(IEnumerable<RefreshToken> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            entity.Id = _tokens.Count + 1;
            _tokens.Add(entity);
        }
        return Task.FromResult(entities);
    }

    public Task<RefreshToken> UpdateAsync(RefreshToken entity, CancellationToken cancellationToken = default)
        => Task.FromResult(entity);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var token = _tokens.FirstOrDefault(t => t.Id == id);
        if (token != null)
        {
            _tokens.Remove(token);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(RefreshToken entity, CancellationToken cancellationToken = default)
    {
        _tokens.Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<RefreshToken> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            _tokens.Remove(entity);
        }
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(int id, string? deletedBy = null, CancellationToken cancellationToken = default)
    {
        var token = _tokens.FirstOrDefault(t => t.Id == id);
        if (token != null)
        {
            token.IsDeleted = true;
            token.DeletedAt = DateTime.UtcNow;
            token.DeletedBy = deletedBy;
        }
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(RefreshToken entity, string? deletedBy = null, CancellationToken cancellationToken = default)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = deletedBy;
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(1);
}
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Models;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// User management service implementation
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IAuditService _auditService;
    private readonly ILogger<UserService> _logger;
    private readonly SecuritySettings _securitySettings;

    public UserService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IAuditService auditService,
        ILogger<UserService> logger,
        IOptions<SecuritySettings> securitySettings)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _auditService = auditService;
        _logger = logger;
        _securitySettings = securitySettings.Value;
    }

    public async Task<User> CreateUserAsync(int employeeId, string email, string username, string? password = null, bool forcePasswordChange = true)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(email))
        {
            throw new InvalidOperationException($"User with email '{email}' already exists");
        }

        // Check if username already exists
        if (await _userRepository.UsernameExistsAsync(username))
        {
            throw new InvalidOperationException($"Username '{username}' already exists");
        }

        // Generate password if not provided
        if (string.IsNullOrEmpty(password))
        {
            password = _passwordService.GenerateRandomPassword();
        }

        // Validate password strength
        var passwordValidation = _passwordService.ValidatePasswordStrength(password);
        if (!passwordValidation.IsValid)
        {
            throw new ArgumentException($"Password validation failed: {string.Join(", ", passwordValidation.Errors)}");
        }

        // Hash password
        var (hash, salt) = _passwordService.HashPassword(password);

        // Create user
        var user = new User
        {
            EmployeeId = employeeId,
            Email = email,
            Username = username,
            PasswordHash = hash,
            PasswordSalt = salt,
            Status = UserStatus.Active,
            ForcePasswordChange = forcePasswordChange,
            PasswordChangedAt = DateTime.UtcNow,
            EmailVerificationToken = _passwordService.GenerateSecureToken(),
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(user.Id, "User Created", AuditSeverity.Information, 
            $"User account created for employee {employeeId}");

        _logger.LogInformation("User account created for employee {EmployeeId} with email {Email}", employeeId, email);
        return user;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetByUsernameAsync(username);
    }

    public async Task<User> UpdateUserProfileAsync(int userId, string? email = null, string? username = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found");
        }

        var changes = new List<string>();

        // Update email if provided
        if (!string.IsNullOrEmpty(email) && email != user.Email)
        {
            if (await _userRepository.EmailExistsAsync(email))
            {
                throw new InvalidOperationException($"Email '{email}' is already in use");
            }
            
            user.Email = email;
            user.IsEmailVerified = false;
            user.EmailVerificationToken = _passwordService.GenerateSecureToken();
            changes.Add($"Email changed to {email}");
        }

        // Update username if provided
        if (!string.IsNullOrEmpty(username) && username != user.Username)
        {
            if (await _userRepository.UsernameExistsAsync(username))
            {
                throw new InvalidOperationException($"Username '{username}' is already in use");
            }
            
            user.Username = username;
            changes.Add($"Username changed to {username}");
        }

        if (changes.Any())
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            await _auditService.LogSecurityEventAsync(userId, "User Profile Updated", AuditSeverity.Information, 
                string.Join(", ", changes));

            _logger.LogInformation("User {UserId} profile updated: {Changes}", userId, string.Join(", ", changes));
        }

        return user;
    }

    public async Task<bool> ActivateUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        if (user.Status == UserStatus.Active)
        {
            return true;
        }

        user.Status = UserStatus.Active;
        user.LockedUntil = null;
        user.FailedLoginAttempts = 0;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(userId, "User Activated", AuditSeverity.Information, 
            "User account activated");

        _logger.LogInformation("User {UserId} activated", userId);
        return true;
    }

    public async Task<bool> DeactivateUserAsync(int userId, string? reason = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.Status = UserStatus.Inactive;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(userId, "User Deactivated", AuditSeverity.Warning, 
            $"User account deactivated. Reason: {reason ?? "Not specified"}");

        _logger.LogInformation("User {UserId} deactivated. Reason: {Reason}", userId, reason ?? "Not specified");
        return true;
    }

    public async Task<bool> LockUserAsync(int userId, TimeSpan? lockDuration = null, string? reason = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var lockUntil = DateTime.UtcNow.Add(lockDuration ?? TimeSpan.FromMinutes(_securitySettings.LockoutDurationMinutes));
        
        user.Status = UserStatus.Locked;
        user.LockedUntil = lockUntil;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(userId, "User Locked", AuditSeverity.Warning, 
            $"User account locked until {lockUntil}. Reason: {reason ?? "Not specified"}");

        _logger.LogInformation("User {UserId} locked until {LockUntil}. Reason: {Reason}", 
            userId, lockUntil, reason ?? "Not specified");
        return true;
    }

    public async Task<bool> UnlockUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.Status = UserStatus.Active;
        user.LockedUntil = null;
        user.FailedLoginAttempts = 0;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(userId, "User Unlocked", AuditSeverity.Information, 
            "User account unlocked");

        _logger.LogInformation("User {UserId} unlocked", userId);
        return true;
    }

    public async Task<bool> SuspendUserAsync(int userId, string? reason = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.Status = UserStatus.Suspended;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(userId, "User Suspended", AuditSeverity.Warning, 
            $"User account suspended. Reason: {reason ?? "Not specified"}");

        _logger.LogInformation("User {UserId} suspended. Reason: {Reason}", userId, reason ?? "Not specified");
        return true;
    }

    public async Task<string> GenerateEmailVerificationTokenAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found");
        }

        var token = _passwordService.GenerateSecureToken();
        user.EmailVerificationToken = token;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(userId, "Email Verification Token Generated", AuditSeverity.Information, 
            "Email verification token generated");

        _logger.LogInformation("Email verification token generated for user {UserId}", userId);
        return token;
    }

    public async Task<bool> VerifyEmailAsync(string verificationToken)
    {
        var user = await _userRepository.GetByEmailVerificationTokenAsync(verificationToken);
        if (user == null)
        {
            return false;
        }

        user.IsEmailVerified = true;
        user.EmailVerifiedAt = DateTime.UtcNow;
        user.EmailVerificationToken = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(user.Id, "Email Verified", AuditSeverity.Information, 
            "Email address verified successfully");

        _logger.LogInformation("Email verified for user {UserId}", user.Id);
        return true;
    }

    public async Task<string> EnableTwoFactorAuthenticationAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found");
        }

        // Generate a secret key for 2FA
        var secretKey = _passwordService.GenerateSecureToken();
        
        user.TwoFactorEnabled = true;
        user.TwoFactorSecretKey = secretKey;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(userId, "Two-Factor Authentication Enabled", AuditSeverity.Information, 
            "Two-factor authentication enabled");

        _logger.LogInformation("Two-factor authentication enabled for user {UserId}", userId);
        return secretKey;
    }

    public async Task<bool> DisableTwoFactorAuthenticationAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.TwoFactorEnabled = false;
        user.TwoFactorSecretKey = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(userId, "Two-Factor Authentication Disabled", AuditSeverity.Information, 
            "Two-factor authentication disabled");

        _logger.LogInformation("Two-factor authentication disabled for user {UserId}", userId);
        return true;
    }

    public async Task<bool> VerifyTwoFactorCodeAsync(int userId, string code)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecretKey))
        {
            return false;
        }

        // In a real implementation, you would use a TOTP library to verify the code
        // For now, we'll just check if the code is not empty
        var isValid = !string.IsNullOrEmpty(code) && code.Length >= 6;

        if (isValid)
        {
            await _auditService.LogSecurityEventAsync(userId, "Two-Factor Code Verified", AuditSeverity.Information, 
                "Two-factor authentication code verified successfully");
        }
        else
        {
            await _auditService.LogSecurityEventAsync(userId, "Two-Factor Code Failed", AuditSeverity.Warning, 
                "Invalid two-factor authentication code provided");
        }

        return isValid;
    }

    public async Task<Dictionary<string, object>> GetUserSettingsAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.Settings))
        {
            return new Dictionary<string, object>();
        }

        try
        {
            var settings = JsonSerializer.Deserialize<Dictionary<string, object>>(user.Settings);
            return settings ?? new Dictionary<string, object>();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize user settings for user {UserId}", userId);
            return new Dictionary<string, object>();
        }
    }

    public async Task<bool> UpdateUserSettingsAsync(int userId, Dictionary<string, object> settings)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        try
        {
            user.Settings = JsonSerializer.Serialize(settings);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("User settings updated for user {UserId}", userId);
            return true;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to serialize user settings for user {UserId}", userId);
            return false;
        }
    }

    public async Task<List<User>> GetUsersByStatusAsync(UserStatus status)
    {
        return await _userRepository.GetByStatusAsync(status);
    }

    public async Task<List<User>> GetUsersByBranchAsync(int branchId)
    {
        var users = await _userRepository.GetAllAsync();
        return users.Where(u => u.Employee?.BranchId == branchId).ToList();
    }

    public async Task<List<User>> SearchUsersAsync(string searchTerm, int? branchId = null, UserStatus? status = null)
    {
        var users = await _userRepository.GetAllAsync();
        
        var query = users.AsQueryable();

        // Filter by search term
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(u => 
                u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (u.Employee != null && 
                 (u.Employee.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                  u.Employee.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))));
        }

        // Filter by branch
        if (branchId.HasValue)
        {
            query = query.Where(u => u.Employee != null && u.Employee.BranchId == branchId.Value);
        }

        // Filter by status
        if (status.HasValue)
        {
            query = query.Where(u => u.Status == status.Value);
        }

        return query.ToList();
    }

    public async Task<UserActivitySummary> GetUserActivitySummaryAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found");
        }

        // Get audit logs for the user
        var auditLogs = await _auditService.GetUserAuditLogsAsync(userId, fromDate, toDate);

        var summary = new UserActivitySummary
        {
            UserId = userId,
            LastLoginAt = user.LastLoginAt,
            LastLoginIp = user.LastLoginIp,
            LoginCount = auditLogs.Count(log => log.Action == "Login Successful"),
            FailedLoginAttempts = user.FailedLoginAttempts,
            LastPasswordChange = user.PasswordChangedAt,
            IsActive = user.Status == UserStatus.Active,
            IsLocked = user.Status == UserStatus.Locked,
            LockedUntil = user.LockedUntil,
            RecentActivities = auditLogs
                .OrderByDescending(log => log.CreatedAt)
                .Take(10)
                .Select(log => $"{log.CreatedAt:yyyy-MM-dd HH:mm}: {log.Action}")
                .ToList()
        };

        return summary;
    }

    public async Task<int> CleanupInactiveUsersAsync(TimeSpan inactivityThreshold)
    {
        var cutoffDate = DateTime.UtcNow.Subtract(inactivityThreshold);
        var users = await _userRepository.GetAllAsync();
        
        var inactiveUsers = users.Where(u => 
            u.Status == UserStatus.Active &&
            (u.LastLoginAt == null || u.LastLoginAt < cutoffDate)).ToList();

        var cleanedUpCount = 0;
        foreach (var user in inactiveUsers)
        {
            await DeactivateUserAsync(user.Id, "Automatic cleanup - inactive user");
            cleanedUpCount++;
        }

        _logger.LogInformation("Cleaned up {Count} inactive users", cleanedUpCount);
        return cleanedUpCount;
    }
}
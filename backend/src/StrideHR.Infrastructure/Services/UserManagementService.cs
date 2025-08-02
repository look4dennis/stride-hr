using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Authentication;

namespace StrideHR.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeService _employeeService;
    private readonly IRoleService _roleService;
    private readonly IPasswordService _passwordService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        IUserRepository userRepository,
        IEmployeeService employeeService,
        IRoleService roleService,
        IPasswordService passwordService,
        IAuditLogService auditLogService,
        ILogger<UserManagementService> logger)
    {
        _userRepository = userRepository;
        _employeeService = employeeService;
        _roleService = roleService;
        _passwordService = passwordService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        // Validate employee exists
        var employee = await _employeeService.GetByEmployeeIdAsync(request.EmployeeId);
        if (employee == null)
        {
            throw new InvalidOperationException($"Employee with ID {request.EmployeeId} not found");
        }

        // Check if user already exists for this employee
        var existingUser = await _userRepository.GetByEmployeeIdAsync(request.EmployeeId);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User already exists for employee {request.EmployeeId}");
        }

        // Check if email is already taken
        if (await _userRepository.IsEmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException($"Email {request.Email} is already in use");
        }

        // Check if username is already taken
        if (await _userRepository.IsUsernameExistsAsync(request.Username))
        {
            throw new InvalidOperationException($"Username {request.Username} is already in use");
        }

        // Generate password if not provided
        var password = request.TemporaryPassword ?? _passwordService.GenerateRandomPassword();
        var passwordHash = _passwordService.HashPassword(password);

        var user = new User
        {
            EmployeeId = employee.Id,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            IsActive = request.IsActive,
            ForcePasswordChange = request.ForcePasswordChange,
            IsFirstLogin = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Assign roles
        if (request.RoleIds.Any())
        {
            foreach (var roleId in request.RoleIds)
            {
                await _roleService.AssignRoleToEmployeeAsync(employee.Id, roleId);
            }
        }

        // Log audit event
        await _auditLogService.LogEventAsync(
            "User.Created",
            $"User created for employee {employee.FullName}",
            user.Id,
            new { request.Username, request.Email, request.RoleIds });

        _logger.LogInformation("User created successfully for employee {EmployeeId}", request.EmployeeId);

        // In a real implementation, you would send the temporary password via email
        _logger.LogInformation("Temporary password for user {Username}: {Password}", request.Username, password);

        return user;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetWithEmployeeAsync(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetWithEmployeeByEmailAsync(email);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return (await _userRepository.GetAllAsync()).ToList();
    }

    public async Task<List<User>> GetUsersByBranchAsync(int branchId)
    {
        var users = await _userRepository.GetAllAsync();
        return users.Where(u => u.Employee?.BranchId == branchId).ToList();
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", id);
            return false;
        }

        // Check if email is already taken by another user
        if (user.Email != request.Email && await _userRepository.IsEmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException($"Email {request.Email} is already in use");
        }

        // Check if username is already taken by another user
        if (user.Username != request.Username && await _userRepository.IsUsernameExistsAsync(request.Username))
        {
            throw new InvalidOperationException($"Username {request.Username} is already in use");
        }

        var oldValues = new { user.Username, user.Email, user.IsActive, user.ForcePasswordChange };

        user.Username = request.Username;
        user.Email = request.Email;
        user.IsActive = request.IsActive;
        user.ForcePasswordChange = request.ForcePasswordChange;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        var result = await _userRepository.SaveChangesAsync();

        // Update roles if provided
        if (request.RoleIds != null)
        {
            // Get current roles
            var currentRoles = await _roleService.GetEmployeeRolesAsync(user.EmployeeId);
            var currentRoleIds = currentRoles.Select(r => r.Id).ToList();

            // Remove roles that are not in the new list
            var rolesToRemove = currentRoleIds.Except(request.RoleIds).ToList();
            foreach (var roleId in rolesToRemove)
            {
                await _roleService.RemoveRoleFromEmployeeAsync(user.EmployeeId, roleId);
            }

            // Add new roles
            var rolesToAdd = request.RoleIds.Except(currentRoleIds).ToList();
            foreach (var roleId in rolesToAdd)
            {
                await _roleService.AssignRoleToEmployeeAsync(user.EmployeeId, roleId);
            }
        }

        if (result)
        {
            // Log audit event
            await _auditLogService.LogEventAsync(
                "User.Updated",
                $"User {user.Username} updated",
                user.Id,
                new { OldValues = oldValues, NewValues = request });

            _logger.LogInformation("User {UserId} updated successfully", id);
        }

        return result;
    }

    public async Task<bool> DeactivateUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", id);
            return false;
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        var result = await _userRepository.SaveChangesAsync();

        if (result)
        {
            // Revoke all refresh tokens and end sessions
            await _userRepository.RevokeAllRefreshTokensAsync(id);
            await _userRepository.EndAllUserSessionsAsync(id);

            // Log audit event
            await _auditLogService.LogEventAsync(
                "User.Deactivated",
                $"User {user.Username} deactivated",
                user.Id);

            _logger.LogInformation("User {UserId} deactivated successfully", id);
        }

        return result;
    }

    public async Task<bool> ActivateUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", id);
            return false;
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        var result = await _userRepository.SaveChangesAsync();

        if (result)
        {
            // Log audit event
            await _auditLogService.LogEventAsync(
                "User.Activated",
                $"User {user.Username} activated",
                user.Id);

            _logger.LogInformation("User {UserId} activated successfully", id);
        }

        return result;
    }

    public async Task<bool> ForcePasswordChangeAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", id);
            return false;
        }

        user.ForcePasswordChange = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        var result = await _userRepository.SaveChangesAsync();

        if (result)
        {
            // Revoke all refresh tokens to force re-login
            await _userRepository.RevokeAllRefreshTokensAsync(id);

            // Log audit event
            await _auditLogService.LogEventAsync(
                "User.ForcePasswordChange",
                $"Password change forced for user {user.Username}",
                user.Id);

            _logger.LogInformation("Password change forced for user {UserId}", id);
        }

        return result;
    }

    public async Task<bool> UnlockUserAccountAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", id);
            return false;
        }

        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        var result = await _userRepository.SaveChangesAsync();

        if (result)
        {
            // Log audit event
            await _auditLogService.LogEventAsync(
                "User.Unlocked",
                $"User account {user.Username} unlocked",
                user.Id);

            _logger.LogInformation("User account {UserId} unlocked successfully", id);
        }

        return result;
    }

    public async Task<UserProfile?> GetUserProfileAsync(int userId)
    {
        var user = await _userRepository.GetWithEmployeeAsync(userId);
        if (user?.Employee == null)
            return null;

        var roles = await _userRepository.GetUserRolesAsync(userId);
        var permissions = await _userRepository.GetUserPermissionsAsync(userId);

        return new UserProfile
        {
            Id = user.Id,
            EmployeeId = user.Employee.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.Employee.FullName,
            ProfilePhoto = user.Employee.ProfilePhoto,
            BranchId = user.Employee.BranchId,
            BranchName = user.Employee.Branch?.Name ?? string.Empty,
            Department = user.Employee.Department,
            Designation = user.Employee.Designation,
            IsActive = user.IsActive,
            IsFirstLogin = user.IsFirstLogin,
            ForcePasswordChange = user.ForcePasswordChange,
            IsTwoFactorEnabled = user.IsTwoFactorEnabled,
            LastLoginAt = user.LastLoginAt,
            LastPasswordChangeAt = user.LastPasswordChangeAt,
            Roles = roles,
            Permissions = permissions
        };
    }

    public async Task<bool> UpdateUserProfileAsync(int userId, UpdateUserProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
            return false;
        }

        var oldValues = new
        {
            user.SecurityQuestion,
            HasSecurityAnswer = !string.IsNullOrEmpty(user.SecurityAnswerHash),
            user.IsTwoFactorEnabled
        };

        user.SecurityQuestion = request.SecurityQuestion;
        if (!string.IsNullOrEmpty(request.SecurityAnswer))
        {
            user.SecurityAnswerHash = _passwordService.HashPassword(request.SecurityAnswer);
        }
        user.IsTwoFactorEnabled = request.IsTwoFactorEnabled;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        var result = await _userRepository.SaveChangesAsync();

        if (result)
        {
            // Log audit event
            await _auditLogService.LogEventAsync(
                "User.ProfileUpdated",
                $"Profile updated for user {user.Username}",
                user.Id,
                new { OldValues = oldValues, NewValues = request });

            _logger.LogInformation("Profile updated for user {UserId}", userId);
        }

        return result;
    }

    public async Task<List<UserSession>> GetUserSessionsAsync(int userId)
    {
        return await _userRepository.GetActiveUserSessionsAsync(userId);
    }

    public async Task<bool> TerminateUserSessionAsync(int userId, string sessionId)
    {
        var result = await _userRepository.EndUserSessionAsync(userId, sessionId);

        if (result)
        {
            // Log audit event
            await _auditLogService.LogEventAsync(
                "User.SessionTerminated",
                $"Session {sessionId} terminated for user {userId}",
                userId,
                new { sessionId });

            _logger.LogInformation("Session {SessionId} terminated for user {UserId}", sessionId, userId);
        }

        return result;
    }

    public async Task<bool> TerminateAllUserSessionsAsync(int userId)
    {
        var result = await _userRepository.EndAllUserSessionsAsync(userId);

        if (result)
        {
            // Also revoke all refresh tokens
            await _userRepository.RevokeAllRefreshTokensAsync(userId);

            // Log audit event
            await _auditLogService.LogEventAsync(
                "User.AllSessionsTerminated",
                $"All sessions terminated for user {userId}",
                userId);

            _logger.LogInformation("All sessions terminated for user {UserId}", userId);
        }

        return result;
    }
}
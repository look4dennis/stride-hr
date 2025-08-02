using StrideHR.Core.Entities;
using StrideHR.Core.Models.Authentication;

namespace StrideHR.Core.Interfaces.Services;

public interface IUserManagementService
{
    Task<User> CreateUserAsync(CreateUserRequest request);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<List<User>> GetAllUsersAsync();
    Task<List<User>> GetUsersByBranchAsync(int branchId);
    Task<bool> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<bool> DeactivateUserAsync(int id);
    Task<bool> ActivateUserAsync(int id);
    Task<bool> ForcePasswordChangeAsync(int id);
    Task<bool> UnlockUserAccountAsync(int id);
    Task<UserProfile?> GetUserProfileAsync(int userId);
    Task<bool> UpdateUserProfileAsync(int userId, UpdateUserProfileRequest request);
    Task<List<UserSession>> GetUserSessionsAsync(int userId);
    Task<bool> TerminateUserSessionAsync(int userId, string sessionId);
    Task<bool> TerminateAllUserSessionsAsync(int userId);
}

public class CreateUserRequest
{
    public string EmployeeId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? TemporaryPassword { get; set; }
    public bool ForcePasswordChange { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public List<int> RoleIds { get; set; } = new();
}

public class UpdateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool ForcePasswordChange { get; set; }
    public List<int>? RoleIds { get; set; }
}

public class UpdateUserProfileRequest
{
    public string? SecurityQuestion { get; set; }
    public string? SecurityAnswer { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
}

public class UserProfile
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePhoto { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsFirstLogin { get; set; }
    public bool ForcePasswordChange { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastPasswordChangeAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}
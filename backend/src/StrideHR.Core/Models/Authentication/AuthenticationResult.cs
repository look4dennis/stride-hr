namespace StrideHR.Core.Models.Authentication;

public class AuthenticationResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserInfo? User { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class UserInfo
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ProfilePhoto { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public int OrganizationId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public bool IsFirstLogin { get; set; }
    public bool ForcePasswordChange { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
}
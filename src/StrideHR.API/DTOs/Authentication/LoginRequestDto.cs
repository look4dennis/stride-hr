using System.ComponentModel.DataAnnotations;

namespace StrideHR.API.DTOs.Authentication;

/// <summary>
/// Login request DTO
/// </summary>
public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(1, ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Remember me option for extended token expiration
    /// </summary>
    public bool RememberMe { get; set; } = false;
}

/// <summary>
/// Login response DTO
/// </summary>
public class LoginResponseDto
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserInfoDto? User { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresPasswordChange { get; set; }
    public bool RequiresTwoFactor { get; set; }
}

/// <summary>
/// User information DTO
/// </summary>
public class UserInfoDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePhotoPath { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public BranchInfoDto? Branch { get; set; }
}

/// <summary>
/// Branch information DTO
/// </summary>
public class BranchInfoDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
}
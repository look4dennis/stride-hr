using System.ComponentModel.DataAnnotations;

namespace StrideHR.API.DTOs.Authentication;

/// <summary>
/// Change password request DTO
/// </summary>
public class ChangePasswordRequestDto
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare("NewPassword", ErrorMessage = "Password confirmation does not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Reset password request DTO
/// </summary>
public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "Reset token is required")]
    public string ResetToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare("NewPassword", ErrorMessage = "Password confirmation does not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Forgot password request DTO
/// </summary>
public class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Force password change request DTO (Admin only)
/// </summary>
public class ForcePasswordChangeRequestDto
{
    [Required(ErrorMessage = "User ID is required")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public string NewPassword { get; set; } = string.Empty;

    public bool ForceChangeOnNextLogin { get; set; } = true;
}

/// <summary>
/// Password validation response DTO
/// </summary>
public class PasswordValidationResponseDto
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public string Strength { get; set; } = string.Empty;
    public int StrengthScore { get; set; }
}
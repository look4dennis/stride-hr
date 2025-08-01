using System.ComponentModel.DataAnnotations;

namespace StrideHR.API.DTOs.Authentication;

/// <summary>
/// Refresh token request DTO
/// </summary>
public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Refresh token response DTO
/// </summary>
public class RefreshTokenResponseDto
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Revoke token request DTO
/// </summary>
public class RevokeTokenRequestDto
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
    
    public string? Reason { get; set; }
}
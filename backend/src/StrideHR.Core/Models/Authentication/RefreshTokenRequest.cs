using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Authentication;

public class RefreshTokenRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;

    public string? IpAddress { get; set; }
}

public class RefreshTokenResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}
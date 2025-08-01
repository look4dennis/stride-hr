namespace StrideHR.Core.Interfaces;

/// <summary>
/// Interface for password hashing and validation service
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hash password with salt
    /// </summary>
    (string hash, string salt) HashPassword(string password);
    
    /// <summary>
    /// Verify password against hash and salt
    /// </summary>
    bool VerifyPassword(string password, string hash, string salt);
    
    /// <summary>
    /// Generate secure random password
    /// </summary>
    string GenerateRandomPassword(int length = 12);
    
    /// <summary>
    /// Validate password strength
    /// </summary>
    PasswordValidationResult ValidatePasswordStrength(string password);
    
    /// <summary>
    /// Generate secure token for password reset
    /// </summary>
    string GenerateSecureToken();
}

/// <summary>
/// Password validation result
/// </summary>
public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public PasswordStrength Strength { get; set; }
}

/// <summary>
/// Password strength enumeration
/// </summary>
public enum PasswordStrength
{
    VeryWeak = 1,
    Weak = 2,
    Fair = 3,
    Good = 4,
    Strong = 5
}
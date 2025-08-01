using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Models;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// Password hashing and validation service implementation
/// </summary>
public class PasswordService : IPasswordService
{
    private readonly SecuritySettings _securitySettings;
    private const int SaltSize = 32; // 256 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100000; // PBKDF2 iterations

    public PasswordService(IOptions<SecuritySettings> securitySettings)
    {
        _securitySettings = securitySettings.Value;
    }

    public (string hash, string salt) HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        // Generate a random salt
        var salt = GenerateSalt();
        
        // Hash the password with the salt
        var hash = HashPasswordWithSalt(password, salt);
        
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt))
            return false;

        try
        {
            var saltBytes = Convert.FromBase64String(salt);
            var hashBytes = Convert.FromBase64String(hash);
            
            var computedHash = HashPasswordWithSalt(password, saltBytes);
            
            // Use constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(hashBytes, computedHash);
        }
        catch
        {
            return false;
        }
    }

    public string GenerateRandomPassword(int length = 12)
    {
        if (length < 8)
            length = 8;

        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        
        var password = new StringBuilder();
        var random = new Random();

        // Ensure at least one character from each required category
        if (_securitySettings.RequireLowercase)
            password.Append(lowercase[random.Next(lowercase.Length)]);
        
        if (_securitySettings.RequireUppercase)
            password.Append(uppercase[random.Next(uppercase.Length)]);
        
        if (_securitySettings.RequireDigits)
            password.Append(digits[random.Next(digits.Length)]);
        
        if (_securitySettings.RequireSpecialCharacters)
            password.Append(specialChars[random.Next(specialChars.Length)]);

        // Fill the rest with random characters from all categories
        var allChars = lowercase + uppercase + digits + specialChars;
        for (int i = password.Length; i < length; i++)
        {
            password.Append(allChars[random.Next(allChars.Length)]);
        }

        // Shuffle the password to avoid predictable patterns
        return ShuffleString(password.ToString());
    }

    public PasswordValidationResult ValidatePasswordStrength(string password)
    {
        var result = new PasswordValidationResult();
        var errors = new List<string>();

        if (string.IsNullOrEmpty(password))
        {
            errors.Add("Password is required");
            result.IsValid = false;
            result.Errors = errors;
            result.Strength = PasswordStrength.VeryWeak;
            return result;
        }

        // Check length
        if (password.Length < _securitySettings.PasswordMinLength)
        {
            errors.Add($"Password must be at least {_securitySettings.PasswordMinLength} characters long");
        }

        if (password.Length > _securitySettings.PasswordMaxLength)
        {
            errors.Add($"Password must not exceed {_securitySettings.PasswordMaxLength} characters");
        }

        // Check character requirements
        if (_securitySettings.RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add("Password must contain at least one uppercase letter");
        }

        if (_securitySettings.RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add("Password must contain at least one lowercase letter");
        }

        if (_securitySettings.RequireDigits && !password.Any(char.IsDigit))
        {
            errors.Add("Password must contain at least one digit");
        }

        if (_securitySettings.RequireSpecialCharacters && !Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]"))
        {
            errors.Add("Password must contain at least one special character");
        }

        // Check for common weak patterns
        if (ContainsCommonPatterns(password))
        {
            errors.Add("Password contains common patterns and may be easily guessed");
        }

        result.IsValid = errors.Count == 0;
        result.Errors = errors;
        result.Strength = CalculatePasswordStrength(password);

        return result;
    }

    public string GenerateSecureToken()
    {
        var tokenBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private byte[] GenerateSalt()
    {
        var salt = new byte[SaltSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    private byte[] HashPasswordWithSalt(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(HashSize);
    }

    private string ShuffleString(string input)
    {
        var array = input.ToCharArray();
        var random = new Random();
        
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
        
        return new string(array);
    }

    private bool ContainsCommonPatterns(string password)
    {
        var commonPatterns = new[]
        {
            @"123456", @"password", @"qwerty", @"abc123", @"admin",
            @"letmein", @"welcome", @"monkey", @"dragon", @"master"
        };

        return commonPatterns.Any(pattern => 
            password.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private PasswordStrength CalculatePasswordStrength(string password)
    {
        var score = 0;

        // Length scoring
        if (password.Length >= 8) score++;
        if (password.Length >= 12) score++;
        if (password.Length >= 16) score++;

        // Character variety scoring
        if (password.Any(char.IsLower)) score++;
        if (password.Any(char.IsUpper)) score++;
        if (password.Any(char.IsDigit)) score++;
        if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]")) score++;

        // Complexity scoring
        if (password.Length > 12 && HasGoodVariety(password)) score++;

        return score switch
        {
            <= 2 => PasswordStrength.VeryWeak,
            3 => PasswordStrength.Weak,
            4 => PasswordStrength.Fair,
            5 => PasswordStrength.Good,
            >= 6 => PasswordStrength.Strong,
        };
    }

    private bool HasGoodVariety(string password)
    {
        var categories = 0;
        if (password.Any(char.IsLower)) categories++;
        if (password.Any(char.IsUpper)) categories++;
        if (password.Any(char.IsDigit)) categories++;
        if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]")) categories++;
        
        return categories >= 3;
    }
}
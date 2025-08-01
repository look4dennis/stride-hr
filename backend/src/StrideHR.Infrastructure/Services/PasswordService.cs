using StrideHR.Core.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace StrideHR.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        var salt = GenerateSalt();
        return HashPassword(password, salt);
    }

    public string HashPassword(string password, string salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);
        return Convert.ToBase64String(hash) + ":" + salt;
    }

    public bool VerifyPassword(string password, string hash)
    {
        var parts = hash.Split(':');
        if (parts.Length != 2)
            return false;

        var storedHash = parts[0];
        var salt = parts[1];

        return VerifyPassword(password, storedHash, salt);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), Iterations, HashAlgorithmName.SHA256);
        var computedHash = pbkdf2.GetBytes(HashSize);
        var computedHashString = Convert.ToBase64String(computedHash);

        return computedHashString == hash;
    }

    public string GenerateSalt()
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);
        return Convert.ToBase64String(salt);
    }

    public bool IsPasswordStrong(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        // Check for at least one uppercase letter
        if (!Regex.IsMatch(password, @"[A-Z]"))
            return false;

        // Check for at least one lowercase letter
        if (!Regex.IsMatch(password, @"[a-z]"))
            return false;

        // Check for at least one digit
        if (!Regex.IsMatch(password, @"\d"))
            return false;

        // Check for at least one special character
        if (!Regex.IsMatch(password, @"[@$!%*?&]"))
            return false;

        return true;
    }

    public string GenerateRandomPassword(int length = 12)
    {
        const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specialChars = "@$!%*?&";
        const string allChars = upperCase + lowerCase + digits + specialChars;

        using var rng = RandomNumberGenerator.Create();
        var password = new StringBuilder();

        // Ensure at least one character from each category
        password.Append(GetRandomChar(upperCase, rng));
        password.Append(GetRandomChar(lowerCase, rng));
        password.Append(GetRandomChar(digits, rng));
        password.Append(GetRandomChar(specialChars, rng));

        // Fill the rest randomly
        for (int i = 4; i < length; i++)
        {
            password.Append(GetRandomChar(allChars, rng));
        }

        // Shuffle the password
        return ShuffleString(password.ToString(), rng);
    }

    public string GeneratePasswordResetToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenBytes = new byte[32];
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    public bool ValidatePasswordResetToken(string token)
    {
        try
        {
            var tokenBytes = Convert.FromBase64String(token.Replace("-", "+").Replace("_", "/") + "==");
            return tokenBytes.Length == 32;
        }
        catch
        {
            return false;
        }
    }

    private static char GetRandomChar(string chars, RandomNumberGenerator rng)
    {
        var randomBytes = new byte[4];
        rng.GetBytes(randomBytes);
        var randomIndex = Math.Abs(BitConverter.ToInt32(randomBytes, 0)) % chars.Length;
        return chars[randomIndex];
    }

    private static string ShuffleString(string input, RandomNumberGenerator rng)
    {
        var array = input.ToCharArray();
        for (int i = array.Length - 1; i > 0; i--)
        {
            var randomBytes = new byte[4];
            rng.GetBytes(randomBytes);
            var j = Math.Abs(BitConverter.ToInt32(randomBytes, 0)) % (i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
        return new string(array);
    }
}
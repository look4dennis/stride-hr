using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    static void Main()
    {
        string password = "adminsuper2025$";
        string salt = GenerateSalt();
        string hash = HashPassword(password, salt);
        
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash: {hash}");
        Console.WriteLine($"Salt: {salt}");
        
        // Test verification
        var parts = hash.Split(':');
        bool isValid = VerifyPassword(password, parts[0], parts[1]);
        Console.WriteLine($"Verification: {isValid}");
    }
    
    static bool VerifyPassword(string password, string hash, string salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), Iterations, HashAlgorithmName.SHA256);
        var computedHash = pbkdf2.GetBytes(HashSize);
        var computedHashString = Convert.ToBase64String(computedHash);

        return computedHashString == hash;
    }

    static string HashPassword(string password, string salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);
        return Convert.ToBase64String(hash) + ":" + salt;
    }

    static string GenerateSalt()
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);
        return Convert.ToBase64String(salt);
    }
}

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
        string password = "admin123";
        string salt = GenerateSalt();
        string hash = HashPassword(password, salt);
        
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash: {hash}");
        Console.WriteLine($"Salt: {salt}");
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

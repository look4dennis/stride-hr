using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Interfaces;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// Data encryption service implementation
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly string _encryptionKey;
    private readonly ILogger<EncryptionService> _logger;

    public EncryptionService(IConfiguration configuration, ILogger<EncryptionService> logger)
    {
        _encryptionKey = configuration["Encryption:Key"] ?? "default-encryption-key-change-in-production";
        _logger = logger;
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        try
        {
            return EncryptWithKey(plainText, _encryptionKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data");
            throw;
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            return DecryptWithKey(cipherText, _encryptionKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data");
            throw;
        }
    }

    public string Hash(string data)
    {
        if (string.IsNullOrEmpty(data))
            return string.Empty;

        try
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hashedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing data");
            throw;
        }
    }

    public string GenerateKey()
    {
        try
        {
            using var aes = Aes.Create();
            aes.GenerateKey();
            return Convert.ToBase64String(aes.Key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating encryption key");
            throw;
        }
    }

    public string EncryptWithKey(string plainText, string key)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        try
        {
            var keyBytes = Convert.FromBase64String(PadKey(key));
            
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            
            // Write IV to the beginning of the stream
            msEncrypt.Write(aes.IV, 0, aes.IV.Length);
            
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data with custom key");
            throw;
        }
    }

    public string DecryptWithKey(string cipherText, string key)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            var keyBytes = Convert.FromBase64String(PadKey(key));
            var cipherBytes = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = keyBytes;

            // Extract IV from the beginning of the cipher text
            var iv = new byte[aes.IV.Length];
            Array.Copy(cipherBytes, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(cipherBytes, iv.Length, cipherBytes.Length - iv.Length);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data with custom key");
            throw;
        }
    }

    public bool VerifyHash(string data, string hash)
    {
        if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            var computedHash = Hash(data);
            return string.Equals(computedHash, hash, StringComparison.Ordinal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying hash");
            return false;
        }
    }

    private string PadKey(string key)
    {
        // Ensure key is 32 bytes (256 bits) for AES-256
        var keyBytes = Encoding.UTF8.GetBytes(key);
        
        if (keyBytes.Length == 32)
        {
            return Convert.ToBase64String(keyBytes);
        }
        
        var paddedKey = new byte[32];
        if (keyBytes.Length > 32)
        {
            Array.Copy(keyBytes, paddedKey, 32);
        }
        else
        {
            Array.Copy(keyBytes, paddedKey, keyBytes.Length);
            // Fill remaining bytes with a pattern
            for (int i = keyBytes.Length; i < 32; i++)
            {
                paddedKey[i] = (byte)(i % 256);
            }
        }
        
        return Convert.ToBase64String(paddedKey);
    }
}
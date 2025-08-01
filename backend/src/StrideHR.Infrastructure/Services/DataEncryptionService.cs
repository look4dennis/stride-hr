using Microsoft.Extensions.Options;
using StrideHR.Core.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;

namespace StrideHR.Infrastructure.Services;

public class DataEncryptionService : IDataEncryptionService
{
    private readonly EncryptionSettings _settings;
    private const string EncryptionPrefix = "ENC:";
    private const int IvSize = 16; // 128 bits
    private const int KeySize = 32; // 256 bits

    public DataEncryptionService(IOptions<EncryptionSettings> settings)
    {
        _settings = settings.Value;
    }

    public string Encrypt(string plainText)
    {
        if (!_settings.EnableEncryption || string.IsNullOrEmpty(plainText))
            return plainText;

        try
        {
            using var aes = Aes.Create();
            var key = DeriveKey(_settings.MasterKey, _settings.Salt);
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Combine IV and encrypted data
            var result = new byte[IvSize + encryptedBytes.Length];
            Array.Copy(aes.IV, 0, result, 0, IvSize);
            Array.Copy(encryptedBytes, 0, result, IvSize, encryptedBytes.Length);

            return EncryptionPrefix + Convert.ToBase64String(result);
        }
        catch
        {
            // If encryption fails, return original text (for backward compatibility)
            return plainText;
        }
    }

    public string Decrypt(string cipherText)
    {
        if (!_settings.EnableEncryption || string.IsNullOrEmpty(cipherText) || !IsDataEncrypted(cipherText))
            return cipherText;

        try
        {
            var encryptedData = Convert.FromBase64String(cipherText[EncryptionPrefix.Length..]);
            
            using var aes = Aes.Create();
            var key = DeriveKey(_settings.MasterKey, _settings.Salt);
            aes.Key = key;

            // Extract IV and encrypted data
            var iv = new byte[IvSize];
            var encrypted = new byte[encryptedData.Length - IvSize];
            Array.Copy(encryptedData, 0, iv, 0, IvSize);
            Array.Copy(encryptedData, IvSize, encrypted, 0, encrypted.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            // If decryption fails, return original text
            return cipherText;
        }
    }

    public string EncryptSensitiveData(string data, string purpose)
    {
        if (string.IsNullOrEmpty(data))
            return data;

        // Use purpose-specific key derivation for additional security
        var purposeKey = DeriveKey(_settings.MasterKey, _settings.Salt + purpose);
        
        try
        {
            using var aes = Aes.Create();
            aes.Key = purposeKey;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(data);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var result = new byte[IvSize + encryptedBytes.Length];
            Array.Copy(aes.IV, 0, result, 0, IvSize);
            Array.Copy(encryptedBytes, 0, result, IvSize, encryptedBytes.Length);

            return EncryptionPrefix + Convert.ToBase64String(result);
        }
        catch
        {
            return data;
        }
    }

    public string DecryptSensitiveData(string encryptedData, string purpose)
    {
        if (string.IsNullOrEmpty(encryptedData) || !IsDataEncrypted(encryptedData))
            return encryptedData;

        var purposeKey = DeriveKey(_settings.MasterKey, _settings.Salt + purpose);

        try
        {
            var cipherData = Convert.FromBase64String(encryptedData[EncryptionPrefix.Length..]);
            
            using var aes = Aes.Create();
            aes.Key = purposeKey;

            var iv = new byte[IvSize];
            var encrypted = new byte[cipherData.Length - IvSize];
            Array.Copy(cipherData, 0, iv, 0, IvSize);
            Array.Copy(cipherData, IvSize, encrypted, 0, encrypted.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return encryptedData;
        }
    }

    public string HashSensitiveData(string data)
    {
        if (string.IsNullOrEmpty(data))
            return data;

        using var sha256 = SHA256.Create();
        var saltedData = data + _settings.Salt;
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedData));
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyHashedData(string data, string hash)
    {
        if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(hash))
            return false;

        var computedHash = HashSensitiveData(data);
        return computedHash == hash;
    }

    public string GenerateSecureToken(int length = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenBytes = new byte[length];
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "")[..length];
    }

    public bool IsDataEncrypted(string data)
    {
        return !string.IsNullOrEmpty(data) && data.StartsWith(EncryptionPrefix);
    }

    private byte[] DeriveKey(string password, string salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            Encoding.UTF8.GetBytes(salt),
            _settings.KeyDerivationIterations,
            HashAlgorithmName.SHA256);
        
        return pbkdf2.GetBytes(KeySize);
    }
}
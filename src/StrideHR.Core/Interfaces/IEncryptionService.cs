namespace StrideHR.Core.Interfaces;

/// <summary>
/// Interface for data encryption service
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypt sensitive data
    /// </summary>
    string Encrypt(string plainText);
    
    /// <summary>
    /// Decrypt sensitive data
    /// </summary>
    string Decrypt(string cipherText);
    
    /// <summary>
    /// Hash sensitive data (one-way)
    /// </summary>
    string Hash(string data);
    
    /// <summary>
    /// Generate secure random key
    /// </summary>
    string GenerateKey();
    
    /// <summary>
    /// Encrypt data with custom key
    /// </summary>
    string EncryptWithKey(string plainText, string key);
    
    /// <summary>
    /// Decrypt data with custom key
    /// </summary>
    string DecryptWithKey(string cipherText, string key);
    
    /// <summary>
    /// Verify hash
    /// </summary>
    bool VerifyHash(string data, string hash);
}
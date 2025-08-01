namespace StrideHR.Core.Interfaces.Services;

public interface IDataEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string EncryptSensitiveData(string data, string purpose);
    string DecryptSensitiveData(string encryptedData, string purpose);
    string HashSensitiveData(string data);
    bool VerifyHashedData(string data, string hash);
    string GenerateSecureToken(int length = 32);
    bool IsDataEncrypted(string data);
}

public class EncryptionSettings
{
    public const string SectionName = "EncryptionSettings";
    
    public string MasterKey { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public int KeyDerivationIterations { get; set; } = 10000;
    public bool EnableEncryption { get; set; } = true;
}
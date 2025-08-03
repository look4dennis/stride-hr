namespace StrideHR.Core.Interfaces.Services;

public interface IDigitalSignatureService
{
    Task<string> CreateSignatureHashAsync(string signatureData, int documentId, int signerId);
    Task<bool> ValidateSignatureAsync(string signatureHash, string signatureData, int documentId, int signerId);
    Task<byte[]> ApplyDigitalSignatureAsync(byte[] documentContent, string signatureData, string signerName, DateTime signedAt);
    Task<bool> VerifyDocumentIntegrityAsync(int documentId);
    Task<string> GenerateSignatureTokenAsync(int documentId, int signerId, TimeSpan? expiry = null);
    Task<bool> ValidateSignatureTokenAsync(string token, int documentId, int signerId);
    Task<Dictionary<string, object>> GetSignatureMetadataAsync(int documentId);
}
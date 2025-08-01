namespace StrideHR.Core.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(byte[] fileData, string fileName, string folder);
    Task<byte[]?> GetFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    Task<bool> FileExistsAsync(string filePath);
    string GetFileUrl(string filePath);
}
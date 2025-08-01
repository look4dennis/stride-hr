namespace StrideHR.Core.Interfaces.Services;

public interface IPasswordService
{
    string HashPassword(string password);
    string HashPassword(string password, string salt);
    bool VerifyPassword(string password, string hash);
    bool VerifyPassword(string password, string hash, string salt);
    string GenerateSalt();
    bool IsPasswordStrong(string password);
    string GenerateRandomPassword(int length = 12);
    string GeneratePasswordResetToken();
    bool ValidatePasswordResetToken(string token);
}
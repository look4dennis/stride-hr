using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService;

    public PasswordServiceTests()
    {
        _passwordService = new PasswordService();
    }

    [Fact]
    public void HashPassword_ValidPassword_ReturnsHashWithSalt()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _passwordService.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.Contains(":", hash); // Hash should contain salt separator
        
        var parts = hash.Split(':');
        Assert.Equal(2, parts.Length); // Should have hash and salt parts
    }

    [Fact]
    public void HashPassword_WithSalt_ReturnsConsistentHash()
    {
        // Arrange
        var password = "TestPassword123!";
        var salt = _passwordService.GenerateSalt();

        // Act
        var hash1 = _passwordService.HashPassword(password, salt);
        var hash2 = _passwordService.HashPassword(password, salt);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithSeparateSalt_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var salt = _passwordService.GenerateSalt();
        var hashWithSalt = _passwordService.HashPassword(password, salt);
        var hashPart = hashWithSalt.Split(':')[0];

        // Act
        var result = _passwordService.VerifyPassword(password, hashPart, salt);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GenerateSalt_ReturnsValidBase64String()
    {
        // Act
        var salt = _passwordService.GenerateSalt();

        // Assert
        Assert.NotNull(salt);
        Assert.NotEmpty(salt);

        // Should be valid base64
        var bytes = Convert.FromBase64String(salt);
        Assert.Equal(32, bytes.Length); // Should be 32 bytes
    }

    [Fact]
    public void GenerateSalt_GeneratesUniqueSalts()
    {
        // Act
        var salt1 = _passwordService.GenerateSalt();
        var salt2 = _passwordService.GenerateSalt();

        // Assert
        Assert.NotEqual(salt1, salt2);
    }

    [Theory]
    [InlineData("Password123!", true)]  // Valid: uppercase, lowercase, digit, special char, 8+ chars
    [InlineData("password123!", false)] // Invalid: no uppercase
    [InlineData("PASSWORD123!", false)] // Invalid: no lowercase
    [InlineData("Password!", false)]    // Invalid: no digit
    [InlineData("Password123", false)]  // Invalid: no special char
    [InlineData("Pass1!", false)]       // Invalid: less than 8 chars
    [InlineData("", false)]             // Invalid: empty
    [InlineData("   ", false)]          // Invalid: whitespace only
    public void IsPasswordStrong_VariousPasswords_ReturnsExpectedResult(string password, bool expected)
    {
        // Act
        var result = _passwordService.IsPasswordStrong(password);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateRandomPassword_DefaultLength_ReturnsValidPassword()
    {
        // Act
        var password = _passwordService.GenerateRandomPassword();

        // Assert
        Assert.NotNull(password);
        Assert.Equal(12, password.Length); // Default length
        Assert.True(_passwordService.IsPasswordStrong(password));
    }

    [Fact]
    public void GenerateRandomPassword_CustomLength_ReturnsPasswordWithCorrectLength()
    {
        // Arrange
        var length = 16;

        // Act
        var password = _passwordService.GenerateRandomPassword(length);

        // Assert
        Assert.NotNull(password);
        Assert.Equal(length, password.Length);
        Assert.True(_passwordService.IsPasswordStrong(password));
    }

    [Fact]
    public void GenerateRandomPassword_MultipleGenerations_ReturnsDifferentPasswords()
    {
        // Act
        var password1 = _passwordService.GenerateRandomPassword();
        var password2 = _passwordService.GenerateRandomPassword();

        // Assert
        Assert.NotEqual(password1, password2);
    }

    [Fact]
    public void GeneratePasswordResetToken_ReturnsValidToken()
    {
        // Act
        var token = _passwordService.GeneratePasswordResetToken();

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.DoesNotContain("+", token); // Should be URL-safe
        Assert.DoesNotContain("/", token); // Should be URL-safe
        Assert.DoesNotContain("=", token); // Should be URL-safe
    }

    [Fact]
    public void GeneratePasswordResetToken_MultipleGenerations_ReturnsDifferentTokens()
    {
        // Act
        var token1 = _passwordService.GeneratePasswordResetToken();
        var token2 = _passwordService.GeneratePasswordResetToken();

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void ValidatePasswordResetToken_ValidToken_ReturnsTrue()
    {
        // Arrange
        var token = _passwordService.GeneratePasswordResetToken();

        // Act
        var result = _passwordService.ValidatePasswordResetToken(token);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-token")]
    [InlineData("short")]
    public void ValidatePasswordResetToken_InvalidToken_ReturnsFalse(string invalidToken)
    {
        // Act
        var result = _passwordService.ValidatePasswordResetToken(invalidToken);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_MalformedHash_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var malformedHash = "malformed-hash-without-separator";

        // Act
        var result = _passwordService.VerifyPassword(password, malformedHash);

        // Assert
        Assert.False(result);
    }
}
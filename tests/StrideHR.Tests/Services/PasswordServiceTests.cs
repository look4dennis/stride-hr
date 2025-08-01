using FluentAssertions;
using Microsoft.Extensions.Options;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Models;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService;
    private readonly SecuritySettings _securitySettings;

    public PasswordServiceTests()
    {
        _securitySettings = new SecuritySettings
        {
            PasswordMinLength = 8,
            PasswordMaxLength = 128,
            RequireUppercase = true,
            RequireLowercase = true,
            RequireDigits = true,
            RequireSpecialCharacters = true
        };

        var options = Options.Create(_securitySettings);
        _passwordService = new PasswordService(options);
    }

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnHashAndSalt()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var (hash, salt) = _passwordService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        salt.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
        salt.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_WithSamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var (hash1, salt1) = _passwordService.HashPassword(password);
        var (hash2, salt2) = _passwordService.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2);
        salt1.Should().NotBe(salt2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void HashPassword_WithInvalidPassword_ShouldThrowException(string? password)
    {
        // Act & Assert
        var action = () => _passwordService.HashPassword(password!);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var (hash, salt) = _passwordService.HashPassword(password);

        // Act
        var isValid = _passwordService.VerifyPassword(password, hash, salt);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword123!";
        var (hash, salt) = _passwordService.HashPassword(password);

        // Act
        var isValid = _passwordService.VerifyPassword(wrongPassword, hash, salt);

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(null, "hash", "salt")]
    [InlineData("password", null, "salt")]
    [InlineData("password", "hash", null)]
    [InlineData("", "hash", "salt")]
    [InlineData("password", "", "salt")]
    [InlineData("password", "hash", "")]
    public void VerifyPassword_WithInvalidParameters_ShouldReturnFalse(string? password, string? hash, string? salt)
    {
        // Act
        var isValid = _passwordService.VerifyPassword(password!, hash!, salt!);

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(8)]
    [InlineData(12)]
    [InlineData(16)]
    [InlineData(20)]
    public void GenerateRandomPassword_WithValidLength_ShouldReturnPasswordOfCorrectLength(int length)
    {
        // Act
        var password = _passwordService.GenerateRandomPassword(length);

        // Assert
        password.Should().NotBeNullOrEmpty();
        password.Length.Should().Be(length);
    }

    [Fact]
    public void GenerateRandomPassword_WithMinimumRequirements_ShouldMeetAllRequirements()
    {
        // Act
        var password = _passwordService.GenerateRandomPassword(12);

        // Assert
        password.Should().NotBeNullOrEmpty();
        password.Should().MatchRegex("[a-z]"); // Has lowercase
        password.Should().MatchRegex("[A-Z]"); // Has uppercase
        password.Should().MatchRegex("[0-9]"); // Has digit
        password.Should().MatchRegex("[!@#$%^&*()_+\\-=\\[\\]{}|;:,.<>?]"); // Has special char
    }

    [Fact]
    public void GenerateRandomPassword_MultipleCalls_ShouldReturnDifferentPasswords()
    {
        // Act
        var password1 = _passwordService.GenerateRandomPassword(12);
        var password2 = _passwordService.GenerateRandomPassword(12);

        // Assert
        password1.Should().NotBe(password2);
    }

    [Fact]
    public void ValidatePasswordStrength_WithStrongPassword_ShouldReturnValid()
    {
        // Arrange
        var strongPassword = "MySecure123!Pass";

        // Act
        var result = _passwordService.ValidatePasswordStrength(strongPassword);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Strength.Should().BeOneOf(PasswordStrength.Good, PasswordStrength.Strong);
    }

    [Fact]
    public void ValidatePasswordStrength_WithWeakPassword_ShouldReturnInvalid()
    {
        // Arrange
        var weakPassword = "weak";

        // Act
        var result = _passwordService.ValidatePasswordStrength(weakPassword);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Strength.Should().Be(PasswordStrength.VeryWeak);
    }

    [Theory]
    [InlineData("short", "Password must be at least 8 characters long")]
    [InlineData("nouppercase123!", "Password must contain at least one uppercase letter")]
    [InlineData("NOLOWERCASE123!", "Password must contain at least one lowercase letter")]
    [InlineData("NoDigits!", "Password must contain at least one digit")]
    [InlineData("NoSpecialChars123", "Password must contain at least one special character")]
    public void ValidatePasswordStrength_WithSpecificWeakness_ShouldReturnSpecificError(string password, string expectedError)
    {
        // Act
        var result = _passwordService.ValidatePasswordStrength(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(expectedError);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ValidatePasswordStrength_WithInvalidPassword_ShouldReturnInvalid(string? password)
    {
        // Act
        var result = _passwordService.ValidatePasswordStrength(password!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Password is required");
        result.Strength.Should().Be(PasswordStrength.VeryWeak);
    }

    [Fact]
    public void GenerateSecureToken_ShouldReturnUniqueTokens()
    {
        // Act
        var token1 = _passwordService.GenerateSecureToken();
        var token2 = _passwordService.GenerateSecureToken();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
        
        // Should be URL-safe base64
        token1.Should().NotContain("+");
        token1.Should().NotContain("/");
        token1.Should().NotContain("=");
    }

    [Fact]
    public void GenerateSecureToken_ShouldReturnReasonableLength()
    {
        // Act
        var token = _passwordService.GenerateSecureToken();

        // Assert
        token.Length.Should().BeGreaterThan(20);
        token.Length.Should().BeLessThan(100);
    }
}
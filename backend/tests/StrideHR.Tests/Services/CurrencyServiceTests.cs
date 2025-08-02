using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class CurrencyServiceTests
{
    private readonly Mock<ILogger<CurrencyService>> _mockLogger;
    private readonly CurrencyService _currencyService;

    public CurrencyServiceTests()
    {
        _mockLogger = new Mock<ILogger<CurrencyService>>();
        _currencyService = new CurrencyService(_mockLogger.Object);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_SameCurrency_ReturnsOriginalAmount()
    {
        // Arrange
        var amount = 100m;
        var currency = "USD";

        // Act
        var result = await _currencyService.ConvertCurrencyAsync(amount, currency, currency);

        // Assert
        Assert.Equal(amount, result);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_USDToEUR_ReturnsConvertedAmount()
    {
        // Arrange
        var amount = 100m;
        var fromCurrency = "USD";
        var toCurrency = "EUR";

        // Act
        var result = await _currencyService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency);

        // Assert
        Assert.NotEqual(amount, result);
        Assert.True(result > 0);
        Assert.Equal(85m, result); // Based on mock exchange rate
    }

    [Fact]
    public async Task ConvertCurrencyAsync_EURToUSD_ReturnsConvertedAmount()
    {
        // Arrange
        var amount = 85m;
        var fromCurrency = "EUR";
        var toCurrency = "USD";

        // Act
        var result = await _currencyService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency);

        // Assert
        Assert.NotEqual(amount, result);
        Assert.True(result > 0);
        Assert.Equal(100m, result); // Based on mock exchange rate
    }

    [Fact]
    public async Task ConvertCurrencyAsync_InvalidFromCurrency_ThrowsArgumentException()
    {
        // Arrange
        var amount = 100m;
        var fromCurrency = "INVALID";
        var toCurrency = "USD";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _currencyService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency));
    }

    [Fact]
    public async Task ConvertCurrencyAsync_InvalidToCurrency_ThrowsArgumentException()
    {
        // Arrange
        var amount = 100m;
        var fromCurrency = "USD";
        var toCurrency = "INVALID";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _currencyService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency));
    }

    [Fact]
    public async Task ConvertCurrencyAsync_NullFromCurrency_ThrowsArgumentException()
    {
        // Arrange
        var amount = 100m;
        string fromCurrency = null!;
        var toCurrency = "USD";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _currencyService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency));
    }

    [Fact]
    public async Task ConvertCurrencyAsync_EmptyToCurrency_ThrowsArgumentException()
    {
        // Arrange
        var amount = 100m;
        var fromCurrency = "USD";
        var toCurrency = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _currencyService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency));
    }

    [Fact]
    public async Task GetExchangeRateAsync_SameCurrency_ReturnsOne()
    {
        // Arrange
        var currency = "USD";

        // Act
        var result = await _currencyService.GetExchangeRateAsync(currency, currency);

        // Assert
        Assert.Equal(1.0m, result);
    }

    [Fact]
    public async Task GetExchangeRateAsync_USDToEUR_ReturnsCorrectRate()
    {
        // Arrange
        var fromCurrency = "USD";
        var toCurrency = "EUR";

        // Act
        var result = await _currencyService.GetExchangeRateAsync(fromCurrency, toCurrency);

        // Assert
        Assert.Equal(0.85m, result);
    }

    [Fact]
    public async Task GetExchangeRateAsync_InvalidCurrency_ThrowsArgumentException()
    {
        // Arrange
        var fromCurrency = "INVALID";
        var toCurrency = "USD";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _currencyService.GetExchangeRateAsync(fromCurrency, toCurrency));
    }

    [Fact]
    public async Task GetSupportedCurrenciesAsync_ReturnsExpectedCurrencies()
    {
        // Act
        var result = await _currencyService.GetSupportedCurrenciesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("USD", result);
        Assert.Contains("EUR", result);
        Assert.Contains("GBP", result);
        Assert.Contains("INR", result);
        Assert.Contains("CAD", result);
        Assert.Contains("AUD", result);
        Assert.Contains("JPY", result);
        Assert.Contains("SGD", result);
        Assert.Contains("AED", result);
    }

    [Fact]
    public async Task GetCurrencySymbolAsync_USD_ReturnsDollarSign()
    {
        // Arrange
        var currencyCode = "USD";

        // Act
        var result = await _currencyService.GetCurrencySymbolAsync(currencyCode);

        // Assert
        Assert.Equal("$", result);
    }

    [Fact]
    public async Task GetCurrencySymbolAsync_EUR_ReturnsEuroSign()
    {
        // Arrange
        var currencyCode = "EUR";

        // Act
        var result = await _currencyService.GetCurrencySymbolAsync(currencyCode);

        // Assert
        Assert.Equal("€", result);
    }

    [Fact]
    public async Task GetCurrencySymbolAsync_InvalidCurrency_ThrowsArgumentException()
    {
        // Arrange
        var currencyCode = "INVALID";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _currencyService.GetCurrencySymbolAsync(currencyCode));
    }

    [Fact]
    public async Task GetCurrencySymbolAsync_NullCurrency_ThrowsArgumentException()
    {
        // Arrange
        string currencyCode = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _currencyService.GetCurrencySymbolAsync(currencyCode));
    }

    [Fact]
    public async Task IsCurrencySupportedAsync_ValidCurrency_ReturnsTrue()
    {
        // Arrange
        var currencyCode = "USD";

        // Act
        var result = await _currencyService.IsCurrencySupportedAsync(currencyCode);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsCurrencySupportedAsync_InvalidCurrency_ReturnsFalse()
    {
        // Arrange
        var currencyCode = "INVALID";

        // Act
        var result = await _currencyService.IsCurrencySupportedAsync(currencyCode);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsCurrencySupportedAsync_NullCurrency_ReturnsFalse()
    {
        // Arrange
        string currencyCode = null!;

        // Act
        var result = await _currencyService.IsCurrencySupportedAsync(currencyCode);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsCurrencySupportedAsync_EmptyCurrency_ReturnsFalse()
    {
        // Arrange
        var currencyCode = "";

        // Act
        var result = await _currencyService.IsCurrencySupportedAsync(currencyCode);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("usd", "USD")]
    [InlineData("eur", "EUR")]
    [InlineData("gbp", "GBP")]
    public async Task IsCurrencySupportedAsync_CaseInsensitive_ReturnsTrue(string inputCurrency, string expectedCurrency)
    {
        // Act
        var result = await _currencyService.IsCurrencySupportedAsync(inputCurrency);

        // Assert
        Assert.True(result, $"Currency {inputCurrency} should be supported (normalized to {expectedCurrency})");
    }
}
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Interfaces;
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
    public async Task ConvertCurrencyAsync_UsdToEur_ReturnsConvertedAmount()
    {
        // Arrange
        var amount = 100m;
        var fromCurrency = "USD";
        var toCurrency = "EUR";

        // Act
        var result = await _currencyService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency);

        // Assert
        Assert.True(result > 0);
        Assert.NotEqual(amount, result); // Should be different from original
        Assert.Equal(85m, result); // Based on the static exchange rate in the service
    }

    [Fact]
    public async Task ConvertCurrencyAsync_EurToUsd_ReturnsConvertedAmount()
    {
        // Arrange
        var amount = 85m;
        var fromCurrency = "EUR";
        var toCurrency = "USD";

        // Act
        var result = await _currencyService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency);

        // Assert
        Assert.True(result > 0);
        Assert.Equal(100m, result); // Should convert back to original USD amount
    }

    [Fact]
    public async Task ConvertCurrencyAsync_UnsupportedFromCurrency_ThrowsArgumentException()
    {
        // Arrange
        var amount = 100m;
        var fromCurrency = "XYZ";
        var toCurrency = "USD";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _currencyService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency));
        Assert.Contains("Unsupported currency for conversion", exception.Message);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_UnsupportedToCurrency_ThrowsArgumentException()
    {
        // Arrange
        var amount = 100m;
        var fromCurrency = "USD";
        var toCurrency = "XYZ";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _currencyService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency));
        Assert.Contains("Unsupported currency for conversion", exception.Message);
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
    public async Task GetExchangeRateAsync_UsdToEur_ReturnsCorrectRate()
    {
        // Arrange
        var fromCurrency = "USD";
        var toCurrency = "EUR";

        // Act
        var result = await _currencyService.GetExchangeRateAsync(fromCurrency, toCurrency);

        // Assert
        Assert.True(result > 0);
        Assert.Equal(0.85m, result); // Based on static exchange rate
    }

    [Fact]
    public async Task GetExchangeRateAsync_UnsupportedCurrency_ThrowsArgumentException()
    {
        // Arrange
        var fromCurrency = "USD";
        var toCurrency = "XYZ";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _currencyService.GetExchangeRateAsync(fromCurrency, toCurrency));
        Assert.Contains("Unsupported currency", exception.Message);
    }

    [Fact]
    public async Task GetSupportedCurrenciesAsync_ReturnsActiveCurrencies()
    {
        // Act
        var result = await _currencyService.GetSupportedCurrenciesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, currency => Assert.True(currency.IsActive));
        Assert.Contains(result, c => c.Code == "USD");
        Assert.Contains(result, c => c.Code == "EUR");
        Assert.Contains(result, c => c.Code == "GBP");
    }

    [Fact]
    public async Task GetCurrencySymbolAsync_ValidCurrency_ReturnsSymbol()
    {
        // Arrange
        var currencyCode = "USD";

        // Act
        var result = await _currencyService.GetCurrencySymbolAsync(currencyCode);

        // Assert
        Assert.Equal("$", result);
    }

    [Fact]
    public async Task GetCurrencySymbolAsync_InvalidCurrency_ReturnsCurrencyCode()
    {
        // Arrange
        var currencyCode = "XYZ";

        // Act
        var result = await _currencyService.GetCurrencySymbolAsync(currencyCode);

        // Assert
        Assert.Equal(currencyCode, result); // Should fallback to currency code
    }

    [Fact]
    public async Task FormatCurrencyAsync_ValidCurrency_ReturnsFormattedString()
    {
        // Arrange
        var amount = 1234.56m;
        var currencyCode = "USD";

        // Act
        var result = await _currencyService.FormatCurrencyAsync(amount, currencyCode);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("$", result);
        Assert.Contains("1,234.56", result);
    }

    [Fact]
    public async Task FormatCurrencyAsync_JapaneseCurrency_ReturnsFormattedWithoutDecimals()
    {
        // Arrange
        var amount = 1234.56m;
        var currencyCode = "JPY";

        // Act
        var result = await _currencyService.FormatCurrencyAsync(amount, currencyCode);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("¥", result);
        Assert.Contains("1,235", result); // Should be rounded to no decimals
        Assert.DoesNotContain(".56", result);
    }

    [Fact]
    public async Task FormatCurrencyAsync_InvalidCurrency_ReturnsFallbackFormat()
    {
        // Arrange
        var amount = 1234.56m;
        var currencyCode = "XYZ";

        // Act
        var result = await _currencyService.FormatCurrencyAsync(amount, currencyCode);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("XYZ", result);
        Assert.Contains("1,234.56", result);
    }

    [Fact]
    public async Task UpdateExchangeRatesAsync_CompletesSuccessfully()
    {
        // Act & Assert
        // Should not throw any exceptions
        await _currencyService.UpdateExchangeRatesAsync();
        
        // Verify that the method completes without errors
        Assert.True(true);
    }

    [Theory]
    [InlineData("USD", "EUR", 100, 85)]
    [InlineData("EUR", "USD", 85, 100)]
    [InlineData("USD", "GBP", 100, 73)]
    [InlineData("USD", "INR", 100, 8300)]
    [InlineData("USD", "JPY", 100, 15000)]
    public async Task ConvertCurrencyAsync_VariousCurrencyPairs_ReturnsExpectedAmounts(
        string fromCurrency, string toCurrency, decimal amount, decimal expectedAmount)
    {
        // Act
        var result = await _currencyService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency);

        // Assert
        Assert.Equal(expectedAmount, result);
    }

    [Theory]
    [InlineData("USD", "$")]
    [InlineData("EUR", "€")]
    [InlineData("GBP", "£")]
    [InlineData("INR", "₹")]
    [InlineData("JPY", "¥")]
    [InlineData("CAD", "C$")]
    [InlineData("AUD", "A$")]
    public async Task GetCurrencySymbolAsync_VariousCurrencies_ReturnsCorrectSymbols(
        string currencyCode, string expectedSymbol)
    {
        // Act
        var result = await _currencyService.GetCurrencySymbolAsync(currencyCode);

        // Assert
        Assert.Equal(expectedSymbol, result);
    }

    [Theory]
    [InlineData("USD", 2)]
    [InlineData("EUR", 2)]
    [InlineData("JPY", 0)]
    [InlineData("KWD", 3)]
    [InlineData("BHD", 3)]
    public async Task GetSupportedCurrenciesAsync_CheckDecimalPlaces_ReturnsCorrectDecimalPlaces(
        string currencyCode, int expectedDecimalPlaces)
    {
        // Act
        var currencies = await _currencyService.GetSupportedCurrenciesAsync();
        var currency = currencies.FirstOrDefault(c => c.Code == currencyCode);

        // Assert
        Assert.NotNull(currency);
        Assert.Equal(expectedDecimalPlaces, currency.DecimalPlaces);
    }
}
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class BranchServiceTests
{
    private readonly Mock<IBranchRepository> _mockBranchRepository;
    private readonly Mock<IOrganizationRepository> _mockOrganizationRepository;
    private readonly Mock<ILogger<BranchService>> _mockLogger;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ICurrencyService> _mockCurrencyService;
    private readonly Mock<ITimeZoneService> _mockTimeZoneService;
    private readonly BranchService _branchService;

    public BranchServiceTests()
    {
        _mockBranchRepository = new Mock<IBranchRepository>();
        _mockOrganizationRepository = new Mock<IOrganizationRepository>();
        _mockLogger = new Mock<ILogger<BranchService>>();
        _mockAuditService = new Mock<IAuditService>();
        _mockCurrencyService = new Mock<ICurrencyService>();
        _mockTimeZoneService = new Mock<ITimeZoneService>();

        _branchService = new BranchService(
            _mockBranchRepository.Object,
            _mockOrganizationRepository.Object,
            _mockLogger.Object,
            _mockAuditService.Object,
            _mockCurrencyService.Object,
            _mockTimeZoneService.Object);
    }

    [Fact]
    public async Task CreateBranchAsync_ValidRequest_ReturnsBranch()
    {
        // Arrange
        var request = new CreateBranchRequest
        {
            OrganizationId = 1,
            Name = "Test Branch",
            Country = "United States",
            Currency = "USD",
            TimeZone = "America/New_York",
            Address = "123 Test St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Phone = "123-456-7890",
            Email = "test@branch.com"
        };

        var organization = new Organization { Id = 1, Name = "Test Organization" };
        var expectedBranch = new Branch
        {
            Id = 1,
            OrganizationId = 1,
            Name = "Test Branch",
            Country = "United States",
            Currency = "USD",
            TimeZone = "America/New_York"
        };

        _mockOrganizationRepository.Setup(x => x.GetByIdAsync(request.OrganizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);
        _mockBranchRepository.Setup(x => x.IsNameUniqueAsync(request.Name, request.OrganizationId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockBranchRepository.Setup(x => x.AddAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBranch);
        _mockBranchRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _branchService.CreateBranchAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedBranch.Name, result.Name);
        Assert.Equal(expectedBranch.Country, result.Country);
        Assert.Equal(expectedBranch.Currency, result.Currency);
        Assert.Equal(expectedBranch.TimeZone, result.TimeZone);
        _mockBranchRepository.Verify(x => x.AddAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockBranchRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBranchAsync_OrganizationNotFound_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateBranchRequest
        {
            OrganizationId = 999,
            Name = "Test Branch"
        };

        _mockOrganizationRepository.Setup(x => x.GetByIdAsync(request.OrganizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _branchService.CreateBranchAsync(request));
        Assert.Contains("Organization with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task CreateBranchAsync_DuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateBranchRequest
        {
            OrganizationId = 1,
            Name = "Duplicate Branch"
        };

        var organization = new Organization { Id = 1, Name = "Test Organization" };

        _mockOrganizationRepository.Setup(x => x.GetByIdAsync(request.OrganizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);
        _mockBranchRepository.Setup(x => x.IsNameUniqueAsync(request.Name, request.OrganizationId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _branchService.CreateBranchAsync(request));
        Assert.Contains("Branch name 'Duplicate Branch' already exists in this organization", exception.Message);
    }

    [Fact]
    public async Task GetBranchByIdAsync_ExistingBranch_ReturnsBranch()
    {
        // Arrange
        var branchId = 1;
        var expectedBranch = new Branch
        {
            Id = branchId,
            Name = "Test Branch",
            Country = "United States"
        };

        _mockBranchRepository.Setup(x => x.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBranch);

        // Act
        var result = await _branchService.GetBranchByIdAsync(branchId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedBranch.Id, result.Id);
        Assert.Equal(expectedBranch.Name, result.Name);
        Assert.Equal(expectedBranch.Country, result.Country);
    }

    [Fact]
    public async Task GetBranchByIdAsync_NonExistingBranch_ReturnsNull()
    {
        // Arrange
        var branchId = 999;
        _mockBranchRepository.Setup(x => x.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        // Act
        var result = await _branchService.GetBranchByIdAsync(branchId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateBranchAsync_ValidRequest_ReturnsUpdatedBranch()
    {
        // Arrange
        var branchId = 1;
        var existingBranch = new Branch
        {
            Id = branchId,
            OrganizationId = 1,
            Name = "Original Branch",
            Country = "United States",
            Currency = "USD"
        };

        var updateRequest = new UpdateBranchRequest
        {
            Name = "Updated Branch",
            Country = "Canada",
            Currency = "CAD"
        };

        _mockBranchRepository.Setup(x => x.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBranch);
        _mockBranchRepository.Setup(x => x.IsNameUniqueAsync(updateRequest.Name!, existingBranch.OrganizationId, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockBranchRepository.Setup(x => x.UpdateAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBranch);
        _mockBranchRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _branchService.UpdateBranchAsync(branchId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Branch", result.Name);
        Assert.Equal("Canada", result.Country);
        Assert.Equal("CAD", result.Currency);
        _mockBranchRepository.Verify(x => x.UpdateAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockBranchRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBranchAsync_BranchNotFound_ThrowsArgumentException()
    {
        // Arrange
        var branchId = 999;
        var updateRequest = new UpdateBranchRequest { Name = "Updated Branch" };

        _mockBranchRepository.Setup(x => x.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _branchService.UpdateBranchAsync(branchId, updateRequest));
        Assert.Contains("Branch with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task DeleteBranchAsync_ExistingBranch_ReturnsTrue()
    {
        // Arrange
        var branchId = 1;
        var existingBranch = new Branch
        {
            Id = branchId,
            Name = "Test Branch"
        };

        _mockBranchRepository.Setup(x => x.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBranch);
        _mockBranchRepository.Setup(x => x.SoftDeleteAsync(branchId, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockBranchRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _branchService.DeleteBranchAsync(branchId, "test-user");

        // Assert
        Assert.True(result);
        _mockBranchRepository.Verify(x => x.SoftDeleteAsync(branchId, "test-user", It.IsAny<CancellationToken>()), Times.Once);
        _mockBranchRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteBranchAsync_NonExistingBranch_ReturnsFalse()
    {
        // Arrange
        var branchId = 999;
        _mockBranchRepository.Setup(x => x.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        // Act
        var result = await _branchService.DeleteBranchAsync(branchId);

        // Assert
        Assert.False(result);
        _mockBranchRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetBranchesByOrganizationAsync_ReturnsBranches()
    {
        // Arrange
        var organizationId = 1;
        var branches = new List<Branch>
        {
            new() { Id = 1, OrganizationId = organizationId, Name = "Branch 1" },
            new() { Id = 2, OrganizationId = organizationId, Name = "Branch 2" }
        };

        _mockBranchRepository.Setup(x => x.GetByOrganizationIdAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branches);

        // Act
        var result = await _branchService.GetBranchesByOrganizationAsync(organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockBranchRepository.Verify(x => x.GetByOrganizationIdAsync(organizationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActiveBranchesAsync_ReturnsActiveBranches()
    {
        // Arrange
        var branches = new List<Branch>
        {
            new() { Id = 1, Name = "Active Branch 1", IsActive = true },
            new() { Id = 2, Name = "Active Branch 2", IsActive = true }
        };

        _mockBranchRepository.Setup(x => x.GetActiveBranchesAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branches);

        // Act
        var result = await _branchService.GetActiveBranchesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, branch => Assert.True(branch.IsActive));
        _mockBranchRepository.Verify(x => x.GetActiveBranchesAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBranchesByCountryAsync_ReturnsBranchesInCountry()
    {
        // Arrange
        var country = "United States";
        var branches = new List<Branch>
        {
            new() { Id = 1, Name = "US Branch 1", Country = country },
            new() { Id = 2, Name = "US Branch 2", Country = country }
        };

        _mockBranchRepository.Setup(x => x.GetByCountryAsync(country, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branches);

        // Act
        var result = await _branchService.GetBranchesByCountryAsync(country);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, branch => Assert.Equal(country, branch.Country));
        _mockBranchRepository.Verify(x => x.GetByCountryAsync(country, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSupportedCurrenciesAsync_ReturnsCurrencies()
    {
        // Arrange
        var currencies = new List<CurrencyInfo>
        {
            new() { Code = "USD", Name = "US Dollar", Symbol = "$" },
            new() { Code = "EUR", Name = "Euro", Symbol = "€" }
        };

        _mockCurrencyService.Setup(x => x.GetSupportedCurrenciesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currencies);

        // Act
        var result = await _branchService.GetSupportedCurrenciesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains("USD", result);
        Assert.Contains("EUR", result);
        _mockCurrencyService.Verify(x => x.GetSupportedCurrenciesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSupportedTimeZonesAsync_ReturnsTimeZones()
    {
        // Arrange
        var timeZones = new List<TimeZoneInfo>
        {
            TimeZoneInfo.Utc,
            TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
        };

        _mockTimeZoneService.Setup(x => x.GetSupportedTimeZonesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(timeZones);

        // Act
        var result = await _branchService.GetSupportedTimeZonesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains("UTC", result);
        Assert.Contains("Eastern Standard Time", result);
        _mockTimeZoneService.Verify(x => x.GetSupportedTimeZonesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_ValidCurrencies_ReturnsConvertedAmount()
    {
        // Arrange
        var amount = 100m;
        var fromCurrency = "USD";
        var toCurrency = "EUR";
        var expectedAmount = 85m;

        _mockCurrencyService.Setup(x => x.ConvertCurrencyAsync(amount, fromCurrency, toCurrency, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAmount);

        // Act
        var result = await _branchService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency);

        // Assert
        Assert.Equal(expectedAmount, result);
        _mockCurrencyService.Verify(x => x.ConvertCurrencyAsync(amount, fromCurrency, toCurrency, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConvertToLocalTimeAsync_ValidTimeZone_ReturnsLocalTime()
    {
        // Arrange
        var utcDateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var timeZone = "America/New_York";
        var expectedLocalTime = new DateTime(2024, 1, 1, 7, 0, 0);

        _mockTimeZoneService.Setup(x => x.ConvertToLocalTimeAsync(utcDateTime, timeZone, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedLocalTime);

        // Act
        var result = await _branchService.ConvertToLocalTimeAsync(utcDateTime, timeZone);

        // Assert
        Assert.Equal(expectedLocalTime, result);
        _mockTimeZoneService.Verify(x => x.ConvertToLocalTimeAsync(utcDateTime, timeZone, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConvertToUtcAsync_ValidTimeZone_ReturnsUtcTime()
    {
        // Arrange
        var localDateTime = new DateTime(2024, 1, 1, 7, 0, 0);
        var timeZone = "America/New_York";
        var expectedUtcTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        _mockTimeZoneService.Setup(x => x.ConvertToUtcAsync(localDateTime, timeZone, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUtcTime);

        // Act
        var result = await _branchService.ConvertToUtcAsync(localDateTime, timeZone);

        // Assert
        Assert.Equal(expectedUtcTime, result);
        _mockTimeZoneService.Verify(x => x.ConvertToUtcAsync(localDateTime, timeZone, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateComplianceSettingsAsync_ValidRequest_ReturnsUpdatedBranch()
    {
        // Arrange
        var branchId = 1;
        var existingBranch = new Branch
        {
            Id = branchId,
            Name = "Test Branch"
        };

        var complianceRequest = new ComplianceSettingsRequest
        {
            TaxSettings = new Dictionary<string, object> { { "TaxRate", 0.15 } },
            LaborLawSettings = new Dictionary<string, object> { { "MinWage", 15.0 } },
            StatutorySettings = new Dictionary<string, object> { { "PF", true } },
            ReportingSettings = new Dictionary<string, object> { { "Quarterly", true } },
            RequiredDocuments = new List<string> { "TaxForm", "LaborContract" },
            CustomCompliance = new Dictionary<string, object> { { "CustomRule", "Value" } }
        };

        _mockBranchRepository.Setup(x => x.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBranch);
        _mockBranchRepository.Setup(x => x.UpdateAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBranch);
        _mockBranchRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _branchService.UpdateComplianceSettingsAsync(branchId, complianceRequest);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ComplianceSettings);
        _mockBranchRepository.Verify(x => x.UpdateAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockBranchRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateComplianceSettingsAsync_BranchNotFound_ThrowsArgumentException()
    {
        // Arrange
        var branchId = 999;
        var complianceRequest = new ComplianceSettingsRequest();

        _mockBranchRepository.Setup(x => x.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _branchService.UpdateComplianceSettingsAsync(branchId, complianceRequest));
        Assert.Contains("Branch with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task GetComplianceSettingsAsync_ExistingBranch_ReturnsSettings()
    {
        // Arrange
        var branchId = 1;
        var complianceSettings = new Dictionary<string, object>
        {
            { "TaxSettings", new Dictionary<string, object> { { "TaxRate", 0.15 } } },
            { "LaborLawSettings", new Dictionary<string, object> { { "MinWage", 15.0 } } }
        };

        var branch = new Branch
        {
            Id = branchId,
            Name = "Test Branch",
            ComplianceSettings = System.Text.Json.JsonSerializer.Serialize(complianceSettings)
        };

        _mockBranchRepository.Setup(x => x.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);

        // Act
        var result = await _branchService.GetComplianceSettingsAsync(branchId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        _mockBranchRepository.Verify(x => x.GetByIdAsync(branchId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetComplianceSettingsAsync_BranchNotFound_ThrowsArgumentException()
    {
        // Arrange
        var branchId = 999;
        _mockBranchRepository.Setup(x => x.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _branchService.GetComplianceSettingsAsync(branchId));
        Assert.Contains("Branch with ID 999 not found", exception.Message);
    }
}
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Branch;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class BranchServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICurrencyService> _mockCurrencyService;
    private readonly Mock<ITimeZoneService> _mockTimeZoneService;
    private readonly Mock<ILogger<BranchService>> _mockLogger;
    private readonly Mock<IRepository<Branch>> _mockBranchRepository;
    private readonly Mock<IRepository<Organization>> _mockOrganizationRepository;
    private readonly Mock<IRepository<Employee>> _mockEmployeeRepository;
    private readonly Mock<IRepository<User>> _mockUserRepository;
    private readonly BranchService _branchService;

    public BranchServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCurrencyService = new Mock<ICurrencyService>();
        _mockTimeZoneService = new Mock<ITimeZoneService>();
        _mockLogger = new Mock<ILogger<BranchService>>();
        _mockBranchRepository = new Mock<IRepository<Branch>>();
        _mockOrganizationRepository = new Mock<IRepository<Organization>>();
        _mockEmployeeRepository = new Mock<IRepository<Employee>>();
        _mockUserRepository = new Mock<IRepository<User>>();

        _mockUnitOfWork.Setup(u => u.Branches).Returns(_mockBranchRepository.Object);
        _mockUnitOfWork.Setup(u => u.Organizations).Returns(_mockOrganizationRepository.Object);
        _mockUnitOfWork.Setup(u => u.Employees).Returns(_mockEmployeeRepository.Object);
        _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepository.Object);

        _branchService = new BranchService(_mockUnitOfWork.Object, _mockCurrencyService.Object, _mockTimeZoneService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingBranch_ReturnsBranch()
    {
        // Arrange
        var branchId = 1;
        var expectedBranch = new Branch
        {
            Id = branchId,
            Name = "Test Branch",
            Country = "United States",
            CountryCode = "US",
            Currency = "USD",
            Organization = new Organization { Id = 1, Name = "Test Org" },
            Employees = new List<Employee>()
        };

        _mockBranchRepository
            .Setup(r => r.GetByIdAsync(branchId, It.IsAny<System.Linq.Expressions.Expression<Func<Branch, object>>[]>()))
            .ReturnsAsync(expectedBranch);

        // Act
        var result = await _branchService.GetByIdAsync(branchId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedBranch.Id, result.Id);
        Assert.Equal(expectedBranch.Name, result.Name);
        Assert.Equal(expectedBranch.Country, result.Country);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingBranch_ReturnsNull()
    {
        // Arrange
        var branchId = 999;
        _mockBranchRepository
            .Setup(r => r.GetByIdAsync(branchId, It.IsAny<System.Linq.Expressions.Expression<Func<Branch, object>>[]>()))
            .ReturnsAsync((Branch?)null);

        // Act
        var result = await _branchService.GetByIdAsync(branchId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedBranch()
    {
        // Arrange
        var dto = new CreateBranchDto
        {
            OrganizationId = 1,
            Name = "New Branch",
            Country = "United States",
            CountryCode = "US",
            Currency = "USD",
            CurrencySymbol = "$",
            TimeZone = "America/New_York",
            Address = "123 Test Street",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Phone = "1234567890",
            Email = "branch@example.com",
            LocalHolidays = new List<LocalHolidayDto>(),
            ComplianceSettings = new BranchComplianceDto(),
            IsActive = true
        };

        var organization = new Organization { Id = 1, Name = "Test Org" };

        _mockOrganizationRepository
            .Setup(r => r.GetByIdAsync(dto.OrganizationId))
            .ReturnsAsync(organization);

        _mockBranchRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>()))
            .ReturnsAsync((Branch?)null);

        _mockCurrencyService
            .Setup(s => s.IsCurrencySupportedAsync(dto.Currency))
            .ReturnsAsync(true);

        _mockTimeZoneService
            .Setup(s => s.IsTimeZoneSupportedAsync(dto.TimeZone))
            .ReturnsAsync(true);

        _mockBranchRepository
            .Setup(r => r.AddAsync(It.IsAny<Branch>()))
            .ReturnsAsync((Branch b) => b);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _branchService.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Country, result.Country);
        Assert.Equal(dto.Currency, result.Currency);
        _mockBranchRepository.Verify(r => r.AddAsync(It.IsAny<Branch>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidOrganization_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateBranchDto
        {
            OrganizationId = 999,
            Name = "New Branch",
            CountryCode = "US",
            Currency = "USD",
            TimeZone = "America/New_York"
        };

        _mockOrganizationRepository
            .Setup(r => r.GetByIdAsync(dto.OrganizationId))
            .ReturnsAsync((Organization?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _branchService.CreateAsync(dto));
    }

    [Fact]
    public async Task UpdateAsync_ExistingBranch_UpdatesSuccessfully()
    {
        // Arrange
        var branchId = 1;
        var branch = new Branch
        {
            Id = branchId,
            OrganizationId = 1,
            Name = "Original Branch",
            Country = "United States",
            CountryCode = "US",
            Currency = "USD",
            Organization = new Organization { Id = 1, Name = "Test Org" },
            Employees = new List<Employee>()
        };

        var updateDto = new UpdateBranchDto
        {
            Name = "Updated Branch",
            Country = "Canada",
            CountryCode = "CA",
            Currency = "CAD",
            CurrencySymbol = "C$",
            TimeZone = "America/Toronto",
            Address = "Updated Address",
            City = "Toronto",
            State = "ON",
            PostalCode = "M5V 3A8",
            Phone = "9876543210",
            Email = "updated@example.com",
            IsActive = true
        };

        _mockBranchRepository
            .Setup(r => r.GetByIdAsync(branchId, It.IsAny<System.Linq.Expressions.Expression<Func<Branch, object>>[]>()))
            .ReturnsAsync(branch);

        _mockBranchRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>()))
            .ReturnsAsync((Branch?)null);

        _mockCurrencyService
            .Setup(s => s.IsCurrencySupportedAsync(updateDto.Currency))
            .ReturnsAsync(true);

        _mockTimeZoneService
            .Setup(s => s.IsTimeZoneSupportedAsync(updateDto.TimeZone))
            .ReturnsAsync(true);

        _mockBranchRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Branch>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _branchService.UpdateAsync(branchId, updateDto);

        // Assert
        _mockBranchRepository.Verify(r => r.UpdateAsync(It.IsAny<Branch>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingBranch_ThrowsArgumentException()
    {
        // Arrange
        var branchId = 999;
        var updateDto = new UpdateBranchDto
        {
            Name = "Updated Branch",
            CountryCode = "US",
            Currency = "USD",
            TimeZone = "America/New_York"
        };

        _mockBranchRepository
            .Setup(r => r.GetByIdAsync(branchId, It.IsAny<System.Linq.Expressions.Expression<Func<Branch, object>>[]>()))
            .ReturnsAsync((Branch?)null);

        _mockBranchRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>()))
            .ReturnsAsync((Branch?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _branchService.UpdateAsync(branchId, updateDto));
    }

    [Fact]
    public async Task DeleteAsync_BranchWithEmployees_ThrowsInvalidOperationException()
    {
        // Arrange
        var branchId = 1;
        var branch = new Branch
        {
            Id = branchId,
            Name = "Test Branch",
            Employees = new List<Employee> { new Employee { Id = 1, FirstName = "John", LastName = "Doe" } }
        };

        _mockBranchRepository
            .Setup(r => r.GetByIdAsync(branchId, It.IsAny<System.Linq.Expressions.Expression<Func<Branch, object>>[]>()))
            .ReturnsAsync(branch);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _branchService.DeleteAsync(branchId));
    }

    [Fact]
    public async Task GetByOrganizationAsync_ValidOrganization_ReturnsBranches()
    {
        // Arrange
        var organizationId = 1;
        var expectedBranches = new List<Branch>
        {
            new Branch { Id = 1, Name = "Branch 1", OrganizationId = organizationId },
            new Branch { Id = 2, Name = "Branch 2", OrganizationId = organizationId }
        };

        _mockBranchRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>(), It.IsAny<System.Linq.Expressions.Expression<Func<Branch, object>>[]>()))
            .ReturnsAsync(expectedBranches);

        // Act
        var result = await _branchService.GetByOrganizationAsync(organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, b => Assert.Equal(organizationId, b.OrganizationId));
    }

    [Fact]
    public async Task GetSupportedCountriesAsync_ReturnsCountryList()
    {
        // Act
        var result = await _branchService.GetSupportedCountriesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("United States", result);
        Assert.Contains("Canada", result);
        Assert.Contains("United Kingdom", result);
        Assert.Contains("India", result);
    }

    [Fact]
    public async Task GetSupportedCurrenciesAsync_ReturnsCurrencyList()
    {
        // Arrange
        var expectedCurrencies = new[] { "USD", "CAD", "GBP", "INR", "EUR" };
        _mockCurrencyService.Setup(s => s.GetSupportedCurrenciesAsync())
            .ReturnsAsync(expectedCurrencies);

        // Act
        var result = await _branchService.GetSupportedCurrenciesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("USD", result);
        Assert.Contains("CAD", result);
        Assert.Contains("GBP", result);
        Assert.Contains("INR", result);
        Assert.Contains("EUR", result);
    }

    [Fact]
    public async Task GetSupportedTimeZonesAsync_ReturnsTimeZoneList()
    {
        // Arrange
        var expectedTimeZones = new[] { "UTC", "America/New_York", "Europe/London", "Asia/Kolkata" };
        _mockTimeZoneService.Setup(s => s.GetSupportedTimeZonesAsync())
            .ReturnsAsync(expectedTimeZones);

        // Act
        var result = await _branchService.GetSupportedTimeZonesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("UTC", result);
        Assert.Contains("America/New_York", result);
        Assert.Contains("Europe/London", result);
        Assert.Contains("Asia/Kolkata", result);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_SameCurrency_ReturnsOriginalAmount()
    {
        // Arrange
        var amount = 100m;
        var currency = "USD";
        _mockCurrencyService.Setup(s => s.ConvertCurrencyAsync(amount, currency, currency))
            .ReturnsAsync(amount);

        // Act
        var result = await _branchService.ConvertCurrencyAsync(amount, currency, currency);

        // Assert
        Assert.Equal(amount, result);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_DifferentCurrencies_ReturnsConvertedAmount()
    {
        // Arrange
        var amount = 100m;
        var fromCurrency = "USD";
        var toCurrency = "EUR";
        var expectedResult = 85m;
        _mockCurrencyService.Setup(s => s.ConvertCurrencyAsync(amount, fromCurrency, toCurrency))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _branchService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_UnsupportedCurrency_ThrowsArgumentException()
    {
        // Arrange
        var amount = 100m;
        var fromCurrency = "INVALID";
        var toCurrency = "USD";
        _mockCurrencyService.Setup(s => s.ConvertCurrencyAsync(amount, fromCurrency, toCurrency))
            .ThrowsAsync(new ArgumentException("Unsupported currency"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _branchService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency));
    }

    [Fact]
    public async Task ConvertTimeZoneAsync_ValidTimeZones_ReturnsConvertedTime()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 1, 12, 0, 0);
        var fromTimeZone = "UTC";
        var toTimeZone = "America/New_York";
        var expectedResult = new DateTime(2024, 1, 1, 7, 0, 0);
        _mockTimeZoneService.Setup(s => s.ConvertTimeZoneAsync(dateTime, fromTimeZone, toTimeZone))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _branchService.ConvertTimeZoneAsync(dateTime, fromTimeZone, toTimeZone);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task HasAccessToBranchAsync_ValidUser_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var branchId = 1;
        var user = new User { Id = userId, Email = "test@example.com" };

        _mockUserRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        _mockBranchRepository
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _branchService.HasAccessToBranchAsync(userId, branchId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasAccessToBranchAsync_InvalidUser_ReturnsFalse()
    {
        // Arrange
        var userId = 999;
        var branchId = 1;

        _mockUserRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _branchService.HasAccessToBranchAsync(userId, branchId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateBranchDataAsync_ValidData_ReturnsTrue()
    {
        // Arrange
        var dto = new CreateBranchDto
        {
            OrganizationId = 1,
            Name = "Unique Branch",
            CountryCode = "US",
            Currency = "USD",
            TimeZone = "America/New_York"
        };

        var organization = new Organization { Id = 1, Name = "Test Org" };

        _mockOrganizationRepository
            .Setup(r => r.GetByIdAsync(dto.OrganizationId))
            .ReturnsAsync(organization);

        _mockBranchRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>()))
            .ReturnsAsync((Branch?)null);

        _mockCurrencyService
            .Setup(s => s.IsCurrencySupportedAsync(dto.Currency))
            .ReturnsAsync(true);

        _mockTimeZoneService
            .Setup(s => s.IsTimeZoneSupportedAsync(dto.TimeZone))
            .ReturnsAsync(true);

        // Act
        var result = await _branchService.ValidateBranchDataAsync(dto);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateBranchDataAsync_DuplicateName_ReturnsFalse()
    {
        // Arrange
        var dto = new CreateBranchDto
        {
            OrganizationId = 1,
            Name = "Existing Branch",
            CountryCode = "US",
            Currency = "USD",
            TimeZone = "America/New_York"
        };

        var organization = new Organization { Id = 1, Name = "Test Org" };
        var existingBranch = new Branch { Id = 2, Name = dto.Name, OrganizationId = dto.OrganizationId };

        _mockOrganizationRepository
            .Setup(r => r.GetByIdAsync(dto.OrganizationId))
            .ReturnsAsync(organization);

        _mockBranchRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>()))
            .ReturnsAsync(existingBranch);

        // Act
        var result = await _branchService.ValidateBranchDataAsync(dto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateBranchDataAsync_InvalidCountryCode_ReturnsFalse()
    {
        // Arrange
        var dto = new CreateBranchDto
        {
            OrganizationId = 1,
            Name = "Test Branch",
            CountryCode = "INVALID",
            Currency = "USD",
            TimeZone = "America/New_York"
        };

        var organization = new Organization { Id = 1, Name = "Test Org" };

        _mockOrganizationRepository
            .Setup(r => r.GetByIdAsync(dto.OrganizationId))
            .ReturnsAsync(organization);

        _mockBranchRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>()))
            .ReturnsAsync((Branch?)null);

        // Act
        var result = await _branchService.ValidateBranchDataAsync(dto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateBranchDataAsync_InvalidCurrency_ReturnsFalse()
    {
        // Arrange
        var dto = new CreateBranchDto
        {
            OrganizationId = 1,
            Name = "Test Branch",
            CountryCode = "US",
            Currency = "INVALID",
            TimeZone = "America/New_York"
        };

        var organization = new Organization { Id = 1, Name = "Test Org" };

        _mockOrganizationRepository
            .Setup(r => r.GetByIdAsync(dto.OrganizationId))
            .ReturnsAsync(organization);

        _mockBranchRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>()))
            .ReturnsAsync((Branch?)null);

        _mockCurrencyService
            .Setup(s => s.IsCurrencySupportedAsync(dto.Currency))
            .ReturnsAsync(false);

        _mockTimeZoneService
            .Setup(s => s.IsTimeZoneSupportedAsync(dto.TimeZone))
            .ReturnsAsync(true);

        // Act
        var result = await _branchService.ValidateBranchDataAsync(dto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateBranchDataAsync_InvalidTimeZone_ReturnsFalse()
    {
        // Arrange
        var dto = new CreateBranchDto
        {
            OrganizationId = 1,
            Name = "Test Branch",
            CountryCode = "US",
            Currency = "USD",
            TimeZone = "Invalid/TimeZone"
        };

        var organization = new Organization { Id = 1, Name = "Test Org" };

        _mockOrganizationRepository
            .Setup(r => r.GetByIdAsync(dto.OrganizationId))
            .ReturnsAsync(organization);

        _mockBranchRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>()))
            .ReturnsAsync((Branch?)null);

        _mockCurrencyService
            .Setup(s => s.IsCurrencySupportedAsync(dto.Currency))
            .ReturnsAsync(true);

        _mockTimeZoneService
            .Setup(s => s.IsTimeZoneSupportedAsync(dto.TimeZone))
            .ReturnsAsync(false);

        // Act
        var result = await _branchService.ValidateBranchDataAsync(dto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateComplianceSettingsAsync_ValidBranch_UpdatesSuccessfully()
    {
        // Arrange
        var branchId = 1;
        var branch = new Branch
        {
            Id = branchId,
            Name = "Test Branch",
            Organization = new Organization { Id = 1, Name = "Test Org" },
            Employees = new List<Employee>()
        };

        var complianceDto = new BranchComplianceDto
        {
            TaxSettings = new Dictionary<string, object> { { "TaxRate", 0.15 } },
            LaborLaws = new Dictionary<string, object> { { "MinimumWage", 15.0 } },
            StatutoryRequirements = new Dictionary<string, object> { { "PF", true } }
        };

        _mockBranchRepository
            .Setup(r => r.GetByIdAsync(branchId, It.IsAny<System.Linq.Expressions.Expression<Func<Branch, object>>[]>()))
            .ReturnsAsync(branch);

        _mockBranchRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Branch>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _branchService.UpdateComplianceSettingsAsync(branchId, complianceDto);

        // Assert
        _mockBranchRepository.Verify(r => r.UpdateAsync(It.IsAny<Branch>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateLocalHolidaysAsync_ValidBranch_UpdatesSuccessfully()
    {
        // Arrange
        var branchId = 1;
        var branch = new Branch
        {
            Id = branchId,
            Name = "Test Branch",
            Organization = new Organization { Id = 1, Name = "Test Org" },
            Employees = new List<Employee>()
        };

        var holidays = new List<LocalHolidayDto>
        {
            new LocalHolidayDto { Name = "New Year", Date = new DateTime(2024, 1, 1), IsRecurring = true },
            new LocalHolidayDto { Name = "Independence Day", Date = new DateTime(2024, 7, 4), IsRecurring = true }
        };

        _mockBranchRepository
            .Setup(r => r.GetByIdAsync(branchId, It.IsAny<System.Linq.Expressions.Expression<Func<Branch, object>>[]>()))
            .ReturnsAsync(branch);

        _mockBranchRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Branch>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _branchService.UpdateLocalHolidaysAsync(branchId, holidays);

        // Assert
        _mockBranchRepository.Verify(r => r.UpdateAsync(It.IsAny<Branch>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_ExistingBranch_ReturnsTrue()
    {
        // Arrange
        var branchId = 1;
        _mockBranchRepository
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _branchService.ExistsAsync(branchId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_NonExistingBranch_ReturnsFalse()
    {
        // Arrange
        var branchId = 999;
        _mockBranchRepository
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await _branchService.ExistsAsync(branchId);

        // Assert
        Assert.False(result);
    }
}
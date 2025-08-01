using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class OrganizationServiceTests
{
    private readonly Mock<IOrganizationRepository> _mockOrganizationRepository;
    private readonly Mock<ILogger<OrganizationService>> _mockLogger;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly OrganizationService _organizationService;

    public OrganizationServiceTests()
    {
        _mockOrganizationRepository = new Mock<IOrganizationRepository>();
        _mockLogger = new Mock<ILogger<OrganizationService>>();
        _mockAuditService = new Mock<IAuditService>();

        _organizationService = new OrganizationService(
            _mockOrganizationRepository.Object,
            _mockLogger.Object,
            _mockAuditService.Object);
    }

    [Fact]
    public async Task CreateOrganizationAsync_ValidRequest_ReturnsOrganization()
    {
        // Arrange
        var request = new CreateOrganizationRequest
        {
            Name = "Test Organization",
            Address = "123 Test St",
            Email = "test@org.com",
            Phone = "123-456-7890",
            NormalWorkingHours = 8.0m,
            OvertimeRate = 1.5m,
            ProductiveHoursThreshold = 6,
            BranchIsolationEnabled = false
        };

        var expectedOrganization = new Organization
        {
            Id = 1,
            Name = "Test Organization",
            Address = "123 Test St",
            Email = "test@org.com",
            Phone = "123-456-7890"
        };

        _mockOrganizationRepository.Setup(x => x.IsNameUniqueAsync(request.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockOrganizationRepository.Setup(x => x.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrganization);
        _mockOrganizationRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _organizationService.CreateOrganizationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrganization.Name, result.Name);
        Assert.Equal(expectedOrganization.Address, result.Address);
        Assert.Equal(expectedOrganization.Email, result.Email);
        _mockOrganizationRepository.Verify(x => x.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockOrganizationRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrganizationAsync_DuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateOrganizationRequest
        {
            Name = "Duplicate Organization"
        };

        _mockOrganizationRepository.Setup(x => x.IsNameUniqueAsync(request.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _organizationService.CreateOrganizationAsync(request));
        Assert.Contains("Organization name 'Duplicate Organization' is already in use", exception.Message);
    }

    [Fact]
    public async Task GetOrganizationByIdAsync_ExistingOrganization_ReturnsOrganization()
    {
        // Arrange
        var organizationId = 1;
        var expectedOrganization = new Organization
        {
            Id = organizationId,
            Name = "Test Organization"
        };

        _mockOrganizationRepository.Setup(x => x.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrganization);

        // Act
        var result = await _organizationService.GetOrganizationByIdAsync(organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrganization.Id, result.Id);
        Assert.Equal(expectedOrganization.Name, result.Name);
    }

    [Fact]
    public async Task GetOrganizationByIdAsync_NonExistingOrganization_ReturnsNull()
    {
        // Arrange
        var organizationId = 999;
        _mockOrganizationRepository.Setup(x => x.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        // Act
        var result = await _organizationService.GetOrganizationByIdAsync(organizationId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateOrganizationAsync_ValidRequest_ReturnsUpdatedOrganization()
    {
        // Arrange
        var organizationId = 1;
        var existingOrganization = new Organization
        {
            Id = organizationId,
            Name = "Original Name",
            Address = "Original Address"
        };

        var updateRequest = new UpdateOrganizationRequest
        {
            Name = "Updated Name",
            Address = "Updated Address"
        };

        _mockOrganizationRepository.Setup(x => x.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrganization);
        _mockOrganizationRepository.Setup(x => x.IsNameUniqueAsync(updateRequest.Name!, organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockOrganizationRepository.Setup(x => x.UpdateAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrganization);
        _mockOrganizationRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _organizationService.UpdateOrganizationAsync(organizationId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated Address", result.Address);
        _mockOrganizationRepository.Verify(x => x.UpdateAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockOrganizationRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrganizationAsync_OrganizationNotFound_ThrowsArgumentException()
    {
        // Arrange
        var organizationId = 999;
        var updateRequest = new UpdateOrganizationRequest { Name = "Updated Name" };

        _mockOrganizationRepository.Setup(x => x.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _organizationService.UpdateOrganizationAsync(organizationId, updateRequest));
        Assert.Contains("Organization with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task DeleteOrganizationAsync_ExistingOrganization_ReturnsTrue()
    {
        // Arrange
        var organizationId = 1;
        var existingOrganization = new Organization
        {
            Id = organizationId,
            Name = "Test Organization"
        };

        _mockOrganizationRepository.Setup(x => x.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrganization);
        _mockOrganizationRepository.Setup(x => x.SoftDeleteAsync(organizationId, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockOrganizationRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _organizationService.DeleteOrganizationAsync(organizationId, "test-user");

        // Assert
        Assert.True(result);
        _mockOrganizationRepository.Verify(x => x.SoftDeleteAsync(organizationId, "test-user", It.IsAny<CancellationToken>()), Times.Once);
        _mockOrganizationRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteOrganizationAsync_NonExistingOrganization_ReturnsFalse()
    {
        // Arrange
        var organizationId = 999;
        _mockOrganizationRepository.Setup(x => x.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        // Act
        var result = await _organizationService.DeleteOrganizationAsync(organizationId);

        // Assert
        Assert.False(result);
        _mockOrganizationRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllOrganizationsAsync_ReturnsAllOrganizations()
    {
        // Arrange
        var organizations = new List<Organization>
        {
            new() { Id = 1, Name = "Organization 1" },
            new() { Id = 2, Name = "Organization 2" }
        };

        _mockOrganizationRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(organizations);

        // Act
        var result = await _organizationService.GetAllOrganizationsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockOrganizationRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateConfigurationAsync_ValidRequest_ReturnsUpdatedOrganization()
    {
        // Arrange
        var organizationId = 1;
        var existingOrganization = new Organization
        {
            Id = organizationId,
            Name = "Test Organization",
            NormalWorkingHours = 8.0m,
            OvertimeRate = 1.5m
        };

        var configRequest = new OrganizationConfigurationRequest
        {
            NormalWorkingHours = 9.0m,
            OvertimeRate = 2.0m,
            CustomSettings = new Dictionary<string, object> { { "TestSetting", "TestValue" } }
        };

        _mockOrganizationRepository.Setup(x => x.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrganization);
        _mockOrganizationRepository.Setup(x => x.UpdateAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrganization);
        _mockOrganizationRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _organizationService.UpdateConfigurationAsync(organizationId, configRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(9.0m, result.NormalWorkingHours);
        Assert.Equal(2.0m, result.OvertimeRate);
        _mockOrganizationRepository.Verify(x => x.UpdateAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Organization;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class OrganizationServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly Mock<ILogger<OrganizationService>> _mockLogger;
    private readonly Mock<IRepository<Organization>> _mockOrganizationRepository;
    private readonly Mock<IRepository<Employee>> _mockEmployeeRepository;
    private readonly OrganizationService _organizationService;

    public OrganizationServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockLogger = new Mock<ILogger<OrganizationService>>();
        _mockOrganizationRepository = new Mock<IRepository<Organization>>();
        _mockEmployeeRepository = new Mock<IRepository<Employee>>();

        _mockUnitOfWork.Setup(u => u.Organizations).Returns(_mockOrganizationRepository.Object);
        _mockUnitOfWork.Setup(u => u.Employees).Returns(_mockEmployeeRepository.Object);

        _organizationService = new OrganizationService(_mockUnitOfWork.Object, _mockFileStorageService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingOrganization_ReturnsOrganization()
    {
        // Arrange
        var organizationId = 1;
        var expectedOrganization = new Organization
        {
            Id = organizationId,
            Name = "Test Organization",
            Email = "test@example.com",
            Branches = new List<Branch>()
        };

        _mockOrganizationRepository
            .Setup(r => r.GetByIdAsync(organizationId, It.IsAny<System.Linq.Expressions.Expression<Func<Organization, object>>[]>()))
            .ReturnsAsync(expectedOrganization);

        // Act
        var result = await _organizationService.GetByIdAsync(organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrganization.Id, result.Id);
        Assert.Equal(expectedOrganization.Name, result.Name);
        Assert.Equal(expectedOrganization.Email, result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingOrganization_ReturnsNull()
    {
        // Arrange
        var organizationId = 999;
        _mockOrganizationRepository
            .Setup(r => r.GetByIdAsync(organizationId, It.IsAny<System.Linq.Expressions.Expression<Func<Organization, object>>[]>()))
            .ReturnsAsync((Organization?)null);

        // Act
        var result = await _organizationService.GetByIdAsync(organizationId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedOrganization()
    {
        // Arrange
        var dto = new CreateOrganizationDto
        {
            Name = "New Organization",
            Address = "123 Test Street",
            Email = "new@example.com",
            Phone = "1234567890",
            NormalWorkingHours = 8,
            OvertimeRate = 1.5m,
            ProductiveHoursThreshold = 6
        };

        _mockOrganizationRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Organization, bool>>>()))
            .ReturnsAsync((Organization?)null);

        _mockOrganizationRepository
            .Setup(r => r.AddAsync(It.IsAny<Organization>()))
            .ReturnsAsync((Organization o) => o);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _organizationService.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal(TimeSpan.FromHours(dto.NormalWorkingHours), result.NormalWorkingHours);
        _mockOrganizationRepository.Verify(r => r.AddAsync(It.IsAny<Organization>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingOrganization_UpdatesSuccessfully()
    {
        // Arrange
        var organizationId = 1;
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Original Name",
            Email = "original@example.com",
            Branches = new List<Branch>()
        };

        var updateDto = new UpdateOrganizationDto
        {
            Name = "Updated Name",
            Address = "Updated Address",
            Email = "updated@example.com",
            Phone = "9876543210",
            NormalWorkingHours = 9,
            OvertimeRate = 2.0m,
            ProductiveHoursThreshold = 7,
            BranchIsolationEnabled = false
        };

        _mockOrganizationRepository
            .Setup(r => r.GetByIdAsync(organizationId, It.IsAny<System.Linq.Expressions.Expression<Func<Organization, object>>[]>()))
            .ReturnsAsync(organization);

        _mockOrganizationRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Organization, bool>>>()))
            .ReturnsAsync((Organization?)null);

        _mockOrganizationRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Organization>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _organizationService.UpdateAsync(organizationId, updateDto);

        // Assert
        _mockOrganizationRepository.Verify(r => r.UpdateAsync(It.IsAny<Organization>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingOrganization_ThrowsArgumentException()
    {
        // Arrange
        var organizationId = 999;
        var updateDto = new UpdateOrganizationDto
        {
            Name = "Updated Name",
            Address = "Updated Address",
            Email = "updated@example.com",
            Phone = "9876543210",
            NormalWorkingHours = 9,
            OvertimeRate = 2.0m,
            ProductiveHoursThreshold = 7
        };

        _mockOrganizationRepository
            .Setup(r => r.GetByIdAsync(organizationId, It.IsAny<System.Linq.Expressions.Expression<Func<Organization, object>>[]>()))
            .ReturnsAsync((Organization?)null);

        _mockOrganizationRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Organization, bool>>>()))
            .ReturnsAsync((Organization?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _organizationService.UpdateAsync(organizationId, updateDto));
    }

    [Fact]
    public async Task DeleteAsync_OrganizationWithBranches_ThrowsInvalidOperationException()
    {
        // Arrange
        var organizationId = 1;
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Organization",
            Branches = new List<Branch> { new Branch { Id = 1, Name = "Test Branch" } }
        };

        _mockOrganizationRepository
            .Setup(r => r.GetByIdAsync(organizationId, It.IsAny<System.Linq.Expressions.Expression<Func<Organization, object>>[]>()))
            .ReturnsAsync(organization);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _organizationService.DeleteAsync(organizationId));
    }

    [Fact]
    public async Task ExistsAsync_ExistingOrganization_ReturnsTrue()
    {
        // Arrange
        var organizationId = 1;
        _mockOrganizationRepository
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Organization, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _organizationService.ExistsAsync(organizationId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_NonExistingOrganization_ReturnsFalse()
    {
        // Arrange
        var organizationId = 999;
        _mockOrganizationRepository
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Organization, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await _organizationService.ExistsAsync(organizationId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UploadLogoAsync_ValidData_ReturnsFilePath()
    {
        // Arrange
        var organizationId = 1;
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Organization",
            Branches = new List<Branch>()
        };

        var dto = new OrganizationLogoUploadDto
        {
            OrganizationId = organizationId,
            LogoData = new byte[] { 1, 2, 3, 4, 5 },
            FileName = "logo.png",
            ContentType = "image/png",
            FileSize = 5
        };

        var expectedFilePath = "organization-logos/unique-filename.png";

        _mockOrganizationRepository
            .Setup(r => r.GetByIdAsync(organizationId, It.IsAny<System.Linq.Expressions.Expression<Func<Organization, object>>[]>()))
            .ReturnsAsync(organization);

        _mockFileStorageService
            .Setup(s => s.SaveFileAsync(dto.LogoData, dto.FileName, "organization-logos"))
            .ReturnsAsync(expectedFilePath);

        _mockOrganizationRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Organization>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _organizationService.UploadLogoAsync(dto);

        // Assert
        Assert.Equal(expectedFilePath, result);
        _mockFileStorageService.Verify(s => s.SaveFileAsync(dto.LogoData, dto.FileName, "organization-logos"), Times.Once);
        _mockOrganizationRepository.Verify(r => r.UpdateAsync(It.IsAny<Organization>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ValidateOrganizationDataAsync_ValidData_ReturnsTrue()
    {
        // Arrange
        var dto = new CreateOrganizationDto
        {
            Name = "Unique Organization",
            Email = "unique@example.com"
        };

        _mockOrganizationRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Organization, bool>>>()))
            .ReturnsAsync((Organization?)null);

        // Act
        var result = await _organizationService.ValidateOrganizationDataAsync(dto);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateOrganizationDataAsync_DuplicateName_ReturnsFalse()
    {
        // Arrange
        var dto = new CreateOrganizationDto
        {
            Name = "Existing Organization",
            Email = "unique@example.com"
        };

        _mockOrganizationRepository
            .SetupSequence(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Organization, bool>>>()))
            .ReturnsAsync(new Organization { Name = dto.Name }) // First call for name check
            .ReturnsAsync((Organization?)null); // Second call for email check

        // Act
        var result = await _organizationService.ValidateOrganizationDataAsync(dto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateOrganizationDataAsync_DuplicateEmail_ReturnsFalse()
    {
        // Arrange
        var dto = new CreateOrganizationDto
        {
            Name = "Unique Organization",
            Email = "existing@example.com"
        };

        _mockOrganizationRepository
            .SetupSequence(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Organization, bool>>>()))
            .ReturnsAsync((Organization?)null) // First call for name check
            .ReturnsAsync(new Organization { Email = dto.Email }); // Second call for email check

        // Act
        var result = await _organizationService.ValidateOrganizationDataAsync(dto);

        // Assert
        Assert.False(result);
    }
}
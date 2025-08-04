using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Infrastructure.Services;
using Xunit;
using FluentAssertions;

namespace StrideHR.Tests.Services;

public class BasicEmployeeServiceTests
{
    private readonly Mock<ILogger<EmployeeService>> _mockLogger;
    private readonly Mock<IRepository<Employee>> _mockEmployeeRepository;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly EmployeeService _employeeService;

    public BasicEmployeeServiceTests()
    {
        _mockLogger = new Mock<ILogger<EmployeeService>>();
        _mockEmployeeRepository = new Mock<IRepository<Employee>>();
        _mockFileStorageService = new Mock<IFileStorageService>();

        // Create a minimal mock UnitOfWork
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.Employees).Returns(_mockEmployeeRepository.Object);

        _employeeService = new EmployeeService(
            mockUnitOfWork.Object,
            _mockFileStorageService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEmployee_ReturnsEmployee()
    {
        // Arrange
        var employeeId = 1;
        var expectedEmployee = new Employee
        {
            Id = employeeId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            EmployeeId = "EMP001"
        };

        _mockEmployeeRepository
            .Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(expectedEmployee);

        // Act
        var result = await _employeeService.GetByIdAsync(employeeId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(employeeId);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john.doe@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingEmployee_ReturnsNull()
    {
        // Arrange
        var employeeId = 999;
        _mockEmployeeRepository
            .Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync((Employee?)null);

        // Act
        var result = await _employeeService.GetByIdAsync(employeeId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ExistingEmployee_ReturnsTrue()
    {
        // Arrange
        var employeeId = 1;
        _mockEmployeeRepository
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Employee, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _employeeService.ExistsAsync(employeeId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingEmployee_ReturnsFalse()
    {
        // Arrange
        var employeeId = 999;
        _mockEmployeeRepository
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Employee, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await _employeeService.ExistsAsync(employeeId);

        // Assert
        result.Should().BeFalse();
    }
}
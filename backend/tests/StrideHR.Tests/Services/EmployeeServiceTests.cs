using FluentAssertions;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Services;

namespace StrideHR.Tests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<Employee>> _mockEmployeeRepository;
    private readonly EmployeeService _employeeService;

    public EmployeeServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockEmployeeRepository = new Mock<IRepository<Employee>>();
        _mockUnitOfWork.Setup(u => u.Employees).Returns(_mockEmployeeRepository.Object);
        _employeeService = new EmployeeService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEmployee_ReturnsEmployee()
    {
        // Arrange
        var employeeId = 1;
        var expectedEmployee = new Employee
        {
            Id = employeeId,
            EmployeeId = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Status = EmployeeStatus.Active
        };

        _mockEmployeeRepository
            .Setup(r => r.GetByIdAsync(employeeId, It.IsAny<System.Linq.Expressions.Expression<Func<Employee, object>>[]>()))
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
            .Setup(r => r.GetByIdAsync(employeeId, It.IsAny<System.Linq.Expressions.Expression<Func<Employee, object>>[]>()))
            .ReturnsAsync((Employee?)null);

        // Act
        var result = await _employeeService.GetByIdAsync(employeeId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByEmployeeIdAsync_ExistingEmployeeId_ReturnsTrue()
    {
        // Arrange
        var employeeId = "EMP001";
        _mockEmployeeRepository
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _employeeService.ExistsByEmployeeIdAsync(employeeId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmployeeIdAsync_NonExistingEmployeeId_ReturnsFalse()
    {
        // Arrange
        var employeeId = "EMP999";
        _mockEmployeeRepository
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await _employeeService.ExistsByEmployeeIdAsync(employeeId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ValidEmployee_ReturnsCreatedEmployee()
    {
        // Arrange
        var newEmployee = new Employee
        {
            EmployeeId = "EMP002",
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Status = EmployeeStatus.Active,
            BranchId = 1
        };

        _mockEmployeeRepository
            .Setup(r => r.AddAsync(It.IsAny<Employee>()))
            .ReturnsAsync(newEmployee);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _employeeService.CreateAsync(newEmployee);

        // Assert
        result.Should().NotBeNull();
        result.EmployeeId.Should().Be("EMP002");
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");

        _mockEmployeeRepository.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
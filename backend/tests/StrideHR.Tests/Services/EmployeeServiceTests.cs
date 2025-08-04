using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Employee;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly Mock<ILogger<EmployeeService>> _mockLogger;
    private readonly Mock<IRepository<Employee>> _mockEmployeeRepository;
    private readonly Mock<IRepository<Branch>> _mockBranchRepository;
    private readonly EmployeeService _employeeService;

    public EmployeeServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockLogger = new Mock<ILogger<EmployeeService>>();
        _mockEmployeeRepository = new Mock<IRepository<Employee>>();
        _mockBranchRepository = new Mock<IRepository<Branch>>();

        // Setup UnitOfWork to return the mocked repositories
        _mockUnitOfWork.Setup(u => u.Employees).Returns(_mockEmployeeRepository.Object);
        _mockUnitOfWork.Setup(u => u.Branches).Returns(_mockBranchRepository.Object);

        _employeeService = new EmployeeService(_mockUnitOfWork.Object, _mockFileStorageService.Object, _mockLogger.Object);
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
            EmployeeId = "NYC-25-001"
        };

        _mockEmployeeRepository
            .Setup(r => r.GetByIdAsync(employeeId, It.IsAny<System.Linq.Expressions.Expression<Func<Employee, object>>[]>()))
            .ReturnsAsync(expectedEmployee);

        // Act
        var result = await _employeeService.GetByIdAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedEmployee.Id, result.Id);
        Assert.Equal(expectedEmployee.FirstName, result.FirstName);
        Assert.Equal(expectedEmployee.LastName, result.LastName);
        Assert.Equal(expectedEmployee.Email, result.Email);
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
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidEmployee_ReturnsCreatedEmployee()
    {
        // Arrange
        var employee = new Employee
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            BranchId = 1
        };

        _mockEmployeeRepository
            .Setup(r => r.AddAsync(It.IsAny<Employee>()))
            .ReturnsAsync(employee);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _employeeService.CreateAsync(employee);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employee.FirstName, result.FirstName);
        Assert.Equal(employee.LastName, result.LastName);
        Assert.Equal(employee.Email, result.Email);
        _mockEmployeeRepository.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_ExistingEmployee_ReturnsTrue()
    {
        // Arrange
        var employeeId = 1;
        _mockEmployeeRepository
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _employeeService.ExistsAsync(employeeId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_NonExistingEmployee_ReturnsFalse()
    {
        // Arrange
        var employeeId = 999;
        _mockEmployeeRepository
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await _employeeService.ExistsAsync(employeeId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GenerateEmployeeIdAsync_ValidBranch_ReturnsFormattedId()
    {
        // Arrange
        var branchId = 1;
        var branch = new Branch { Id = branchId, Name = "New York" };
        var existingEmployees = new List<Employee>();

        _mockBranchRepository
            .Setup(r => r.GetByIdAsync(branchId))
            .ReturnsAsync(branch);

        _mockEmployeeRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(existingEmployees);

        // Act
        var result = await _employeeService.GenerateEmployeeIdAsync(branchId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("NEW", result); // Branch code
        Assert.Contains(DateTime.Now.Year.ToString().Substring(2), result); // Year
        Assert.Contains("001", result); // Sequence number
    }

    [Fact]
    public async Task GenerateEmployeeIdAsync_InvalidBranch_ThrowsArgumentException()
    {
        // Arrange
        var branchId = 999;
        _mockBranchRepository
            .Setup(r => r.GetByIdAsync(branchId))
            .ReturnsAsync((Branch?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _employeeService.GenerateEmployeeIdAsync(branchId));
    }
}
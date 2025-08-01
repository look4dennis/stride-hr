using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
    private readonly Mock<IRepository<Branch>> _mockBranchRepository;
    private readonly Mock<ILogger<EmployeeService>> _mockLogger;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly EmployeeService _employeeService;

    public EmployeeServiceTests()
    {
        _mockEmployeeRepository = new Mock<IEmployeeRepository>();
        _mockBranchRepository = new Mock<IRepository<Branch>>();
        _mockLogger = new Mock<ILogger<EmployeeService>>();
        _mockAuditService = new Mock<IAuditService>();

        _employeeService = new EmployeeService(
            _mockEmployeeRepository.Object,
            _mockBranchRepository.Object,
            _mockLogger.Object,
            _mockAuditService.Object);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ValidRequest_ReturnsEmployee()
    {
        // Arrange
        var branchId = 1;
        var branch = new Branch { Id = branchId, Name = "Test Branch" };
        var request = new CreateEmployeeRequest
        {
            BranchId = branchId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            JoiningDate = DateTime.Now,
            BasicSalary = 50000
        };

        var expectedEmployee = new Employee
        {
            Id = 1,
            BranchId = branchId,
            EmployeeId = "TST-EMP-25-001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Branch = branch
        };

        _mockBranchRepository.Setup(x => x.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _mockEmployeeRepository.Setup(x => x.IsEmailUniqueAsync(request.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockEmployeeRepository.Setup(x => x.GetNextEmployeeSequenceAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockEmployeeRepository.Setup(x => x.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEmployee);
        _mockEmployeeRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _employeeService.CreateEmployeeAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedEmployee.FirstName, result.FirstName);
        Assert.Equal(expectedEmployee.LastName, result.LastName);
        Assert.Equal(expectedEmployee.Email, result.Email);
        _mockEmployeeRepository.Verify(x => x.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEmployeeRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEmployeeAsync_BranchNotFound_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            BranchId = 999,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com"
        };

        _mockBranchRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _employeeService.CreateEmployeeAsync(request));
        Assert.Contains("Branch with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task CreateEmployeeAsync_EmailNotUnique_ThrowsInvalidOperationException()
    {
        // Arrange
        var branchId = 1;
        var branch = new Branch { Id = branchId, Name = "Test Branch" };
        var request = new CreateEmployeeRequest
        {
            BranchId = branchId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com"
        };

        _mockBranchRepository.Setup(x => x.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _mockEmployeeRepository.Setup(x => x.IsEmailUniqueAsync(request.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _employeeService.CreateEmployeeAsync(request));
        Assert.Contains("Email john.doe@test.com is already in use", exception.Message);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_ExistingEmployee_ReturnsEmployee()
    {
        // Arrange
        var employeeId = 1;
        var expectedEmployee = new Employee
        {
            Id = employeeId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com"
        };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEmployee);

        // Act
        var result = await _employeeService.GetEmployeeByIdAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedEmployee.Id, result.Id);
        Assert.Equal(expectedEmployee.FirstName, result.FirstName);
        Assert.Equal(expectedEmployee.LastName, result.LastName);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_NonExistingEmployee_ReturnsNull()
    {
        // Arrange
        var employeeId = 999;
        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act
        var result = await _employeeService.GetEmployeeByIdAsync(employeeId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_ValidRequest_ReturnsUpdatedEmployee()
    {
        // Arrange
        var employeeId = 1;
        var existingEmployee = new Employee
        {
            Id = employeeId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com"
        };

        var updateRequest = new UpdateEmployeeRequest
        {
            FirstName = "Jane",
            Email = "jane.doe@test.com"
        };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEmployee);
        _mockEmployeeRepository.Setup(x => x.IsEmailUniqueAsync(updateRequest.Email!, employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockEmployeeRepository.Setup(x => x.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEmployee);
        _mockEmployeeRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _employeeService.UpdateEmployeeAsync(employeeId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("jane.doe@test.com", result.Email);
        _mockEmployeeRepository.Verify(x => x.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEmployeeRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_EmployeeNotFound_ThrowsArgumentException()
    {
        // Arrange
        var employeeId = 999;
        var updateRequest = new UpdateEmployeeRequest { FirstName = "Jane" };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _employeeService.UpdateEmployeeAsync(employeeId, updateRequest));
        Assert.Contains("Employee with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_ExistingEmployee_ReturnsTrue()
    {
        // Arrange
        var employeeId = 1;
        var existingEmployee = new Employee
        {
            Id = employeeId,
            EmployeeId = "TST-EMP-25-001",
            FirstName = "John",
            LastName = "Doe"
        };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEmployee);
        _mockEmployeeRepository.Setup(x => x.SoftDeleteAsync(employeeId, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockEmployeeRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _employeeService.DeleteEmployeeAsync(employeeId, "test-user");

        // Assert
        Assert.True(result);
        _mockEmployeeRepository.Verify(x => x.SoftDeleteAsync(employeeId, "test-user", It.IsAny<CancellationToken>()), Times.Once);
        _mockEmployeeRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_NonExistingEmployee_ReturnsFalse()
    {
        // Arrange
        var employeeId = 999;
        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act
        var result = await _employeeService.DeleteEmployeeAsync(employeeId);

        // Assert
        Assert.False(result);
        _mockEmployeeRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchEmployeesAsync_ValidCriteria_ReturnsPagedResults()
    {
        // Arrange
        var criteria = new EmployeeSearchCriteria
        {
            SearchTerm = "John",
            PageNumber = 1,
            PageSize = 10
        };

        var employees = new List<Employee>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe" },
            new() { Id = 2, FirstName = "John", LastName = "Smith" }
        };

        _mockEmployeeRepository.Setup(x => x.SearchAsync(criteria, It.IsAny<CancellationToken>()))
            .ReturnsAsync((employees, 2));

        // Act
        var (result, totalCount) = await _employeeService.SearchEmployeesAsync(criteria);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, totalCount);
        Assert.Equal(2, result.Count());
        _mockEmployeeRepository.Verify(x => x.SearchAsync(criteria, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateEmployeeIdAsync_ValidBranch_ReturnsFormattedId()
    {
        // Arrange
        var branchId = 1;
        var branch = new Branch { Id = branchId, Name = "Test Branch" };
        var sequence = 5;

        _mockBranchRepository.Setup(x => x.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _mockEmployeeRepository.Setup(x => x.GetNextEmployeeSequenceAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sequence);

        // Act
        var result = await _employeeService.GenerateEmployeeIdAsync(branchId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TES-EMP-", result); // First 3 chars of "Test Branch"
        Assert.Contains("-005", result); // Sequence formatted as 3 digits
    }

    [Fact]
    public async Task IsEmailUniqueAsync_UniqueEmail_ReturnsTrue()
    {
        // Arrange
        var email = "unique@test.com";
        _mockEmployeeRepository.Setup(x => x.IsEmailUniqueAsync(email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _employeeService.IsEmailUniqueAsync(email);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsEmailUniqueAsync_DuplicateEmail_ReturnsFalse()
    {
        // Arrange
        var email = "duplicate@test.com";
        _mockEmployeeRepository.Setup(x => x.IsEmailUniqueAsync(email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _employeeService.IsEmailUniqueAsync(email);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateReportingStructureAsync_ValidStructure_ReturnsTrue()
    {
        // Arrange
        var employeeId = 1;
        var managerId = 2;
        var manager = new Employee { Id = managerId, FirstName = "Manager" };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(managerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(manager);
        _mockEmployeeRepository.Setup(x => x.IsCircularReferenceAsync(employeeId, managerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _employeeService.ValidateReportingStructureAsync(employeeId, managerId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateReportingStructureAsync_CircularReference_ReturnsFalse()
    {
        // Arrange
        var employeeId = 1;
        var managerId = 2;
        var manager = new Employee { Id = managerId, FirstName = "Manager" };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(managerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(manager);
        _mockEmployeeRepository.Setup(x => x.IsCircularReferenceAsync(employeeId, managerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _employeeService.ValidateReportingStructureAsync(employeeId, managerId);

        // Assert
        Assert.False(result);
    }
}
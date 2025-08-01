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
    private readonly Mock<IRepository<EmployeeOnboarding>> _mockOnboardingRepository;
    private readonly Mock<IRepository<EmployeeOnboardingTask>> _mockOnboardingTaskRepository;
    private readonly Mock<IRepository<EmployeeExit>> _mockExitRepository;
    private readonly Mock<IRepository<EmployeeExitTask>> _mockExitTaskRepository;
    private readonly EmployeeService _employeeService;

    public EmployeeServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockLogger = new Mock<ILogger<EmployeeService>>();
        _mockEmployeeRepository = new Mock<IRepository<Employee>>();
        _mockBranchRepository = new Mock<IRepository<Branch>>();
        _mockOnboardingRepository = new Mock<IRepository<EmployeeOnboarding>>();
        _mockOnboardingTaskRepository = new Mock<IRepository<EmployeeOnboardingTask>>();
        _mockExitRepository = new Mock<IRepository<EmployeeExit>>();
        _mockExitTaskRepository = new Mock<IRepository<EmployeeExitTask>>();

        _mockUnitOfWork.Setup(u => u.Employees).Returns(_mockEmployeeRepository.Object);
        _mockUnitOfWork.Setup(u => u.Branches).Returns(_mockBranchRepository.Object);
        _mockUnitOfWork.Setup(u => u.EmployeeOnboardings).Returns(_mockOnboardingRepository.Object);
        _mockUnitOfWork.Setup(u => u.EmployeeOnboardingTasks).Returns(_mockOnboardingTaskRepository.Object);
        _mockUnitOfWork.Setup(u => u.EmployeeExits).Returns(_mockExitRepository.Object);
        _mockUnitOfWork.Setup(u => u.EmployeeExitTasks).Returns(_mockExitTaskRepository.Object);

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
    public async Task CreateAsync_WithDto_ValidData_ReturnsCreatedEmployee()
    {
        // Arrange
        var dto = new CreateEmployeeDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "1234567890",
            DateOfBirth = new DateTime(1990, 1, 1),
            JoiningDate = DateTime.Today,
            Designation = "Software Engineer",
            Department = "IT",
            BasicSalary = 50000,
            BranchId = 1
        };

        var branch = new Branch { Id = 1, Name = "New York" };
        var existingEmployees = new List<Employee>();

        _mockBranchRepository
            .Setup(r => r.GetByIdAsync(dto.BranchId))
            .ReturnsAsync(branch);

        _mockEmployeeRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
            .ReturnsAsync((Employee?)null);

        _mockEmployeeRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
            .ReturnsAsync(existingEmployees);

        _mockEmployeeRepository
            .Setup(r => r.AddAsync(It.IsAny<Employee>()))
            .ReturnsAsync((Employee e) => e);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _employeeService.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.FirstName, result.FirstName);
        Assert.Equal(dto.LastName, result.LastName);
        Assert.Equal(dto.Email, result.Email);
        Assert.NotNull(result.EmployeeId);
        Assert.Contains("NEW", result.EmployeeId); // Branch code
        _mockEmployeeRepository.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingEmployee_UpdatesSuccessfully()
    {
        // Arrange
        var employeeId = 1;
        var employee = new Employee
        {
            Id = employeeId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        var updateDto = new UpdateEmployeeDto
        {
            FirstName = "John Updated",
            LastName = "Doe Updated",
            Email = "john.updated@example.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            Designation = "Senior Software Engineer",
            Department = "IT",
            BasicSalary = 60000,
            Status = EmployeeStatus.Active
        };

        _mockEmployeeRepository
            .Setup(r => r.GetByIdAsync(employeeId, It.IsAny<System.Linq.Expressions.Expression<Func<Employee, object>>[]>()))
            .ReturnsAsync(employee);

        _mockEmployeeRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
            .ReturnsAsync((Employee?)null);

        _mockEmployeeRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Employee>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _employeeService.UpdateAsync(employeeId, updateDto);

        // Assert
        _mockEmployeeRepository.Verify(r => r.UpdateAsync(It.IsAny<Employee>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingEmployee_ThrowsArgumentException()
    {
        // Arrange
        var employeeId = 999;
        var updateDto = new UpdateEmployeeDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            Designation = "Software Engineer",
            Department = "IT",
            BasicSalary = 50000,
            Status = EmployeeStatus.Active
        };

        _mockEmployeeRepository
            .Setup(r => r.GetByIdAsync(employeeId, It.IsAny<System.Linq.Expressions.Expression<Func<Employee, object>>[]>()))
            .ReturnsAsync((Employee?)null);

        _mockEmployeeRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
            .ReturnsAsync((Employee?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _employeeService.UpdateAsync(employeeId, updateDto));
    }

    [Fact]
    public async Task DeleteAsync_ExistingEmployee_DeletesSuccessfully()
    {
        // Arrange
        var employeeId = 1;

        _mockEmployeeRepository
            .Setup(r => r.DeleteAsync(employeeId))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _employeeService.DeleteAsync(employeeId);

        // Assert
        _mockEmployeeRepository.Verify(r => r.DeleteAsync(employeeId), Times.Once);
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
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
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

    [Fact]
    public async Task UploadProfilePhotoAsync_ValidData_ReturnsFilePath()
    {
        // Arrange
        var employeeId = 1;
        var employee = new Employee { Id = employeeId, FirstName = "John", LastName = "Doe" };
        var dto = new ProfilePhotoUploadDto
        {
            EmployeeId = employeeId,
            PhotoData = new byte[] { 1, 2, 3, 4, 5 },
            FileName = "profile.jpg",
            ContentType = "image/jpeg",
            FileSize = 5
        };

        var expectedFilePath = "profile-photos/unique-filename.jpg";

        _mockEmployeeRepository
            .Setup(r => r.GetByIdAsync(employeeId, It.IsAny<System.Linq.Expressions.Expression<Func<Employee, object>>[]>()))
            .ReturnsAsync(employee);

        _mockFileStorageService
            .Setup(s => s.SaveFileAsync(dto.PhotoData, dto.FileName, "profile-photos"))
            .ReturnsAsync(expectedFilePath);

        _mockEmployeeRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Employee>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _employeeService.UploadProfilePhotoAsync(dto);

        // Assert
        Assert.Equal(expectedFilePath, result);
        _mockFileStorageService.Verify(s => s.SaveFileAsync(dto.PhotoData, dto.FileName, "profile-photos"), Times.Once);
        _mockEmployeeRepository.Verify(r => r.UpdateAsync(It.IsAny<Employee>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ValidateEmployeeDataAsync_ValidData_ReturnsTrue()
    {
        // Arrange
        var dto = new CreateEmployeeDto
        {
            Email = "unique@example.com",
            BranchId = 1
        };

        _mockEmployeeRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
            .ReturnsAsync((Employee?)null);

        _mockBranchRepository
            .Setup(r => r.GetByIdAsync(dto.BranchId))
            .ReturnsAsync(new Branch { Id = dto.BranchId });

        // Act
        var result = await _employeeService.ValidateEmployeeDataAsync(dto);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateEmployeeDataAsync_DuplicateEmail_ReturnsFalse()
    {
        // Arrange
        var dto = new CreateEmployeeDto
        {
            Email = "existing@example.com",
            BranchId = 1
        };

        _mockEmployeeRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
            .ReturnsAsync(new Employee { Email = dto.Email });

        // Act
        var result = await _employeeService.ValidateEmployeeDataAsync(dto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateEmployeeDataAsync_InvalidBranch_ReturnsFalse()
    {
        // Arrange
        var dto = new CreateEmployeeDto
        {
            Email = "unique@example.com",
            BranchId = 999
        };

        _mockEmployeeRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
            .ReturnsAsync((Employee?)null);

        _mockBranchRepository
            .Setup(r => r.GetByIdAsync(dto.BranchId))
            .ReturnsAsync((Branch?)null);

        // Act
        var result = await _employeeService.ValidateEmployeeDataAsync(dto);

        // Assert
        Assert.False(result);
    }
}
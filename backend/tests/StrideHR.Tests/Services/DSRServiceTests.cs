using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.DSR;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class DSRServiceTests
{
    private readonly Mock<IDSRRepository> _mockDsrRepository;
    private readonly Mock<IProjectRepository> _mockProjectRepository;
    private readonly Mock<IProjectTaskRepository> _mockTaskRepository;
    private readonly Mock<IRepository<Employee>> _mockEmployeeRepository;
    private readonly Mock<IRepository<AttendanceRecord>> _mockAttendanceRepository;
    private readonly Mock<IRepository<Organization>> _mockOrganizationRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly DSRService _dsrService;

    public DSRServiceTests()
    {
        _mockDsrRepository = new Mock<IDSRRepository>();
        _mockProjectRepository = new Mock<IProjectRepository>();
        _mockTaskRepository = new Mock<IProjectTaskRepository>();
        _mockEmployeeRepository = new Mock<IRepository<Employee>>();
        _mockAttendanceRepository = new Mock<IRepository<AttendanceRecord>>();
        _mockOrganizationRepository = new Mock<IRepository<Organization>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _dsrService = new DSRService(
            _mockDsrRepository.Object,
            _mockProjectRepository.Object,
            _mockTaskRepository.Object,
            _mockEmployeeRepository.Object,
            _mockAttendanceRepository.Object,
            _mockOrganizationRepository.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task CreateDSRAsync_ValidRequest_ReturnsDSR()
    {
        // Arrange
        var request = new CreateDSRRequest
        {
            EmployeeId = 1,
            Date = DateTime.Today,
            ProjectId = 1,
            TaskId = 1,
            HoursWorked = 8,
            Description = "Test work description"
        };

        var employee = new Employee { Id = 1, FirstName = "John", LastName = "Doe" };
        var project = new Project { Id = 1, Name = "Test Project" };
        var task = new ProjectTask { Id = 1, ProjectId = 1, Title = "Test Task" };
        var dsr = new DSR { Id = 1, EmployeeId = 1, Date = DateTime.Today };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(employee);
        _mockDsrRepository.Setup(x => x.GetDSRByEmployeeAndDateAsync(1, DateTime.Today)).ReturnsAsync((DSR?)null);
        _mockProjectRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(project);
        _mockProjectRepository.Setup(x => x.IsEmployeeAssignedToProjectAsync(1, 1)).ReturnsAsync(true);
        _mockTaskRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(task);
        _mockDsrRepository.Setup(x => x.AddAsync(It.IsAny<DSR>())).ReturnsAsync(dsr);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        _mockDsrRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(dsr);

        // Act
        var result = await _dsrService.CreateDSRAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.EmployeeId);
        Assert.Equal(DateTime.Today, result.Date);
        _mockDsrRepository.Verify(x => x.AddAsync(It.IsAny<DSR>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateDSRAsync_EmployeeNotFound_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateDSRRequest
        {
            EmployeeId = 999,
            Date = DateTime.Today,
            HoursWorked = 8,
            Description = "Test work description"
        };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Employee?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _dsrService.CreateDSRAsync(request));
        Assert.Equal("Employee not found", exception.Message);
    }

    [Fact]
    public async Task SubmitDSRAsync_ValidRequest_ReturnsSubmittedDSR()
    {
        // Arrange
        var dsrId = 1;
        var employeeId = 1;
        var dsr = new DSR 
        { 
            Id = 1, 
            EmployeeId = 1, 
            Status = DSRStatus.Draft 
        };

        _mockDsrRepository.Setup(x => x.GetByIdAsync(dsrId)).ReturnsAsync(dsr);
        _mockDsrRepository.Setup(x => x.UpdateAsync(It.IsAny<DSR>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _dsrService.SubmitDSRAsync(dsrId, employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DSRStatus.Submitted, result.Status);
        Assert.NotNull(result.SubmittedAt);
        _mockDsrRepository.Verify(x => x.UpdateAsync(It.IsAny<DSR>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ReviewDSRAsync_ValidApproval_ReturnsApprovedDSR()
    {
        // Arrange
        var dsrId = 1;
        var request = new ReviewDSRRequest
        {
            ReviewerId = 2,
            Status = DSRStatus.Approved,
            ReviewComments = "Good work"
        };

        var dsr = new DSR 
        { 
            Id = 1, 
            EmployeeId = 1, 
            Status = DSRStatus.Submitted 
        };

        _mockDsrRepository.Setup(x => x.GetByIdAsync(dsrId)).ReturnsAsync(dsr);
        _mockDsrRepository.Setup(x => x.UpdateAsync(It.IsAny<DSR>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _dsrService.ReviewDSRAsync(dsrId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DSRStatus.Approved, result.Status);
        Assert.Equal(2, result.ReviewedBy);
        Assert.Equal("Good work", result.ReviewComments);
        Assert.NotNull(result.ReviewedAt);
        _mockDsrRepository.Verify(x => x.UpdateAsync(It.IsAny<DSR>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CanEmployeeSubmitDSRAsync_ValidEmployee_ReturnsTrue()
    {
        // Arrange
        var employeeId = 1;
        var date = DateTime.Today;
        var employee = new Employee { Id = 1, Status = EmployeeStatus.Active };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockDsrRepository.Setup(x => x.GetDSRByEmployeeAndDateAsync(employeeId, date)).ReturnsAsync((DSR?)null);

        // Act
        var result = await _dsrService.CanEmployeeSubmitDSRAsync(employeeId, date);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetTotalHoursByProjectAsync_ValidProject_ReturnsCorrectHours()
    {
        // Arrange
        var projectId = 1;
        var startDate = DateTime.Today.AddDays(-7);
        var endDate = DateTime.Today;
        var expectedHours = 40m;

        _mockDsrRepository.Setup(x => x.GetTotalHoursByProjectAsync(projectId, startDate, endDate))
            .ReturnsAsync(expectedHours);

        // Act
        var result = await _dsrService.GetTotalHoursByProjectAsync(projectId, startDate, endDate);

        // Assert
        Assert.Equal(expectedHours, result);
        _mockDsrRepository.Verify(x => x.GetTotalHoursByProjectAsync(projectId, startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task GetTotalHoursByEmployeeAsync_ValidEmployee_ReturnsCorrectHours()
    {
        // Arrange
        var employeeId = 1;
        var startDate = DateTime.Today.AddDays(-7);
        var endDate = DateTime.Today;
        var expectedHours = 35m;

        _mockDsrRepository.Setup(x => x.GetTotalHoursByEmployeeAsync(employeeId, startDate, endDate))
            .ReturnsAsync(expectedHours);

        // Act
        var result = await _dsrService.GetTotalHoursByEmployeeAsync(employeeId, startDate, endDate);

        // Assert
        Assert.Equal(expectedHours, result);
        _mockDsrRepository.Verify(x => x.GetTotalHoursByEmployeeAsync(employeeId, startDate, endDate), Times.Once);
    }
}
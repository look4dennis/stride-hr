using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Services;
using Xunit;
using TaskStatus = StrideHR.Core.Entities.TaskStatus;

namespace StrideHR.Tests.Services;

/// <summary>
/// Unit tests for ProjectService
/// </summary>
public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _mockProjectRepository;
    private readonly Mock<IProjectTaskRepository> _mockTaskRepository;
    private readonly Mock<IProjectAssignmentRepository> _mockAssignmentRepository;
    private readonly Mock<ITaskAssignmentRepository> _mockTaskAssignmentRepository;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ILogger<ProjectService>> _mockLogger;
    private readonly ProjectService _projectService;

    public ProjectServiceTests()
    {
        _mockProjectRepository = new Mock<IProjectRepository>();
        _mockTaskRepository = new Mock<IProjectTaskRepository>();
        _mockAssignmentRepository = new Mock<IProjectAssignmentRepository>();
        _mockTaskAssignmentRepository = new Mock<ITaskAssignmentRepository>();
        _mockEmployeeRepository = new Mock<IEmployeeRepository>();
        _mockAuditService = new Mock<IAuditService>();
        _mockLogger = new Mock<ILogger<ProjectService>>();

        _projectService = new ProjectService(
            _mockProjectRepository.Object,
            _mockTaskRepository.Object,
            _mockAssignmentRepository.Object,
            _mockTaskAssignmentRepository.Object,
            _mockEmployeeRepository.Object,
            _mockAuditService.Object,
            _mockLogger.Object);
    }

    #region CreateProjectAsync Tests

    [Fact]
    public async Task CreateProjectAsync_ValidRequest_ReturnsProject()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 1,
            EmployeeId = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        var request = new CreateProjectRequest
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            EstimatedHours = 100,
            Budget = 10000,
            Priority = ProjectPriority.High,
            CreatedByEmployeeId = 1
        };

        var expectedProject = new Project
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            EstimatedHours = request.EstimatedHours,
            Budget = request.Budget,
            Priority = request.Priority,
            Status = ProjectStatus.Planning,
            CreatedByEmployeeId = request.CreatedByEmployeeId
        };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockProjectRepository.Setup(x => x.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProject);

        _mockProjectRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockAuditService.Setup(x => x.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _projectService.CreateProjectAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProject.Name, result.Name);
        Assert.Equal(expectedProject.Description, result.Description);
        Assert.Equal(ProjectStatus.Planning, result.Status);

        _mockEmployeeRepository.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _mockProjectRepository.Verify(x => x.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockProjectRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockAuditService.Verify(x => x.LogAsync("Project", It.IsAny<int>(), "Created", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProjectAsync_EmployeeNotFound_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "Test Project",
            CreatedByEmployeeId = 999
        };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _projectService.CreateProjectAsync(request));
        Assert.Contains("Employee with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task CreateProjectAsync_EndDateBeforeStartDate_ThrowsArgumentException()
    {
        // Arrange
        var employee = new Employee { Id = 1, EmployeeId = "EMP001" };
        var request = new CreateProjectRequest
        {
            Name = "Test Project",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(-1), // End date before start date
            CreatedByEmployeeId = 1
        };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _projectService.CreateProjectAsync(request));
        Assert.Contains("End date must be after start date", exception.Message);
    }

    #endregion

    #region UpdateProjectAsync Tests

    [Fact]
    public async Task UpdateProjectAsync_ValidRequest_ReturnsUpdatedProject()
    {
        // Arrange
        var existingProject = new Project
        {
            Id = 1,
            Name = "Old Name",
            Status = ProjectStatus.Planning,
            Priority = ProjectPriority.Medium
        };

        var request = new UpdateProjectRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            EstimatedHours = 150,
            Budget = 15000,
            Status = ProjectStatus.InProgress,
            Priority = ProjectPriority.High
        };

        _mockProjectRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProject);

        _mockProjectRepository.Setup(x => x.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProject);

        _mockProjectRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockAuditService.Setup(x => x.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _projectService.UpdateProjectAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Status, result.Status);
        Assert.Equal(request.Priority, result.Priority);

        _mockProjectRepository.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _mockProjectRepository.Verify(x => x.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockProjectRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockAuditService.Verify(x => x.LogAsync("Project", 1, "Updated", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProjectAsync_ProjectNotFound_ThrowsArgumentException()
    {
        // Arrange
        var request = new UpdateProjectRequest { Name = "Test" };

        _mockProjectRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _projectService.UpdateProjectAsync(999, request));
        Assert.Contains("Project with ID 999 not found", exception.Message);
    }

    #endregion

    #region AssignTeamMembersAsync Tests

    [Fact]
    public async Task AssignTeamMembersAsync_ValidRequest_ReturnsAssignments()
    {
        // Arrange
        var project = new Project { Id = 1, Name = "Test Project" };
        var employee = new Employee { Id = 1, EmployeeId = "EMP001", FirstName = "John", LastName = "Doe" };

        var request = new AssignTeamMembersRequest
        {
            TeamMembers = new List<TeamMemberAssignment>
            {
                new TeamMemberAssignment
                {
                    EmployeeId = 1,
                    Role = "Developer",
                    StartDate = DateTime.UtcNow
                }
            }
        };

        var expectedAssignment = new ProjectAssignment
        {
            Id = 1,
            EmployeeId = 1,
            ProjectId = 1,
            Role = "Developer",
            Status = AssignmentStatus.Active
        };

        _mockProjectRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockAssignmentRepository.Setup(x => x.IsEmployeeAssignedToProjectAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockAssignmentRepository.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProjectAssignment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProjectAssignment> { expectedAssignment });

        _mockAssignmentRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockAuditService.Setup(x => x.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _projectService.AssignTeamMembersAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var assignment = result.First();
        Assert.Equal(1, assignment.EmployeeId);
        Assert.Equal("Developer", assignment.Role);
        Assert.Equal(AssignmentStatus.Active, assignment.Status);

        _mockProjectRepository.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _mockEmployeeRepository.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _mockAssignmentRepository.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProjectAssignment>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignTeamMembersAsync_ProjectNotFound_ThrowsArgumentException()
    {
        // Arrange
        var request = new AssignTeamMembersRequest();

        _mockProjectRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _projectService.AssignTeamMembersAsync(999, request));
        Assert.Contains("Project with ID 999 not found", exception.Message);
    }

    #endregion

    #region CreateTaskAsync Tests

    [Fact]
    public async Task CreateTaskAsync_ValidRequest_ReturnsTask()
    {
        // Arrange
        var project = new Project { Id = 1, Name = "Test Project" };

        var request = new CreateTaskRequest
        {
            ProjectId = 1,
            Title = "Test Task",
            Description = "Test Description",
            EstimatedHours = 8,
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedEmployeeIds = new List<int>()
        };

        var expectedTask = new ProjectTask
        {
            Id = 1,
            ProjectId = 1,
            Title = request.Title,
            Description = request.Description,
            EstimatedHours = request.EstimatedHours,
            Priority = request.Priority,
            Status = Core.Entities.TaskStatus.ToDo,
            DueDate = request.DueDate
        };

        _mockProjectRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _mockTaskRepository.Setup(x => x.AddAsync(It.IsAny<ProjectTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTask);

        _mockTaskRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockAuditService.Setup(x => x.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _projectService.CreateTaskAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTask.Title, result.Title);
        Assert.Equal(expectedTask.Description, result.Description);
        Assert.Equal(Core.Entities.TaskStatus.ToDo, result.Status);

        _mockProjectRepository.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _mockTaskRepository.Verify(x => x.AddAsync(It.IsAny<ProjectTask>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockTaskRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockAuditService.Verify(x => x.LogAsync("ProjectTask", It.IsAny<int>(), "Created", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTaskAsync_ProjectNotFound_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            ProjectId = 999,
            Title = "Test Task"
        };

        _mockProjectRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _projectService.CreateTaskAsync(request));
        Assert.Contains("Project with ID 999 not found", exception.Message);
    }

    #endregion

    #region GetProjectProgressAsync Tests

    [Fact]
    public async Task GetProjectProgressAsync_ValidProject_ReturnsProgress()
    {
        // Arrange
        var tasks = new List<ProjectTask>
        {
            new ProjectTask { Id = 1, Status = Core.Entities.TaskStatus.Done, EstimatedHours = 8 },
            new ProjectTask { Id = 2, Status = Core.Entities.TaskStatus.InProgress, EstimatedHours = 16 },
            new ProjectTask { Id = 3, Status = Core.Entities.TaskStatus.ToDo, EstimatedHours = 24 }
        };

        var project = new Project
        {
            Id = 1,
            Name = "Test Project",
            EstimatedHours = 100,
            Tasks = tasks
        };

        _mockProjectRepository.Setup(x => x.GetProjectWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act
        var result = await _projectService.GetProjectProgressAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ProjectId);
        Assert.Equal("Test Project", result.ProjectName);
        Assert.Equal(3, result.TotalTasks);
        Assert.Equal(1, result.CompletedTasks);
        Assert.Equal(1, result.InProgressTasks);
        Assert.Equal(1, result.TodoTasks);
        Assert.Equal(33.33m, Math.Round(result.CompletionPercentage, 2));
        Assert.Equal(100, result.TotalEstimatedHours);

        _mockProjectRepository.Verify(x => x.GetProjectWithDetailsAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProjectProgressAsync_ProjectNotFound_ThrowsArgumentException()
    {
        // Arrange
        _mockProjectRepository.Setup(x => x.GetProjectWithDetailsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _projectService.GetProjectProgressAsync(999));
        Assert.Contains("Project with ID 999 not found", exception.Message);
    }

    #endregion

    #region DeleteProjectAsync Tests

    [Fact]
    public async Task DeleteProjectAsync_ValidProject_ReturnsTrue()
    {
        // Arrange
        var project = new Project { Id = 1, Name = "Test Project" };

        _mockProjectRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _mockProjectRepository.Setup(x => x.SoftDeleteAsync(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockProjectRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockAuditService.Setup(x => x.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _projectService.DeleteProjectAsync(1, "testuser");

        // Assert
        Assert.True(result);

        _mockProjectRepository.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _mockProjectRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<Project>(), "testuser", It.IsAny<CancellationToken>()), Times.Once);
        _mockProjectRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockAuditService.Verify(x => x.LogAsync("Project", 1, "Deleted", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProjectAsync_ProjectNotFound_ReturnsFalse()
    {
        // Arrange
        _mockProjectRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act
        var result = await _projectService.DeleteProjectAsync(999, "testuser");

        // Assert
        Assert.False(result);

        _mockProjectRepository.Verify(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()), Times.Once);
        _mockProjectRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Project;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _mockProjectRepository;
    private readonly Mock<IProjectTaskRepository> _mockTaskRepository;
    private readonly Mock<IProjectAssignmentRepository> _mockAssignmentRepository;
    private readonly Mock<ITaskAssignmentRepository> _mockTaskAssignmentRepository;
    private readonly Mock<IDSRRepository> _mockDsrRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ProjectService>> _mockLogger;
    private readonly ProjectService _projectService;

    public ProjectServiceTests()
    {
        _mockProjectRepository = new Mock<IProjectRepository>();
        _mockTaskRepository = new Mock<IProjectTaskRepository>();
        _mockAssignmentRepository = new Mock<IProjectAssignmentRepository>();
        _mockTaskAssignmentRepository = new Mock<ITaskAssignmentRepository>();
        _mockDsrRepository = new Mock<IDSRRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ProjectService>>();

        _projectService = new ProjectService(
            _mockProjectRepository.Object,
            _mockTaskRepository.Object,
            _mockAssignmentRepository.Object,
            _mockTaskAssignmentRepository.Object,
            _mockDsrRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateProjectAsync_ValidDto_ReturnsProjectDto()
    {
        // Arrange
        var createDto = new CreateProjectDto
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30),
            EstimatedHours = 100,
            Budget = 10000,
            BranchId = 1,
            TeamMemberIds = new List<int> { 1, 2 }
        };

        var project = new Project
        {
            Id = 1,
            Name = createDto.Name,
            Description = createDto.Description,
            StartDate = createDto.StartDate,
            EndDate = createDto.EndDate,
            EstimatedHours = createDto.EstimatedHours,
            Budget = createDto.Budget,
            BranchId = createDto.BranchId,
            Status = ProjectStatus.Planning
        };

        var projectDto = new ProjectDto
        {
            Id = 1,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status
        };

        _mockMapper.Setup(m => m.Map<Project>(createDto)).Returns(project);
        _mockProjectRepository.Setup(r => r.AddAsync(It.IsAny<Project>())).ReturnsAsync(project);
        _mockProjectRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(project.Id)).ReturnsAsync(project);
        _mockMapper.Setup(m => m.Map<ProjectDto>(project)).Returns(projectDto);

        // Act
        var result = await _projectService.CreateProjectAsync(createDto, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projectDto.Name, result.Name);
        Assert.Equal(projectDto.Description, result.Description);
        _mockProjectRepository.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Once);
        _mockProjectRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateProjectAsync_ValidDto_ReturnsUpdatedProjectDto()
    {
        // Arrange
        var projectId = 1;
        var updateDto = new UpdateProjectDto
        {
            Name = "Updated Project Name",
            Description = "Updated Description"
        };

        var existingProject = new Project
        {
            Id = projectId,
            Name = "Original Name",
            Description = "Original Description",
            Status = ProjectStatus.Planning
        };

        var updatedProject = new Project
        {
            Id = projectId,
            Name = updateDto.Name,
            Description = updateDto.Description,
            Status = ProjectStatus.Planning
        };

        var projectDto = new ProjectDto
        {
            Id = projectId,
            Name = updateDto.Name,
            Description = updateDto.Description
        };

        _mockProjectRepository.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(existingProject);
        _mockProjectRepository.Setup(r => r.UpdateAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);
        _mockProjectRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId)).ReturnsAsync(updatedProject);
        _mockMapper.Setup(m => m.Map<ProjectDto>(updatedProject)).Returns(projectDto);

        // Act
        var result = await _projectService.UpdateProjectAsync(projectId, updateDto, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateDto.Name, result.Name);
        Assert.Equal(updateDto.Description, result.Description);
        _mockProjectRepository.Verify(r => r.UpdateAsync(It.IsAny<Project>()), Times.Once);
        _mockProjectRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateProjectAsync_ProjectNotFound_ThrowsArgumentException()
    {
        // Arrange
        var projectId = 1;
        var updateDto = new UpdateProjectDto { Name = "Updated Name" };

        _mockProjectRepository.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync((Project?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _projectService.UpdateProjectAsync(projectId, updateDto, 1));
    }

    [Fact]
    public async Task DeleteProjectAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        var projectId = 1;
        var project = new Project { Id = projectId, Name = "Test Project" };

        _mockProjectRepository.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockProjectRepository.Setup(r => r.UpdateAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);
        _mockProjectRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _projectService.DeleteProjectAsync(projectId, 1);

        // Assert
        Assert.True(result);
        Assert.True(project.IsDeleted);
        Assert.NotNull(project.DeletedAt);
        _mockProjectRepository.Verify(r => r.UpdateAsync(It.IsAny<Project>()), Times.Once);
        _mockProjectRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteProjectAsync_ProjectNotFound_ReturnsFalse()
    {
        // Arrange
        var projectId = 1;
        _mockProjectRepository.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync((Project?)null);

        // Act
        var result = await _projectService.DeleteProjectAsync(projectId, 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetProjectByIdAsync_ValidId_ReturnsProjectDto()
    {
        // Arrange
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            Description = "Test Description"
        };

        var projectDto = new ProjectDto
        {
            Id = projectId,
            Name = project.Name,
            Description = project.Description
        };

        var progressDto = new ProjectProgressDto
        {
            ProjectId = projectId,
            CompletionPercentage = 50
        };

        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId)).ReturnsAsync(project);
        _mockMapper.Setup(m => m.Map<ProjectDto>(project)).Returns(projectDto);
        _mockTaskRepository.Setup(r => r.GetTasksByProjectAsync(projectId)).ReturnsAsync(new List<ProjectTask>());
        _mockDsrRepository.Setup(r => r.GetTotalHoursByProjectAsync(projectId, null, null)).ReturnsAsync(0);

        // Act
        var result = await _projectService.GetProjectByIdAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projectDto.Name, result.Name);
        Assert.Equal(projectDto.Description, result.Description);
    }

    [Fact]
    public async Task GetProjectByIdAsync_ProjectNotFound_ReturnsNull()
    {
        // Arrange
        var projectId = 1;
        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId)).ReturnsAsync((Project?)null);

        // Act
        var result = await _projectService.GetProjectByIdAsync(projectId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateTaskAsync_ValidDto_ReturnsTaskDto()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            ProjectId = 1,
            Title = "Test Task",
            Description = "Test Description",
            EstimatedHours = 10,
            Priority = TaskPriority.Medium
        };

        var task = new ProjectTask
        {
            Id = 1,
            ProjectId = createDto.ProjectId,
            Title = createDto.Title,
            Description = createDto.Description,
            EstimatedHours = createDto.EstimatedHours,
            Priority = createDto.Priority,
            Status = ProjectTaskStatus.ToDo
        };

        var taskDto = new ProjectTaskDto
        {
            Id = 1,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status
        };

        _mockMapper.Setup(m => m.Map<ProjectTask>(createDto)).Returns(task);
        _mockTaskRepository.Setup(r => r.GetMaxDisplayOrderAsync(createDto.ProjectId)).ReturnsAsync(0);
        _mockTaskRepository.Setup(r => r.AddAsync(It.IsAny<ProjectTask>())).ReturnsAsync(task);
        _mockTaskRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        _mockTaskRepository.Setup(r => r.GetTaskWithDetailsAsync(task.Id)).ReturnsAsync(task);
        _mockMapper.Setup(m => m.Map<ProjectTaskDto>(task)).Returns(taskDto);

        // Act
        var result = await _projectService.CreateTaskAsync(createDto, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskDto.Title, result.Title);
        Assert.Equal(taskDto.Description, result.Description);
        _mockTaskRepository.Verify(r => r.AddAsync(It.IsAny<ProjectTask>()), Times.Once);
        _mockTaskRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AssignTeamMembersAsync_ValidData_ReturnsTrue()
    {
        // Arrange
        var projectId = 1;
        var employeeIds = new List<int> { 1, 2, 3 };
        var project = new Project { Id = projectId, Name = "Test Project" };

        _mockProjectRepository.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockAssignmentRepository.Setup(r => r.GetAssignmentAsync(projectId, It.IsAny<int>()))
            .ReturnsAsync((ProjectAssignment?)null);
        _mockAssignmentRepository.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProjectAssignment>>()))
            .ReturnsAsync(It.IsAny<IEnumerable<ProjectAssignment>>());
        _mockAssignmentRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _projectService.AssignTeamMembersAsync(projectId, employeeIds, 1);

        // Assert
        Assert.True(result);
        _mockAssignmentRepository.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProjectAssignment>>()), Times.Once);
        _mockAssignmentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AssignTeamMembersAsync_ProjectNotFound_ReturnsFalse()
    {
        // Arrange
        var projectId = 1;
        var employeeIds = new List<int> { 1, 2 };

        _mockProjectRepository.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync((Project?)null);

        // Act
        var result = await _projectService.AssignTeamMembersAsync(projectId, employeeIds, 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_ValidData_ReturnsTrue()
    {
        // Arrange
        var taskId = 1;
        var newStatus = ProjectTaskStatus.Done;
        var task = new ProjectTask
        {
            Id = taskId,
            Title = "Test Task",
            Status = ProjectTaskStatus.InProgress
        };

        var assignments = new List<TaskAssignment>
        {
            new TaskAssignment { Id = 1, TaskId = taskId, EmployeeId = 1, CompletedDate = null }
        };

        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);
        _mockTaskAssignmentRepository.Setup(r => r.GetAssignmentsByTaskAsync(taskId)).ReturnsAsync(assignments);
        _mockTaskRepository.Setup(r => r.UpdateAsync(It.IsAny<ProjectTask>())).Returns(Task.CompletedTask);
        _mockTaskAssignmentRepository.Setup(r => r.UpdateAsync(It.IsAny<TaskAssignment>())).Returns(Task.CompletedTask);
        _mockTaskRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _projectService.UpdateTaskStatusAsync(taskId, newStatus, 1);

        // Assert
        Assert.True(result);
        Assert.Equal(newStatus, task.Status);
        Assert.NotNull(assignments[0].CompletedDate);
        _mockTaskRepository.Verify(r => r.UpdateAsync(It.IsAny<ProjectTask>()), Times.Once);
        _mockTaskAssignmentRepository.Verify(r => r.UpdateAsync(It.IsAny<TaskAssignment>()), Times.Once);
    }

    [Fact]
    public async Task GetProjectProgressAsync_ValidProjectId_ReturnsProgressDto()
    {
        // Arrange
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            EstimatedHours = 100,
            Budget = 10000
        };

        var tasks = new List<ProjectTask>
        {
            new ProjectTask { Id = 1, Status = ProjectTaskStatus.Done },
            new ProjectTask { Id = 2, Status = ProjectTaskStatus.InProgress },
            new ProjectTask { Id = 3, Status = ProjectTaskStatus.ToDo }
        };

        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId)).ReturnsAsync(project);
        _mockTaskRepository.Setup(r => r.GetTasksByProjectAsync(projectId)).ReturnsAsync(tasks);
        _mockDsrRepository.Setup(r => r.GetTotalHoursByProjectAsync(projectId, null, null)).ReturnsAsync(50);

        // Act
        var result = await _projectService.GetProjectProgressAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projectId, result.ProjectId);
        Assert.Equal(100, result.TotalEstimatedHours);
        Assert.Equal(50, result.ActualHoursWorked);
        Assert.Equal(3, result.TotalTasks);
        Assert.Equal(1, result.CompletedTasks);
        Assert.Equal(1, result.InProgressTasks);
        Assert.Equal(1, result.TodoTasks);
        Assert.True(result.IsOnTrack);
    }

    [Fact]
    public async Task IsProjectOnTrackAsync_ProjectOnTrack_ReturnsTrue()
    {
        // Arrange
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            EstimatedHours = 100
        };

        _mockProjectRepository.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockDsrRepository.Setup(r => r.GetTotalHoursByProjectAsync(projectId, null, null)).ReturnsAsync(80);

        // Act
        var result = await _projectService.IsProjectOnTrackAsync(projectId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsProjectOnTrackAsync_ProjectOverBudget_ReturnsFalse()
    {
        // Arrange
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            EstimatedHours = 100
        };

        _mockProjectRepository.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mockDsrRepository.Setup(r => r.GetTotalHoursByProjectAsync(projectId, null, null)).ReturnsAsync(120);

        // Act
        var result = await _projectService.IsProjectOnTrackAsync(projectId);

        // Assert
        Assert.False(result);
    }
}
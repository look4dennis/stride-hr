using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Project;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class ProjectMonitoringServiceTests
{
    private readonly Mock<IProjectRepository> _mockProjectRepository;
    private readonly Mock<IProjectAlertRepository> _mockAlertRepository;
    private readonly Mock<IProjectRiskRepository> _mockRiskRepository;
    private readonly Mock<IDSRRepository> _mockDsrRepository;
    private readonly Mock<IProjectAssignmentRepository> _mockAssignmentRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ProjectMonitoringService>> _mockLogger;
    private readonly ProjectMonitoringService _service;

    public ProjectMonitoringServiceTests()
    {
        _mockProjectRepository = new Mock<IProjectRepository>();
        _mockAlertRepository = new Mock<IProjectAlertRepository>();
        _mockRiskRepository = new Mock<IProjectRiskRepository>();
        _mockDsrRepository = new Mock<IDSRRepository>();
        _mockAssignmentRepository = new Mock<IProjectAssignmentRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ProjectMonitoringService>>();

        _service = new ProjectMonitoringService(
            _mockProjectRepository.Object,
            _mockAlertRepository.Object,
            _mockRiskRepository.Object,
            _mockDsrRepository.Object,
            _mockAssignmentRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetProjectHoursTrackingAsync_ValidProjectId_ReturnsHoursReport()
    {
        // Arrange
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            EstimatedHours = 100,
            StartDate = DateTime.Today.AddDays(-30),
            EndDate = DateTime.Today.AddDays(30)
        };

        var dsrRecords = new List<DSR>
        {
            new DSR { Id = 1, ProjectId = projectId, HoursWorked = 8, Date = DateTime.Today.AddDays(-1) },
            new DSR { Id = 2, ProjectId = projectId, HoursWorked = 6, Date = DateTime.Today.AddDays(-2) }
        };

        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId))
            .ReturnsAsync(project);
        _mockDsrRepository.Setup(r => r.GetProjectDSRsAsync(projectId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(dsrRecords);
        _mockAssignmentRepository.Setup(r => r.GetProjectTeamMembersAsync(projectId))
            .ReturnsAsync(new List<ProjectAssignment>());

        // Act
        var result = await _service.GetProjectHoursTrackingAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projectId, result.ProjectId);
        Assert.Equal("Test Project", result.ProjectName);
        Assert.Equal(100, result.EstimatedHours);
        Assert.Equal(14, result.TotalHoursWorked);
        Assert.Equal(-86, result.HoursVariance);
    }

    [Fact]
    public async Task GetProjectHoursTrackingAsync_InvalidProjectId_ThrowsArgumentException()
    {
        // Arrange
        var projectId = 999;
        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId))
            .ReturnsAsync((Project)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetProjectHoursTrackingAsync(projectId));
    }

    [Fact]
    public async Task GetProjectAnalyticsAsync_ValidProjectId_ReturnsAnalytics()
    {
        // Arrange
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            EstimatedHours = 100,
            Budget = 10000,
            Tasks = new List<ProjectTask>
            {
                new ProjectTask { Id = 1, Status = Core.Enums.ProjectTaskStatus.Completed },
                new ProjectTask { Id = 2, Status = Core.Enums.ProjectTaskStatus.InProgress },
                new ProjectTask { Id = 3, Status = Core.Enums.ProjectTaskStatus.Todo }
            }
        };

        var dsrRecords = new List<DSR>
        {
            new DSR { Id = 1, ProjectId = projectId, HoursWorked = 8 },
            new DSR { Id = 2, ProjectId = projectId, HoursWorked = 6 }
        };

        var teamMembers = new List<ProjectAssignment>
        {
            new ProjectAssignment { Id = 1, ProjectId = projectId, EmployeeId = 1 },
            new ProjectAssignment { Id = 2, ProjectId = projectId, EmployeeId = 2 }
        };

        var risks = new List<ProjectRisk>();

        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId))
            .ReturnsAsync(project);
        _mockDsrRepository.Setup(r => r.GetProjectDSRsAsync(projectId))
            .ReturnsAsync(dsrRecords);
        _mockAssignmentRepository.Setup(r => r.GetProjectTeamMembersAsync(projectId))
            .ReturnsAsync(teamMembers);
        _mockRiskRepository.Setup(r => r.GetProjectRisksAsync(projectId))
            .ReturnsAsync(risks);
        _mockMapper.Setup(m => m.Map<List<ProjectRiskDto>>(risks))
            .Returns(new List<ProjectRiskDto>());

        // Act
        var result = await _service.GetProjectAnalyticsAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projectId, result.ProjectId);
        Assert.Equal("Test Project", result.ProjectName);
        Assert.NotNull(result.Metrics);
        Assert.Equal(14, result.Metrics.TotalHoursWorked);
        Assert.Equal(100, result.Metrics.EstimatedHours);
        Assert.Equal(3, result.Metrics.TotalTasks);
        Assert.Equal(1, result.Metrics.CompletedTasks);
        Assert.Equal(2, result.Metrics.TeamMembersCount);
    }

    [Fact]
    public async Task CreateProjectAlertAsync_ValidData_CreatesAlert()
    {
        // Arrange
        var projectId = 1;
        var alertType = "Budget Alert";
        var message = "Project is over budget";
        var severity = "High";

        var createdAlert = new ProjectAlert
        {
            Id = 1,
            ProjectId = projectId,
            AlertType = alertType,
            Message = message,
            Severity = severity,
            IsResolved = false,
            CreatedAt = DateTime.UtcNow
        };

        _mockAlertRepository.Setup(r => r.AddAsync(It.IsAny<ProjectAlert>()))
            .Returns(Task.CompletedTask);
        _mockAlertRepository.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _mockMapper.Setup(m => m.Map<ProjectAlertDto>(It.IsAny<ProjectAlert>()))
            .Returns(new ProjectAlertDto
            {
                Id = 1,
                ProjectId = projectId,
                AlertType = alertType,
                Message = message,
                Severity = severity
            });

        // Act
        var result = await _service.CreateProjectAlertAsync(projectId, alertType, message, severity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projectId, result.ProjectId);
        Assert.Equal(alertType, result.AlertType);
        Assert.Equal(message, result.Message);
        Assert.Equal(severity, result.Severity);

        _mockAlertRepository.Verify(r => r.AddAsync(It.IsAny<ProjectAlert>()), Times.Once);
        _mockAlertRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ResolveProjectAlertAsync_ValidAlertId_ResolvesAlert()
    {
        // Arrange
        var alertId = 1;
        var resolvedBy = 123;
        var alert = new ProjectAlert
        {
            Id = alertId,
            ProjectId = 1,
            AlertType = "Test Alert",
            Message = "Test message",
            Severity = "Medium",
            IsResolved = false
        };

        _mockAlertRepository.Setup(r => r.GetByIdAsync(alertId))
            .ReturnsAsync(alert);
        _mockAlertRepository.Setup(r => r.UpdateAsync(It.IsAny<ProjectAlert>()))
            .Returns(Task.CompletedTask);
        _mockAlertRepository.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ResolveProjectAlertAsync(alertId, resolvedBy);

        // Assert
        Assert.True(result);
        Assert.True(alert.IsResolved);
        Assert.Equal(resolvedBy, alert.ResolvedBy);
        Assert.NotNull(alert.ResolvedAt);

        _mockAlertRepository.Verify(r => r.UpdateAsync(alert), Times.Once);
        _mockAlertRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ResolveProjectAlertAsync_InvalidAlertId_ReturnsFalse()
    {
        // Arrange
        var alertId = 999;
        var resolvedBy = 123;

        _mockAlertRepository.Setup(r => r.GetByIdAsync(alertId))
            .ReturnsAsync((ProjectAlert)null);

        // Act
        var result = await _service.ResolveProjectAlertAsync(alertId, resolvedBy);

        // Assert
        Assert.False(result);
        _mockAlertRepository.Verify(r => r.UpdateAsync(It.IsAny<ProjectAlert>()), Times.Never);
        _mockAlertRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateProjectRiskAsync_ValidData_CreatesRisk()
    {
        // Arrange
        var projectId = 1;
        var riskType = "Technical Risk";
        var description = "Technology may become obsolete";
        var severity = "High";
        var probability = 0.3m;
        var impact = 0.8m;

        var createdRisk = new ProjectRisk
        {
            Id = 1,
            ProjectId = projectId,
            RiskType = riskType,
            Description = description,
            Severity = severity,
            Probability = probability,
            Impact = impact,
            Status = "Identified",
            IdentifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _mockRiskRepository.Setup(r => r.AddAsync(It.IsAny<ProjectRisk>()))
            .Returns(Task.CompletedTask);
        _mockRiskRepository.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _mockMapper.Setup(m => m.Map<ProjectRiskDto>(It.IsAny<ProjectRisk>()))
            .Returns(new ProjectRiskDto
            {
                Id = 1,
                ProjectId = projectId,
                RiskType = riskType,
                Description = description,
                Severity = severity,
                Probability = probability,
                Impact = impact
            });

        // Act
        var result = await _service.CreateProjectRiskAsync(projectId, riskType, description, severity, probability, impact);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projectId, result.ProjectId);
        Assert.Equal(riskType, result.RiskType);
        Assert.Equal(description, result.Description);
        Assert.Equal(severity, result.Severity);
        Assert.Equal(probability, result.Probability);
        Assert.Equal(impact, result.Impact);

        _mockRiskRepository.Verify(r => r.AddAsync(It.IsAny<ProjectRisk>()), Times.Once);
        _mockRiskRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CalculateProjectHealthScoreAsync_ValidProject_ReturnsHealthScore()
    {
        // Arrange
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            EstimatedHours = 100,
            Budget = 10000,
            StartDate = DateTime.Today.AddDays(-30),
            EndDate = DateTime.Today.AddDays(30),
            Tasks = new List<ProjectTask>
            {
                new ProjectTask { Id = 1, Status = Core.Enums.ProjectTaskStatus.Completed },
                new ProjectTask { Id = 2, Status = Core.Enums.ProjectTaskStatus.InProgress }
            },
            ProjectAssignments = new List<ProjectAssignment>
            {
                new ProjectAssignment { Id = 1, EmployeeId = 1 },
                new ProjectAssignment { Id = 2, EmployeeId = 2 }
            }
        };

        var dsrRecords = new List<DSR>
        {
            new DSR { Id = 1, ProjectId = projectId, HoursWorked = 40 }
        };

        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId))
            .ReturnsAsync(project);
        _mockDsrRepository.Setup(r => r.GetProjectDSRsAsync(projectId))
            .ReturnsAsync(dsrRecords);

        // Act
        var result = await _service.CalculateProjectHealthScoreAsync(projectId);

        // Assert
        Assert.True(result >= 0);
        Assert.True(result <= 10);
    }

    [Fact]
    public async Task IsProjectAtRiskAsync_HealthyProject_ReturnsFalse()
    {
        // Arrange
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Name = "Healthy Project",
            EstimatedHours = 100,
            Budget = 10000,
            StartDate = DateTime.Today.AddDays(-10),
            EndDate = DateTime.Today.AddDays(20),
            Tasks = new List<ProjectTask>
            {
                new ProjectTask { Id = 1, Status = Core.Enums.ProjectTaskStatus.Completed },
                new ProjectTask { Id = 2, Status = Core.Enums.ProjectTaskStatus.Completed }
            },
            ProjectAssignments = new List<ProjectAssignment>
            {
                new ProjectAssignment { Id = 1, EmployeeId = 1 }
            }
        };

        var dsrRecords = new List<DSR>
        {
            new DSR { Id = 1, ProjectId = projectId, HoursWorked = 30 }
        };

        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId))
            .ReturnsAsync(project);
        _mockDsrRepository.Setup(r => r.GetProjectDSRsAsync(projectId))
            .ReturnsAsync(dsrRecords);

        // Act
        var result = await _service.IsProjectAtRiskAsync(projectId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckProjectHealthAsync_UnhealthyProject_CreatesAlerts()
    {
        // Arrange
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Name = "Unhealthy Project",
            EstimatedHours = 100,
            Budget = 1000,
            StartDate = DateTime.Today.AddDays(-60),
            EndDate = DateTime.Today.AddDays(-10), // Overdue
            Tasks = new List<ProjectTask>
            {
                new ProjectTask 
                { 
                    Id = 1, 
                    Status = Core.Enums.ProjectTaskStatus.Todo,
                    DueDate = DateTime.Today.AddDays(-5) // Overdue task
                }
            }
        };

        var dsrRecords = new List<DSR>
        {
            new DSR { Id = 1, ProjectId = projectId, HoursWorked = 150 } // Over budget
        };

        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId))
            .ReturnsAsync(project);
        _mockDsrRepository.Setup(r => r.GetProjectDSRsAsync(projectId))
            .ReturnsAsync(dsrRecords);
        _mockAlertRepository.Setup(r => r.AddAsync(It.IsAny<ProjectAlert>()))
            .Returns(Task.CompletedTask);
        _mockAlertRepository.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _service.CheckProjectHealthAsync(projectId);

        // Assert
        _mockAlertRepository.Verify(r => r.AddAsync(It.IsAny<ProjectAlert>()), Times.AtLeastOnce);
        _mockAlertRepository.Verify(r => r.SaveChangesAsync(), Times.AtLeastOnce);
    }
}
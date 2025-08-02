using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Project;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class ProjectMonitoringServiceTests
{
    private readonly Mock<IProjectRepository> _mockProjectRepository;
    private readonly Mock<IProjectAlertRepository> _mockAlertRepository;
    private readonly Mock<IProjectAssignmentRepository> _mockAssignmentRepository;
    private readonly Mock<IProjectTaskRepository> _mockTaskRepository;
    private readonly Mock<IDSRRepository> _mockDsrRepository;
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ProjectMonitoringService>> _mockLogger;
    private readonly ProjectMonitoringService _service;

    public ProjectMonitoringServiceTests()
    {
        _mockProjectRepository = new Mock<IProjectRepository>();
        _mockAlertRepository = new Mock<IProjectAlertRepository>();
        _mockAssignmentRepository = new Mock<IProjectAssignmentRepository>();
        _mockTaskRepository = new Mock<IProjectTaskRepository>();
        _mockDsrRepository = new Mock<IDSRRepository>();
        _mockProjectService = new Mock<IProjectService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ProjectMonitoringService>>();

        _service = new ProjectMonitoringService(
            _mockProjectRepository.Object,
            _mockAlertRepository.Object,
            _mockAssignmentRepository.Object,
            _mockTaskRepository.Object,
            _mockDsrRepository.Object,
            _mockProjectService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetProjectMonitoringDataAsync_ValidProjectId_ReturnsMonitoringData()
    {
        // Arrange
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            EstimatedHours = 100,
            Status = ProjectStatus.Active
        };

        var progress = new ProjectProgressDto
        {
            ProjectId = projectId,
            TotalEstimatedHours = 100,
            ActualHoursWorked = 80,
            CompletionPercentage = 80,
            IsOnTrack = true
        };

        var variance = new ProjectVarianceDto
        {
            HoursVariance = -20,
            IsOverBudget = false,
            IsBehindSchedule = false
        };

        var alerts = new List<ProjectAlertDto>();
        var teamMembers = new List<ProjectTeamMemberDto>();

        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId))
            .ReturnsAsync(project);

        _mockProjectService.Setup(s => s.GetProjectProgressAsync(projectId))
            .ReturnsAsync(progress);

        _mockProjectService.Setup(s => s.GetProjectTeamMembersAsync(projectId))
            .ReturnsAsync(teamMembers);

        _mockAlertRepository.Setup(r => r.GetAlertsByProjectAsync(projectId))
            .ReturnsAsync(new List<ProjectAlert>());

        _mockMapper.Setup(m => m.Map<List<ProjectAlertDto>>(It.IsAny<IEnumerable<ProjectAlert>>()))
            .Returns(alerts);

        _mockDsrRepository.Setup(r => r.GetTotalHoursByProjectAsync(projectId, null, null))
            .ReturnsAsync(80);

        // Act
        var result = await _service.GetProjectMonitoringDataAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projectId, result.ProjectId);
        Assert.Equal("Test Project", result.ProjectName);
        Assert.Equal(progress, result.Progress);
        Assert.Equal(alerts, result.Alerts);
        Assert.Equal(teamMembers, result.TeamMembers);
    }

    [Fact]
    public async Task GetProjectMonitoringDataAsync_InvalidProjectId_ThrowsArgumentException()
    {
        // Arrange
        var projectId = 999;
        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId))
            .ReturnsAsync((Project?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.GetProjectMonitoringDataAsync(projectId));
    }

    [Fact]
    public async Task CalculateProjectVarianceAsync_ValidProject_ReturnsVariance()
    {
        // Arrange
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            EstimatedHours = 100,
            Budget = 5000,
            EndDate = DateTime.Today.AddDays(5)
        };

        var progress = new ProjectProgressDto
        {
            ProjectId = projectId,
            CompletionPercentage = 75
        };

        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId))
            .ReturnsAsync(project);

        _mockDsrRepository.Setup(r => r.GetTotalHoursByProjectAsync(projectId, null, null))
            .ReturnsAsync(120);

        _mockProjectService.Setup(s => s.GetProjectProgressAsync(projectId))
            .ReturnsAsync(progress);

        // Act
        var result = await _service.CalculateProjectVarianceAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20, result.HoursVariance); // 120 - 100
        Assert.True(result.IsOverBudget);
        Assert.False(result.IsBehindSchedule); // End date is in future
        Assert.Equal(1.2m, result.PerformanceIndex); // 120 / 100
    }

    [Fact]
    public async Task GetTeamLeaderDashboardAsync_ValidTeamLeader_ReturnsDashboard()
    {
        // Arrange
        var teamLeaderId = 1;
        var project1Progress = new ProjectProgressDto { TotalEstimatedHours = 100, ActualHoursWorked = 80, IsOnTrack = true };
        var project2Progress = new ProjectProgressDto { TotalEstimatedHours = 150, ActualHoursWorked = 180, IsOnTrack = false };
        
        var project1Variance = new ProjectVarianceDto { IsOverBudget = false, IsBehindSchedule = false };
        var project2Variance = new ProjectVarianceDto { IsOverBudget = true, IsBehindSchedule = true };

        var employee = new Employee
        {
            Id = teamLeaderId,
            FirstName = "John",
            LastName = "Doe"
        };

        _mockProjectRepository.Setup(r => r.GetProjectsByTeamLeadAsync(teamLeaderId))
            .ReturnsAsync(new List<Project>
            {
                new Project { Id = 1, Name = "Project 1" },
                new Project { Id = 2, Name = "Project 2" }
            });

        _mockProjectRepository.Setup(r => r.GetEmployeeAsync(teamLeaderId))
            .ReturnsAsync(employee);

        _mockAlertRepository.Setup(r => r.GetAlertsByTeamLeadAsync(teamLeaderId))
            .ReturnsAsync(new List<ProjectAlert>());

        _mockMapper.Setup(m => m.Map<List<ProjectAlertDto>>(It.IsAny<IEnumerable<ProjectAlert>>()))
            .Returns(new List<ProjectAlertDto>());

        // Mock the GetProjectMonitoringDataAsync calls for Project 1
        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(1))
            .ReturnsAsync(new Project { Id = 1, Name = "Project 1", EstimatedHours = 100, EndDate = DateTime.Today.AddDays(5) });

        _mockProjectService.Setup(s => s.GetProjectProgressAsync(1))
            .ReturnsAsync(project1Progress);

        _mockProjectService.Setup(s => s.GetProjectTeamMembersAsync(1))
            .ReturnsAsync(new List<ProjectTeamMemberDto>());

        _mockAlertRepository.Setup(r => r.GetAlertsByProjectAsync(1))
            .ReturnsAsync(new List<ProjectAlert>());

        _mockDsrRepository.Setup(r => r.GetTotalHoursByProjectAsync(1, null, null))
            .ReturnsAsync(80);

        // Mock the GetProjectMonitoringDataAsync calls for Project 2
        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(2))
            .ReturnsAsync(new Project { Id = 2, Name = "Project 2", EstimatedHours = 150, EndDate = DateTime.Today.AddDays(-2) });

        _mockProjectService.Setup(s => s.GetProjectProgressAsync(2))
            .ReturnsAsync(project2Progress);

        _mockProjectService.Setup(s => s.GetProjectTeamMembersAsync(2))
            .ReturnsAsync(new List<ProjectTeamMemberDto>());

        _mockAlertRepository.Setup(r => r.GetAlertsByProjectAsync(2))
            .ReturnsAsync(new List<ProjectAlert>());

        _mockDsrRepository.Setup(r => r.GetTotalHoursByProjectAsync(2, null, null))
            .ReturnsAsync(180);

        // Act
        var result = await _service.GetTeamLeaderDashboardAsync(teamLeaderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(teamLeaderId, result.TeamLeaderId);
        Assert.Equal("John Doe", result.TeamLeaderName);
        Assert.Equal(2, result.Summary.TotalProjects);
        Assert.Equal(1, result.Summary.OnTrackProjects);
        Assert.Equal(1, result.Summary.DelayedProjects);
        Assert.Equal(1, result.Summary.OverBudgetProjects);
        Assert.Equal(250, result.Summary.TotalEstimatedHours); // 100 + 150
        Assert.Equal(260, result.Summary.TotalActualHours); // 80 + 180
    }

    [Fact]
    public async Task CreateProjectAlertAsync_ValidData_CreatesAlert()
    {
        // Arrange
        var projectId = 1;
        var alertType = ProjectAlertType.ScheduleDelay;
        var message = "Project is behind schedule";
        var severity = AlertSeverity.High;

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

        var alertDto = new ProjectAlertDto
        {
            Id = 1,
            ProjectId = projectId,
            AlertType = alertType.ToString(),
            Message = message,
            Severity = severity.ToString(),
            IsResolved = false,
            CreatedAt = DateTime.UtcNow
        };

        _mockAlertRepository.Setup(r => r.AddAsync(It.IsAny<ProjectAlert>()))
            .ReturnsAsync(createdAlert);

        _mockAlertRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockMapper.Setup(m => m.Map<ProjectAlertDto>(It.IsAny<ProjectAlert>()))
            .Returns(alertDto);

        // Act
        var result = await _service.CreateProjectAlertAsync(projectId, alertType, message, severity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projectId, result.ProjectId);
        Assert.Equal(alertType.ToString(), result.AlertType);
        Assert.Equal(message, result.Message);
        Assert.Equal(severity.ToString(), result.Severity);
        Assert.False(result.IsResolved);

        _mockAlertRepository.Verify(r => r.AddAsync(It.IsAny<ProjectAlert>()), Times.Once);
        _mockAlertRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ResolveProjectAlertAsync_ValidAlert_ResolvesAlert()
    {
        // Arrange
        var alertId = 1;
        var resolvedByEmployeeId = 2;
        var resolutionNotes = "Issue resolved";

        _mockAlertRepository.Setup(r => r.ResolveAlertAsync(alertId, resolvedByEmployeeId, resolutionNotes))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ResolveProjectAlertAsync(alertId, resolvedByEmployeeId, resolutionNotes);

        // Assert
        Assert.True(result);
        _mockAlertRepository.Verify(r => r.ResolveAlertAsync(alertId, resolvedByEmployeeId, resolutionNotes), Times.Once);
    }

    [Fact]
    public async Task CheckAndCreateAutomatedAlertsAsync_ProjectBehindSchedule_CreatesScheduleAlert()
    {
        // Arrange
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            EstimatedHours = 100,
            EndDate = DateTime.Today.AddDays(-5) // 5 days overdue
        };

        var progress = new ProjectProgressDto
        {
            ProjectId = projectId,
            CompletionPercentage = 70,
            IsOnTrack = false
        };

        var variance = new ProjectVarianceDto
        {
            IsBehindSchedule = true,
            ScheduleVarianceDays = 5, // 5 days behind (> 3)
            IsOverBudget = false,
            HoursVariance = -20
        };

        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId))
            .ReturnsAsync(project);

        _mockProjectService.Setup(s => s.GetProjectProgressAsync(projectId))
            .ReturnsAsync(progress);

        _mockDsrRepository.Setup(r => r.GetTotalHoursByProjectAsync(projectId, null, null))
            .ReturnsAsync(80);

        _mockTaskRepository.Setup(r => r.GetTasksByProjectAsync(projectId))
            .ReturnsAsync(new List<ProjectTask>());

        // Mock the CreateProjectAlertAsync method to capture the call
        _mockAlertRepository.Setup(r => r.AddAsync(It.IsAny<ProjectAlert>()))
            .ReturnsAsync(It.IsAny<ProjectAlert>());

        _mockAlertRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockMapper.Setup(m => m.Map<ProjectAlertDto>(It.IsAny<ProjectAlert>()))
            .Returns(new ProjectAlertDto());

        // We need to create a partial mock or use a different approach
        // Since CalculateProjectVarianceAsync is called internally, we need to mock its dependencies
        // The variance calculation depends on the project end date and completion percentage
        
        // Act
        await _service.CheckAndCreateAutomatedAlertsAsync(projectId);

        // Assert - The alert should be created because:
        // - Project end date is 5 days ago (behind schedule)
        // - Completion percentage is 70% (< 100%)
        // - Schedule variance days (5) > 3
        // - Since variance days (5) < 7, severity should be Medium, not High
        _mockAlertRepository.Verify(r => r.AddAsync(It.Is<ProjectAlert>(a => 
            a.ProjectId == projectId && 
            a.AlertType == ProjectAlertType.ScheduleDelay &&
            a.Severity == AlertSeverity.Medium)), Times.Once);
    }

    [Fact]
    public async Task CheckAndCreateAutomatedAlertsAsync_ProjectOverBudget_CreatesBudgetAlert()
    {
        // Arrange
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            EstimatedHours = 100,
            EndDate = DateTime.Today.AddDays(5)
        };

        var progress = new ProjectProgressDto
        {
            ProjectId = projectId,
            CompletionPercentage = 80,
            IsOnTrack = false
        };

        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId))
            .ReturnsAsync(project);

        _mockProjectService.Setup(s => s.GetProjectProgressAsync(projectId))
            .ReturnsAsync(progress);

        _mockDsrRepository.Setup(r => r.GetTotalHoursByProjectAsync(projectId, null, null))
            .ReturnsAsync(130); // 30% over budget (30 hours over 100 estimated)

        _mockTaskRepository.Setup(r => r.GetTasksByProjectAsync(projectId))
            .ReturnsAsync(new List<ProjectTask>());

        _mockAlertRepository.Setup(r => r.AddAsync(It.IsAny<ProjectAlert>()))
            .ReturnsAsync(It.IsAny<ProjectAlert>());

        _mockAlertRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockMapper.Setup(m => m.Map<ProjectAlertDto>(It.IsAny<ProjectAlert>()))
            .Returns(new ProjectAlertDto());

        // Act
        await _service.CheckAndCreateAutomatedAlertsAsync(projectId);

        // Assert - The alert should be created because:
        // - Actual hours (130) > Estimated hours (100) -> IsOverBudget = true
        // - Hours variance (30) > 10% of estimated hours (10) -> meets threshold
        // - Since hours variance (30) > 20% of estimated hours (20), severity should be High
        _mockAlertRepository.Verify(r => r.AddAsync(It.Is<ProjectAlert>(a => 
            a.ProjectId == projectId && 
            a.AlertType == ProjectAlertType.BudgetOverrun &&
            a.Severity == AlertSeverity.High)), Times.Once);
    }

    [Fact]
    public async Task CalculateProjectEfficiencyAsync_ValidProject_ReturnsEfficiency()
    {
        // Arrange
        var projectId = 1;
        var progress = new ProjectProgressDto
        {
            ProjectId = projectId,
            TotalEstimatedHours = 100,
            ActualHoursWorked = 80
        };

        _mockProjectService.Setup(s => s.GetProjectProgressAsync(projectId))
            .ReturnsAsync(progress);

        // Act
        var result = await _service.CalculateProjectEfficiencyAsync(projectId);

        // Assert
        Assert.Equal(125m, result); // (100 / 80) * 100 = 125%
    }

    [Fact]
    public async Task CalculateProjectEfficiencyAsync_NoActualHours_ReturnsZero()
    {
        // Arrange
        var projectId = 1;
        var progress = new ProjectProgressDto
        {
            ProjectId = projectId,
            TotalEstimatedHours = 100,
            ActualHoursWorked = 0
        };

        _mockProjectService.Setup(s => s.GetProjectProgressAsync(projectId))
            .ReturnsAsync(progress);

        // Act
        var result = await _service.CalculateProjectEfficiencyAsync(projectId);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task IsProjectAtRiskAsync_ProjectOverBudgetAndBehindSchedule_ReturnsTrue()
    {
        // Arrange
        var projectId = 1;
        var variance = new ProjectVarianceDto
        {
            IsOverBudget = true,
            IsBehindSchedule = true
        };

        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId))
            .ReturnsAsync(new Project { Id = projectId, EstimatedHours = 100, EndDate = DateTime.Today.AddDays(-1) });

        _mockDsrRepository.Setup(r => r.GetTotalHoursByProjectAsync(projectId, null, null))
            .ReturnsAsync(120);

        _mockProjectService.Setup(s => s.GetProjectProgressAsync(projectId))
            .ReturnsAsync(new ProjectProgressDto { CompletionPercentage = 80 });

        _mockAlertRepository.Setup(r => r.GetUnresolvedAlertCountAsync(projectId))
            .ReturnsAsync(1);

        // Act
        var result = await _service.IsProjectAtRiskAsync(projectId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsProjectAtRiskAsync_ProjectOnTrack_ReturnsFalse()
    {
        // Arrange
        var projectId = 1;

        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId))
            .ReturnsAsync(new Project { Id = projectId, EstimatedHours = 100, EndDate = DateTime.Today.AddDays(5) });

        _mockDsrRepository.Setup(r => r.GetTotalHoursByProjectAsync(projectId, null, null))
            .ReturnsAsync(80);

        _mockProjectService.Setup(s => s.GetProjectProgressAsync(projectId))
            .ReturnsAsync(new ProjectProgressDto { CompletionPercentage = 80 });

        _mockAlertRepository.Setup(r => r.GetUnresolvedAlertCountAsync(projectId))
            .ReturnsAsync(0);

        // Act
        var result = await _service.IsProjectAtRiskAsync(projectId);

        // Assert
        Assert.False(result);
    }
}
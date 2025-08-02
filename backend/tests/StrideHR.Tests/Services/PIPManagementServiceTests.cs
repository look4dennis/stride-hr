using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class PIPManagementServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<PIPManagementService>> _mockLogger;
    private readonly Mock<IPerformanceImprovementPlanRepository> _mockPIPRepository;
    private readonly Mock<IRepository<PIPGoal>> _mockPIPGoalRepository;
    private readonly Mock<IRepository<PIPReview>> _mockPIPReviewRepository;
    private readonly PIPManagementService _service;

    public PIPManagementServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<PIPManagementService>>();
        _mockPIPRepository = new Mock<IPerformanceImprovementPlanRepository>();
        _mockPIPGoalRepository = new Mock<IRepository<PIPGoal>>();
        _mockPIPReviewRepository = new Mock<IRepository<PIPReview>>();

        _mockUnitOfWork.Setup(u => u.PerformanceImprovementPlans).Returns(_mockPIPRepository.Object);
        _mockUnitOfWork.Setup(u => u.PIPGoals).Returns(_mockPIPGoalRepository.Object);
        _mockUnitOfWork.Setup(u => u.PIPReviews).Returns(_mockPIPReviewRepository.Object);

        _service = new PIPManagementService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    #region PIP Management Tests

    [Fact]
    public async Task CreatePIPAsync_ValidPIP_ReturnsCreatedPIP()
    {
        // Arrange
        var pip = new PerformanceImprovementPlan
        {
            EmployeeId = 1,
            ManagerId = 2,
            Title = "Performance Improvement Plan",
            Description = "Plan to improve performance",
            PerformanceIssues = "Issues identified",
            ExpectedImprovements = "Expected improvements",
            SupportProvided = "Support provided",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(90),
            ReviewFrequencyDays = 30
        };

        _mockPIPRepository.Setup(r => r.AddAsync(It.IsAny<PerformanceImprovementPlan>()))
            .ReturnsAsync(pip);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.CreatePIPAsync(pip);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PIPStatus.Draft, result.Status);
        Assert.Equal("Performance Improvement Plan", result.Title);
        _mockPIPRepository.Verify(r => r.AddAsync(It.IsAny<PerformanceImprovementPlan>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPIPByIdAsync_ExistingPIP_ReturnsPIP()
    {
        // Arrange
        var pipId = 1;
        var expectedPIP = new PerformanceImprovementPlan
        {
            Id = pipId,
            Title = "Test PIP",
            EmployeeId = 1
        };

        _mockPIPRepository.Setup(r => r.GetByIdAsync(pipId, It.IsAny<System.Linq.Expressions.Expression<Func<PerformanceImprovementPlan, object>>[]>()))
            .ReturnsAsync(expectedPIP);

        // Act
        var result = await _service.GetPIPByIdAsync(pipId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pipId, result.Id);
        Assert.Equal("Test PIP", result.Title);
    }

    [Fact]
    public async Task StartPIPAsync_ValidPIP_UpdatesStatusToActive()
    {
        // Arrange
        var pipId = 1;
        var pip = new PerformanceImprovementPlan
        {
            Id = pipId,
            Status = PIPStatus.Draft
        };

        _mockPIPRepository.Setup(r => r.GetByIdAsync(pipId))
            .ReturnsAsync(pip);
        _mockPIPRepository.Setup(r => r.UpdateAsync(It.IsAny<PerformanceImprovementPlan>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.StartPIPAsync(pipId);

        // Assert
        Assert.True(result);
        Assert.Equal(PIPStatus.Active, pip.Status);
        _mockPIPRepository.Verify(r => r.UpdateAsync(It.IsAny<PerformanceImprovementPlan>()), Times.Once);
    }

    [Fact]
    public async Task CompletePIPAsync_SuccessfulCompletion_UpdatesStatusAndOutcome()
    {
        // Arrange
        var pipId = 1;
        var isSuccessful = true;
        var finalOutcome = "Employee has successfully improved performance";
        var pip = new PerformanceImprovementPlan
        {
            Id = pipId,
            Status = PIPStatus.InProgress
        };

        _mockPIPRepository.Setup(r => r.GetByIdAsync(pipId))
            .ReturnsAsync(pip);
        _mockPIPRepository.Setup(r => r.UpdateAsync(It.IsAny<PerformanceImprovementPlan>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.CompletePIPAsync(pipId, isSuccessful, finalOutcome);

        // Assert
        Assert.True(result);
        Assert.Equal(PIPStatus.Successful, pip.Status);
        Assert.True(pip.IsSuccessful);
        Assert.Equal(finalOutcome, pip.FinalOutcome);
        Assert.NotNull(pip.CompletedDate);
    }

    [Fact]
    public async Task CompletePIPAsync_FailedCompletion_UpdatesStatusToFailed()
    {
        // Arrange
        var pipId = 1;
        var isSuccessful = false;
        var finalOutcome = "Employee did not meet improvement goals";
        var pip = new PerformanceImprovementPlan
        {
            Id = pipId,
            Status = PIPStatus.InProgress
        };

        _mockPIPRepository.Setup(r => r.GetByIdAsync(pipId))
            .ReturnsAsync(pip);
        _mockPIPRepository.Setup(r => r.UpdateAsync(It.IsAny<PerformanceImprovementPlan>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.CompletePIPAsync(pipId, isSuccessful, finalOutcome);

        // Assert
        Assert.True(result);
        Assert.Equal(PIPStatus.Failed, pip.Status);
        Assert.False(pip.IsSuccessful);
        Assert.Equal(finalOutcome, pip.FinalOutcome);
    }

    #endregion

    #region PIP Goals Tests

    [Fact]
    public async Task AddPIPGoalAsync_ValidGoal_ReturnsCreatedGoal()
    {
        // Arrange
        var goal = new PIPGoal
        {
            PIPId = 1,
            Title = "Improve Communication",
            Description = "Improve communication with team members",
            MeasurableObjective = "Attend weekly team meetings and provide updates",
            TargetDate = DateTime.UtcNow.AddDays(30)
        };

        _mockPIPGoalRepository.Setup(r => r.AddAsync(It.IsAny<PIPGoal>()))
            .ReturnsAsync(goal);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.AddPIPGoalAsync(goal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PerformanceGoalStatus.Active, result.Status);
        Assert.Equal("Improve Communication", result.Title);
        _mockPIPGoalRepository.Verify(r => r.AddAsync(It.IsAny<PIPGoal>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePIPGoalProgressAsync_ValidGoal_UpdatesProgress()
    {
        // Arrange
        var goalId = 1;
        var progressPercentage = 75m;
        var comments = "Making good progress";
        var goal = new PIPGoal
        {
            Id = goalId,
            ProgressPercentage = 50m,
            Status = PerformanceGoalStatus.InProgress
        };

        _mockPIPGoalRepository.Setup(r => r.GetByIdAsync(goalId))
            .ReturnsAsync(goal);
        _mockPIPGoalRepository.Setup(r => r.UpdateAsync(It.IsAny<PIPGoal>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.UpdatePIPGoalProgressAsync(goalId, progressPercentage, comments);

        // Assert
        Assert.True(result);
        Assert.Equal(progressPercentage, goal.ProgressPercentage);
        Assert.Equal(comments, goal.EmployeeComments);
        Assert.Equal(PerformanceGoalStatus.InProgress, goal.Status);
    }

    [Fact]
    public async Task UpdatePIPGoalProgressAsync_100PercentProgress_MarksAsCompleted()
    {
        // Arrange
        var goalId = 1;
        var progressPercentage = 100m;
        var goal = new PIPGoal
        {
            Id = goalId,
            ProgressPercentage = 75m,
            Status = PerformanceGoalStatus.InProgress
        };

        _mockPIPGoalRepository.Setup(r => r.GetByIdAsync(goalId))
            .ReturnsAsync(goal);
        _mockPIPGoalRepository.Setup(r => r.UpdateAsync(It.IsAny<PIPGoal>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.UpdatePIPGoalProgressAsync(goalId, progressPercentage);

        // Assert
        Assert.True(result);
        Assert.Equal(progressPercentage, goal.ProgressPercentage);
        Assert.Equal(PerformanceGoalStatus.Completed, goal.Status);
        Assert.True(goal.IsAchieved);
        Assert.NotNull(goal.CompletedDate);
    }

    #endregion

    #region PIP Reviews Tests

    [Fact]
    public async Task AddPIPReviewAsync_ValidReview_ReturnsCreatedReview()
    {
        // Arrange
        var review = new PIPReview
        {
            PIPId = 1,
            ReviewedBy = 2,
            ProgressSummary = "Employee is making steady progress",
            EmployeeFeedback = "I feel supported and am working hard to improve",
            ManagerFeedback = "Good effort shown, continue current approach",
            OverallProgress = PerformanceRating.MeetsExpectations,
            IsOnTrack = true
        };

        _mockPIPReviewRepository.Setup(r => r.AddAsync(It.IsAny<PIPReview>()))
            .ReturnsAsync(review);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.AddPIPReviewAsync(review);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Employee is making steady progress", result.ProgressSummary);
        Assert.True(result.IsOnTrack);
        _mockPIPReviewRepository.Verify(r => r.AddAsync(It.IsAny<PIPReview>()), Times.Once);
    }

    #endregion

    #region PIP Analytics Tests

    [Fact]
    public async Task GetPIPSuccessRateAsync_WithCompletedPIPs_ReturnsCorrectRate()
    {
        // Arrange
        var pips = new List<PerformanceImprovementPlan>
        {
            new() { Status = PIPStatus.Successful },
            new() { Status = PIPStatus.Failed },
            new() { Status = PIPStatus.Successful },
            new() { Status = PIPStatus.Successful },
            new() { Status = PIPStatus.Active } // Not completed, should be excluded
        };

        _mockPIPRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(pips);

        // Act
        var result = await _service.GetPIPSuccessRateAsync();

        // Assert
        Assert.Equal(75m, result); // 3 successful out of 4 completed = 75%
    }

    [Fact]
    public async Task GetPIPSuccessRateAsync_NoCompletedPIPs_ReturnsZero()
    {
        // Arrange
        var pips = new List<PerformanceImprovementPlan>
        {
            new() { Status = PIPStatus.Active },
            new() { Status = PIPStatus.Draft }
        };

        _mockPIPRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(pips);

        // Act
        var result = await _service.GetPIPSuccessRateAsync();

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task GetPIPStatusDistributionAsync_WithVariousStatuses_ReturnsCorrectDistribution()
    {
        // Arrange
        var pips = new List<PerformanceImprovementPlan>
        {
            new() { Status = PIPStatus.Active },
            new() { Status = PIPStatus.Active },
            new() { Status = PIPStatus.Successful },
            new() { Status = PIPStatus.Failed },
            new() { Status = PIPStatus.Draft }
        };

        _mockPIPRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(pips);

        // Act
        var result = await _service.GetPIPStatusDistributionAsync();

        // Assert
        Assert.Equal(2, result[PIPStatus.Active.ToString()]);
        Assert.Equal(1, result[PIPStatus.Successful.ToString()]);
        Assert.Equal(1, result[PIPStatus.Failed.ToString()]);
        Assert.Equal(1, result[PIPStatus.Draft.ToString()]);
        Assert.Equal(0, result[PIPStatus.InProgress.ToString()]);
    }

    [Fact]
    public async Task CreatePIPFromTemplateAsync_ValidTemplate_ReturnsCreatedPIP()
    {
        // Arrange
        var employeeId = 1;
        var managerId = 2;
        var templateName = "Standard PIP";

        _mockPIPRepository.Setup(r => r.AddAsync(It.IsAny<PerformanceImprovementPlan>()))
            .ReturnsAsync((PerformanceImprovementPlan pip) => pip);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.CreatePIPFromTemplateAsync(employeeId, managerId, templateName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(managerId, result.ManagerId);
        Assert.Contains(templateName, result.Title);
        Assert.Equal(PIPStatus.Draft, result.Status);
        Assert.Equal(90, (result.EndDate - result.StartDate).Days); // 90-day PIP
        _mockPIPRepository.Verify(r => r.AddAsync(It.IsAny<PerformanceImprovementPlan>()), Times.Once);
    }

    [Fact]
    public async Task SendPIPNotificationAsync_ValidPIP_ReturnsTrue()
    {
        // Arrange
        var pipId = 1;
        var notificationType = "PIP_STARTED";
        var pip = new PerformanceImprovementPlan
        {
            Id = pipId,
            EmployeeId = 1,
            Title = "Test PIP"
        };

        _mockPIPRepository.Setup(r => r.GetByIdAsync(pipId, It.IsAny<System.Linq.Expressions.Expression<Func<PerformanceImprovementPlan, object>>[]>()))
            .ReturnsAsync(pip);

        // Act
        var result = await _service.SendPIPNotificationAsync(pipId, notificationType);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SendPIPNotificationAsync_NonExistentPIP_ReturnsFalse()
    {
        // Arrange
        var pipId = 999;
        var notificationType = "PIP_STARTED";

        _mockPIPRepository.Setup(r => r.GetByIdAsync(pipId, It.IsAny<System.Linq.Expressions.Expression<Func<PerformanceImprovementPlan, object>>[]>()))
            .ReturnsAsync((PerformanceImprovementPlan?)null);

        // Act
        var result = await _service.SendPIPNotificationAsync(pipId, notificationType);

        // Assert
        Assert.False(result);
    }

    #endregion
}
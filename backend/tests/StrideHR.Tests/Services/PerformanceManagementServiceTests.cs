using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class PerformanceManagementServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<PerformanceManagementService>> _mockLogger;
    private readonly Mock<IPerformanceGoalRepository> _mockGoalRepository;
    private readonly Mock<IRepository<PerformanceGoalCheckIn>> _mockCheckInRepository;
    private readonly Mock<IPerformanceReviewRepository> _mockReviewRepository;
    private readonly Mock<IPerformanceFeedbackRepository> _mockFeedbackRepository;
    private readonly PerformanceManagementService _service;

    public PerformanceManagementServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<PerformanceManagementService>>();
        _mockGoalRepository = new Mock<IPerformanceGoalRepository>();
        _mockCheckInRepository = new Mock<IRepository<PerformanceGoalCheckIn>>();
        _mockReviewRepository = new Mock<IPerformanceReviewRepository>();
        _mockFeedbackRepository = new Mock<IPerformanceFeedbackRepository>();

        _mockUnitOfWork.Setup(u => u.PerformanceGoals).Returns(_mockGoalRepository.Object);
        _mockUnitOfWork.Setup(u => u.PerformanceGoalCheckIns).Returns(_mockCheckInRepository.Object);
        _mockUnitOfWork.Setup(u => u.PerformanceReviews).Returns(_mockReviewRepository.Object);
        _mockUnitOfWork.Setup(u => u.PerformanceFeedbacks).Returns(_mockFeedbackRepository.Object);

        _service = new PerformanceManagementService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    #region Performance Goals Tests

    [Fact]
    public async Task CreateGoalAsync_ValidGoal_ReturnsCreatedGoal()
    {
        // Arrange
        var goal = new PerformanceGoal
        {
            EmployeeId = 1,
            Title = "Test Goal",
            Description = "Test Description",
            SuccessCriteria = "Test Criteria",
            StartDate = DateTime.UtcNow,
            TargetDate = DateTime.UtcNow.AddDays(30),
            WeightPercentage = 25
        };

        _mockGoalRepository.Setup(r => r.AddAsync(It.IsAny<PerformanceGoal>()))
            .ReturnsAsync(goal);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.CreateGoalAsync(goal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PerformanceGoalStatus.Active, result.Status);
        Assert.Equal("Test Goal", result.Title);
        _mockGoalRepository.Verify(r => r.AddAsync(It.IsAny<PerformanceGoal>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetGoalByIdAsync_ExistingGoal_ReturnsGoal()
    {
        // Arrange
        var goalId = 1;
        var expectedGoal = new PerformanceGoal
        {
            Id = goalId,
            Title = "Test Goal",
            EmployeeId = 1
        };

        _mockGoalRepository.Setup(r => r.GetByIdAsync(goalId, It.IsAny<System.Linq.Expressions.Expression<Func<PerformanceGoal, object>>[]>()))
            .ReturnsAsync(expectedGoal);

        // Act
        var result = await _service.GetGoalByIdAsync(goalId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(goalId, result.Id);
        Assert.Equal("Test Goal", result.Title);
    }

    [Fact]
    public async Task GetGoalByIdAsync_NonExistingGoal_ReturnsNull()
    {
        // Arrange
        var goalId = 999;
        _mockGoalRepository.Setup(r => r.GetByIdAsync(goalId, It.IsAny<System.Linq.Expressions.Expression<Func<PerformanceGoal, object>>[]>()))
            .ReturnsAsync((PerformanceGoal?)null);

        // Act
        var result = await _service.GetGoalByIdAsync(goalId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateGoalProgressAsync_ValidGoal_UpdatesProgress()
    {
        // Arrange
        var goalId = 1;
        var progressPercentage = 75m;
        var notes = "Good progress";
        var goal = new PerformanceGoal
        {
            Id = goalId,
            ProgressPercentage = 50m,
            Status = PerformanceGoalStatus.InProgress
        };

        _mockGoalRepository.Setup(r => r.GetByIdAsync(goalId))
            .ReturnsAsync(goal);
        _mockGoalRepository.Setup(r => r.UpdateAsync(It.IsAny<PerformanceGoal>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.UpdateGoalProgressAsync(goalId, progressPercentage, notes);

        // Assert
        Assert.True(result);
        Assert.Equal(progressPercentage, goal.ProgressPercentage);
        Assert.Equal(notes, goal.Notes);
        Assert.Equal(PerformanceGoalStatus.InProgress, goal.Status);
        _mockGoalRepository.Verify(r => r.UpdateAsync(It.IsAny<PerformanceGoal>()), Times.Once);
    }

    [Fact]
    public async Task UpdateGoalProgressAsync_100PercentProgress_MarksAsCompleted()
    {
        // Arrange
        var goalId = 1;
        var progressPercentage = 100m;
        var goal = new PerformanceGoal
        {
            Id = goalId,
            ProgressPercentage = 75m,
            Status = PerformanceGoalStatus.InProgress
        };

        _mockGoalRepository.Setup(r => r.GetByIdAsync(goalId))
            .ReturnsAsync(goal);
        _mockGoalRepository.Setup(r => r.UpdateAsync(It.IsAny<PerformanceGoal>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.UpdateGoalProgressAsync(goalId, progressPercentage);

        // Assert
        Assert.True(result);
        Assert.Equal(progressPercentage, goal.ProgressPercentage);
        Assert.Equal(PerformanceGoalStatus.Completed, goal.Status);
        Assert.NotNull(goal.CompletedDate);
    }

    [Fact]
    public async Task AddGoalCheckInAsync_ValidCheckIn_AddsCheckInAndUpdatesGoal()
    {
        // Arrange
        var checkIn = new PerformanceGoalCheckIn
        {
            PerformanceGoalId = 1,
            EmployeeId = 1,
            ProgressPercentage = 80m,
            EmployeeComments = "Making good progress"
        };

        var goal = new PerformanceGoal
        {
            Id = 1,
            ProgressPercentage = 60m,
            Status = PerformanceGoalStatus.InProgress
        };

        _mockCheckInRepository.Setup(r => r.AddAsync(It.IsAny<PerformanceGoalCheckIn>()))
            .ReturnsAsync(checkIn);
        _mockGoalRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(goal);
        _mockGoalRepository.Setup(r => r.UpdateAsync(It.IsAny<PerformanceGoal>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.AddGoalCheckInAsync(checkIn);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(80m, goal.ProgressPercentage);
        _mockCheckInRepository.Verify(r => r.AddAsync(It.IsAny<PerformanceGoalCheckIn>()), Times.Once);
        _mockGoalRepository.Verify(r => r.UpdateAsync(It.IsAny<PerformanceGoal>()), Times.Once);
    }

    #endregion

    #region Performance Reviews Tests

    [Fact]
    public async Task CreateReviewAsync_ValidReview_ReturnsCreatedReview()
    {
        // Arrange
        var review = new PerformanceReview
        {
            EmployeeId = 1,
            ReviewPeriod = "Q1 2025",
            ReviewStartDate = DateTime.UtcNow.AddDays(-90),
            ReviewEndDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        _mockReviewRepository.Setup(r => r.AddAsync(It.IsAny<PerformanceReview>()))
            .ReturnsAsync(review);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.CreateReviewAsync(review);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PerformanceReviewStatus.NotStarted, result.Status);
        Assert.Equal("Q1 2025", result.ReviewPeriod);
        _mockReviewRepository.Verify(r => r.AddAsync(It.IsAny<PerformanceReview>()), Times.Once);
    }

    [Fact]
    public async Task SubmitSelfAssessmentAsync_ValidReview_UpdatesStatusAndAssessment()
    {
        // Arrange
        var reviewId = 1;
        var selfAssessment = "I have achieved most of my goals this quarter.";
        var review = new PerformanceReview
        {
            Id = reviewId,
            Status = PerformanceReviewStatus.NotStarted
        };

        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId))
            .ReturnsAsync(review);
        _mockReviewRepository.Setup(r => r.UpdateAsync(It.IsAny<PerformanceReview>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.SubmitSelfAssessmentAsync(reviewId, selfAssessment);

        // Assert
        Assert.True(result);
        Assert.Equal(selfAssessment, review.EmployeeSelfAssessment);
        Assert.Equal(PerformanceReviewStatus.SelfAssessmentComplete, review.Status);
        _mockReviewRepository.Verify(r => r.UpdateAsync(It.IsAny<PerformanceReview>()), Times.Once);
    }

    [Fact]
    public async Task CompleteManagerReviewAsync_ValidReview_UpdatesReviewWithRating()
    {
        // Arrange
        var reviewId = 1;
        var managerComments = "Employee has shown excellent performance.";
        var overallRating = PerformanceRating.ExceedsExpectations;
        var review = new PerformanceReview
        {
            Id = reviewId,
            Status = PerformanceReviewStatus.SelfAssessmentComplete
        };

        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId))
            .ReturnsAsync(review);
        _mockReviewRepository.Setup(r => r.UpdateAsync(It.IsAny<PerformanceReview>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.CompleteManagerReviewAsync(reviewId, managerComments, overallRating);

        // Assert
        Assert.True(result);
        Assert.Equal(managerComments, review.ManagerComments);
        Assert.Equal(overallRating, review.OverallRating);
        Assert.Equal(80m, review.OverallScore); // 4 * 20 = 80
        Assert.Equal(PerformanceReviewStatus.ManagerReviewComplete, review.Status);
    }

    #endregion

    #region 360-Degree Feedback Tests

    [Fact]
    public async Task SubmitFeedbackAsync_ValidFeedback_ReturnsFeedbackWithSubmittedStatus()
    {
        // Arrange
        var feedback = new PerformanceFeedback
        {
            PerformanceReviewId = 1,
            RevieweeId = 1,
            ReviewerId = 2,
            FeedbackType = FeedbackType.PeerReview,
            CompetencyArea = "Communication",
            Rating = PerformanceRating.MeetsExpectations,
            Comments = "Good communication skills"
        };

        _mockFeedbackRepository.Setup(r => r.AddAsync(It.IsAny<PerformanceFeedback>()))
            .ReturnsAsync(feedback);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.SubmitFeedbackAsync(feedback);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSubmitted);
        Assert.NotNull(result.SubmittedDate);
        _mockFeedbackRepository.Verify(r => r.AddAsync(It.IsAny<PerformanceFeedback>()), Times.Once);
    }

    [Fact]
    public async Task RequestFeedbackAsync_ValidRequest_CreatesFeedbackRequest()
    {
        // Arrange
        var reviewId = 1;
        var reviewerId = 2;
        var feedbackType = FeedbackType.PeerReview;
        var review = new PerformanceReview
        {
            Id = reviewId,
            EmployeeId = 1
        };

        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId))
            .ReturnsAsync(review);
        _mockFeedbackRepository.Setup(r => r.AddAsync(It.IsAny<PerformanceFeedback>()))
            .ReturnsAsync(new PerformanceFeedback());
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.RequestFeedbackAsync(reviewId, reviewerId, feedbackType);

        // Assert
        Assert.True(result);
        _mockFeedbackRepository.Verify(r => r.AddAsync(It.Is<PerformanceFeedback>(f => 
            f.PerformanceReviewId == reviewId && 
            f.ReviewerId == reviewerId && 
            f.FeedbackType == feedbackType &&
            !f.IsSubmitted)), Times.Once);
    }

    #endregion

    #region Analytics Tests

    [Fact]
    public async Task GetEmployeePerformanceScoreAsync_WithCompletedReviews_ReturnsAverageScore()
    {
        // Arrange
        var employeeId = 1;
        var reviews = new List<PerformanceReview>
        {
            new() { EmployeeId = employeeId, OverallScore = 80m },
            new() { EmployeeId = employeeId, OverallScore = 90m },
            new() { EmployeeId = employeeId, OverallScore = 85m }
        };

        _mockReviewRepository.Setup(r => r.GetByEmployeeIdAsync(employeeId))
            .ReturnsAsync(reviews);

        // Act
        var result = await _service.GetEmployeePerformanceScoreAsync(employeeId);

        // Assert
        Assert.Equal(85m, result); // Average of 80, 90, 85
    }

    [Fact]
    public async Task GetEmployeePerformanceScoreAsync_NoCompletedReviews_ReturnsZero()
    {
        // Arrange
        var employeeId = 1;
        var reviews = new List<PerformanceReview>
        {
            new() { EmployeeId = employeeId, OverallScore = null }
        };

        _mockReviewRepository.Setup(r => r.GetByEmployeeIdAsync(employeeId))
            .ReturnsAsync(reviews);

        // Act
        var result = await _service.GetEmployeePerformanceScoreAsync(employeeId);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task GetTeamPerformanceMetricsAsync_WithReviews_ReturnsCorrectMetrics()
    {
        // Arrange
        var managerId = 1;
        var reviews = new List<PerformanceReview>
        {
            new() { ManagerId = managerId, OverallScore = 85m, RequiresPIP = false },
            new() { ManagerId = managerId, OverallScore = 55m, RequiresPIP = true },
            new() { ManagerId = managerId, OverallScore = 90m, RequiresPIP = false },
            new() { ManagerId = managerId, OverallScore = null, RequiresPIP = false }
        };

        _mockReviewRepository.Setup(r => r.GetByManagerIdAsync(managerId))
            .ReturnsAsync(reviews);

        // Act
        var result = await _service.GetTeamPerformanceMetricsAsync(managerId);

        // Assert
        Assert.Equal(4, result["TotalReviews"]);
        Assert.Equal(3, result["CompletedReviews"]);
        Assert.Equal(76.67m, Math.Round(result["AverageScore"], 2)); // Average of 85, 55, 90
        Assert.Equal(2, result["HighPerformers"]); // >= 80
        Assert.Equal(1, result["LowPerformers"]); // < 60
        Assert.Equal(1, result["RequirePIP"]);
    }

    #endregion
}
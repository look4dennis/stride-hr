using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Grievance;
using System.Security.Claims;
using Xunit;

namespace StrideHR.Tests.Controllers;

public class GrievanceControllerTests
{
    private readonly Mock<IGrievanceService> _mockGrievanceService;
    private readonly GrievanceController _controller;

    public GrievanceControllerTests()
    {
        _mockGrievanceService = new Mock<IGrievanceService>();
        _controller = new GrievanceController(_mockGrievanceService.Object);

        // Setup HttpContext with claims
        var claims = new List<Claim>
        {
            new Claim("EmployeeId", "1"),
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [Fact]
    public async Task CreateGrievance_ValidDto_ReturnsSuccessResult()
    {
        // Arrange
        var dto = new CreateGrievanceDto
        {
            Title = "Test Grievance",
            Description = "Test Description",
            Category = GrievanceCategory.WorkplaceHarassment,
            Priority = GrievancePriority.High,
            IsAnonymous = false
        };

        var expectedGrievance = new GrievanceDto
        {
            Id = 1,
            GrievanceNumber = "GRV-2025-01-0001",
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Priority = dto.Priority,
            Status = GrievanceStatus.Submitted,
            SubmittedById = 1
        };

        _mockGrievanceService.Setup(s => s.CreateGrievanceAsync(dto, 1))
            .ReturnsAsync(expectedGrievance);

        // Act
        var result = await _controller.CreateGrievance(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        _mockGrievanceService.Verify(s => s.CreateGrievanceAsync(dto, 1), Times.Once);
    }

    [Fact]
    public async Task GetGrievance_ExistingId_ReturnsGrievance()
    {
        // Arrange
        var grievanceId = 1;
        var expectedGrievance = new GrievanceDto
        {
            Id = grievanceId,
            GrievanceNumber = "GRV-2025-01-0001",
            Title = "Test Grievance",
            Status = GrievanceStatus.Submitted
        };

        _mockGrievanceService.Setup(s => s.GetGrievanceByIdAsync(grievanceId))
            .ReturnsAsync(expectedGrievance);

        // Act
        var result = await _controller.GetGrievance(grievanceId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        _mockGrievanceService.Verify(s => s.GetGrievanceByIdAsync(grievanceId), Times.Once);
    }

    [Fact]
    public async Task GetGrievanceByNumber_ExistingNumber_ReturnsGrievance()
    {
        // Arrange
        var grievanceNumber = "GRV-2025-01-0001";
        var expectedGrievance = new GrievanceDto
        {
            Id = 1,
            GrievanceNumber = grievanceNumber,
            Title = "Test Grievance",
            Status = GrievanceStatus.Submitted
        };

        _mockGrievanceService.Setup(s => s.GetGrievanceByNumberAsync(grievanceNumber))
            .ReturnsAsync(expectedGrievance);

        // Act
        var result = await _controller.GetGrievanceByNumber(grievanceNumber);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        _mockGrievanceService.Verify(s => s.GetGrievanceByNumberAsync(grievanceNumber), Times.Once);
    }

    [Fact]
    public async Task GetGrievanceByNumber_NonExistingNumber_ReturnsNotFound()
    {
        // Arrange
        var grievanceNumber = "GRV-2025-01-9999";

        _mockGrievanceService.Setup(s => s.GetGrievanceByNumberAsync(grievanceNumber))
            .ReturnsAsync((GrievanceDto?)null);

        // Act
        var result = await _controller.GetGrievanceByNumber(grievanceNumber);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Contains($"Grievance with number {grievanceNumber} not found", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task SearchGrievances_ValidCriteria_ReturnsPagedResults()
    {
        // Arrange
        var criteria = new GrievanceSearchCriteria
        {
            SearchTerm = "test",
            Status = GrievanceStatus.Submitted,
            PageNumber = 1,
            PageSize = 10
        };

        var grievances = new List<GrievanceDto>
        {
            new GrievanceDto
            {
                Id = 1,
                GrievanceNumber = "GRV-2025-01-0001",
                Title = "Test Grievance 1",
                Status = GrievanceStatus.Submitted
            },
            new GrievanceDto
            {
                Id = 2,
                GrievanceNumber = "GRV-2025-01-0002",
                Title = "Test Grievance 2",
                Status = GrievanceStatus.Submitted
            }
        };

        var totalCount = 2;

        _mockGrievanceService.Setup(s => s.SearchGrievancesAsync(criteria))
            .ReturnsAsync((grievances, totalCount));

        // Act
        var result = await _controller.SearchGrievances(criteria);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        _mockGrievanceService.Verify(s => s.SearchGrievancesAsync(criteria), Times.Once);
    }

    [Fact]
    public async Task AssignGrievance_ValidRequest_ReturnsUpdatedGrievance()
    {
        // Arrange
        var grievanceId = 1;
        var request = new AssignGrievanceRequest { AssignedToId = 2 };

        var expectedGrievance = new GrievanceDto
        {
            Id = grievanceId,
            GrievanceNumber = "GRV-2025-01-0001",
            AssignedToId = request.AssignedToId,
            Status = GrievanceStatus.UnderReview
        };

        _mockGrievanceService.Setup(s => s.AssignGrievanceAsync(grievanceId, request.AssignedToId, 1))
            .ReturnsAsync(expectedGrievance);

        // Act
        var result = await _controller.AssignGrievance(grievanceId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        _mockGrievanceService.Verify(s => s.AssignGrievanceAsync(grievanceId, request.AssignedToId, 1), Times.Once);
    }

    [Fact]
    public async Task UpdateGrievanceStatus_ValidRequest_ReturnsUpdatedGrievance()
    {
        // Arrange
        var grievanceId = 1;
        var request = new UpdateGrievanceStatusRequest
        {
            Status = GrievanceStatus.UnderReview,
            Reason = "Starting investigation"
        };

        var expectedGrievance = new GrievanceDto
        {
            Id = grievanceId,
            GrievanceNumber = "GRV-2025-01-0001",
            Status = request.Status
        };

        _mockGrievanceService.Setup(s => s.UpdateStatusAsync(grievanceId, request.Status, 1, request.Reason))
            .ReturnsAsync(expectedGrievance);

        // Act
        var result = await _controller.UpdateGrievanceStatus(grievanceId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        _mockGrievanceService.Verify(s => s.UpdateStatusAsync(grievanceId, request.Status, 1, request.Reason), Times.Once);
    }

    [Fact]
    public async Task ResolveGrievance_ValidRequest_ReturnsResolvedGrievance()
    {
        // Arrange
        var grievanceId = 1;
        var request = new ResolveGrievanceRequest
        {
            Resolution = "Issue has been resolved",
            ResolutionNotes = "Additional notes"
        };

        var expectedGrievance = new GrievanceDto
        {
            Id = grievanceId,
            GrievanceNumber = "GRV-2025-01-0001",
            Status = GrievanceStatus.Resolved,
            Resolution = request.Resolution,
            ResolutionNotes = request.ResolutionNotes,
            ResolvedById = 1
        };

        _mockGrievanceService.Setup(s => s.ResolveGrievanceAsync(grievanceId, request.Resolution, request.ResolutionNotes, 1))
            .ReturnsAsync(expectedGrievance);

        // Act
        var result = await _controller.ResolveGrievance(grievanceId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        _mockGrievanceService.Verify(s => s.ResolveGrievanceAsync(grievanceId, request.Resolution, request.ResolutionNotes, 1), Times.Once);
    }

    [Fact]
    public async Task EscalateGrievance_ValidRequest_ReturnsEscalatedGrievance()
    {
        // Arrange
        var grievanceId = 1;
        var request = new EscalateGrievanceRequest
        {
            ToLevel = EscalationLevel.Level3_HRManager,
            Reason = "Requires higher level attention",
            EscalatedToId = 3
        };

        var expectedGrievance = new GrievanceDto
        {
            Id = grievanceId,
            GrievanceNumber = "GRV-2025-01-0001",
            CurrentEscalationLevel = request.ToLevel,
            IsEscalated = true,
            EscalationReason = request.Reason,
            EscalatedById = 1,
            AssignedToId = request.EscalatedToId
        };

        _mockGrievanceService.Setup(s => s.EscalateGrievanceAsync(grievanceId, request.ToLevel, request.Reason, 1, request.EscalatedToId))
            .ReturnsAsync(expectedGrievance);

        // Act
        var result = await _controller.EscalateGrievance(grievanceId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        _mockGrievanceService.Verify(s => s.EscalateGrievanceAsync(grievanceId, request.ToLevel, request.Reason, 1, request.EscalatedToId), Times.Once);
    }

    [Fact]
    public async Task WithdrawGrievance_ValidRequest_ReturnsWithdrawnGrievance()
    {
        // Arrange
        var grievanceId = 1;
        var request = new WithdrawGrievanceRequest
        {
            Reason = "No longer needed"
        };

        var expectedGrievance = new GrievanceDto
        {
            Id = grievanceId,
            GrievanceNumber = "GRV-2025-01-0001",
            Status = GrievanceStatus.Withdrawn
        };

        _mockGrievanceService.Setup(s => s.WithdrawGrievanceAsync(grievanceId, request.Reason, 1))
            .ReturnsAsync(expectedGrievance);

        // Act
        var result = await _controller.WithdrawGrievance(grievanceId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        _mockGrievanceService.Verify(s => s.WithdrawGrievanceAsync(grievanceId, request.Reason, 1), Times.Once);
    }

    [Fact]
    public async Task GetMyGrievances_ReturnsUserGrievances()
    {
        // Arrange
        var expectedGrievances = new List<GrievanceDto>
        {
            new GrievanceDto
            {
                Id = 1,
                GrievanceNumber = "GRV-2025-01-0001",
                Title = "My Grievance 1",
                SubmittedById = 1
            },
            new GrievanceDto
            {
                Id = 2,
                GrievanceNumber = "GRV-2025-01-0002",
                Title = "My Grievance 2",
                SubmittedById = 1
            }
        };

        _mockGrievanceService.Setup(s => s.GetMyGrievancesAsync(1))
            .ReturnsAsync(expectedGrievances);

        // Act
        var result = await _controller.GetMyGrievances();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        _mockGrievanceService.Verify(s => s.GetMyGrievancesAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetAssignedGrievances_ReturnsAssignedGrievances()
    {
        // Arrange
        var expectedGrievances = new List<GrievanceDto>
        {
            new GrievanceDto
            {
                Id = 1,
                GrievanceNumber = "GRV-2025-01-0001",
                Title = "Assigned Grievance 1",
                AssignedToId = 1
            }
        };

        _mockGrievanceService.Setup(s => s.GetAssignedGrievancesAsync(1))
            .ReturnsAsync(expectedGrievances);

        // Act
        var result = await _controller.GetAssignedGrievances();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        _mockGrievanceService.Verify(s => s.GetAssignedGrievancesAsync(1), Times.Once);
    }

    [Fact]
    public async Task AddComment_ValidDto_ReturnsCreatedComment()
    {
        // Arrange
        var grievanceId = 1;
        var dto = new CreateGrievanceCommentDto
        {
            Comment = "This is a test comment",
            IsInternal = false
        };

        var expectedComment = new GrievanceCommentDto
        {
            Id = 1,
            GrievanceId = grievanceId,
            Comment = dto.Comment,
            AuthorId = 1,
            IsInternal = dto.IsInternal
        };

        _mockGrievanceService.Setup(s => s.AddCommentAsync(grievanceId, dto, 1))
            .ReturnsAsync(expectedComment);

        // Act
        var result = await _controller.AddComment(grievanceId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        _mockGrievanceService.Verify(s => s.AddCommentAsync(grievanceId, dto, 1), Times.Once);
    }

    [Fact]
    public async Task GetGrievanceComments_ReturnsComments()
    {
        // Arrange
        var grievanceId = 1;
        var includeInternal = false;

        var expectedComments = new List<GrievanceCommentDto>
        {
            new GrievanceCommentDto
            {
                Id = 1,
                GrievanceId = grievanceId,
                Comment = "Comment 1",
                AuthorId = 1,
                IsInternal = false
            },
            new GrievanceCommentDto
            {
                Id = 2,
                GrievanceId = grievanceId,
                Comment = "Comment 2",
                AuthorId = 2,
                IsInternal = false
            }
        };

        _mockGrievanceService.Setup(s => s.GetGrievanceCommentsAsync(grievanceId, includeInternal))
            .ReturnsAsync(expectedComments);

        // Act
        var result = await _controller.GetGrievanceComments(grievanceId, includeInternal);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        _mockGrievanceService.Verify(s => s.GetGrievanceCommentsAsync(grievanceId, includeInternal), Times.Once);
    }

    [Fact]
    public async Task ScheduleFollowUp_ValidDto_ReturnsScheduledFollowUp()
    {
        // Arrange
        var grievanceId = 1;
        var dto = new CreateGrievanceFollowUpDto
        {
            Title = "Follow-up meeting",
            Description = "Schedule a follow-up meeting with the employee",
            ScheduledDate = DateTime.UtcNow.AddDays(7)
        };

        var expectedFollowUp = new GrievanceFollowUpDto
        {
            Id = 1,
            GrievanceId = grievanceId,
            Title = dto.Title,
            Description = dto.Description,
            ScheduledDate = dto.ScheduledDate,
            ScheduledById = 1,
            IsCompleted = false
        };

        _mockGrievanceService.Setup(s => s.ScheduleFollowUpAsync(grievanceId, dto, 1))
            .ReturnsAsync(expectedFollowUp);

        // Act
        var result = await _controller.ScheduleFollowUp(grievanceId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        _mockGrievanceService.Verify(s => s.ScheduleFollowUpAsync(grievanceId, dto, 1), Times.Once);
    }

    [Fact]
    public async Task GetGrievanceAnalytics_ReturnsAnalytics()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddDays(-30);
        var toDate = DateTime.UtcNow;

        var expectedAnalytics = new GrievanceAnalyticsDto
        {
            TotalGrievances = 10,
            OpenGrievances = 3,
            ResolvedGrievances = 6,
            ClosedGrievances = 1,
            EscalatedGrievances = 2,
            OverdueGrievances = 1,
            AnonymousGrievances = 2,
            AverageResolutionTimeHours = 48.5,
            SatisfactionRating = 4.2
        };

        _mockGrievanceService.Setup(s => s.GetAnalyticsAsync(fromDate, toDate))
            .ReturnsAsync(expectedAnalytics);

        // Act
        var result = await _controller.GetGrievanceAnalytics(fromDate, toDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        _mockGrievanceService.Verify(s => s.GetAnalyticsAsync(fromDate, toDate), Times.Once);
    }
}
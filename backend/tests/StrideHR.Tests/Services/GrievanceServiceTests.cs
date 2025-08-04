using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Grievance;
using StrideHR.Core.Models.Notification;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class GrievanceServiceTests
{
    private readonly Mock<IGrievanceRepository> _mockGrievanceRepository;
    private readonly Mock<IGrievanceCommentRepository> _mockCommentRepository;
    private readonly Mock<IGrievanceFollowUpRepository> _mockFollowUpRepository;
    private readonly Mock<IRepository<GrievanceStatusHistory>> _mockStatusHistoryRepository;
    private readonly Mock<IRepository<GrievanceEscalation>> _mockEscalationRepository;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<GrievanceService>> _mockLogger;
    private readonly GrievanceService _grievanceService;

    public GrievanceServiceTests()
    {
        _mockGrievanceRepository = new Mock<IGrievanceRepository>();
        _mockCommentRepository = new Mock<IGrievanceCommentRepository>();
        _mockFollowUpRepository = new Mock<IGrievanceFollowUpRepository>();
        _mockStatusHistoryRepository = new Mock<IRepository<GrievanceStatusHistory>>();
        _mockEscalationRepository = new Mock<IRepository<GrievanceEscalation>>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<GrievanceService>>();

        _grievanceService = new GrievanceService(
            _mockGrievanceRepository.Object,
            _mockCommentRepository.Object,
            _mockFollowUpRepository.Object,
            _mockStatusHistoryRepository.Object,
            _mockEscalationRepository.Object,
            _mockNotificationService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateGrievanceAsync_ValidDto_ReturnsGrievanceDto()
    {
        // Arrange
        var dto = new CreateGrievanceDto
        {
            Title = "Test Grievance",
            Description = "Test Description",
            Category = GrievanceCategory.WorkplaceHarassment,
            Priority = GrievancePriority.High,
            IsAnonymous = false,
            RequiresInvestigation = true
        };

        var submitterId = 1;
        var grievanceNumber = "GRV-2025-01-0001";

        _mockGrievanceRepository.Setup(r => r.GenerateGrievanceNumberAsync())
            .ReturnsAsync(grievanceNumber);

        _mockGrievanceRepository.Setup(r => r.AddAsync(It.IsAny<Grievance>()))
            .ReturnsAsync(It.IsAny<Grievance>());

        _mockGrievanceRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockStatusHistoryRepository.Setup(r => r.AddAsync(It.IsAny<GrievanceStatusHistory>()))
            .ReturnsAsync(It.IsAny<GrievanceStatusHistory>());

        _mockStatusHistoryRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        var grievance = new Grievance
        {
            Id = 1,
            GrievanceNumber = grievanceNumber,
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Priority = dto.Priority,
            IsAnonymous = dto.IsAnonymous,
            SubmittedById = submitterId,
            Status = GrievanceStatus.Submitted
        };

        _mockGrievanceRepository.Setup(r => r.GetWithDetailsAsync(It.IsAny<int>()))
            .ReturnsAsync(grievance);

        var expectedDto = new GrievanceDto
        {
            Id = 1,
            GrievanceNumber = grievanceNumber,
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Priority = dto.Priority,
            Status = GrievanceStatus.Submitted,
            IsAnonymous = dto.IsAnonymous,
            SubmittedById = submitterId
        };

        _mockMapper.Setup(m => m.Map<GrievanceDto>(It.IsAny<Grievance>()))
            .Returns(expectedDto);

        // Act
        var result = await _grievanceService.CreateGrievanceAsync(dto, submitterId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.GrievanceNumber, result.GrievanceNumber);
        Assert.Equal(expectedDto.Title, result.Title);
        Assert.Equal(expectedDto.Category, result.Category);
        Assert.Equal(expectedDto.Priority, result.Priority);
        Assert.Equal(expectedDto.Status, result.Status);

        _mockGrievanceRepository.Verify(r => r.GenerateGrievanceNumberAsync(), Times.Once);
        _mockGrievanceRepository.Verify(r => r.AddAsync(It.IsAny<Grievance>()), Times.Once);
        _mockGrievanceRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockStatusHistoryRepository.Verify(r => r.AddAsync(It.IsAny<GrievanceStatusHistory>()), Times.Once);
    }

    [Fact]
    public async Task GetGrievanceByIdAsync_ExistingId_ReturnsGrievanceDto()
    {
        // Arrange
        var grievanceId = 1;
        var grievance = new Grievance
        {
            Id = grievanceId,
            GrievanceNumber = "GRV-2025-01-0001",
            Title = "Test Grievance",
            Description = "Test Description",
            Category = GrievanceCategory.WorkplaceHarassment,
            Priority = GrievancePriority.High,
            Status = GrievanceStatus.Submitted,
            SubmittedById = 1,
            CreatedAt = DateTime.UtcNow
        };

        var expectedDto = new GrievanceDto
        {
            Id = grievanceId,
            GrievanceNumber = grievance.GrievanceNumber,
            Title = grievance.Title,
            Description = grievance.Description,
            Category = grievance.Category,
            Priority = grievance.Priority,
            Status = grievance.Status,
            SubmittedById = grievance.SubmittedById
        };

        _mockGrievanceRepository.Setup(r => r.GetWithDetailsAsync(grievanceId))
            .ReturnsAsync(grievance);

        _mockCommentRepository.Setup(r => r.GetCommentsCountAsync(grievanceId))
            .ReturnsAsync(0);

        _mockFollowUpRepository.Setup(r => r.GetFollowUpsCountAsync(grievanceId))
            .ReturnsAsync(0);

        _mockMapper.Setup(m => m.Map<GrievanceDto>(grievance))
            .Returns(expectedDto);

        // Act
        var result = await _grievanceService.GetGrievanceByIdAsync(grievanceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.GrievanceNumber, result.GrievanceNumber);
        Assert.Equal(expectedDto.Title, result.Title);

        _mockGrievanceRepository.Verify(r => r.GetWithDetailsAsync(grievanceId), Times.Once);
        _mockCommentRepository.Verify(r => r.GetCommentsCountAsync(grievanceId), Times.Once);
        _mockFollowUpRepository.Verify(r => r.GetFollowUpsCountAsync(grievanceId), Times.Once);
    }

    [Fact]
    public async Task GetGrievanceByIdAsync_NonExistingId_ThrowsArgumentException()
    {
        // Arrange
        var grievanceId = 999;

        _mockGrievanceRepository.Setup(r => r.GetWithDetailsAsync(grievanceId))
            .ReturnsAsync((Grievance?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _grievanceService.GetGrievanceByIdAsync(grievanceId));

        Assert.Contains($"Grievance with ID {grievanceId} not found", exception.Message);
    }

    [Fact]
    public async Task AssignGrievanceAsync_ValidData_ReturnsUpdatedGrievanceDto()
    {
        // Arrange
        var grievanceId = 1;
        var assignedToId = 2;
        var assignedById = 3;

        var grievance = new Grievance
        {
            Id = grievanceId,
            GrievanceNumber = "GRV-2025-01-0001",
            Title = "Test Grievance",
            Status = GrievanceStatus.Submitted,
            SubmittedById = 1
        };

        _mockGrievanceRepository.Setup(r => r.GetByIdAsync(grievanceId))
            .ReturnsAsync(grievance);

        _mockGrievanceRepository.Setup(r => r.UpdateAsync(It.IsAny<Grievance>()))
            .Returns(Task.CompletedTask);

        _mockGrievanceRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        var expectedDto = new GrievanceDto
        {
            Id = grievanceId,
            GrievanceNumber = grievance.GrievanceNumber,
            AssignedToId = assignedToId,
            Status = GrievanceStatus.UnderReview
        };

        _mockMapper.Setup(m => m.Map<GrievanceDto>(It.IsAny<Grievance>()))
            .Returns(expectedDto);

        // Mock the GetGrievanceByIdAsync call that happens at the end
        _mockGrievanceRepository.Setup(r => r.GetWithDetailsAsync(grievanceId))
            .ReturnsAsync(grievance);

        _mockCommentRepository.Setup(r => r.GetCommentsCountAsync(grievanceId))
            .ReturnsAsync(0);

        _mockFollowUpRepository.Setup(r => r.GetFollowUpsCountAsync(grievanceId))
            .ReturnsAsync(0);

        // Act
        var result = await _grievanceService.AssignGrievanceAsync(grievanceId, assignedToId, assignedById);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(assignedToId, result.AssignedToId);

        _mockGrievanceRepository.Verify(r => r.GetByIdAsync(grievanceId), Times.AtLeastOnce);
        _mockGrievanceRepository.Verify(r => r.UpdateAsync(It.IsAny<Grievance>()), Times.AtLeastOnce);
        _mockNotificationService.Verify(n => n.CreateNotificationAsync(
            It.IsAny<CreateNotificationDto>()), Times.Once);
    }

    [Fact]
    public async Task ResolveGrievanceAsync_ValidData_ReturnsResolvedGrievanceDto()
    {
        // Arrange
        var grievanceId = 1;
        var resolution = "Issue has been resolved";
        var resolutionNotes = "Additional notes";
        var resolvedById = 2;

        var grievance = new Grievance
        {
            Id = grievanceId,
            GrievanceNumber = "GRV-2025-01-0001",
            Title = "Test Grievance",
            Status = GrievanceStatus.UnderReview,
            SubmittedById = 1
        };

        _mockGrievanceRepository.Setup(r => r.GetByIdAsync(grievanceId))
            .ReturnsAsync(grievance);

        _mockGrievanceRepository.Setup(r => r.UpdateAsync(It.IsAny<Grievance>()))
            .Returns(Task.CompletedTask);

        _mockGrievanceRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockStatusHistoryRepository.Setup(r => r.AddAsync(It.IsAny<GrievanceStatusHistory>()))
            .ReturnsAsync(It.IsAny<GrievanceStatusHistory>());

        var expectedDto = new GrievanceDto
        {
            Id = grievanceId,
            GrievanceNumber = grievance.GrievanceNumber,
            Status = GrievanceStatus.Resolved,
            Resolution = resolution,
            ResolutionNotes = resolutionNotes,
            ResolvedById = resolvedById
        };

        _mockMapper.Setup(m => m.Map<GrievanceDto>(It.IsAny<Grievance>()))
            .Returns(expectedDto);

        // Mock the GetGrievanceByIdAsync call that happens at the end
        _mockGrievanceRepository.Setup(r => r.GetWithDetailsAsync(grievanceId))
            .ReturnsAsync(grievance);

        _mockCommentRepository.Setup(r => r.GetCommentsCountAsync(grievanceId))
            .ReturnsAsync(0);

        _mockFollowUpRepository.Setup(r => r.GetFollowUpsCountAsync(grievanceId))
            .ReturnsAsync(0);

        // Act
        var result = await _grievanceService.ResolveGrievanceAsync(grievanceId, resolution, resolutionNotes, resolvedById);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(GrievanceStatus.Resolved, result.Status);
        Assert.Equal(resolution, result.Resolution);
        Assert.Equal(resolutionNotes, result.ResolutionNotes);
        Assert.Equal(resolvedById, result.ResolvedById);

        _mockGrievanceRepository.Verify(r => r.GetByIdAsync(grievanceId), Times.Once);
        _mockGrievanceRepository.Verify(r => r.UpdateAsync(It.IsAny<Grievance>()), Times.Once);
        _mockStatusHistoryRepository.Verify(r => r.AddAsync(It.IsAny<GrievanceStatusHistory>()), Times.Once);
        _mockNotificationService.Verify(n => n.CreateNotificationAsync(
            It.IsAny<CreateNotificationDto>()), Times.Once);
    }

    [Fact]
    public async Task EscalateGrievanceAsync_ValidData_ReturnsEscalatedGrievanceDto()
    {
        // Arrange
        var grievanceId = 1;
        var toLevel = EscalationLevel.Level3_HRManager;
        var reason = "Requires higher level attention";
        var escalatedById = 2;
        var escalatedToId = 3;

        var grievance = new Grievance
        {
            Id = grievanceId,
            GrievanceNumber = "GRV-2025-01-0001",
            Title = "Test Grievance",
            Status = GrievanceStatus.UnderReview,
            CurrentEscalationLevel = EscalationLevel.Level1_DirectManager,
            SubmittedById = 1
        };

        _mockGrievanceRepository.Setup(r => r.GetByIdAsync(grievanceId))
            .ReturnsAsync(grievance);

        _mockGrievanceRepository.Setup(r => r.UpdateAsync(It.IsAny<Grievance>()))
            .Returns(Task.CompletedTask);

        _mockGrievanceRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockEscalationRepository.Setup(r => r.AddAsync(It.IsAny<GrievanceEscalation>()))
            .ReturnsAsync(It.IsAny<GrievanceEscalation>());

        var expectedDto = new GrievanceDto
        {
            Id = grievanceId,
            GrievanceNumber = grievance.GrievanceNumber,
            CurrentEscalationLevel = toLevel,
            IsEscalated = true,
            EscalationReason = reason,
            EscalatedById = escalatedById,
            AssignedToId = escalatedToId
        };

        _mockMapper.Setup(m => m.Map<GrievanceDto>(It.IsAny<Grievance>()))
            .Returns(expectedDto);

        // Mock the GetGrievanceByIdAsync call that happens at the end
        _mockGrievanceRepository.Setup(r => r.GetWithDetailsAsync(grievanceId))
            .ReturnsAsync(grievance);

        _mockCommentRepository.Setup(r => r.GetCommentsCountAsync(grievanceId))
            .ReturnsAsync(0);

        _mockFollowUpRepository.Setup(r => r.GetFollowUpsCountAsync(grievanceId))
            .ReturnsAsync(0);

        // Act
        var result = await _grievanceService.EscalateGrievanceAsync(grievanceId, toLevel, reason, escalatedById, escalatedToId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(toLevel, result.CurrentEscalationLevel);
        Assert.True(result.IsEscalated);
        Assert.Equal(reason, result.EscalationReason);
        Assert.Equal(escalatedById, result.EscalatedById);

        _mockGrievanceRepository.Verify(r => r.GetByIdAsync(grievanceId), Times.Once);
        _mockGrievanceRepository.Verify(r => r.UpdateAsync(It.IsAny<Grievance>()), Times.Once);
        _mockEscalationRepository.Verify(r => r.AddAsync(It.IsAny<GrievanceEscalation>()), Times.Once);
        _mockNotificationService.Verify(n => n.CreateNotificationAsync(
            It.IsAny<CreateNotificationDto>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task AddCommentAsync_ValidData_ReturnsGrievanceCommentDto()
    {
        // Arrange
        var grievanceId = 1;
        var authorId = 2;
        var dto = new CreateGrievanceCommentDto
        {
            Comment = "This is a test comment",
            IsInternal = false
        };

        var grievance = new Grievance
        {
            Id = grievanceId,
            GrievanceNumber = "GRV-2025-01-0001",
            SubmittedById = 1
        };

        var comment = new GrievanceComment
        {
            Id = 1,
            GrievanceId = grievanceId,
            Comment = dto.Comment,
            AuthorId = authorId,
            IsInternal = dto.IsInternal
        };

        _mockGrievanceRepository.Setup(r => r.GetByIdAsync(grievanceId))
            .ReturnsAsync(grievance);

        _mockCommentRepository.Setup(r => r.AddAsync(It.IsAny<GrievanceComment>()))
            .ReturnsAsync(comment);

        _mockCommentRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockCommentRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(comment);

        var expectedDto = new GrievanceCommentDto
        {
            Id = 1,
            GrievanceId = grievanceId,
            Comment = dto.Comment,
            AuthorId = authorId,
            IsInternal = dto.IsInternal
        };

        _mockMapper.Setup(m => m.Map<GrievanceCommentDto>(comment))
            .Returns(expectedDto);

        // Act
        var result = await _grievanceService.AddCommentAsync(grievanceId, dto, authorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Comment, result.Comment);
        Assert.Equal(expectedDto.AuthorId, result.AuthorId);
        Assert.Equal(expectedDto.IsInternal, result.IsInternal);

        _mockGrievanceRepository.Verify(r => r.GetByIdAsync(grievanceId), Times.Once);
        _mockCommentRepository.Verify(r => r.AddAsync(It.IsAny<GrievanceComment>()), Times.Once);
        _mockCommentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockNotificationService.Verify(n => n.CreateNotificationAsync(
            It.IsAny<CreateNotificationDto>()), Times.Once);
    }

    [Fact]
    public async Task SearchGrievancesAsync_ValidCriteria_ReturnsPagedResults()
    {
        // Arrange
        var criteria = new GrievanceSearchCriteria
        {
            SearchTerm = "test",
            Status = GrievanceStatus.Submitted,
            PageNumber = 1,
            PageSize = 10
        };

        var grievances = new List<Grievance>
        {
            new Grievance
            {
                Id = 1,
                GrievanceNumber = "GRV-2025-01-0001",
                Title = "Test Grievance 1",
                Status = GrievanceStatus.Submitted
            },
            new Grievance
            {
                Id = 2,
                GrievanceNumber = "GRV-2025-01-0002",
                Title = "Test Grievance 2",
                Status = GrievanceStatus.Submitted
            }
        };

        var totalCount = 2;

        _mockGrievanceRepository.Setup(r => r.SearchAsync(criteria))
            .ReturnsAsync((grievances, totalCount));

        _mockCommentRepository.Setup(r => r.GetCommentsCountAsync(It.IsAny<int>()))
            .ReturnsAsync(0);

        _mockFollowUpRepository.Setup(r => r.GetFollowUpsCountAsync(It.IsAny<int>()))
            .ReturnsAsync(0);

        var expectedDtos = grievances.Select(g => new GrievanceDto
        {
            Id = g.Id,
            GrievanceNumber = g.GrievanceNumber,
            Title = g.Title,
            Status = g.Status
        }).ToList();

        _mockMapper.Setup(m => m.Map<GrievanceDto>(It.IsAny<Grievance>()))
            .Returns((Grievance g) => expectedDtos.First(dto => dto.Id == g.Id));

        // Act
        var (result, resultTotalCount) = await _grievanceService.SearchGrievancesAsync(criteria);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(totalCount, resultTotalCount);
        Assert.Equal(expectedDtos[0].GrievanceNumber, result[0].GrievanceNumber);
        Assert.Equal(expectedDtos[1].GrievanceNumber, result[1].GrievanceNumber);

        _mockGrievanceRepository.Verify(r => r.SearchAsync(criteria), Times.Once);
    }

    [Fact]
    public async Task WithdrawGrievanceAsync_ValidData_ReturnsWithdrawnGrievanceDto()
    {
        // Arrange
        var grievanceId = 1;
        var reason = "No longer needed";
        var withdrawnById = 1; // Same as submitter

        var grievance = new Grievance
        {
            Id = grievanceId,
            GrievanceNumber = "GRV-2025-01-0001",
            Title = "Test Grievance",
            Status = GrievanceStatus.UnderReview,
            SubmittedById = withdrawnById
        };

        _mockGrievanceRepository.Setup(r => r.GetByIdAsync(grievanceId))
            .ReturnsAsync(grievance);

        _mockGrievanceRepository.Setup(r => r.UpdateAsync(It.IsAny<Grievance>()))
            .Returns(Task.CompletedTask);

        _mockGrievanceRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockStatusHistoryRepository.Setup(r => r.AddAsync(It.IsAny<GrievanceStatusHistory>()))
            .ReturnsAsync(It.IsAny<GrievanceStatusHistory>());

        var expectedDto = new GrievanceDto
        {
            Id = grievanceId,
            GrievanceNumber = grievance.GrievanceNumber,
            Status = GrievanceStatus.Withdrawn
        };

        _mockMapper.Setup(m => m.Map<GrievanceDto>(It.IsAny<Grievance>()))
            .Returns(expectedDto);

        // Mock the GetGrievanceByIdAsync call that happens at the end
        _mockGrievanceRepository.Setup(r => r.GetWithDetailsAsync(grievanceId))
            .ReturnsAsync(grievance);

        _mockCommentRepository.Setup(r => r.GetCommentsCountAsync(grievanceId))
            .ReturnsAsync(0);

        _mockFollowUpRepository.Setup(r => r.GetFollowUpsCountAsync(grievanceId))
            .ReturnsAsync(0);

        // Act
        var result = await _grievanceService.WithdrawGrievanceAsync(grievanceId, reason, withdrawnById);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(GrievanceStatus.Withdrawn, result.Status);

        _mockGrievanceRepository.Verify(r => r.GetByIdAsync(grievanceId), Times.AtLeastOnce);
    }

    [Fact]
    public async Task WithdrawGrievanceAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var grievanceId = 1;
        var reason = "No longer needed";
        var withdrawnById = 2; // Different from submitter

        var grievance = new Grievance
        {
            Id = grievanceId,
            GrievanceNumber = "GRV-2025-01-0001",
            Title = "Test Grievance",
            Status = GrievanceStatus.UnderReview,
            SubmittedById = 1 // Different from withdrawnById
        };

        _mockGrievanceRepository.Setup(r => r.GetByIdAsync(grievanceId))
            .ReturnsAsync(grievance);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _grievanceService.WithdrawGrievanceAsync(grievanceId, reason, withdrawnById));

        Assert.Contains("Only the submitter can withdraw a grievance", exception.Message);
    }
}
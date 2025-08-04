using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Project;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class ProjectCollaborationServiceTests
{
    private readonly Mock<IProjectCommentRepository> _mockCommentRepository;
    private readonly Mock<IProjectCommentReplyRepository> _mockReplyRepository;
    private readonly Mock<IProjectActivityRepository> _mockActivityRepository;
    private readonly Mock<IProjectRepository> _mockProjectRepository;
    private readonly Mock<IProjectAssignmentRepository> _mockAssignmentRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ProjectCollaborationService>> _mockLogger;
    private readonly Mock<IHubContext<ProjectHub>> _mockHubContext;
    private readonly ProjectCollaborationService _service;

    public ProjectCollaborationServiceTests()
    {
        _mockCommentRepository = new Mock<IProjectCommentRepository>();
        _mockReplyRepository = new Mock<IProjectCommentReplyRepository>();
        _mockActivityRepository = new Mock<IProjectActivityRepository>();
        _mockProjectRepository = new Mock<IProjectRepository>();
        _mockAssignmentRepository = new Mock<IProjectAssignmentRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ProjectCollaborationService>>();
        _mockHubContext = new Mock<IHubContext<ProjectHub>>();

        _service = new ProjectCollaborationService(
            _mockCommentRepository.Object,
            _mockReplyRepository.Object,
            _mockActivityRepository.Object,
            _mockProjectRepository.Object,
            _mockAssignmentRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockHubContext.Object);
    }

    [Fact]
    public async Task GetProjectCollaborationAsync_ValidProjectId_ReturnsCollaborationData()
    {
        // Arrange
        var projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project"
        };

        var comments = new List<ProjectCommentDto>
        {
            new ProjectCommentDto { Id = 1, ProjectId = projectId, Comment = "Test comment" }
        };

        var activities = new List<ProjectActivityDto>
        {
            new ProjectActivityDto { Id = 1, ProjectId = projectId, Description = "Test activity" }
        };

        var teamMembers = new List<ProjectAssignment>
        {
            new ProjectAssignment { Id = 1, ProjectId = projectId, EmployeeId = 1 }
        };

        var communicationStats = new ProjectCommunicationStatsDto
        {
            TotalComments = 1,
            TotalActivities = 1,
            ActiveTeamMembers = 1
        };

        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId))
            .ReturnsAsync(project);
        _mockCommentRepository.Setup(r => r.GetProjectCommentsAsync(projectId))
            .ReturnsAsync(new List<ProjectComment>());
        _mockActivityRepository.Setup(r => r.GetProjectActivitiesAsync(projectId, 50))
            .ReturnsAsync(new List<ProjectActivity>());
        _mockAssignmentRepository.Setup(r => r.GetProjectTeamMembersAsync(projectId))
            .ReturnsAsync(teamMembers);

        _mockMapper.Setup(m => m.Map<List<ProjectCommentDto>>(It.IsAny<List<ProjectComment>>()))
            .Returns(comments);
        _mockMapper.Setup(m => m.Map<List<ProjectActivityDto>>(It.IsAny<List<ProjectActivity>>()))
            .Returns(activities);
        _mockMapper.Setup(m => m.Map<List<ProjectTeamMemberDto>>(teamMembers))
            .Returns(new List<ProjectTeamMemberDto>());

        // Mock the communication stats calculation
        _mockCommentRepository.Setup(r => r.GetProjectCommentsCountAsync(projectId))
            .ReturnsAsync(1);

        // Act
        var result = await _service.GetProjectCollaborationAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projectId, result.ProjectId);
        Assert.Equal("Test Project", result.ProjectName);
        Assert.Equal(comments, result.Comments);
        Assert.Equal(activities, result.Activities);
    }

    [Fact]
    public async Task GetProjectCollaborationAsync_InvalidProjectId_ThrowsArgumentException()
    {
        // Arrange
        var projectId = 999;
        _mockProjectRepository.Setup(r => r.GetProjectWithDetailsAsync(projectId))
            .ReturnsAsync((Project)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetProjectCollaborationAsync(projectId));
    }

    [Fact]
    public async Task AddProjectCommentAsync_ValidData_AddsComment()
    {
        // Arrange
        var dto = new CreateProjectCommentDto
        {
            ProjectId = 1,
            Comment = "Test comment"
        };
        var employeeId = 123;

        var savedComment = new ProjectComment
        {
            Id = 1,
            ProjectId = dto.ProjectId,
            EmployeeId = employeeId,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow,
            Employee = new Employee { Id = employeeId, FirstName = "John", LastName = "Doe" }
        };

        _mockCommentRepository.Setup(r => r.AddAsync(It.IsAny<ProjectComment>()))
            .Returns(Task.CompletedTask);
        _mockCommentRepository.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _mockCommentRepository.Setup(r => r.GetCommentWithRepliesAsync(It.IsAny<int>()))
            .ReturnsAsync(savedComment);

        var expectedDto = new ProjectCommentDto
        {
            Id = 1,
            ProjectId = dto.ProjectId,
            EmployeeId = employeeId,
            EmployeeName = "John Doe",
            Comment = dto.Comment
        };

        _mockMapper.Setup(m => m.Map<ProjectCommentDto>(savedComment))
            .Returns(expectedDto);

        // Mock SignalR
        var mockClients = new Mock<IHubCallerClients>();
        var mockGroup = new Mock<IClientProxy>();
        _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockGroup.Object);

        // Act
        var result = await _service.AddProjectCommentAsync(dto, employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.ProjectId, result.ProjectId);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(dto.Comment, result.Comment);

        _mockCommentRepository.Verify(r => r.AddAsync(It.IsAny<ProjectComment>()), Times.Once);
        _mockCommentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddCommentReplyAsync_ValidData_AddsReply()
    {
        // Arrange
        var dto = new CreateCommentReplyDto
        {
            CommentId = 1,
            Reply = "Test reply"
        };
        var employeeId = 123;

        var originalComment = new ProjectComment
        {
            Id = 1,
            ProjectId = 1,
            EmployeeId = 456,
            Comment = "Original comment"
        };

        var savedReply = new ProjectCommentReply
        {
            Id = 1,
            CommentId = dto.CommentId,
            EmployeeId = employeeId,
            Reply = dto.Reply,
            CreatedAt = DateTime.UtcNow
        };

        _mockCommentRepository.Setup(r => r.GetByIdAsync(dto.CommentId))
            .ReturnsAsync(originalComment);
        _mockReplyRepository.Setup(r => r.AddAsync(It.IsAny<ProjectCommentReply>()))
            .Returns(Task.CompletedTask);
        _mockReplyRepository.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _mockReplyRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(savedReply);

        var expectedDto = new ProjectCommentReplyDto
        {
            Id = 1,
            CommentId = dto.CommentId,
            EmployeeId = employeeId,
            Reply = dto.Reply
        };

        _mockMapper.Setup(m => m.Map<ProjectCommentReplyDto>(savedReply))
            .Returns(expectedDto);

        // Act
        var result = await _service.AddCommentReplyAsync(dto, employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.CommentId, result.CommentId);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(dto.Reply, result.Reply);

        _mockReplyRepository.Verify(r => r.AddAsync(It.IsAny<ProjectCommentReply>()), Times.Once);
        _mockReplyRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentAsync_ValidCommentAndOwner_DeletesComment()
    {
        // Arrange
        var commentId = 1;
        var employeeId = 123;
        var comment = new ProjectComment
        {
            Id = commentId,
            ProjectId = 1,
            EmployeeId = employeeId,
            Comment = "Test comment"
        };

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(comment);
        _mockCommentRepository.Setup(r => r.DeleteAsync(comment))
            .Returns(Task.CompletedTask);
        _mockCommentRepository.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteCommentAsync(commentId, employeeId);

        // Assert
        Assert.True(result);
        _mockCommentRepository.Verify(r => r.DeleteAsync(comment), Times.Once);
        _mockCommentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentAsync_InvalidCommentId_ReturnsFalse()
    {
        // Arrange
        var commentId = 999;
        var employeeId = 123;

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync((ProjectComment)null);

        // Act
        var result = await _service.DeleteCommentAsync(commentId, employeeId);

        // Assert
        Assert.False(result);
        _mockCommentRepository.Verify(r => r.DeleteAsync(It.IsAny<ProjectComment>()), Times.Never);
        _mockCommentRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteCommentAsync_NotOwner_ReturnsFalse()
    {
        // Arrange
        var commentId = 1;
        var employeeId = 123;
        var comment = new ProjectComment
        {
            Id = commentId,
            ProjectId = 1,
            EmployeeId = 456, // Different employee
            Comment = "Test comment"
        };

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        // Act
        var result = await _service.DeleteCommentAsync(commentId, employeeId);

        // Assert
        Assert.False(result);
        _mockCommentRepository.Verify(r => r.DeleteAsync(It.IsAny<ProjectComment>()), Times.Never);
        _mockCommentRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task LogProjectActivityAsync_ValidData_LogsActivity()
    {
        // Arrange
        var projectId = 1;
        var employeeId = 123;
        var activityType = "Comment";
        var description = "Added comment to project";
        var details = "Comment details";

        var savedActivity = new ProjectActivity
        {
            Id = 1,
            ProjectId = projectId,
            EmployeeId = employeeId,
            ActivityType = activityType,
            Description = description,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };

        _mockActivityRepository.Setup(r => r.AddAsync(It.IsAny<ProjectActivity>()))
            .Returns(Task.CompletedTask);
        _mockActivityRepository.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _mockActivityRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(savedActivity);

        var expectedDto = new ProjectActivityDto
        {
            Id = 1,
            ProjectId = projectId,
            EmployeeId = employeeId,
            ActivityType = activityType,
            Description = description,
            Details = details
        };

        _mockMapper.Setup(m => m.Map<ProjectActivityDto>(savedActivity))
            .Returns(expectedDto);

        // Mock SignalR
        var mockClients = new Mock<IHubCallerClients>();
        var mockGroup = new Mock<IClientProxy>();
        _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockGroup.Object);

        // Act
        var result = await _service.LogProjectActivityAsync(projectId, employeeId, activityType, description, details);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projectId, result.ProjectId);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(activityType, result.ActivityType);
        Assert.Equal(description, result.Description);
        Assert.Equal(details, result.Details);

        _mockActivityRepository.Verify(r => r.AddAsync(It.IsAny<ProjectActivity>()), Times.Once);
        _mockActivityRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetProjectCommunicationStatsAsync_ValidProjectId_ReturnsStats()
    {
        // Arrange
        var projectId = 1;
        var comments = new List<ProjectComment>
        {
            new ProjectComment { Id = 1, ProjectId = projectId, EmployeeId = 1, CreatedAt = DateTime.UtcNow },
            new ProjectComment { Id = 2, ProjectId = projectId, EmployeeId = 2, CreatedAt = DateTime.UtcNow }
        };

        var activities = new List<ProjectActivity>
        {
            new ProjectActivity { Id = 1, ProjectId = projectId, EmployeeId = 1, CreatedAt = DateTime.UtcNow },
            new ProjectActivity { Id = 2, ProjectId = projectId, EmployeeId = 2, CreatedAt = DateTime.UtcNow }
        };

        var teamMembers = new List<ProjectAssignment>
        {
            new ProjectAssignment { Id = 1, ProjectId = projectId, EmployeeId = 1 },
            new ProjectAssignment { Id = 2, ProjectId = projectId, EmployeeId = 2 }
        };

        _mockCommentRepository.Setup(r => r.GetProjectCommentsAsync(projectId))
            .ReturnsAsync(comments);
        _mockActivityRepository.Setup(r => r.GetProjectActivitiesAsync(projectId, 1000))
            .ReturnsAsync(activities);
        _mockAssignmentRepository.Setup(r => r.GetProjectTeamMembersAsync(projectId))
            .ReturnsAsync(teamMembers);

        // Act
        var result = await _service.GetProjectCommunicationStatsAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalComments);
        Assert.Equal(2, result.TotalActivities);
        Assert.Equal(2, result.MemberActivities.Count);
        Assert.True(result.LastActivity > DateTime.MinValue);
    }

    [Fact]
    public async Task GetTeamActivitiesAsync_ValidTeamLeaderId_ReturnsActivities()
    {
        // Arrange
        var teamLeaderId = 123;
        var projects = new List<Project>
        {
            new Project { Id = 1, Name = "Project 1" },
            new Project { Id = 2, Name = "Project 2" }
        };

        var activities = new List<ProjectActivity>
        {
            new ProjectActivity { Id = 1, ProjectId = 1, EmployeeId = 456, Description = "Activity 1" },
            new ProjectActivity { Id = 2, ProjectId = 2, EmployeeId = 789, Description = "Activity 2" }
        };

        var expectedDtos = new List<ProjectActivityDto>
        {
            new ProjectActivityDto { Id = 1, ProjectId = 1, EmployeeId = 456, Description = "Activity 1" },
            new ProjectActivityDto { Id = 2, ProjectId = 2, EmployeeId = 789, Description = "Activity 2" }
        };

        _mockProjectRepository.Setup(r => r.GetProjectsByTeamLeadAsync(teamLeaderId))
            .ReturnsAsync(projects);
        _mockActivityRepository.Setup(r => r.GetTeamActivitiesAsync(It.IsAny<List<int>>(), 100))
            .ReturnsAsync(activities);
        _mockMapper.Setup(m => m.Map<List<ProjectActivityDto>>(activities))
            .Returns(expectedDtos);

        // Act
        var result = await _service.GetTeamActivitiesAsync(teamLeaderId, 100);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(expectedDtos, result);
    }
}
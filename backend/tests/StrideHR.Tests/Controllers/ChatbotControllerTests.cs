using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Chatbot;
using System.Security.Claims;
using Xunit;

namespace StrideHR.Tests.Controllers;

public class ChatbotControllerTests
{
    private readonly Mock<IChatbotService> _mockChatbotService;
    private readonly Mock<IChatbotKnowledgeBaseService> _mockKnowledgeBaseService;
    private readonly Mock<IChatbotLearningService> _mockLearningService;
    private readonly ChatbotController _controller;

    public ChatbotControllerTests()
    {
        _mockChatbotService = new Mock<IChatbotService>();
        _mockKnowledgeBaseService = new Mock<IChatbotKnowledgeBaseService>();
        _mockLearningService = new Mock<IChatbotLearningService>();

        _controller = new ChatbotController(
            _mockChatbotService.Object,
            _mockKnowledgeBaseService.Object,
            _mockLearningService.Object);

        // Setup user context
        var claims = new List<Claim>
        {
            new("EmployeeId", "1"),
            new(ClaimTypes.Name, "Test User")
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
    public async Task StartConversation_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var expectedConversation = new ChatbotConversationDto
        {
            Id = 1,
            SessionId = "test-session-id",
            EmployeeId = 1,
            Status = ChatbotConversationStatus.Active
        };

        _mockChatbotService
            .Setup(s => s.StartConversationAsync(1))
            .ReturnsAsync(expectedConversation);

        // Act
        var result = await _controller.StartConversation();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockChatbotService.Verify(s => s.StartConversationAsync(1), Times.Once);
    }

    [Fact]
    public async Task SendMessage_ValidMessage_ReturnsResponse()
    {
        // Arrange
        var dto = new SendMessageDto
        {
            SessionId = "test-session-id",
            Message = "Hello, I need help",
            MessageType = ChatbotMessageType.Text
        };

        var expectedResponse = new ChatbotResponseDto
        {
            SessionId = dto.SessionId,
            Response = "Hello! How can I help you today?",
            Intent = "greeting",
            ConfidenceScore = 0.9m
        };

        _mockChatbotService
            .Setup(s => s.ProcessMessageAsync(dto, 1))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.SendMessage(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockChatbotService.Verify(s => s.ProcessMessageAsync(dto, 1), Times.Once);
    }

    [Fact]
    public async Task GetConversation_ValidSessionId_ReturnsConversation()
    {
        // Arrange
        var sessionId = "test-session-id";
        var expectedConversation = new ChatbotConversationDto
        {
            Id = 1,
            SessionId = sessionId,
            EmployeeId = 1,
            Status = ChatbotConversationStatus.Active
        };

        _mockChatbotService
            .Setup(s => s.GetConversationAsync(sessionId))
            .ReturnsAsync(expectedConversation);

        // Act
        var result = await _controller.GetConversation(sessionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockChatbotService.Verify(s => s.GetConversationAsync(sessionId), Times.Once);
    }

    [Fact]
    public async Task EndConversation_ValidRequest_ReturnsUpdatedConversation()
    {
        // Arrange
        var sessionId = "test-session-id";
        var dto = new EndConversationDto
        {
            SatisfactionRating = 4,
            FeedbackComments = "Very helpful"
        };

        var expectedConversation = new ChatbotConversationDto
        {
            Id = 1,
            SessionId = sessionId,
            Status = ChatbotConversationStatus.Completed,
            SatisfactionRating = 4,
            FeedbackComments = "Very helpful"
        };

        _mockChatbotService
            .Setup(s => s.EndConversationAsync(sessionId, dto.SatisfactionRating, dto.FeedbackComments))
            .ReturnsAsync(expectedConversation);

        // Act
        var result = await _controller.EndConversation(sessionId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockChatbotService.Verify(s => s.EndConversationAsync(sessionId, dto.SatisfactionRating, dto.FeedbackComments), Times.Once);
    }

    [Fact]
    public async Task EscalateToHuman_ValidRequest_ReturnsEscalatedConversation()
    {
        // Arrange
        var dto = new EscalateToHumanDto
        {
            SessionId = "test-session-id",
            Reason = "Complex issue requiring human assistance",
            PreferredAgentId = 5
        };

        var expectedConversation = new ChatbotConversationDto
        {
            Id = 1,
            SessionId = dto.SessionId,
            Status = ChatbotConversationStatus.EscalatedToHuman,
            EscalatedToHuman = true,
            EscalationReason = dto.Reason
        };

        _mockChatbotService
            .Setup(s => s.EscalateToHumanAsync(dto, 1))
            .ReturnsAsync(expectedConversation);

        // Act
        var result = await _controller.EscalateToHuman(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockChatbotService.Verify(s => s.EscalateToHumanAsync(dto, 1), Times.Once);
    }

    [Fact]
    public async Task GetMyConversations_ValidRequest_ReturnsConversations()
    {
        // Arrange
        var page = 1;
        var pageSize = 20;
        var expectedConversations = new List<ChatbotConversationDto>
        {
            new() { Id = 1, SessionId = "session-1", EmployeeId = 1 },
            new() { Id = 2, SessionId = "session-2", EmployeeId = 1 }
        };

        _mockChatbotService
            .Setup(s => s.GetEmployeeConversationsAsync(1, page, pageSize))
            .ReturnsAsync(expectedConversations);

        // Act
        var result = await _controller.GetMyConversations(page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockChatbotService.Verify(s => s.GetEmployeeConversationsAsync(1, page, pageSize), Times.Once);
    }

    [Fact]
    public async Task SearchKnowledgeBase_ValidSearchTerm_ReturnsArticles()
    {
        // Arrange
        var searchTerm = "leave request";
        var expectedArticles = new List<KnowledgeBaseDto>
        {
            new() { Id = 1, Title = "How to Request Leave", Category = "Leave Management" },
            new() { Id = 2, Title = "Leave Policies", Category = "Leave Management" }
        };

        _mockKnowledgeBaseService
            .Setup(s => s.SearchArticlesAsync(searchTerm))
            .ReturnsAsync(expectedArticles);

        // Act
        var result = await _controller.SearchKnowledgeBase(searchTerm);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.SearchArticlesAsync(searchTerm), Times.Once);
    }

    [Fact]
    public async Task SearchKnowledgeBase_EmptySearchTerm_ReturnsBadRequest()
    {
        // Arrange
        var searchTerm = "";

        // Act
        var result = await _controller.SearchKnowledgeBase(searchTerm);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.SearchArticlesAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetKnowledgeBaseByCategory_ValidCategory_ReturnsArticles()
    {
        // Arrange
        var category = "Leave Management";
        var expectedArticles = new List<KnowledgeBaseDto>
        {
            new() { Id = 1, Title = "Leave Request Process", Category = category },
            new() { Id = 2, Title = "Leave Balance Inquiry", Category = category }
        };

        _mockKnowledgeBaseService
            .Setup(s => s.GetArticlesByCategoryAsync(category))
            .ReturnsAsync(expectedArticles);

        // Act
        var result = await _controller.GetKnowledgeBaseByCategory(category);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.GetArticlesByCategoryAsync(category), Times.Once);
    }

    [Fact]
    public async Task ProvideArticleFeedback_ValidFeedback_ReturnsSuccess()
    {
        // Arrange
        var articleId = 1;
        var dto = new ArticleFeedbackDto
        {
            IsHelpful = true,
            Comments = "Very helpful article"
        };

        _mockKnowledgeBaseService
            .Setup(s => s.ProvideArticleFeedbackAsync(articleId, 1, dto.IsHelpful, dto.Comments))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ProvideArticleFeedback(articleId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.ProvideArticleFeedbackAsync(articleId, 1, dto.IsHelpful, dto.Comments), Times.Once);
    }

    [Fact]
    public async Task ProvideArticleFeedback_FailedFeedback_ReturnsBadRequest()
    {
        // Arrange
        var articleId = 999; // Non-existent article
        var dto = new ArticleFeedbackDto
        {
            IsHelpful = true,
            Comments = "Test comment"
        };

        _mockKnowledgeBaseService
            .Setup(s => s.ProvideArticleFeedbackAsync(articleId, 1, dto.IsHelpful, dto.Comments))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ProvideArticleFeedback(articleId, dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetAnalytics_ValidRequest_ReturnsAnalytics()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddDays(-30);
        var toDate = DateTime.UtcNow;
        var expectedAnalytics = new ChatbotAnalyticsDto
        {
            TotalConversations = 100,
            ActiveConversations = 5,
            CompletedConversations = 90,
            EscalatedConversations = 5,
            EscalationRate = 5.0m,
            AverageConversationDuration = 15.5m
        };

        _mockChatbotService
            .Setup(s => s.GetAnalyticsAsync(fromDate, toDate))
            .ReturnsAsync(expectedAnalytics);

        // Act
        var result = await _controller.GetAnalytics(fromDate, toDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockChatbotService.Verify(s => s.GetAnalyticsAsync(fromDate, toDate), Times.Once);
    }
}
using AutoMapper;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Chatbot;
using StrideHR.Core.Models.Notification;
using StrideHR.Infrastructure.Services;
using System.Security.Claims;
using Xunit;

namespace StrideHR.Tests.Services;

public class ChatbotServiceTests
{
    private readonly Mock<IChatbotConversationRepository> _mockConversationRepository;
    private readonly Mock<IChatbotMessageRepository> _mockMessageRepository;
    private readonly Mock<INaturalLanguageProcessingService> _mockNlpService;
    private readonly Mock<IChatbotKnowledgeBaseService> _mockKnowledgeBaseService;
    private readonly Mock<IChatbotLearningService> _mockLearningService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ChatbotService _service;

    public ChatbotServiceTests()
    {
        _mockConversationRepository = new Mock<IChatbotConversationRepository>();
        _mockMessageRepository = new Mock<IChatbotMessageRepository>();
        _mockNlpService = new Mock<INaturalLanguageProcessingService>();
        _mockKnowledgeBaseService = new Mock<IChatbotKnowledgeBaseService>();
        _mockLearningService = new Mock<IChatbotLearningService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();

        _service = new ChatbotService(
            _mockConversationRepository.Object,
            _mockMessageRepository.Object,
            _mockNlpService.Object,
            _mockKnowledgeBaseService.Object,
            _mockLearningService.Object,
            _mockNotificationService.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task StartConversationAsync_NewConversation_ReturnsConversationDto()
    {
        // Arrange
        var employeeId = 1;
        var expectedConversation = new ChatbotConversation
        {
            Id = 1,
            SessionId = "test-session-id",
            EmployeeId = employeeId,
            Status = ChatbotConversationStatus.Active,
            StartedAt = DateTime.UtcNow
        };

        var expectedDto = new ChatbotConversationDto
        {
            Id = 1,
            SessionId = "test-session-id",
            EmployeeId = employeeId,
            Status = ChatbotConversationStatus.Active
        };

        _mockConversationRepository
            .Setup(r => r.GetActiveConversationByEmployeeAsync(employeeId))
            .ReturnsAsync((ChatbotConversation?)null);

        _mockConversationRepository
            .Setup(r => r.AddAsync(It.IsAny<ChatbotConversation>()))
            .Returns(Task.FromResult(expectedConversation));

        _mockMessageRepository
            .Setup(r => r.AddAsync(It.IsAny<ChatbotMessage>()))
            .Returns(Task.FromResult(new ChatbotMessage()));

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper
            .Setup(m => m.Map<ChatbotConversationDto>(It.IsAny<ChatbotConversation>()))
            .Returns(expectedDto);

        _mockMapper
            .Setup(m => m.Map<ChatbotMessageDto>(It.IsAny<ChatbotMessage>()))
            .Returns(new ChatbotMessageDto());

        // Act
        var result = await _service.StartConversationAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(ChatbotConversationStatus.Active, result.Status);
        _mockConversationRepository.Verify(r => r.AddAsync(It.IsAny<ChatbotConversation>()), Times.Once);
        _mockMessageRepository.Verify(r => r.AddAsync(It.IsAny<ChatbotMessage>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_ValidMessage_ReturnsResponse()
    {
        // Arrange
        var dto = new SendMessageDto
        {
            SessionId = "test-session-id",
            Message = "Hello, I need help with leave request",
            MessageType = ChatbotMessageType.Text
        };

        var employeeId = 1;
        var intent = "leave_request";
        var confidenceScore = 0.85m;
        var entities = new Dictionary<string, object> { { "leave_type", "vacation" } };

        var conversation = new ChatbotConversation
        {
            Id = 1,
            SessionId = dto.SessionId,
            EmployeeId = employeeId,
            Status = ChatbotConversationStatus.Active
        };

        _mockConversationRepository
            .Setup(r => r.GetBySessionIdAsync(dto.SessionId))
            .ReturnsAsync(conversation);

        _mockMessageRepository
            .Setup(r => r.AddAsync(It.IsAny<ChatbotMessage>()))
            .Returns(Task.FromResult(new ChatbotMessage()));

        _mockNlpService
            .Setup(n => n.DetectIntentAsync(dto.Message, dto.Context))
            .ReturnsAsync(intent);

        _mockNlpService
            .Setup(n => n.ExtractEntitiesAsync(dto.Message))
            .ReturnsAsync(entities);

        _mockNlpService
            .Setup(n => n.GetConfidenceScoreAsync(dto.Message, intent))
            .ReturnsAsync(confidenceScore);

        _mockNlpService
            .Setup(n => n.ShouldEscalateAsync(dto.Message, intent, confidenceScore))
            .ReturnsAsync(false);

        _mockNlpService
            .Setup(n => n.GenerateResponseAsync(intent, entities, dto.Context))
            .ReturnsAsync("I can help you with leave requests. What type of leave do you need?");

        _mockNlpService
            .Setup(n => n.GetSuggestedResponsesAsync(intent))
            .ReturnsAsync(new List<string> { "Check leave balance", "Request leave", "View leave policy" });

        _mockKnowledgeBaseService
            .Setup(k => k.GetRelevantArticlesAsync(intent, entities))
            .ReturnsAsync(new List<KnowledgeBaseDto>());

        _mockLearningService
            .Setup(l => l.RecordInteractionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.ProcessMessageAsync(dto, employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.SessionId, result.SessionId);
        Assert.Equal(intent, result.Intent);
        Assert.Equal(confidenceScore, result.ConfidenceScore);
        Assert.False(result.ShouldEscalate);
        Assert.NotNull(result.SuggestedResponses);
        Assert.Contains("Check leave balance", result.SuggestedResponses);
    }

    [Fact]
    public async Task ProcessMessageAsync_LowConfidence_ShouldEscalate()
    {
        // Arrange
        var dto = new SendMessageDto
        {
            SessionId = "test-session-id",
            Message = "I have a complex issue",
            MessageType = ChatbotMessageType.Text
        };

        var employeeId = 1;
        var intent = "general_inquiry";
        var confidenceScore = 0.3m; // Low confidence

        var conversation = new ChatbotConversation
        {
            Id = 1,
            SessionId = dto.SessionId,
            EmployeeId = employeeId,
            Status = ChatbotConversationStatus.Active
        };

        _mockConversationRepository
            .Setup(r => r.GetBySessionIdAsync(dto.SessionId))
            .ReturnsAsync(conversation);

        _mockMessageRepository
            .Setup(r => r.AddAsync(It.IsAny<ChatbotMessage>()))
            .Returns(Task.FromResult(new ChatbotMessage()));

        _mockNlpService
            .Setup(n => n.DetectIntentAsync(dto.Message, dto.Context))
            .ReturnsAsync(intent);

        _mockNlpService
            .Setup(n => n.ExtractEntitiesAsync(dto.Message))
            .ReturnsAsync(new Dictionary<string, object>());

        _mockNlpService
            .Setup(n => n.GetConfidenceScoreAsync(dto.Message, intent))
            .ReturnsAsync(confidenceScore);

        _mockNlpService
            .Setup(n => n.ShouldEscalateAsync(dto.Message, intent, confidenceScore))
            .ReturnsAsync(true); // Should escalate due to low confidence

        _mockLearningService
            .Setup(l => l.RecordInteractionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.ProcessMessageAsync(dto, employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ShouldEscalate);
        Assert.NotNull(result.EscalationReason);
        Assert.Contains("human agent", result.Response.ToLower());
    }

    [Fact]
    public async Task EscalateToHumanAsync_ValidRequest_UpdatesConversation()
    {
        // Arrange
        var dto = new EscalateToHumanDto
        {
            SessionId = "test-session-id",
            Reason = "Complex payroll issue",
            PreferredAgentId = 5
        };

        var employeeId = 1;
        var conversation = new ChatbotConversation
        {
            Id = 1,
            SessionId = dto.SessionId,
            EmployeeId = employeeId,
            Status = ChatbotConversationStatus.Active,
            Employee = new Employee { FirstName = "John", LastName = "Doe" }
        };

        var expectedDto = new ChatbotConversationDto
        {
            Id = 1,
            SessionId = dto.SessionId,
            Status = ChatbotConversationStatus.EscalatedToHuman,
            EscalatedToHuman = true
        };

        _mockConversationRepository
            .Setup(r => r.GetBySessionIdAsync(dto.SessionId))
            .ReturnsAsync(conversation);

        _mockMessageRepository
            .Setup(r => r.AddAsync(It.IsAny<ChatbotMessage>()))
            .Returns(Task.FromResult(new ChatbotMessage()));

        _mockNotificationService
            .Setup(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>()))
            .ReturnsAsync(new NotificationDto());

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper
            .Setup(m => m.Map<ChatbotConversationDto>(It.IsAny<ChatbotConversation>()))
            .Returns(expectedDto);

        // Act
        var result = await _service.EscalateToHumanAsync(dto, employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ChatbotConversationStatus.EscalatedToHuman, result.Status);
        Assert.True(result.EscalatedToHuman);
        
        // Verify conversation was updated
        Assert.Equal(ChatbotConversationStatus.EscalatedToHuman, conversation.Status);
        Assert.True(conversation.EscalatedToHuman);
        Assert.Equal(dto.Reason, conversation.EscalationReason);
        Assert.Equal(dto.PreferredAgentId, conversation.EscalatedToEmployeeId);

        // Verify notification was sent
        _mockNotificationService.Verify(
            n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>()),
            Times.Once);
    }

    [Fact]
    public async Task EndConversationAsync_ValidRequest_UpdatesConversation()
    {
        // Arrange
        var sessionId = "test-session-id";
        var satisfactionRating = 4;
        var feedbackComments = "Very helpful";

        var conversation = new ChatbotConversation
        {
            Id = 1,
            SessionId = sessionId,
            Status = ChatbotConversationStatus.Active
        };

        var expectedDto = new ChatbotConversationDto
        {
            Id = 1,
            SessionId = sessionId,
            Status = ChatbotConversationStatus.Completed
        };

        _mockConversationRepository
            .Setup(r => r.GetBySessionIdAsync(sessionId))
            .ReturnsAsync(conversation);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper
            .Setup(m => m.Map<ChatbotConversationDto>(It.IsAny<ChatbotConversation>()))
            .Returns(expectedDto);

        // Act
        var result = await _service.EndConversationAsync(sessionId, satisfactionRating, feedbackComments);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ChatbotConversationStatus.Completed, result.Status);
        
        // Verify conversation was updated
        Assert.Equal(ChatbotConversationStatus.Completed, conversation.Status);
        Assert.NotNull(conversation.EndedAt);
        Assert.Equal(satisfactionRating, conversation.SatisfactionRating);
        Assert.Equal(feedbackComments, conversation.FeedbackComments);
    }

    [Fact]
    public async Task GetConversationAsync_InvalidSessionId_ThrowsException()
    {
        // Arrange
        var sessionId = "invalid-session-id";

        _mockConversationRepository
            .Setup(r => r.GetBySessionIdAsync(sessionId))
            .ReturnsAsync((ChatbotConversation?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetConversationAsync(sessionId));
    }
}
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Chatbot;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class ChatbotLearningServiceTests
{
    private readonly Mock<IChatbotLearningDataRepository> _mockLearningDataRepository;
    private readonly Mock<IChatbotMessageRepository> _mockMessageRepository;
    private readonly Mock<INaturalLanguageProcessingService> _mockNlpService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly ChatbotLearningService _service;

    public ChatbotLearningServiceTests()
    {
        _mockLearningDataRepository = new Mock<IChatbotLearningDataRepository>();
        _mockMessageRepository = new Mock<IChatbotMessageRepository>();
        _mockNlpService = new Mock<INaturalLanguageProcessingService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _service = new ChatbotLearningService(
            _mockLearningDataRepository.Object,
            _mockMessageRepository.Object,
            _mockNlpService.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task RecordInteractionAsync_ValidData_CreatesLearningData()
    {
        // Arrange
        var userInput = "I need help with leave request";
        var botResponse = "I can help you with leave requests. What type of leave do you need?";
        var intent = "leave_request";
        var confidenceScore = 0.85m;
        var employeeId = 1;
        var sessionId = "test-session-id";

        _mockLearningDataRepository
            .Setup(r => r.AddAsync(It.IsAny<ChatbotLearningData>()))
            .Returns(Task.FromResult(new ChatbotLearningData()));

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _service.RecordInteractionAsync(userInput, botResponse, intent, confidenceScore, employeeId, sessionId);

        // Assert
        _mockLearningDataRepository.Verify(r => r.AddAsync(It.Is<ChatbotLearningData>(ld =>
            ld.UserInput == userInput &&
            ld.BotResponse == botResponse &&
            ld.Intent == intent &&
            ld.ConfidenceScore == confidenceScore &&
            ld.EmployeeId == employeeId &&
            ld.SessionId == sessionId &&
            ld.WasHelpful == true &&
            ld.IsTrainingData == false
        )), Times.Once);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RecordFeedbackAsync_ValidSessionId_UpdatesLearningData()
    {
        // Arrange
        var sessionId = "test-session-id";
        var wasHelpful = false;
        var userFeedback = "Not helpful";
        var correctResponse = "The correct response should be...";

        var learningDataList = new List<ChatbotLearningData>
        {
            new() { Id = 1, SessionId = sessionId, WasHelpful = true },
            new() { Id = 2, SessionId = sessionId, WasHelpful = true }
        };

        _mockLearningDataRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(learningDataList);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _service.RecordFeedbackAsync(sessionId, wasHelpful, userFeedback, correctResponse);

        // Assert
        Assert.All(learningDataList, ld =>
        {
            Assert.Equal(wasHelpful, ld.WasHelpful);
            Assert.Equal(userFeedback, ld.UserFeedback);
            Assert.Equal(correctResponse, ld.CorrectResponse);
        });

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ProcessLearningDataAsync_ValidData_ProcessesCorrectly()
    {
        // Arrange
        var unprocessedData = new List<ChatbotLearningData>
        {
            new() { Id = 1, ConfidenceScore = 0.9m, WasHelpful = true, IsTrainingData = false },
            new() { Id = 2, ConfidenceScore = 0.5m, WasHelpful = true, IsTrainingData = false },
            new() { Id = 3, ConfidenceScore = 0.85m, WasHelpful = true, IsTrainingData = false }
        };

        _mockLearningDataRepository
            .Setup(r => r.GetUnprocessedLearningDataAsync())
            .ReturnsAsync(unprocessedData);

        _mockLearningDataRepository
            .Setup(r => r.MarkAsProcessedAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _service.ProcessLearningDataAsync();

        // Assert
        // High confidence (0.9) and helpful should be marked as training data
        Assert.True(unprocessedData[0].IsTrainingData);
        Assert.True(unprocessedData[2].IsTrainingData);
        
        // Low confidence (0.5) should not be marked as training data
        Assert.False(unprocessedData[1].IsTrainingData);

        // All should be marked as processed
        _mockLearningDataRepository.Verify(r => r.MarkAsProcessedAsync(1), Times.Once);
        _mockLearningDataRepository.Verify(r => r.MarkAsProcessedAsync(3), Times.Once);
        // Low confidence item should not be processed (continues to next iteration)
        _mockLearningDataRepository.Verify(r => r.MarkAsProcessedAsync(2), Times.Never);
    }

    [Fact]
    public async Task GetLowConfidenceInteractionsAsync_CallsRepository_ReturnsData()
    {
        // Arrange
        var threshold = 0.7m;
        var expectedData = new List<ChatbotLearningData>
        {
            new() { Id = 1, ConfidenceScore = 0.5m },
            new() { Id = 2, ConfidenceScore = 0.6m }
        };

        _mockLearningDataRepository
            .Setup(r => r.GetLowConfidenceInteractionsAsync(threshold))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _service.GetLowConfidenceInteractionsAsync(threshold);

        // Assert
        Assert.Equal(expectedData, result);
        _mockLearningDataRepository.Verify(r => r.GetLowConfidenceInteractionsAsync(threshold), Times.Once);
    }

    [Fact]
    public async Task GetUnhelpfulInteractionsAsync_CallsRepository_ReturnsData()
    {
        // Arrange
        var expectedData = new List<ChatbotLearningData>
        {
            new() { Id = 1, WasHelpful = false },
            new() { Id = 2, WasHelpful = false }
        };

        _mockLearningDataRepository
            .Setup(r => r.GetUnhelpfulInteractionsAsync())
            .ReturnsAsync(expectedData);

        // Act
        var result = await _service.GetUnhelpfulInteractionsAsync();

        // Assert
        Assert.Equal(expectedData, result);
        _mockLearningDataRepository.Verify(r => r.GetUnhelpfulInteractionsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetIntentAccuracyMetricsAsync_CalculatesCorrectMetrics()
    {
        // Arrange
        var allData = new List<ChatbotLearningData>
        {
            new() { Intent = "greeting", WasHelpful = true },
            new() { Intent = "greeting", WasHelpful = true },
            new() { Intent = "greeting", WasHelpful = false },
            new() { Intent = "leave_request", WasHelpful = true },
            new() { Intent = "leave_request", WasHelpful = false }
        };

        _mockLearningDataRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(allData);

        // Act
        var result = await _service.GetIntentAccuracyMetricsAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(2m/3m, result["greeting"]); // 2 helpful out of 3 total
        Assert.Equal(0.5m, result["leave_request"]); // 1 helpful out of 2 total
    }

    [Fact]
    public async Task GetImprovementSuggestionsAsync_GeneratesRelevantSuggestions()
    {
        // Arrange
        var lowConfidenceData = new List<ChatbotLearningData>
        {
            new() { Intent = "leave_request", ConfidenceScore = 0.5m },
            new() { Intent = "leave_request", ConfidenceScore = 0.4m },
            new() { Intent = "payroll_inquiry", ConfidenceScore = 0.3m }
        };

        var unhelpfulData = new List<ChatbotLearningData>
        {
            new() { Intent = "greeting", WasHelpful = false },
            new() { Intent = "greeting", WasHelpful = false }
        };

        var intentStats = new Dictionary<string, int>
        {
            { "leave_request", 50 },
            { "payroll_inquiry", 30 },
            { "greeting", 25 }
        };

        _mockLearningDataRepository
            .Setup(r => r.GetLowConfidenceInteractionsAsync(0.7m))
            .ReturnsAsync(lowConfidenceData);

        _mockLearningDataRepository
            .Setup(r => r.GetUnhelpfulInteractionsAsync())
            .ReturnsAsync(unhelpfulData);

        _mockMessageRepository
            .Setup(r => r.GetIntentStatisticsAsync(null, null))
            .ReturnsAsync(intentStats);

        // Act
        var result = await _service.GetImprovementSuggestionsAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(result, s => s.Contains("leave_request") && s.Contains("low confidence"));
        Assert.Contains(result, s => s.Contains("greeting") && s.Contains("unhelpful"));
        Assert.Contains(result, s => s.Contains("knowledge base"));
    }

    [Fact]
    public async Task RetrainModelAsync_CallsNlpServiceAndMarksDataProcessed()
    {
        // Arrange
        var trainingData = new List<ChatbotLearningData>
        {
            new() { Id = 1, UserInput = "Hello", Intent = "greeting", IsTrainingData = true },
            new() { Id = 2, UserInput = "I need leave", Intent = "leave_request", IsTrainingData = true }
        };

        var unprocessedData = new List<ChatbotLearningData>
        {
            new() { Id = 3, ProcessedAt = null },
            new() { Id = 4, ProcessedAt = null }
        };

        _mockLearningDataRepository
            .Setup(r => r.GetTrainingDataAsync())
            .ReturnsAsync(trainingData);

        _mockLearningDataRepository
            .Setup(r => r.GetUnprocessedLearningDataAsync())
            .ReturnsAsync(unprocessedData);

        _mockNlpService
            .Setup(n => n.TrainModelAsync(trainingData))
            .Returns(Task.CompletedTask);

        _mockLearningDataRepository
            .Setup(r => r.MarkAsProcessedAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _service.RetrainModelAsync();

        // Assert
        _mockNlpService.Verify(n => n.TrainModelAsync(trainingData), Times.Once);
        _mockLearningDataRepository.Verify(r => r.MarkAsProcessedAsync(3), Times.Once);
        _mockLearningDataRepository.Verify(r => r.MarkAsProcessedAsync(4), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
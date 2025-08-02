using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class NaturalLanguageProcessingServiceTests
{
    private readonly NaturalLanguageProcessingService _service;

    public NaturalLanguageProcessingServiceTests()
    {
        _service = new NaturalLanguageProcessingService();
    }

    [Theory]
    [InlineData("Hi there", "greeting")]
    [InlineData("Hello, how are you?", "greeting")]
    [InlineData("Good morning", "greeting")]
    [InlineData("I need to request leave", "leave_request")]
    [InlineData("I want to apply for vacation", "leave_request")]
    [InlineData("What is my leave balance?", "leave_balance")]
    [InlineData("How many vacation days do I have left?", "leave_balance")]
    [InlineData("Where is my payslip?", "payroll_inquiry")]
    [InlineData("I need help with my salary", "payroll_inquiry")]
    [InlineData("Check my attendance", "attendance_query")]
    [InlineData("I can't check in", "attendance_query")]
    public async Task DetectIntentAsync_ValidMessages_ReturnsCorrectIntent(string message, string expectedIntent)
    {
        // Act
        var result = await _service.DetectIntentAsync(message);

        // Assert
        Assert.Equal(expectedIntent, result);
    }

    [Fact]
    public async Task DetectIntentAsync_UnrecognizedMessage_ReturnsGeneralInquiry()
    {
        // Arrange
        var message = "xyz random text that doesn't match any pattern";

        // Act
        var result = await _service.DetectIntentAsync(message);

        // Assert
        Assert.Equal("general_inquiry", result);
    }

    [Theory]
    [InlineData("I need leave on 2025-01-15", "date", "2025-01-15")]
    [InlineData("I need 5 days off", "number", "5")]
    [InlineData("Contact me at john.doe@example.com", "email", "john.doe@example.com")]
    [InlineData("My employee ID is NYC-HR-2025-001", "employee_id", "NYC-HR-2025-001")]
    public async Task ExtractEntitiesAsync_ValidMessages_ExtractsCorrectEntities(string message, string entityType, string expectedValue)
    {
        // Act
        var result = await _service.ExtractEntitiesAsync(message);

        // Assert
        Assert.True(result.ContainsKey(entityType));
        Assert.Equal(expectedValue, result[entityType].ToString());
    }

    [Fact]
    public async Task ExtractEntitiesAsync_NoEntities_ReturnsEmptyDictionary()
    {
        // Arrange
        var message = "Hello there";

        // Act
        var result = await _service.ExtractEntitiesAsync(message);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("I need to request leave", "leave_request", 0.5, 1.0)]
    [InlineData("Hello", "greeting", 0.5, 1.0)]
    [InlineData("Random text", "leave_request", 0.0, 0.5)]
    public async Task GetConfidenceScoreAsync_ValidInputs_ReturnsScoreInRange(string message, string intent, decimal minExpected, decimal maxExpected)
    {
        // Act
        var result = await _service.GetConfidenceScoreAsync(message, intent);

        // Assert
        Assert.True(result >= minExpected && result <= maxExpected, 
            $"Confidence score {result} should be between {minExpected} and {maxExpected}");
    }

    [Theory]
    [InlineData("greeting")]
    [InlineData("leave_request")]
    [InlineData("payroll_inquiry")]
    [InlineData("attendance_query")]
    public async Task GenerateResponseAsync_ValidIntents_ReturnsNonEmptyResponse(string intent)
    {
        // Act
        var result = await _service.GenerateResponseAsync(intent);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GenerateResponseAsync_InvalidIntent_ReturnsDefaultResponse()
    {
        // Arrange
        var intent = "invalid_intent";

        // Act
        var result = await _service.GenerateResponseAsync(intent);

        // Assert
        Assert.Contains("didn't understand", result);
    }

    [Theory]
    [InlineData("greeting")]
    [InlineData("leave_request")]
    [InlineData("payroll_inquiry")]
    public async Task GetSuggestedResponsesAsync_ValidIntents_ReturnsSuggestions(string intent)
    {
        // Act
        var result = await _service.GetSuggestedResponsesAsync(intent);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("I need urgent help", "urgent_issue", 0.9, true)]
    [InlineData("This is an emergency", "general_inquiry", 0.9, true)]
    [InlineData("I want to speak to a manager", "general_inquiry", 0.9, true)]
    [InlineData("Hello there", "greeting", 0.9, false)]
    [InlineData("What is my leave balance?", "leave_balance", 0.3, true)] // Low confidence
    public async Task ShouldEscalateAsync_VariousScenarios_ReturnsCorrectDecision(string message, string intent, decimal confidenceScore, bool expectedEscalation)
    {
        // Act
        var result = await _service.ShouldEscalateAsync(message, intent, confidenceScore);

        // Assert
        Assert.Equal(expectedEscalation, result);
    }

    [Fact]
    public async Task IsModelTrainedAsync_Always_ReturnsTrue()
    {
        // Act
        var result = await _service.IsModelTrainedAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task TrainModelAsync_ValidTrainingData_CompletesSuccessfully()
    {
        // Arrange
        var trainingData = new List<StrideHR.Core.Entities.ChatbotLearningData>
        {
            new() { UserInput = "Hello", Intent = "greeting", ConfidenceScore = 0.9m },
            new() { UserInput = "I need leave", Intent = "leave_request", ConfidenceScore = 0.8m }
        };

        // Act & Assert - Should not throw
        await _service.TrainModelAsync(trainingData);
    }
}
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class SurveyAnalyticsServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<INaturalLanguageProcessingService> _mockNlpService;
    private readonly Mock<ILogger<SurveyAnalyticsService>> _mockLogger;
    private readonly SurveyAnalyticsService _analyticsService;

    public SurveyAnalyticsServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockNlpService = new Mock<INaturalLanguageProcessingService>();
        _mockLogger = new Mock<ILogger<SurveyAnalyticsService>>();

        _analyticsService = new SurveyAnalyticsService(
            _mockUnitOfWork.Object,
            _mockNlpService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_PositiveText_ReturnsPositiveSentiment()
    {
        // Arrange
        var text = "I love working here! The team is amazing and the environment is great.";
        
        _mockNlpService.Setup(s => s.AnalyzeSentimentAsync(text))
            .ThrowsAsync(new Exception("NLP service unavailable"));

        // Act
        var result = await _analyticsService.AnalyzeSentimentAsync(text);

        // Assert
        Assert.Equal(SentimentScore.Positive, result);
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_NegativeText_ReturnsNegativeSentiment()
    {
        // Arrange
        var text = "This is a terrible experience. I hate the poor management and bad policies.";
        
        _mockNlpService.Setup(s => s.AnalyzeSentimentAsync(text))
            .ThrowsAsync(new Exception("NLP service unavailable"));

        // Act
        var result = await _analyticsService.AnalyzeSentimentAsync(text);

        // Assert
        Assert.Equal(SentimentScore.Negative, result);
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_NeutralText_ReturnsNeutralSentiment()
    {
        // Arrange
        var text = "The office is located downtown. We have standard working hours.";

        // Act
        var result = await _analyticsService.AnalyzeSentimentAsync(text);

        // Assert
        Assert.Equal(SentimentScore.Neutral, result);
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_EmptyText_ReturnsNeutralSentiment()
    {
        // Arrange
        var text = "";

        // Act
        var result = await _analyticsService.AnalyzeSentimentAsync(text);

        // Assert
        Assert.Equal(SentimentScore.Neutral, result);
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_NlpServiceFails_FallsBackToBasicAnalysis()
    {
        // Arrange
        var text = "This is excellent work!";
        
        _mockNlpService.Setup(s => s.AnalyzeSentimentAsync(text))
            .ThrowsAsync(new Exception("NLP service unavailable"));

        // Act
        var result = await _analyticsService.AnalyzeSentimentAsync(text);

        // Assert
        Assert.Equal(SentimentScore.Positive, result); // Should fall back to basic analysis
    }

    [Fact]
    public async Task GenerateAnalyticsAsync_ValidSurvey_ReturnsAnalytics()
    {
        // Arrange
        var surveyId = 1;
        var survey = new Survey
        {
            Id = surveyId,
            Title = "Employee Satisfaction Survey",
            Questions = new List<SurveyQuestion>
            {
                new SurveyQuestion
                {
                    Id = 1,
                    QuestionText = "How satisfied are you?",
                    Type = QuestionType.Rating,
                    IsActive = true,
                    Options = new List<SurveyQuestionOption>
                    {
                        new SurveyQuestionOption { Id = 1, OptionText = "Very Satisfied", IsActive = true },
                        new SurveyQuestionOption { Id = 2, OptionText = "Satisfied", IsActive = true }
                    }
                }
            }
        };

        var responses = new List<SurveyResponse>
        {
            new SurveyResponse
            {
                Id = 1,
                SurveyId = surveyId,
                Status = SurveyResponseStatus.Completed,
                CompletedAt = DateTime.UtcNow,
                RespondentEmployee = new Employee { Department = "IT" },
                Answers = new List<SurveyAnswer>
                {
                    new SurveyAnswer
                    {
                        QuestionId = 1,
                        TextAnswer = "Great experience working here!",
                        Question = survey.Questions.First()
                    }
                }
            },
            new SurveyResponse
            {
                Id = 2,
                SurveyId = surveyId,
                Status = SurveyResponseStatus.Completed,
                CompletedAt = DateTime.UtcNow,
                RespondentEmployee = new Employee { Department = "HR" },
                Answers = new List<SurveyAnswer>
                {
                    new SurveyAnswer
                    {
                        QuestionId = 1,
                        TextAnswer = "Good workplace environment.",
                        Question = survey.Questions.First()
                    }
                }
            }
        };

        var distributions = new List<SurveyDistribution>
        {
            new SurveyDistribution { Id = 1, SurveyId = surveyId },
            new SurveyDistribution { Id = 2, SurveyId = surveyId },
            new SurveyDistribution { Id = 3, SurveyId = surveyId }
        };

        _mockUnitOfWork.Setup(u => u.Surveys.GetWithQuestionsAsync(surveyId))
            .ReturnsAsync(survey);

        _mockUnitOfWork.Setup(u => u.SurveyResponses.GetCompletedResponsesAsync(surveyId))
            .ReturnsAsync(responses);

        _mockUnitOfWork.Setup(u => u.SurveyDistributions.GetBySurveyAsync(surveyId))
            .ReturnsAsync(distributions);

        _mockUnitOfWork.Setup(u => u.SurveyResponses.GetAverageCompletionTimeAsync(surveyId))
            .ReturnsAsync(TimeSpan.FromMinutes(5));

        _mockUnitOfWork.Setup(u => u.SurveyAnalytics.AddAsync(It.IsAny<SurveyAnalytics>()))
            .ReturnsAsync(new SurveyAnalytics());

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _analyticsService.GenerateAnalyticsAsync(surveyId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(surveyId, result.SurveyId);
        Assert.Equal("Employee Satisfaction Survey", result.SurveyTitle);
        Assert.Equal(3, result.TotalDistributed);
        Assert.Equal(2, result.TotalResponses);
        Assert.Equal(2, result.CompletedResponses);
        Assert.Equal(66.67, Math.Round(result.ResponseRate, 2)); // 2/3 * 100
        Assert.Equal(100, result.CompletionRate); // All responses are completed
        Assert.Equal(TimeSpan.FromMinutes(5), result.AverageCompletionTime);
        Assert.Single(result.QuestionAnalytics);
        Assert.Equal(2, result.DemographicBreakdown.Count); // IT and HR departments
    }

    [Fact]
    public async Task GenerateAnalyticsAsync_NonExistentSurvey_ThrowsException()
    {
        // Arrange
        var surveyId = 999;

        _mockUnitOfWork.Setup(u => u.Surveys.GetWithQuestionsAsync(surveyId))
            .ReturnsAsync((Survey?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _analyticsService.GenerateAnalyticsAsync(surveyId));
    }

    [Fact]
    public async Task RefreshAnalyticsAsync_ValidSurvey_RefreshesSuccessfully()
    {
        // Arrange
        var surveyId = 1;
        var existingAnalytics = new List<SurveyAnalytics>
        {
            new SurveyAnalytics { Id = 1, SurveyId = surveyId, IsDeleted = false }
        };

        var survey = new Survey
        {
            Id = surveyId,
            Title = "Test Survey",
            Questions = new List<SurveyQuestion>()
        };

        _mockUnitOfWork.Setup(u => u.SurveyAnalytics.GetBySurveyAsync(surveyId))
            .ReturnsAsync(existingAnalytics);

        _mockUnitOfWork.Setup(u => u.Surveys.GetWithQuestionsAsync(surveyId))
            .ReturnsAsync(survey);

        _mockUnitOfWork.Setup(u => u.SurveyResponses.GetCompletedResponsesAsync(surveyId))
            .ReturnsAsync(new List<SurveyResponse>());

        _mockUnitOfWork.Setup(u => u.SurveyDistributions.GetBySurveyAsync(surveyId))
            .ReturnsAsync(new List<SurveyDistribution>());

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _analyticsService.RefreshAnalyticsAsync(surveyId);

        // Assert
        Assert.True(existingAnalytics.First().IsDeleted);
        Assert.NotNull(existingAnalytics.First().DeletedAt);

        _mockUnitOfWork.Verify(u => u.SurveyAnalytics.UpdateAsync(It.IsAny<SurveyAnalytics>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetOverallSentimentAsync_MultipleTextAnswers_ReturnsAverageSentiment()
    {
        // Arrange
        var surveyId = 1;
        var responses = new List<SurveyResponse>
        {
            new SurveyResponse
            {
                Answers = new List<SurveyAnswer>
                {
                    new SurveyAnswer { TextAnswer = "Excellent work environment!" },
                    new SurveyAnswer { TextAnswer = "Great team collaboration." }
                }
            },
            new SurveyResponse
            {
                Answers = new List<SurveyAnswer>
                {
                    new SurveyAnswer { TextAnswer = "Good benefits package." }
                }
            }
        };

        _mockUnitOfWork.Setup(u => u.SurveyResponses.GetCompletedResponsesAsync(surveyId))
            .ReturnsAsync(responses);

        // Mock NLP service to throw exceptions so it falls back to basic analysis
        _mockNlpService.Setup(s => s.AnalyzeSentimentAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("NLP service unavailable"));

        // Act
        var result = await _analyticsService.GetOverallSentimentAsync(surveyId);

        // Assert
        Assert.Equal(SentimentScore.Positive, result);
    }

    [Fact]
    public async Task ExtractKeywordsAsync_TextAnswers_ReturnsKeywords()
    {
        // Arrange
        var surveyId = 1;
        var responses = new List<SurveyResponse>
        {
            new SurveyResponse
            {
                Answers = new List<SurveyAnswer>
                {
                    new SurveyAnswer { TextAnswer = "The management team is very supportive and helpful." },
                    new SurveyAnswer { TextAnswer = "Great work environment with excellent team collaboration." }
                }
            }
        };

        _mockUnitOfWork.Setup(u => u.SurveyResponses.GetCompletedResponsesAsync(surveyId))
            .ReturnsAsync(responses);

        _mockNlpService.Setup(s => s.ExtractKeywordsAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("NLP service unavailable"));

        // Act
        var result = await _analyticsService.ExtractKeywordsAsync(surveyId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("team", result);
        Assert.Contains("management", result);
        Assert.Contains("work", result);
    }

    [Fact]
    public async Task ScheduleAnalyticsRefreshAsync_ValidSurvey_ReturnsTrue()
    {
        // Arrange
        var surveyId = 1;
        var interval = TimeSpan.FromHours(1);

        // Act
        var result = await _analyticsService.ScheduleAnalyticsRefreshAsync(surveyId, interval);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExportAnalyticsAsync_ValidSurvey_ReturnsExportData()
    {
        // Arrange
        var surveyId = 1;
        var survey = new Survey
        {
            Id = surveyId,
            Title = "Test Survey",
            Questions = new List<SurveyQuestion>()
        };

        _mockUnitOfWork.Setup(u => u.Surveys.GetWithQuestionsAsync(surveyId))
            .ReturnsAsync(survey);

        _mockUnitOfWork.Setup(u => u.SurveyResponses.GetCompletedResponsesAsync(surveyId))
            .ReturnsAsync(new List<SurveyResponse>());

        _mockUnitOfWork.Setup(u => u.SurveyDistributions.GetBySurveyAsync(surveyId))
            .ReturnsAsync(new List<SurveyDistribution>());

        _mockUnitOfWork.Setup(u => u.SurveyAnalytics.AddAsync(It.IsAny<SurveyAnalytics>()))
            .ReturnsAsync(new SurveyAnalytics());

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _analyticsService.ExportAnalyticsAsync(surveyId, "json");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }
}
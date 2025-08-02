using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Survey;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class SurveyResponseServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ISurveyRepository> _mockSurveyRepository;
    private readonly Mock<ISurveyResponseRepository> _mockResponseRepository;
    private readonly Mock<IRepository<SurveyAnswer>> _mockAnswerRepository;
    private readonly Mock<ILogger<SurveyResponseService>> _mockLogger;
    private readonly SurveyResponseService _responseService;

    public SurveyResponseServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockSurveyRepository = new Mock<ISurveyRepository>();
        _mockResponseRepository = new Mock<ISurveyResponseRepository>();
        _mockAnswerRepository = new Mock<IRepository<SurveyAnswer>>();
        _mockLogger = new Mock<ILogger<SurveyResponseService>>();

        _mockUnitOfWork.Setup(u => u.Surveys).Returns(_mockSurveyRepository.Object);
        _mockUnitOfWork.Setup(u => u.SurveyResponses).Returns(_mockResponseRepository.Object);
        _mockUnitOfWork.Setup(u => u.SurveyAnswers).Returns(_mockAnswerRepository.Object);

        _responseService = new SurveyResponseService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task StartResponseAsync_ValidSurvey_CreatesResponse()
    {
        // Arrange
        var surveyId = 1;
        var employeeId = 1;
        var survey = new Survey
        {
            Id = surveyId,
            Status = SurveyStatus.Active,
            AllowMultipleResponses = false
        };

        var createdResponse = new SurveyResponse
        {
            Id = 1,
            SurveyId = surveyId,
            RespondentEmployeeId = employeeId,
            Status = SurveyResponseStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            Survey = survey
        };

        _mockSurveyRepository.Setup(r => r.GetByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _mockResponseRepository.Setup(r => r.GetByEmployeeAndSurveyAsync(employeeId, surveyId))
            .ReturnsAsync((SurveyResponse?)null);

        _mockResponseRepository.Setup(r => r.AddAsync(It.IsAny<SurveyResponse>()))
            .ReturnsAsync(createdResponse);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _responseService.StartResponseAsync(surveyId, employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(surveyId, result.SurveyId);
        Assert.Equal(employeeId, result.RespondentEmployeeId);
        Assert.Equal(SurveyResponseStatus.InProgress, result.Status);
        Assert.NotNull(result.StartedAt);

        _mockResponseRepository.Verify(r => r.AddAsync(It.IsAny<SurveyResponse>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task StartResponseAsync_InactiveSurvey_ThrowsException()
    {
        // Arrange
        var surveyId = 1;
        var employeeId = 1;
        var survey = new Survey
        {
            Id = surveyId,
            Status = SurveyStatus.Draft
        };

        _mockSurveyRepository.Setup(r => r.GetByIdAsync(surveyId))
            .ReturnsAsync(survey);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _responseService.StartResponseAsync(surveyId, employeeId));
    }

    [Fact]
    public async Task StartResponseAsync_EmployeeAlreadyResponded_ThrowsException()
    {
        // Arrange
        var surveyId = 1;
        var employeeId = 1;
        var survey = new Survey
        {
            Id = surveyId,
            Status = SurveyStatus.Active,
            AllowMultipleResponses = false
        };

        var existingResponse = new SurveyResponse
        {
            Id = 1,
            SurveyId = surveyId,
            RespondentEmployeeId = employeeId
        };

        _mockSurveyRepository.Setup(r => r.GetByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _mockResponseRepository.Setup(r => r.GetByEmployeeAndSurveyAsync(employeeId, surveyId))
            .ReturnsAsync(existingResponse);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _responseService.StartResponseAsync(surveyId, employeeId));
    }

    [Fact]
    public async Task SubmitResponseAsync_ValidResponse_SubmitsSuccessfully()
    {
        // Arrange
        var submitDto = new SubmitSurveyResponseDto
        {
            SurveyId = 1,
            RespondentEmployeeId = 1,
            Answers = new List<SubmitSurveyAnswerDto>
            {
                new SubmitSurveyAnswerDto
                {
                    QuestionId = 1,
                    TextAnswer = "Great experience",
                    IsSkipped = false
                },
                new SubmitSurveyAnswerDto
                {
                    QuestionId = 2,
                    RatingValue = 5,
                    IsSkipped = false
                }
            },
            Notes = "Completed survey",
            Location = "Office"
        };

        var existingResponse = new SurveyResponse
        {
            Id = 1,
            SurveyId = submitDto.SurveyId,
            RespondentEmployeeId = submitDto.RespondentEmployeeId,
            Status = SurveyResponseStatus.InProgress,
            StartedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        _mockResponseRepository.Setup(r => r.GetByEmployeeAndSurveyAsync(
            submitDto.RespondentEmployeeId.Value, submitDto.SurveyId))
            .ReturnsAsync(existingResponse);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _responseService.SubmitResponseAsync(submitDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(SurveyResponseStatus.Completed, result.Status);
        Assert.NotNull(result.CompletedAt);
        Assert.NotNull(result.SubmittedAt);
        Assert.Equal(100, result.CompletionPercentage);
        Assert.Equal("Completed survey", result.Notes);
        Assert.Equal("Office", result.Location);
        Assert.NotNull(result.TimeTaken);

        _mockAnswerRepository.Verify(r => r.AddAsync(It.IsAny<SurveyAnswer>()), Times.Exactly(2));
        _mockResponseRepository.Verify(r => r.UpdateAsync(existingResponse), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetResponseCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var surveyId = 1;
        var expectedCount = 5;

        _mockResponseRepository.Setup(r => r.GetResponseCountAsync(surveyId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _responseService.GetResponseCountAsync(surveyId);

        // Assert
        Assert.Equal(expectedCount, result);
    }

    [Fact]
    public async Task GetCompletedResponseCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var surveyId = 1;
        var expectedCount = 3;

        _mockResponseRepository.Setup(r => r.GetCompletedResponseCountAsync(surveyId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _responseService.GetCompletedResponseCountAsync(surveyId);

        // Assert
        Assert.Equal(expectedCount, result);
    }

    [Fact]
    public async Task GetResponseRateAsync_CalculatesCorrectRate()
    {
        // Arrange
        var surveyId = 1;
        var distributionCount = 10;
        var responseCount = 7;
        var expectedRate = 70.0;

        _mockUnitOfWork.Setup(u => u.SurveyDistributions.GetDistributionCountAsync(surveyId))
            .ReturnsAsync(distributionCount);

        _mockResponseRepository.Setup(r => r.GetResponseCountAsync(surveyId))
            .ReturnsAsync(responseCount);

        // Act
        var result = await _responseService.GetResponseRateAsync(surveyId);

        // Assert
        Assert.Equal(expectedRate, result);
    }

    [Fact]
    public async Task GetCompletionRateAsync_CalculatesCorrectRate()
    {
        // Arrange
        var surveyId = 1;
        var responseCount = 8;
        var completedCount = 6;
        var expectedRate = 75.0;

        _mockResponseRepository.Setup(r => r.GetResponseCountAsync(surveyId))
            .ReturnsAsync(responseCount);

        _mockResponseRepository.Setup(r => r.GetCompletedResponseCountAsync(surveyId))
            .ReturnsAsync(completedCount);

        // Act
        var result = await _responseService.GetCompletionRateAsync(surveyId);

        // Assert
        Assert.Equal(expectedRate, result);
    }

    [Fact]
    public async Task HasEmployeeRespondedAsync_EmployeeHasResponded_ReturnsTrue()
    {
        // Arrange
        var employeeId = 1;
        var surveyId = 1;

        _mockResponseRepository.Setup(r => r.HasEmployeeRespondedAsync(employeeId, surveyId))
            .ReturnsAsync(true);

        // Act
        var result = await _responseService.HasEmployeeRespondedAsync(employeeId, surveyId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasEmployeeRespondedAsync_EmployeeHasNotResponded_ReturnsFalse()
    {
        // Arrange
        var employeeId = 1;
        var surveyId = 1;

        _mockResponseRepository.Setup(r => r.HasEmployeeRespondedAsync(employeeId, surveyId))
            .ReturnsAsync(false);

        // Act
        var result = await _responseService.HasEmployeeRespondedAsync(employeeId, surveyId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetBySurveyAsync_ReturnsSurveyResponses()
    {
        // Arrange
        var surveyId = 1;
        var responses = new List<SurveyResponse>
        {
            new SurveyResponse
            {
                Id = 1,
                SurveyId = surveyId,
                Status = SurveyResponseStatus.Completed,
                Survey = new Survey { Title = "Test Survey" },
                RespondentEmployee = new Employee { FirstName = "John", LastName = "Doe" }
            },
            new SurveyResponse
            {
                Id = 2,
                SurveyId = surveyId,
                Status = SurveyResponseStatus.InProgress,
                Survey = new Survey { Title = "Test Survey" },
                RespondentEmployee = new Employee { FirstName = "Jane", LastName = "Smith" }
            }
        };

        _mockResponseRepository.Setup(r => r.GetBySurveyAsync(surveyId))
            .ReturnsAsync(responses);

        // Act
        var result = await _responseService.GetBySurveyAsync(surveyId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, response => Assert.Equal(surveyId, response.SurveyId));
    }

    [Fact]
    public async Task DeleteResponseAsync_ExistingResponse_DeletesSuccessfully()
    {
        // Arrange
        var responseId = 1;
        var response = new SurveyResponse
        {
            Id = responseId,
            IsDeleted = false
        };

        _mockResponseRepository.Setup(r => r.GetByIdAsync(responseId))
            .ReturnsAsync(response);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _responseService.DeleteResponseAsync(responseId);

        // Assert
        Assert.True(result);
        Assert.True(response.IsDeleted);
        Assert.NotNull(response.DeletedAt);

        _mockResponseRepository.Verify(r => r.UpdateAsync(response), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteResponseAsync_NonExistentResponse_ReturnsFalse()
    {
        // Arrange
        var responseId = 999;

        _mockResponseRepository.Setup(r => r.GetByIdAsync(responseId))
            .ReturnsAsync((SurveyResponse?)null);

        // Act
        var result = await _responseService.DeleteResponseAsync(responseId);

        // Assert
        Assert.False(result);

        _mockResponseRepository.Verify(r => r.UpdateAsync(It.IsAny<SurveyResponse>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
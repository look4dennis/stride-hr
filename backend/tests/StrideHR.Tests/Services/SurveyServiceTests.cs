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

public class SurveyServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ISurveyRepository> _mockSurveyRepository;
    private readonly Mock<ISurveyQuestionRepository> _mockQuestionRepository;
    private readonly Mock<IRepository<SurveyQuestionOption>> _mockOptionRepository;
    private readonly Mock<ILogger<SurveyService>> _mockLogger;
    private readonly SurveyService _surveyService;

    public SurveyServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockSurveyRepository = new Mock<ISurveyRepository>();
        _mockQuestionRepository = new Mock<ISurveyQuestionRepository>();
        _mockOptionRepository = new Mock<IRepository<SurveyQuestionOption>>();
        _mockLogger = new Mock<ILogger<SurveyService>>();

        _mockUnitOfWork.Setup(u => u.Surveys).Returns(_mockSurveyRepository.Object);
        _mockUnitOfWork.Setup(u => u.SurveyQuestions).Returns(_mockQuestionRepository.Object);
        _mockUnitOfWork.Setup(u => u.SurveyQuestionOptions).Returns(_mockOptionRepository.Object);

        _surveyService = new SurveyService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingSurvey_ReturnsSurveyDto()
    {
        // Arrange
        var surveyId = 1;
        var survey = new Survey
        {
            Id = surveyId,
            Title = "Test Survey",
            Description = "Test Description",
            Type = SurveyType.EmployeeSatisfaction,
            Status = SurveyStatus.Draft,
            CreatedByEmployeeId = 1,
            BranchId = 1,
            CreatedByEmployee = new Employee { FirstName = "John", LastName = "Doe" },
            Branch = new Branch { Name = "Main Branch" }
        };

        _mockSurveyRepository.Setup(r => r.GetByIdAsync(surveyId, It.IsAny<System.Linq.Expressions.Expression<Func<Survey, object>>[]>()))
            .ReturnsAsync(survey);

        // Act
        var result = await _surveyService.GetByIdAsync(surveyId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(surveyId, result.Id);
        Assert.Equal("Test Survey", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(SurveyType.EmployeeSatisfaction, result.Type);
        Assert.Equal(SurveyStatus.Draft, result.Status);
        Assert.Equal("John Doe", result.CreatedByEmployeeName);
        Assert.Equal("Main Branch", result.BranchName);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentSurvey_ReturnsNull()
    {
        // Arrange
        var surveyId = 999;
        _mockSurveyRepository.Setup(r => r.GetByIdAsync(surveyId, It.IsAny<System.Linq.Expressions.Expression<Func<Survey, object>>[]>()))
            .ReturnsAsync((Survey?)null);

        // Act
        var result = await _surveyService.GetByIdAsync(surveyId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CreatesSurveySuccessfully()
    {
        // Arrange
        var createDto = new CreateSurveyDto
        {
            Title = "New Survey",
            Description = "New Description",
            Type = SurveyType.ExitInterview,
            BranchId = 1,
            EstimatedDurationMinutes = 15
        };
        var createdByEmployeeId = 1;

        var createdSurvey = new Survey
        {
            Id = 1,
            Title = createDto.Title,
            Description = createDto.Description,
            Type = createDto.Type,
            Status = SurveyStatus.Draft,
            CreatedByEmployeeId = createdByEmployeeId,
            BranchId = createDto.BranchId,
            EstimatedDurationMinutes = createDto.EstimatedDurationMinutes,
            CreatedByEmployee = new Employee { FirstName = "Jane", LastName = "Smith" },
            Branch = new Branch { Name = "Test Branch" }
        };

        _mockSurveyRepository.Setup(r => r.AddAsync(It.IsAny<Survey>()))
            .ReturnsAsync(createdSurvey);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockSurveyRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<System.Linq.Expressions.Expression<Func<Survey, object>>[]>()))
            .ReturnsAsync(createdSurvey);

        // Act
        var result = await _surveyService.CreateAsync(createDto, createdByEmployeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Survey", result.Title);
        Assert.Equal("New Description", result.Description);
        Assert.Equal(SurveyType.ExitInterview, result.Type);
        Assert.Equal(SurveyStatus.Draft, result.Status);
        Assert.Equal(15, result.EstimatedDurationMinutes);

        _mockSurveyRepository.Verify(r => r.AddAsync(It.IsAny<Survey>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ActivateAsync_ValidSurvey_ActivatesSuccessfully()
    {
        // Arrange
        var surveyId = 1;
        var survey = new Survey
        {
            Id = surveyId,
            Status = SurveyStatus.Draft,
            Questions = new List<SurveyQuestion>
            {
                new SurveyQuestion { Id = 1, IsActive = true }
            }
        };

        _mockSurveyRepository.Setup(r => r.GetWithQuestionsAsync(surveyId))
            .ReturnsAsync(survey);

        _mockSurveyRepository.Setup(r => r.GetByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _mockSurveyRepository.Setup(r => r.HasActiveResponsesAsync(surveyId))
            .ReturnsAsync(false);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _surveyService.ActivateAsync(surveyId);

        // Assert
        Assert.True(result);
        Assert.Equal(SurveyStatus.Active, survey.Status);

        _mockSurveyRepository.Verify(r => r.UpdateAsync(survey), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ActivateAsync_SurveyWithoutQuestions_ReturnsFalse()
    {
        // Arrange
        var surveyId = 1;
        var survey = new Survey
        {
            Id = surveyId,
            Status = SurveyStatus.Draft,
            Questions = new List<SurveyQuestion>() // No questions
        };

        _mockSurveyRepository.Setup(r => r.GetWithQuestionsAsync(surveyId))
            .ReturnsAsync(survey);

        // Act
        var result = await _surveyService.ActivateAsync(surveyId);

        // Assert
        Assert.False(result);

        _mockSurveyRepository.Verify(r => r.UpdateAsync(It.IsAny<Survey>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AddQuestionAsync_ValidQuestion_AddsSuccessfully()
    {
        // Arrange
        var surveyId = 1;
        var survey = new Survey { Id = surveyId, Status = SurveyStatus.Draft };
        var questionDto = new CreateSurveyQuestionDto
        {
            QuestionText = "How satisfied are you?",
            Type = QuestionType.Rating,
            IsRequired = true,
            OrderIndex = 1,
            Options = new List<CreateSurveyQuestionOptionDto>
            {
                new CreateSurveyQuestionOptionDto { OptionText = "Very Satisfied", OrderIndex = 1 },
                new CreateSurveyQuestionOptionDto { OptionText = "Satisfied", OrderIndex = 2 }
            }
        };

        var createdQuestion = new SurveyQuestion
        {
            Id = 1,
            SurveyId = surveyId,
            QuestionText = questionDto.QuestionText,
            Type = questionDto.Type,
            IsRequired = questionDto.IsRequired,
            OrderIndex = questionDto.OrderIndex,
            Options = new List<SurveyQuestionOption>
            {
                new SurveyQuestionOption { Id = 1, OptionText = "Very Satisfied", OrderIndex = 1, IsActive = true },
                new SurveyQuestionOption { Id = 2, OptionText = "Satisfied", OrderIndex = 2, IsActive = true }
            }
        };

        _mockSurveyRepository.Setup(r => r.GetByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _mockSurveyRepository.Setup(r => r.HasActiveResponsesAsync(surveyId))
            .ReturnsAsync(false);

        _mockQuestionRepository.Setup(r => r.GetMaxOrderIndexAsync(surveyId))
            .ReturnsAsync(0);

        _mockQuestionRepository.Setup(r => r.AddAsync(It.IsAny<SurveyQuestion>()))
            .ReturnsAsync(createdQuestion);

        _mockQuestionRepository.Setup(r => r.GetWithOptionsAsync(It.IsAny<int>()))
            .ReturnsAsync(createdQuestion);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _surveyService.AddQuestionAsync(surveyId, questionDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("How satisfied are you?", result.QuestionText);
        Assert.Equal(QuestionType.Rating, result.Type);
        Assert.True(result.IsRequired);
        Assert.Equal(2, result.Options.Count);

        _mockQuestionRepository.Verify(r => r.AddAsync(It.IsAny<SurveyQuestion>()), Times.Once);
        _mockOptionRepository.Verify(r => r.AddAsync(It.IsAny<SurveyQuestionOption>()), Times.Exactly(2));
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task DeleteAsync_SurveyWithoutActiveResponses_DeletesSuccessfully()
    {
        // Arrange
        var surveyId = 1;
        var survey = new Survey { Id = surveyId, IsDeleted = false };

        _mockSurveyRepository.Setup(r => r.HasActiveResponsesAsync(surveyId))
            .ReturnsAsync(false);

        _mockSurveyRepository.Setup(r => r.GetByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _surveyService.DeleteAsync(surveyId);

        // Assert
        Assert.True(result);
        Assert.True(survey.IsDeleted);
        Assert.NotNull(survey.DeletedAt);

        _mockSurveyRepository.Verify(r => r.UpdateAsync(survey), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_SurveyWithActiveResponses_ReturnsFalse()
    {
        // Arrange
        var surveyId = 1;

        _mockSurveyRepository.Setup(r => r.HasActiveResponsesAsync(surveyId))
            .ReturnsAsync(true);

        // Act
        var result = await _surveyService.DeleteAsync(surveyId);

        // Assert
        Assert.False(result);

        _mockSurveyRepository.Verify(r => r.UpdateAsync(It.IsAny<Survey>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsActiveSurveys()
    {
        // Arrange
        var activeSurveys = new List<Survey>
        {
            new Survey
            {
                Id = 1,
                Title = "Active Survey 1",
                Status = SurveyStatus.Active,
                CreatedByEmployee = new Employee { FirstName = "John", LastName = "Doe" },
                Branch = new Branch { Name = "Branch 1" }
            },
            new Survey
            {
                Id = 2,
                Title = "Active Survey 2",
                Status = SurveyStatus.Active,
                CreatedByEmployee = new Employee { FirstName = "Jane", LastName = "Smith" },
                Branch = new Branch { Name = "Branch 2" }
            }
        };

        _mockSurveyRepository.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(activeSurveys);

        // Act
        var result = await _surveyService.GetActiveAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, survey => Assert.Equal(SurveyStatus.Active, survey.Status));
    }

    [Fact]
    public async Task SearchAsync_ValidSearchTerm_ReturnsMatchingSurveys()
    {
        // Arrange
        var searchTerm = "satisfaction";
        var matchingSurveys = new List<Survey>
        {
            new Survey
            {
                Id = 1,
                Title = "Employee Satisfaction Survey",
                Description = "Annual satisfaction survey",
                CreatedByEmployee = new Employee { FirstName = "HR", LastName = "Manager" },
                Branch = new Branch { Name = "Main Branch" }
            }
        };

        _mockSurveyRepository.Setup(r => r.SearchAsync(searchTerm))
            .ReturnsAsync(matchingSurveys);

        // Act
        var result = await _surveyService.SearchAsync(searchTerm);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("Satisfaction", result.First().Title);
    }
}
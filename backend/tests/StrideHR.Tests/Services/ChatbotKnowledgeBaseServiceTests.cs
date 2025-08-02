using AutoMapper;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Chatbot;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class ChatbotKnowledgeBaseServiceTests
{
    private readonly Mock<IChatbotKnowledgeBaseRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ChatbotKnowledgeBaseService _service;

    public ChatbotKnowledgeBaseServiceTests()
    {
        _mockRepository = new Mock<IChatbotKnowledgeBaseRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();

        _service = new ChatbotKnowledgeBaseService(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task CreateArticleAsync_ValidData_ReturnsKnowledgeBaseDto()
    {
        // Arrange
        var dto = new CreateKnowledgeBaseDto
        {
            Title = "How to Request Leave",
            Content = "To request leave, follow these steps...",
            Category = "Leave Management",
            Keywords = new[] { "leave", "request", "vacation" },
            Tags = new[] { "hr", "leave" },
            Status = KnowledgeBaseStatus.Published,
            Priority = 1
        };

        var createdBy = 1;
        var expectedEntity = new ChatbotKnowledgeBase
        {
            Id = 1,
            Title = dto.Title,
            Content = dto.Content,
            Category = dto.Category,
            Keywords = dto.Keywords,
            Tags = dto.Tags,
            Status = dto.Status,
            Priority = dto.Priority,
            UpdatedBy = createdBy
        };

        var expectedDto = new KnowledgeBaseDto
        {
            Id = 1,
            Title = dto.Title,
            Content = dto.Content,
            Category = dto.Category,
            Keywords = dto.Keywords,
            Tags = dto.Tags,
            Status = dto.Status,
            Priority = dto.Priority
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<ChatbotKnowledgeBase>()))
            .Returns(Task.FromResult(expectedEntity));

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper
            .Setup(m => m.Map<KnowledgeBaseDto>(It.IsAny<ChatbotKnowledgeBase>()))
            .Returns(expectedDto);

        // Act
        var result = await _service.CreateArticleAsync(dto, createdBy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Content, result.Content);
        Assert.Equal(dto.Category, result.Category);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<ChatbotKnowledgeBase>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetArticleByIdAsync_ValidId_ReturnsArticleAndIncrementsViewCount()
    {
        // Arrange
        var articleId = 1;
        var article = new ChatbotKnowledgeBase
        {
            Id = articleId,
            Title = "Test Article",
            Content = "Test Content",
            ViewCount = 5
        };

        var expectedDto = new KnowledgeBaseDto
        {
            Id = articleId,
            Title = "Test Article",
            Content = "Test Content",
            ViewCount = 5
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(articleId))
            .ReturnsAsync(article);

        _mockRepository
            .Setup(r => r.IncrementViewCountAsync(articleId))
            .Returns(Task.CompletedTask);

        _mockMapper
            .Setup(m => m.Map<KnowledgeBaseDto>(article))
            .Returns(expectedDto);

        // Act
        var result = await _service.GetArticleByIdAsync(articleId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(articleId, result.Id);
        Assert.Equal("Test Article", result.Title);
        _mockRepository.Verify(r => r.IncrementViewCountAsync(articleId), Times.Once);
    }

    [Fact]
    public async Task GetArticleByIdAsync_InvalidId_ThrowsArgumentException()
    {
        // Arrange
        var articleId = 999;

        _mockRepository
            .Setup(r => r.GetByIdAsync(articleId))
            .ReturnsAsync((ChatbotKnowledgeBase?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetArticleByIdAsync(articleId));
    }

    [Fact]
    public async Task SearchArticlesAsync_ValidSearchTerm_ReturnsMatchingArticles()
    {
        // Arrange
        var searchTerm = "leave request";
        var articles = new List<ChatbotKnowledgeBase>
        {
            new() { Id = 1, Title = "How to Request Leave", Content = "Leave request process..." },
            new() { Id = 2, Title = "Leave Policies", Content = "Company leave policies..." }
        };

        var expectedDtos = new List<KnowledgeBaseDto>
        {
            new() { Id = 1, Title = "How to Request Leave" },
            new() { Id = 2, Title = "Leave Policies" }
        };

        _mockRepository
            .Setup(r => r.SearchByContentAsync(searchTerm))
            .ReturnsAsync(articles);

        _mockMapper
            .Setup(m => m.Map<List<KnowledgeBaseDto>>(articles))
            .Returns(expectedDtos);

        // Act
        var result = await _service.SearchArticlesAsync(searchTerm);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, a => a.Title == "How to Request Leave");
        Assert.Contains(result, a => a.Title == "Leave Policies");
    }

    [Fact]
    public async Task GetArticlesByCategoryAsync_ValidCategory_ReturnsArticlesInCategory()
    {
        // Arrange
        var category = "Leave Management";
        var articles = new List<ChatbotKnowledgeBase>
        {
            new() { Id = 1, Title = "Leave Request Process", Category = category },
            new() { Id = 2, Title = "Leave Balance Inquiry", Category = category }
        };

        var expectedDtos = new List<KnowledgeBaseDto>
        {
            new() { Id = 1, Title = "Leave Request Process", Category = category },
            new() { Id = 2, Title = "Leave Balance Inquiry", Category = category }
        };

        _mockRepository
            .Setup(r => r.SearchByCategoryAsync(category))
            .ReturnsAsync(articles);

        _mockMapper
            .Setup(m => m.Map<List<KnowledgeBaseDto>>(articles))
            .Returns(expectedDtos);

        // Act
        var result = await _service.GetArticlesByCategoryAsync(category);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal(category, a.Category));
    }

    [Fact]
    public async Task ProvideArticleFeedbackAsync_ValidData_UpdatesHelpfulness()
    {
        // Arrange
        var articleId = 1;
        var employeeId = 1;
        var isHelpful = true;
        var comments = "Very helpful article";

        var article = new ChatbotKnowledgeBase
        {
            Id = articleId,
            Title = "Test Article",
            HelpfulCount = 5,
            NotHelpfulCount = 1
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(articleId))
            .ReturnsAsync(article);

        _mockRepository
            .Setup(r => r.UpdateHelpfulnessAsync(articleId, isHelpful))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.ProvideArticleFeedbackAsync(articleId, employeeId, isHelpful, comments);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.UpdateHelpfulnessAsync(articleId, isHelpful), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ProvideArticleFeedbackAsync_InvalidArticleId_ReturnsFalse()
    {
        // Arrange
        var articleId = 999;
        var employeeId = 1;
        var isHelpful = true;

        _mockRepository
            .Setup(r => r.GetByIdAsync(articleId))
            .ReturnsAsync((ChatbotKnowledgeBase?)null);

        // Act
        var result = await _service.ProvideArticleFeedbackAsync(articleId, employeeId, isHelpful);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.UpdateHelpfulnessAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Theory]
    [InlineData("leave_request", "Leave Management")]
    [InlineData("payroll_inquiry", "Payroll")]
    [InlineData("attendance_query", "Attendance")]
    [InlineData("hr_policy", "HR Policies")]
    public async Task GetRelevantArticlesAsync_ValidIntent_ReturnsRelevantArticles(string intent, string expectedCategory)
    {
        // Arrange
        var entities = new Dictionary<string, object>();
        var articles = new List<ChatbotKnowledgeBase>
        {
            new() { Id = 1, Title = "Relevant Article", Category = expectedCategory }
        };

        var expectedDtos = new List<KnowledgeBaseDto>
        {
            new() { Id = 1, Title = "Relevant Article", Category = expectedCategory }
        };

        _mockRepository
            .Setup(r => r.SearchByCategoryAsync(expectedCategory))
            .ReturnsAsync(articles);

        _mockMapper
            .Setup(m => m.Map<List<KnowledgeBaseDto>>(It.IsAny<List<ChatbotKnowledgeBase>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _service.GetRelevantArticlesAsync(intent, entities);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(expectedCategory, result.First().Category);
    }

    [Fact]
    public async Task DeleteArticleAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        var articleId = 1;
        var article = new ChatbotKnowledgeBase { Id = articleId, Title = "Test Article" };

        _mockRepository
            .Setup(r => r.GetByIdAsync(articleId))
            .ReturnsAsync(article);

        _mockRepository
            .Setup(r => r.DeleteAsync(article))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.DeleteArticleAsync(articleId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteAsync(article), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteArticleAsync_InvalidId_ReturnsFalse()
    {
        // Arrange
        var articleId = 999;

        _mockRepository
            .Setup(r => r.GetByIdAsync(articleId))
            .ReturnsAsync((ChatbotKnowledgeBase?)null);

        // Act
        var result = await _service.DeleteArticleAsync(articleId);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<ChatbotKnowledgeBase>()), Times.Never);
    }
}
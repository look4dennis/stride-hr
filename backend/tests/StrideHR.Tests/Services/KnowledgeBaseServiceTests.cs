using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.KnowledgeBase;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class KnowledgeBaseServiceTests
{
    private readonly Mock<IKnowledgeBaseDocumentRepository> _mockDocumentRepository;
    private readonly Mock<IKnowledgeBaseCategoryRepository> _mockCategoryRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<KnowledgeBaseService>> _mockLogger;
    private readonly Mock<IAuditLogService> _mockAuditLogService;
    private readonly KnowledgeBaseService _service;

    public KnowledgeBaseServiceTests()
    {
        _mockDocumentRepository = new Mock<IKnowledgeBaseDocumentRepository>();
        _mockCategoryRepository = new Mock<IKnowledgeBaseCategoryRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<KnowledgeBaseService>>();
        _mockAuditLogService = new Mock<IAuditLogService>();

        _service = new KnowledgeBaseService(
            _mockDocumentRepository.Object,
            _mockCategoryRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockAuditLogService.Object);
    }

    #region Document Management Tests

    [Fact]
    public async Task CreateDocumentAsync_ValidDto_ReturnsDocumentDto()
    {
        // Arrange
        var dto = new CreateKnowledgeBaseDocumentDto
        {
            Title = "Test Document",
            Content = "Test content",
            Summary = "Test summary",
            CategoryId = 1,
            Tags = new[] { "test", "document" },
            Keywords = new[] { "test", "knowledge" }
        };

        var authorId = 1;
        var document = new KnowledgeBaseDocument
        {
            Id = 1,
            Title = dto.Title,
            Content = dto.Content,
            Summary = dto.Summary,
            CategoryId = dto.CategoryId,
            AuthorId = authorId,
            Status = DocumentStatus.Draft
        };

        var documentDto = new KnowledgeBaseDocumentDto
        {
            Id = 1,
            Title = dto.Title,
            Content = dto.Content,
            Summary = dto.Summary,
            CategoryId = dto.CategoryId,
            AuthorId = authorId,
            Status = DocumentStatus.Draft
        };

        _mockMapper.Setup(m => m.Map<KnowledgeBaseDocument>(dto)).Returns(document);
        _mockDocumentRepository.Setup(r => r.AddAsync(It.IsAny<KnowledgeBaseDocument>())).ReturnsAsync(document);
        _mockDocumentRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        _mockDocumentRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<System.Linq.Expressions.Expression<Func<KnowledgeBaseDocument, object>>[]>()))
            .ReturnsAsync(document);
        _mockMapper.Setup(m => m.Map<KnowledgeBaseDocumentDto>(document)).Returns(documentDto);

        // Act
        var result = await _service.CreateDocumentAsync(dto, authorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Content, result.Content);
        Assert.Equal(authorId, result.AuthorId);
        Assert.Equal(DocumentStatus.Draft, result.Status);

        _mockDocumentRepository.Verify(r => r.AddAsync(It.IsAny<KnowledgeBaseDocument>()), Times.Once);
        _mockDocumentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockAuditLogService.Verify(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task UpdateDocumentAsync_ValidDto_ReturnsUpdatedDocumentDto()
    {
        // Arrange
        var documentId = 1;
        var authorId = 1;
        var dto = new UpdateKnowledgeBaseDocumentDto
        {
            Title = "Updated Document",
            Content = "Updated content",
            Summary = "Updated summary",
            CategoryId = 1
        };

        var existingDocument = new KnowledgeBaseDocument
        {
            Id = documentId,
            Title = "Original Document",
            Content = "Original content",
            AuthorId = authorId,
            Status = DocumentStatus.Draft
        };

        var updatedDocumentDto = new KnowledgeBaseDocumentDto
        {
            Id = documentId,
            Title = dto.Title,
            Content = dto.Content,
            Summary = dto.Summary,
            AuthorId = authorId,
            Status = DocumentStatus.Draft
        };

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<System.Linq.Expressions.Expression<Func<KnowledgeBaseDocument, object>>[]>()))
            .ReturnsAsync(existingDocument);
        _mockMapper.Setup(m => m.Map(dto, existingDocument));
        _mockDocumentRepository.Setup(r => r.UpdateAsync(It.IsAny<KnowledgeBaseDocument>())).Returns(Task.CompletedTask);
        _mockDocumentRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        _mockMapper.Setup(m => m.Map<KnowledgeBaseDocumentDto>(existingDocument)).Returns(updatedDocumentDto);

        // Act
        var result = await _service.UpdateDocumentAsync(documentId, dto, authorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Content, result.Content);

        _mockDocumentRepository.Verify(r => r.UpdateAsync(It.IsAny<KnowledgeBaseDocument>()), Times.Once);
        _mockDocumentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockAuditLogService.Verify(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task UpdateDocumentAsync_DocumentNotFound_ThrowsArgumentException()
    {
        // Arrange
        var documentId = 1;
        var authorId = 1;
        var dto = new UpdateKnowledgeBaseDocumentDto { Title = "Updated Document" };

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<System.Linq.Expressions.Expression<Func<KnowledgeBaseDocument, object>>[]>()))
            .ReturnsAsync((KnowledgeBaseDocument?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateDocumentAsync(documentId, dto, authorId));
    }

    [Fact]
    public async Task UpdateDocumentAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var documentId = 1;
        var authorId = 1;
        var unauthorizedUserId = 2;
        var dto = new UpdateKnowledgeBaseDocumentDto { Title = "Updated Document" };

        var existingDocument = new KnowledgeBaseDocument
        {
            Id = documentId,
            AuthorId = authorId,
            Status = DocumentStatus.Draft
        };

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<System.Linq.Expressions.Expression<Func<KnowledgeBaseDocument, object>>[]>()))
            .ReturnsAsync(existingDocument);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateDocumentAsync(documentId, dto, unauthorizedUserId));
    }

    [Fact]
    public async Task GetDocumentByIdAsync_ValidId_ReturnsDocumentDto()
    {
        // Arrange
        var documentId = 1;
        var document = new KnowledgeBaseDocument
        {
            Id = documentId,
            Title = "Test Document",
            Content = "Test content",
            Status = DocumentStatus.Published
        };

        var documentDto = new KnowledgeBaseDocumentDto
        {
            Id = documentId,
            Title = "Test Document",
            Content = "Test content",
            Status = DocumentStatus.Published
        };

        var versions = new List<KnowledgeBaseDocument> { document };
        var versionDtos = new List<KnowledgeBaseDocumentVersionDto>
        {
            new() { Id = documentId, Version = 1, Title = "Test Document" }
        };

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<System.Linq.Expressions.Expression<Func<KnowledgeBaseDocument, object>>[]>()))
            .ReturnsAsync(document);
        _mockDocumentRepository.Setup(r => r.GetDocumentVersionsAsync(documentId)).ReturnsAsync(versions);
        _mockMapper.Setup(m => m.Map<KnowledgeBaseDocumentDto>(document)).Returns(documentDto);
        _mockMapper.Setup(m => m.Map<List<KnowledgeBaseDocumentVersionDto>>(versions)).Returns(versionDtos);

        // Act
        var result = await _service.GetDocumentByIdAsync(documentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(documentId, result.Id);
        Assert.Equal("Test Document", result.Title);
        Assert.NotEmpty(result.Versions);
    }

    [Fact]
    public async Task GetDocumentByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var documentId = 999;

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<System.Linq.Expressions.Expression<Func<KnowledgeBaseDocument, object>>[]>()))
            .ReturnsAsync((KnowledgeBaseDocument?)null);

        // Act
        var result = await _service.GetDocumentByIdAsync(documentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ValidIdAndAuthor_ReturnsTrue()
    {
        // Arrange
        var documentId = 1;
        var authorId = 1;
        var document = new KnowledgeBaseDocument
        {
            Id = documentId,
            Title = "Test Document",
            AuthorId = authorId
        };

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId)).ReturnsAsync(document);
        _mockDocumentRepository.Setup(r => r.DeleteAsync(document)).Returns(Task.CompletedTask);
        _mockDocumentRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteDocumentAsync(documentId, authorId);

        // Assert
        Assert.True(result);
        _mockDocumentRepository.Verify(r => r.DeleteAsync(document), Times.Once);
        _mockDocumentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockAuditLogService.Verify(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task DeleteDocumentAsync_DocumentNotFound_ReturnsFalse()
    {
        // Arrange
        var documentId = 999;
        var authorId = 1;

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId)).ReturnsAsync((KnowledgeBaseDocument?)null);

        // Act
        var result = await _service.DeleteDocumentAsync(documentId, authorId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteDocumentAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var documentId = 1;
        var authorId = 1;
        var unauthorizedUserId = 2;
        var document = new KnowledgeBaseDocument
        {
            Id = documentId,
            AuthorId = authorId
        };

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId)).ReturnsAsync(document);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteDocumentAsync(documentId, unauthorizedUserId));
    }

    #endregion

    #region Approval Workflow Tests

    [Fact]
    public async Task SubmitForApprovalAsync_ValidDocument_ReturnsTrue()
    {
        // Arrange
        var documentId = 1;
        var authorId = 1;
        var document = new KnowledgeBaseDocument
        {
            Id = documentId,
            Title = "Test Document",
            AuthorId = authorId,
            Status = DocumentStatus.Draft
        };

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId)).ReturnsAsync(document);
        _mockDocumentRepository.Setup(r => r.UpdateAsync(document)).Returns(Task.CompletedTask);
        _mockDocumentRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _service.SubmitForApprovalAsync(documentId, authorId);

        // Assert
        Assert.True(result);
        Assert.Equal(DocumentStatus.PendingReview, document.Status);
        _mockDocumentRepository.Verify(r => r.UpdateAsync(document), Times.Once);
        _mockDocumentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockAuditLogService.Verify(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task SubmitForApprovalAsync_DocumentNotFound_ReturnsFalse()
    {
        // Arrange
        var documentId = 999;
        var authorId = 1;

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId)).ReturnsAsync((KnowledgeBaseDocument?)null);

        // Act
        var result = await _service.SubmitForApprovalAsync(documentId, authorId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SubmitForApprovalAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var documentId = 1;
        var authorId = 1;
        var unauthorizedUserId = 2;
        var document = new KnowledgeBaseDocument
        {
            Id = documentId,
            AuthorId = authorId,
            Status = DocumentStatus.Draft
        };

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId)).ReturnsAsync(document);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.SubmitForApprovalAsync(documentId, unauthorizedUserId));
    }

    [Fact]
    public async Task SubmitForApprovalAsync_DocumentNotDraft_ThrowsInvalidOperationException()
    {
        // Arrange
        var documentId = 1;
        var authorId = 1;
        var document = new KnowledgeBaseDocument
        {
            Id = documentId,
            AuthorId = authorId,
            Status = DocumentStatus.Published
        };

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId)).ReturnsAsync(document);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SubmitForApprovalAsync(documentId, authorId));
    }

    [Fact]
    public async Task ApproveDocumentAsync_ValidDocument_ReturnsTrue()
    {
        // Arrange
        var documentId = 1;
        var approverId = 2;
        var approvalDto = new DocumentApprovalDto
        {
            DocumentId = documentId,
            Action = ApprovalAction.Approved,
            Comments = "Looks good"
        };

        var document = new KnowledgeBaseDocument
        {
            Id = documentId,
            Title = "Test Document",
            Status = DocumentStatus.PendingReview
        };

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId)).ReturnsAsync(document);
        _mockDocumentRepository.Setup(r => r.UpdateAsync(document)).Returns(Task.CompletedTask);
        _mockDocumentRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _service.ApproveDocumentAsync(approvalDto, approverId);

        // Assert
        Assert.True(result);
        Assert.Equal(DocumentStatus.Approved, document.Status);
        Assert.Equal(approverId, document.ReviewerId);
        Assert.Equal(approvalDto.Comments, document.ReviewComments);
        Assert.NotNull(document.ReviewedAt);
        Assert.NotNull(document.PublishedAt);

        _mockDocumentRepository.Verify(r => r.UpdateAsync(document), Times.Once);
        _mockDocumentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockAuditLogService.Verify(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task RejectDocumentAsync_ValidDocument_ReturnsTrue()
    {
        // Arrange
        var documentId = 1;
        var approverId = 2;
        var approvalDto = new DocumentApprovalDto
        {
            DocumentId = documentId,
            Action = ApprovalAction.Rejected,
            Comments = "Needs improvement"
        };

        var document = new KnowledgeBaseDocument
        {
            Id = documentId,
            Title = "Test Document",
            Status = DocumentStatus.PendingReview
        };

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId)).ReturnsAsync(document);
        _mockDocumentRepository.Setup(r => r.UpdateAsync(document)).Returns(Task.CompletedTask);
        _mockDocumentRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _service.RejectDocumentAsync(approvalDto, approverId);

        // Assert
        Assert.True(result);
        Assert.Equal(DocumentStatus.Rejected, document.Status);
        Assert.Equal(approverId, document.ReviewerId);
        Assert.Equal(approvalDto.Comments, document.ReviewComments);
        Assert.NotNull(document.ReviewedAt);

        _mockDocumentRepository.Verify(r => r.UpdateAsync(document), Times.Once);
        _mockDocumentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockAuditLogService.Verify(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
    }

    #endregion

    #region Category Management Tests

    [Fact]
    public async Task CreateCategoryAsync_ValidData_ReturnsCategoryDto()
    {
        // Arrange
        var name = "Test Category";
        var description = "Test Description";
        var category = new KnowledgeBaseCategory
        {
            Id = 1,
            Name = name,
            Description = description,
            Slug = "test-category",
            IsActive = true
        };

        var categoryDto = new KnowledgeBaseCategoryDto
        {
            Id = 1,
            Name = name,
            Description = description,
            Slug = "test-category",
            IsActive = true
        };

        _mockCategoryRepository.Setup(r => r.IsSlugUniqueAsync(It.IsAny<string>(), null)).ReturnsAsync(true);
        _mockCategoryRepository.Setup(r => r.AddAsync(It.IsAny<KnowledgeBaseCategory>())).ReturnsAsync(category);
        _mockCategoryRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        _mockMapper.Setup(m => m.Map<KnowledgeBaseCategoryDto>(It.IsAny<KnowledgeBaseCategory>())).Returns(categoryDto);

        // Act
        var result = await _service.CreateCategoryAsync(name, description);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
        Assert.Equal(description, result.Description);
        Assert.True(result.IsActive);

        _mockCategoryRepository.Verify(r => r.AddAsync(It.IsAny<KnowledgeBaseCategory>()), Times.Once);
        _mockCategoryRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ValidData_ReturnsUpdatedCategoryDto()
    {
        // Arrange
        var categoryId = 1;
        var name = "Updated Category";
        var description = "Updated Description";
        var category = new KnowledgeBaseCategory
        {
            Id = categoryId,
            Name = "Original Category",
            Description = "Original Description",
            Slug = "original-category"
        };

        var updatedCategoryDto = new KnowledgeBaseCategoryDto
        {
            Id = categoryId,
            Name = name,
            Description = description,
            Slug = "updated-category"
        };

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _mockCategoryRepository.Setup(r => r.IsSlugUniqueAsync(It.IsAny<string>(), categoryId)).ReturnsAsync(true);
        _mockCategoryRepository.Setup(r => r.UpdateAsync(category)).Returns(Task.CompletedTask);
        _mockCategoryRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        _mockMapper.Setup(m => m.Map<KnowledgeBaseCategoryDto>(category)).Returns(updatedCategoryDto);

        // Act
        var result = await _service.UpdateCategoryAsync(categoryId, name, description);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
        Assert.Equal(description, result.Description);

        _mockCategoryRepository.Verify(r => r.UpdateAsync(category), Times.Once);
        _mockCategoryRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_CategoryNotFound_ThrowsArgumentException()
    {
        // Arrange
        var categoryId = 999;
        var name = "Updated Category";
        var description = "Updated Description";

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync((KnowledgeBaseCategory?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateCategoryAsync(categoryId, name, description));
    }

    [Fact]
    public async Task DeleteCategoryAsync_ValidIdWithNoDocuments_ReturnsTrue()
    {
        // Arrange
        var categoryId = 1;
        var category = new KnowledgeBaseCategory
        {
            Id = categoryId,
            Name = "Test Category"
        };

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _mockCategoryRepository.Setup(r => r.GetDocumentCountAsync(categoryId)).ReturnsAsync(0);
        _mockCategoryRepository.Setup(r => r.DeleteAsync(category)).Returns(Task.CompletedTask);
        _mockCategoryRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteCategoryAsync(categoryId);

        // Assert
        Assert.True(result);
        _mockCategoryRepository.Verify(r => r.DeleteAsync(category), Times.Once);
        _mockCategoryRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCategoryAsync_CategoryWithDocuments_ThrowsInvalidOperationException()
    {
        // Arrange
        var categoryId = 1;
        var category = new KnowledgeBaseCategory
        {
            Id = categoryId,
            Name = "Test Category"
        };

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _mockCategoryRepository.Setup(r => r.GetDocumentCountAsync(categoryId)).ReturnsAsync(5);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteCategoryAsync(categoryId));
    }

    [Fact]
    public async Task DeleteCategoryAsync_CategoryNotFound_ReturnsFalse()
    {
        // Arrange
        var categoryId = 999;

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync((KnowledgeBaseCategory?)null);

        // Act
        var result = await _service.DeleteCategoryAsync(categoryId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Search and Analytics Tests

    [Fact]
    public async Task SearchDocumentsAsync_ValidSearchDto_ReturnsDocuments()
    {
        // Arrange
        var searchDto = new KnowledgeBaseSearchDto
        {
            Query = "test",
            Page = 1,
            PageSize = 10
        };

        var documents = new List<KnowledgeBaseDocument>
        {
            new() { Id = 1, Title = "Test Document 1", Content = "Test content" },
            new() { Id = 2, Title = "Test Document 2", Content = "Test content" }
        };

        var documentDtos = new List<KnowledgeBaseDocumentDto>
        {
            new() { Id = 1, Title = "Test Document 1", Content = "Test content" },
            new() { Id = 2, Title = "Test Document 2", Content = "Test content" }
        };

        _mockDocumentRepository.Setup(r => r.SearchDocumentsAsync(searchDto)).ReturnsAsync(documents);
        _mockMapper.Setup(m => m.Map<IEnumerable<KnowledgeBaseDocumentDto>>(documents)).Returns(documentDtos);

        // Act
        var result = await _service.SearchDocumentsAsync(searchDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, doc => Assert.Contains("Test", doc.Title));
    }

    [Fact]
    public async Task GetSearchResultCountAsync_ValidSearchDto_ReturnsCount()
    {
        // Arrange
        var searchDto = new KnowledgeBaseSearchDto
        {
            Query = "test",
            Page = 1,
            PageSize = 10
        };

        _mockDocumentRepository.Setup(r => r.GetSearchResultCountAsync(searchDto)).ReturnsAsync(25);

        // Act
        var result = await _service.GetSearchResultCountAsync(searchDto);

        // Assert
        Assert.Equal(25, result);
    }

    [Fact]
    public async Task GetFeaturedDocumentsAsync_ReturnsOnlyFeaturedDocuments()
    {
        // Arrange
        var documents = new List<KnowledgeBaseDocument>
        {
            new() { Id = 1, Title = "Featured Document 1", IsFeatured = true, Status = DocumentStatus.Published },
            new() { Id = 2, Title = "Featured Document 2", IsFeatured = true, Status = DocumentStatus.Published }
        };

        var documentDtos = new List<KnowledgeBaseDocumentDto>
        {
            new() { Id = 1, Title = "Featured Document 1", IsFeatured = true, Status = DocumentStatus.Published },
            new() { Id = 2, Title = "Featured Document 2", IsFeatured = true, Status = DocumentStatus.Published }
        };

        _mockDocumentRepository.Setup(r => r.GetFeaturedDocumentsAsync()).ReturnsAsync(documents);
        _mockMapper.Setup(m => m.Map<IEnumerable<KnowledgeBaseDocumentDto>>(documents)).Returns(documentDtos);

        // Act
        var result = await _service.GetFeaturedDocumentsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, doc => Assert.True(doc.IsFeatured));
        Assert.All(result, doc => Assert.Equal(DocumentStatus.Published, doc.Status));
    }

    #endregion
}
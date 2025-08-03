using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.KnowledgeBase;
using System.Security.Claims;
using Xunit;

namespace StrideHR.Tests.Controllers;

public class KnowledgeBaseControllerTests
{
    private readonly Mock<IKnowledgeBaseService> _mockKnowledgeBaseService;
    private readonly Mock<ILogger<KnowledgeBaseController>> _mockLogger;
    private readonly KnowledgeBaseController _controller;

    public KnowledgeBaseControllerTests()
    {
        _mockKnowledgeBaseService = new Mock<IKnowledgeBaseService>();
        _mockLogger = new Mock<ILogger<KnowledgeBaseController>>();
        _controller = new KnowledgeBaseController(_mockKnowledgeBaseService.Object, _mockLogger.Object);

        // Setup user context
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new("EmployeeId", "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }

    #region Document Management Tests

    [Fact]
    public async Task CreateDocument_ValidDto_ReturnsSuccessResponse()
    {
        // Arrange
        var dto = new CreateKnowledgeBaseDocumentDto
        {
            Title = "Test Document",
            Content = "Test content",
            Summary = "Test summary",
            CategoryId = 1
        };

        var documentDto = new KnowledgeBaseDocumentDto
        {
            Id = 1,
            Title = dto.Title,
            Content = dto.Content,
            Summary = dto.Summary,
            CategoryId = dto.CategoryId,
            AuthorId = 1,
            Status = DocumentStatus.Draft
        };

        _mockKnowledgeBaseService.Setup(s => s.CreateDocumentAsync(dto, 1))
            .ReturnsAsync(documentDto);

        // Act
        var result = await _controller.CreateDocument(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.CreateDocumentAsync(dto, 1), Times.Once);
    }

    [Fact]
    public async Task CreateDocument_ServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        var dto = new CreateKnowledgeBaseDocumentDto
        {
            Title = "Test Document",
            Content = "Test content",
            CategoryId = 1
        };

        _mockKnowledgeBaseService.Setup(s => s.CreateDocumentAsync(dto, 1))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.CreateDocument(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateDocument_ValidDto_ReturnsSuccessResponse()
    {
        // Arrange
        var documentId = 1;
        var dto = new UpdateKnowledgeBaseDocumentDto
        {
            Title = "Updated Document",
            Content = "Updated content",
            CategoryId = 1
        };

        var updatedDocumentDto = new KnowledgeBaseDocumentDto
        {
            Id = documentId,
            Title = dto.Title,
            Content = dto.Content,
            CategoryId = dto.CategoryId,
            AuthorId = 1,
            Status = DocumentStatus.Draft
        };

        _mockKnowledgeBaseService.Setup(s => s.UpdateDocumentAsync(documentId, dto, 1))
            .ReturnsAsync(updatedDocumentDto);

        // Act
        var result = await _controller.UpdateDocument(documentId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.UpdateDocumentAsync(documentId, dto, 1), Times.Once);
    }

    [Fact]
    public async Task UpdateDocument_DocumentNotFound_ReturnsErrorResponse()
    {
        // Arrange
        var documentId = 999;
        var dto = new UpdateKnowledgeBaseDocumentDto
        {
            Title = "Updated Document",
            Content = "Updated content",
            CategoryId = 1
        };

        _mockKnowledgeBaseService.Setup(s => s.UpdateDocumentAsync(documentId, dto, 1))
            .ThrowsAsync(new ArgumentException("Document not found"));

        // Act
        var result = await _controller.UpdateDocument(documentId, dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateDocument_UnauthorizedUser_ReturnsErrorResponse()
    {
        // Arrange
        var documentId = 1;
        var dto = new UpdateKnowledgeBaseDocumentDto
        {
            Title = "Updated Document",
            Content = "Updated content",
            CategoryId = 1
        };

        _mockKnowledgeBaseService.Setup(s => s.UpdateDocumentAsync(documentId, dto, 1))
            .ThrowsAsync(new UnauthorizedAccessException("Only the author can update the document"));

        // Act
        var result = await _controller.UpdateDocument(documentId, dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetDocument_ValidId_ReturnsDocument()
    {
        // Arrange
        var documentId = 1;
        var documentDto = new KnowledgeBaseDocumentDto
        {
            Id = documentId,
            Title = "Test Document",
            Content = "Test content",
            Status = DocumentStatus.Published
        };

        _mockKnowledgeBaseService.Setup(s => s.GetDocumentByIdAsync(documentId))
            .ReturnsAsync(documentDto);
        _mockKnowledgeBaseService.Setup(s => s.RecordDocumentViewAsync(documentId, 1, It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.GetDocument(documentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.GetDocumentByIdAsync(documentId), Times.Once);
        _mockKnowledgeBaseService.Verify(s => s.RecordDocumentViewAsync(documentId, 1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetDocument_InvalidId_ReturnsErrorResponse()
    {
        // Arrange
        var documentId = 999;

        _mockKnowledgeBaseService.Setup(s => s.GetDocumentByIdAsync(documentId))
            .ReturnsAsync((KnowledgeBaseDocumentDto?)null);

        // Act
        var result = await _controller.GetDocument(documentId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteDocument_ValidId_ReturnsSuccessResponse()
    {
        // Arrange
        var documentId = 1;

        _mockKnowledgeBaseService.Setup(s => s.DeleteDocumentAsync(documentId, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteDocument(documentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.DeleteDocumentAsync(documentId, 1), Times.Once);
    }

    [Fact]
    public async Task DeleteDocument_DocumentNotFound_ReturnsErrorResponse()
    {
        // Arrange
        var documentId = 999;

        _mockKnowledgeBaseService.Setup(s => s.DeleteDocumentAsync(documentId, 1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteDocument(documentId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task SearchDocuments_ValidSearchDto_ReturnsSearchResults()
    {
        // Arrange
        var searchDto = new KnowledgeBaseSearchDto
        {
            Query = "test",
            Page = 1,
            PageSize = 10
        };

        var documents = new List<KnowledgeBaseDocumentDto>
        {
            new() { Id = 1, Title = "Test Document 1" },
            new() { Id = 2, Title = "Test Document 2" }
        };

        _mockKnowledgeBaseService.Setup(s => s.SearchDocumentsAsync(searchDto))
            .ReturnsAsync(documents);
        _mockKnowledgeBaseService.Setup(s => s.GetSearchResultCountAsync(searchDto))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.SearchDocuments(searchDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.SearchDocumentsAsync(searchDto), Times.Once);
        _mockKnowledgeBaseService.Verify(s => s.GetSearchResultCountAsync(searchDto), Times.Once);
    }

    #endregion

    #region Approval Workflow Tests

    [Fact]
    public async Task SubmitForApproval_ValidDocument_ReturnsSuccessResponse()
    {
        // Arrange
        var documentId = 1;

        _mockKnowledgeBaseService.Setup(s => s.SubmitForApprovalAsync(documentId, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SubmitForApproval(documentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.SubmitForApprovalAsync(documentId, 1), Times.Once);
    }

    [Fact]
    public async Task SubmitForApproval_DocumentNotFound_ReturnsErrorResponse()
    {
        // Arrange
        var documentId = 999;

        _mockKnowledgeBaseService.Setup(s => s.SubmitForApprovalAsync(documentId, 1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.SubmitForApproval(documentId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task ApproveDocument_ValidApproval_ReturnsSuccessResponse()
    {
        // Arrange
        var approvalDto = new DocumentApprovalDto
        {
            DocumentId = 1,
            Action = ApprovalAction.Approved,
            Comments = "Looks good"
        };

        _mockKnowledgeBaseService.Setup(s => s.ApproveDocumentAsync(approvalDto, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ApproveDocument(approvalDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.ApproveDocumentAsync(approvalDto, 1), Times.Once);
    }

    [Fact]
    public async Task RejectDocument_ValidRejection_ReturnsSuccessResponse()
    {
        // Arrange
        var approvalDto = new DocumentApprovalDto
        {
            DocumentId = 1,
            Action = ApprovalAction.Rejected,
            Comments = "Needs improvement"
        };

        _mockKnowledgeBaseService.Setup(s => s.RejectDocumentAsync(approvalDto, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RejectDocument(approvalDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.RejectDocumentAsync(approvalDto, 1), Times.Once);
    }

    [Fact]
    public async Task GetPendingApprovalDocuments_ReturnsDocuments()
    {
        // Arrange
        var documents = new List<KnowledgeBaseDocumentDto>
        {
            new() { Id = 1, Title = "Pending Document 1", Status = DocumentStatus.PendingReview },
            new() { Id = 2, Title = "Pending Document 2", Status = DocumentStatus.UnderReview }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetPendingApprovalDocumentsAsync())
            .ReturnsAsync(documents);

        // Act
        var result = await _controller.GetPendingApprovalDocuments();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.GetPendingApprovalDocumentsAsync(), Times.Once);
    }

    #endregion

    #region Version Control Tests

    [Fact]
    public async Task CreateNewVersion_ValidDto_ReturnsNewVersion()
    {
        // Arrange
        var documentId = 1;
        var dto = new UpdateKnowledgeBaseDocumentDto
        {
            Title = "Updated Document",
            Content = "Updated content",
            CategoryId = 1
        };

        var newVersionDto = new KnowledgeBaseDocumentDto
        {
            Id = 2,
            Title = dto.Title,
            Content = dto.Content,
            Version = 2,
            ParentDocumentId = documentId,
            IsCurrentVersion = true
        };

        _mockKnowledgeBaseService.Setup(s => s.CreateNewVersionAsync(documentId, dto, 1))
            .ReturnsAsync(newVersionDto);

        // Act
        var result = await _controller.CreateNewVersion(documentId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.CreateNewVersionAsync(documentId, dto, 1), Times.Once);
    }

    [Fact]
    public async Task GetDocumentVersions_ValidId_ReturnsVersions()
    {
        // Arrange
        var documentId = 1;
        var versions = new List<KnowledgeBaseDocumentVersionDto>
        {
            new() { Id = 1, Version = 1, Title = "Document v1", IsCurrentVersion = false },
            new() { Id = 2, Version = 2, Title = "Document v2", IsCurrentVersion = true }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetDocumentVersionsAsync(documentId))
            .ReturnsAsync(versions);

        // Act
        var result = await _controller.GetDocumentVersions(documentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.GetDocumentVersionsAsync(documentId), Times.Once);
    }

    [Fact]
    public async Task RestoreVersion_ValidIds_ReturnsSuccessResponse()
    {
        // Arrange
        var documentId = 1;
        var versionId = 2;

        _mockKnowledgeBaseService.Setup(s => s.RestoreVersionAsync(documentId, versionId, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RestoreVersion(documentId, versionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.RestoreVersionAsync(documentId, versionId, 1), Times.Once);
    }

    #endregion

    #region Category Management Tests

    [Fact]
    public async Task CreateCategory_ValidDto_ReturnsCategory()
    {
        // Arrange
        var dto = new CreateCategoryDto
        {
            Name = "Test Category",
            Description = "Test Description"
        };

        var categoryDto = new KnowledgeBaseCategoryDto
        {
            Id = 1,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true
        };

        _mockKnowledgeBaseService.Setup(s => s.CreateCategoryAsync(dto.Name, dto.Description, dto.ParentCategoryId))
            .ReturnsAsync(categoryDto);

        // Act
        var result = await _controller.CreateCategory(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.CreateCategoryAsync(dto.Name, dto.Description, dto.ParentCategoryId), Times.Once);
    }

    [Fact]
    public async Task UpdateCategory_ValidDto_ReturnsUpdatedCategory()
    {
        // Arrange
        var categoryId = 1;
        var dto = new UpdateCategoryDto
        {
            Name = "Updated Category",
            Description = "Updated Description"
        };

        var updatedCategoryDto = new KnowledgeBaseCategoryDto
        {
            Id = categoryId,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true
        };

        _mockKnowledgeBaseService.Setup(s => s.UpdateCategoryAsync(categoryId, dto.Name, dto.Description))
            .ReturnsAsync(updatedCategoryDto);

        // Act
        var result = await _controller.UpdateCategory(categoryId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.UpdateCategoryAsync(categoryId, dto.Name, dto.Description), Times.Once);
    }

    [Fact]
    public async Task DeleteCategory_ValidId_ReturnsSuccessResponse()
    {
        // Arrange
        var categoryId = 1;

        _mockKnowledgeBaseService.Setup(s => s.DeleteCategoryAsync(categoryId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCategory(categoryId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.DeleteCategoryAsync(categoryId), Times.Once);
    }

    [Fact]
    public async Task GetCategories_ReturnsAllCategories()
    {
        // Arrange
        var categories = new List<KnowledgeBaseCategoryDto>
        {
            new() { Id = 1, Name = "Category 1", IsActive = true },
            new() { Id = 2, Name = "Category 2", IsActive = true }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.GetCategoriesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetRootCategories_ReturnsRootCategories()
    {
        // Arrange
        var rootCategories = new List<KnowledgeBaseCategoryDto>
        {
            new() { Id = 1, Name = "Root Category 1", ParentCategoryId = null },
            new() { Id = 2, Name = "Root Category 2", ParentCategoryId = null }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetRootCategoriesAsync())
            .ReturnsAsync(rootCategories);

        // Act
        var result = await _controller.GetRootCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.GetRootCategoriesAsync(), Times.Once);
    }

    #endregion

    #region Analytics Tests

    [Fact]
    public async Task GetFeaturedDocuments_ReturnsFeaturedDocuments()
    {
        // Arrange
        var featuredDocuments = new List<KnowledgeBaseDocumentDto>
        {
            new() { Id = 1, Title = "Featured Document 1", IsFeatured = true },
            new() { Id = 2, Title = "Featured Document 2", IsFeatured = true }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetFeaturedDocumentsAsync())
            .ReturnsAsync(featuredDocuments);

        // Act
        var result = await _controller.GetFeaturedDocuments();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.GetFeaturedDocumentsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetRecentDocuments_ReturnsRecentDocuments()
    {
        // Arrange
        var count = 5;
        var recentDocuments = new List<KnowledgeBaseDocumentDto>
        {
            new() { Id = 1, Title = "Recent Document 1" },
            new() { Id = 2, Title = "Recent Document 2" }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetRecentDocumentsAsync(count))
            .ReturnsAsync(recentDocuments);

        // Act
        var result = await _controller.GetRecentDocuments(count);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.GetRecentDocumentsAsync(count), Times.Once);
    }

    [Fact]
    public async Task GetPopularDocuments_ReturnsPopularDocuments()
    {
        // Arrange
        var count = 5;
        var popularDocuments = new List<KnowledgeBaseDocumentDto>
        {
            new() { Id = 1, Title = "Popular Document 1", ViewCount = 100 },
            new() { Id = 2, Title = "Popular Document 2", ViewCount = 85 }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetPopularDocumentsAsync(count))
            .ReturnsAsync(popularDocuments);

        // Act
        var result = await _controller.GetPopularDocuments(count);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockKnowledgeBaseService.Verify(s => s.GetPopularDocumentsAsync(count), Times.Once);
    }

    #endregion
}
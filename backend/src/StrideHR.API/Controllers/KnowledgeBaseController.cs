using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Attributes;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.KnowledgeBase;

namespace StrideHR.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class KnowledgeBaseController : BaseController
{
    private readonly IKnowledgeBaseService _knowledgeBaseService;
    private readonly ILogger<KnowledgeBaseController> _logger;

    public KnowledgeBaseController(
        IKnowledgeBaseService knowledgeBaseService,
        ILogger<KnowledgeBaseController> logger)
    {
        _knowledgeBaseService = knowledgeBaseService;
        _logger = logger;
    }

    #region Document Management

    /// <summary>
    /// Create a new knowledge base document
    /// </summary>
    [HttpPost("documents")]
    [RequirePermission("KnowledgeBase", "Create")]
    public async Task<IActionResult> CreateDocument([FromBody] CreateKnowledgeBaseDocumentDto dto)
    {
        try
        {
            var authorId = GetCurrentEmployeeId();
            var document = await _knowledgeBaseService.CreateDocumentAsync(dto, authorId);
            return Success(document, "Document created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating knowledge base document");
            return Error("Failed to create document");
        }
    }

    /// <summary>
    /// Update an existing knowledge base document
    /// </summary>
    [HttpPut("documents/{id}")]
    [RequirePermission("KnowledgeBase", "Update")]
    public async Task<IActionResult> UpdateDocument(int id, [FromBody] UpdateKnowledgeBaseDocumentDto dto)
    {
        try
        {
            var authorId = GetCurrentEmployeeId();
            var document = await _knowledgeBaseService.UpdateDocumentAsync(id, dto, authorId);
            return Success(document, "Document updated successfully");
        }
        catch (ArgumentException)
        {
            return Error("Document not found");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating knowledge base document {DocumentId}", id);
            return Error("Failed to update document");
        }
    }

    /// <summary>
    /// Get a knowledge base document by ID
    /// </summary>
    [HttpGet("documents/{id}")]
    [RequirePermission("KnowledgeBase", "Read")]
    public async Task<IActionResult> GetDocument(int id)
    {
        try
        {
            var document = await _knowledgeBaseService.GetDocumentByIdAsync(id);
            if (document == null)
                return Error("Document not found");

            // Record the view
            var userId = GetCurrentEmployeeId();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            await _knowledgeBaseService.RecordDocumentViewAsync(id, userId, ipAddress, userAgent);

            return Success(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving knowledge base document {DocumentId}", id);
            return Error("Failed to retrieve document");
        }
    }

    /// <summary>
    /// Delete a knowledge base document
    /// </summary>
    [HttpDelete("documents/{id}")]
    [RequirePermission("KnowledgeBase", "Delete")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        try
        {
            var userId = GetCurrentEmployeeId();
            var result = await _knowledgeBaseService.DeleteDocumentAsync(id, userId);
            
            if (!result)
                return Error("Document not found");

            return Success("Document deleted successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting knowledge base document {DocumentId}", id);
            return Error("Failed to delete document");
        }
    }

    /// <summary>
    /// Search knowledge base documents
    /// </summary>
    [HttpPost("documents/search")]
    [RequirePermission("KnowledgeBase", "Read")]
    public async Task<IActionResult> SearchDocuments([FromBody] KnowledgeBaseSearchDto searchDto)
    {
        try
        {
            var documents = await _knowledgeBaseService.SearchDocumentsAsync(searchDto);
            var totalCount = await _knowledgeBaseService.GetSearchResultCountAsync(searchDto);

            var result = new
            {
                Documents = documents,
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
            };

            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching knowledge base documents");
            return Error("Failed to search documents");
        }
    }

    #endregion

    #region Document Approval Workflow

    /// <summary>
    /// Submit a document for approval
    /// </summary>
    [HttpPost("documents/{id}/submit-for-approval")]
    [RequirePermission("KnowledgeBase", "Update")]
    public async Task<IActionResult> SubmitForApproval(int id)
    {
        try
        {
            var authorId = GetCurrentEmployeeId();
            var result = await _knowledgeBaseService.SubmitForApprovalAsync(id, authorId);
            
            if (!result)
                return Error("Document not found");

            return Success("Document submitted for approval successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Error(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting document {DocumentId} for approval", id);
            return Error("Failed to submit document for approval");
        }
    }

    /// <summary>
    /// Approve a document
    /// </summary>
    [HttpPost("documents/approve")]
    [RequirePermission("KnowledgeBase", "Approve")]
    public async Task<IActionResult> ApproveDocument([FromBody] DocumentApprovalDto approvalDto)
    {
        try
        {
            var approverId = GetCurrentEmployeeId();
            var result = await _knowledgeBaseService.ApproveDocumentAsync(approvalDto, approverId);
            
            if (!result)
                return Error("Document not found");

            return Success("Document approved successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving document {DocumentId}", approvalDto.DocumentId);
            return Error("Failed to approve document");
        }
    }

    /// <summary>
    /// Reject a document
    /// </summary>
    [HttpPost("documents/reject")]
    [RequirePermission("KnowledgeBase", "Approve")]
    public async Task<IActionResult> RejectDocument([FromBody] DocumentApprovalDto approvalDto)
    {
        try
        {
            var approverId = GetCurrentEmployeeId();
            var result = await _knowledgeBaseService.RejectDocumentAsync(approvalDto, approverId);
            
            if (!result)
                return Error("Document not found");

            return Success("Document rejected successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting document {DocumentId}", approvalDto.DocumentId);
            return Error("Failed to reject document");
        }
    }

    /// <summary>
    /// Get documents pending approval
    /// </summary>
    [HttpGet("documents/pending-approval")]
    [RequirePermission("KnowledgeBase", "Approve")]
    public async Task<IActionResult> GetPendingApprovalDocuments()
    {
        try
        {
            var documents = await _knowledgeBaseService.GetPendingApprovalDocumentsAsync();
            return Success(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending approval documents");
            return Error("Failed to retrieve pending approval documents");
        }
    }

    #endregion

    #region Version Control

    /// <summary>
    /// Create a new version of a document
    /// </summary>
    [HttpPost("documents/{id}/versions")]
    [RequirePermission("KnowledgeBase", "Update")]
    public async Task<IActionResult> CreateNewVersion(int id, [FromBody] UpdateKnowledgeBaseDocumentDto dto)
    {
        try
        {
            var authorId = GetCurrentEmployeeId();
            var document = await _knowledgeBaseService.CreateNewVersionAsync(id, dto, authorId);
            return Success(document, "New version created successfully");
        }
        catch (ArgumentException)
        {
            return Error("Original document not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new version of document {DocumentId}", id);
            return Error("Failed to create new version");
        }
    }

    /// <summary>
    /// Get all versions of a document
    /// </summary>
    [HttpGet("documents/{id}/versions")]
    [RequirePermission("KnowledgeBase", "Read")]
    public async Task<IActionResult> GetDocumentVersions(int id)
    {
        try
        {
            var versions = await _knowledgeBaseService.GetDocumentVersionsAsync(id);
            return Success(versions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving versions for document {DocumentId}", id);
            return Error("Failed to retrieve document versions");
        }
    }

    /// <summary>
    /// Restore a specific version of a document
    /// </summary>
    [HttpPost("documents/{documentId}/versions/{versionId}/restore")]
    [RequirePermission("KnowledgeBase", "Update")]
    public async Task<IActionResult> RestoreVersion(int documentId, int versionId)
    {
        try
        {
            var userId = GetCurrentEmployeeId();
            var result = await _knowledgeBaseService.RestoreVersionAsync(documentId, versionId, userId);
            
            if (!result)
                return Error("Version not found");

            return Success("Version restored successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring version {VersionId} of document {DocumentId}", versionId, documentId);
            return Error("Failed to restore version");
        }
    }

    #endregion

    #region Category Management

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost("categories")]
    [RequirePermission("KnowledgeBase", "ManageCategories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        try
        {
            var category = await _knowledgeBaseService.CreateCategoryAsync(dto.Name, dto.Description, dto.ParentCategoryId);
            return Success(category, "Category created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating knowledge base category");
            return Error("Failed to create category");
        }
    }

    /// <summary>
    /// Update a category
    /// </summary>
    [HttpPut("categories/{id}")]
    [RequirePermission("KnowledgeBase", "ManageCategories")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
    {
        try
        {
            var category = await _knowledgeBaseService.UpdateCategoryAsync(id, dto.Name, dto.Description);
            return Success(category, "Category updated successfully");
        }
        catch (ArgumentException)
        {
            return Error("Category not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating knowledge base category {CategoryId}", id);
            return Error("Failed to update category");
        }
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    [HttpDelete("categories/{id}")]
    [RequirePermission("KnowledgeBase", "ManageCategories")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            var result = await _knowledgeBaseService.DeleteCategoryAsync(id);
            
            if (!result)
                return Error("Category not found");

            return Success("Category deleted successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting knowledge base category {CategoryId}", id);
            return Error("Failed to delete category");
        }
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet("categories")]
    [RequirePermission("KnowledgeBase", "Read")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _knowledgeBaseService.GetCategoriesAsync();
            return Success(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving knowledge base categories");
            return Error("Failed to retrieve categories");
        }
    }

    /// <summary>
    /// Get root categories with subcategories
    /// </summary>
    [HttpGet("categories/root")]
    [RequirePermission("KnowledgeBase", "Read")]
    public async Task<IActionResult> GetRootCategories()
    {
        try
        {
            var categories = await _knowledgeBaseService.GetRootCategoriesAsync();
            return Success(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving root knowledge base categories");
            return Error("Failed to retrieve root categories");
        }
    }

    /// <summary>
    /// Get a category by ID
    /// </summary>
    [HttpGet("categories/{id}")]
    [RequirePermission("KnowledgeBase", "Read")]
    public async Task<IActionResult> GetCategory(int id)
    {
        try
        {
            var category = await _knowledgeBaseService.GetCategoryByIdAsync(id);
            if (category == null)
                return Error("Category not found");

            return Success(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving knowledge base category {CategoryId}", id);
            return Error("Failed to retrieve category");
        }
    }

    #endregion

    #region Analytics and Popular Content

    /// <summary>
    /// Get featured documents
    /// </summary>
    [HttpGet("documents/featured")]
    [RequirePermission("KnowledgeBase", "Read")]
    public async Task<IActionResult> GetFeaturedDocuments()
    {
        try
        {
            var documents = await _knowledgeBaseService.GetFeaturedDocumentsAsync();
            return Success(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving featured documents");
            return Error("Failed to retrieve featured documents");
        }
    }

    /// <summary>
    /// Get recent documents
    /// </summary>
    [HttpGet("documents/recent")]
    [RequirePermission("KnowledgeBase", "Read")]
    public async Task<IActionResult> GetRecentDocuments([FromQuery] int count = 10)
    {
        try
        {
            var documents = await _knowledgeBaseService.GetRecentDocumentsAsync(count);
            return Success(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent documents");
            return Error("Failed to retrieve recent documents");
        }
    }

    /// <summary>
    /// Get popular documents
    /// </summary>
    [HttpGet("documents/popular")]
    [RequirePermission("KnowledgeBase", "Read")]
    public async Task<IActionResult> GetPopularDocuments([FromQuery] int count = 10)
    {
        try
        {
            var documents = await _knowledgeBaseService.GetPopularDocumentsAsync(count);
            return Success(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving popular documents");
            return Error("Failed to retrieve popular documents");
        }
    }

    #endregion

    #region Maintenance

    /// <summary>
    /// Get expired documents
    /// </summary>
    [HttpGet("documents/expired")]
    [RequirePermission("KnowledgeBase", "Manage")]
    public async Task<IActionResult> GetExpiredDocuments()
    {
        try
        {
            var documents = await _knowledgeBaseService.GetExpiredDocumentsAsync();
            return Success(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expired documents");
            return Error("Failed to retrieve expired documents");
        }
    }

    /// <summary>
    /// Get documents expiring in specified days
    /// </summary>
    [HttpGet("documents/expiring")]
    [RequirePermission("KnowledgeBase", "Manage")]
    public async Task<IActionResult> GetDocumentsExpiringInDays([FromQuery] int days = 30)
    {
        try
        {
            var documents = await _knowledgeBaseService.GetDocumentsExpiringInDaysAsync(days);
            return Success(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents expiring in {Days} days", days);
            return Error("Failed to retrieve expiring documents");
        }
    }

    /// <summary>
    /// Archive expired documents
    /// </summary>
    [HttpPost("documents/archive-expired")]
    [RequirePermission("KnowledgeBase", "Manage")]
    public async Task<IActionResult> ArchiveExpiredDocuments()
    {
        try
        {
            var result = await _knowledgeBaseService.ArchiveExpiredDocumentsAsync();
            return Success("Expired documents archived successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving expired documents");
            return Error("Failed to archive expired documents");
        }
    }

    #endregion
}

// DTOs for category management
public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
}

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
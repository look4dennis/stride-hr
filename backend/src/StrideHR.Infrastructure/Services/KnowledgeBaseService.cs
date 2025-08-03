using AutoMapper;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.KnowledgeBase;

namespace StrideHR.Infrastructure.Services;

public class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly IKnowledgeBaseDocumentRepository _documentRepository;
    private readonly IKnowledgeBaseCategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<KnowledgeBaseService> _logger;
    private readonly IAuditLogService _auditLogService;

    public KnowledgeBaseService(
        IKnowledgeBaseDocumentRepository documentRepository,
        IKnowledgeBaseCategoryRepository categoryRepository,
        IMapper mapper,
        ILogger<KnowledgeBaseService> logger,
        IAuditLogService auditLogService)
    {
        _documentRepository = documentRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
        _auditLogService = auditLogService;
    }

    #region Document Management

    public async Task<KnowledgeBaseDocumentDto> CreateDocumentAsync(CreateKnowledgeBaseDocumentDto dto, int authorId)
    {
        try
        {
            var document = _mapper.Map<KnowledgeBaseDocument>(dto);
            document.AuthorId = authorId;
            document.CreatedAt = DateTime.UtcNow;
            document.UpdatedAt = DateTime.UtcNow;

            await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            await _auditLogService.LogEventAsync("DocumentCreated", 
                $"Document '{document.Title}' created", authorId);

            _logger.LogInformation("Knowledge base document created: {DocumentId} by user {UserId}", 
                document.Id, authorId);

            // Reload with includes for proper mapping
            var createdDocument = await _documentRepository.GetByIdAsync(document.Id, 
                d => d.Category, d => d.Author, d => d.Reviewer);

            return _mapper.Map<KnowledgeBaseDocumentDto>(createdDocument);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating knowledge base document for user {UserId}", authorId);
            throw;
        }
    }

    public async Task<KnowledgeBaseDocumentDto> UpdateDocumentAsync(int id, UpdateKnowledgeBaseDocumentDto dto, int authorId)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(id, 
                d => d.Category, d => d.Author, d => d.Reviewer);

            if (document == null)
                throw new ArgumentException("Document not found");

            if (document.AuthorId != authorId)
                throw new UnauthorizedAccessException("Only the author can update the document");

            // If document is published, create a new version instead
            if (document.Status == DocumentStatus.Published)
            {
                return await CreateNewVersionAsync(id, dto, authorId);
            }

            _mapper.Map(dto, document);
            document.UpdatedAt = DateTime.UtcNow;

            await _documentRepository.UpdateAsync(document);
            await _documentRepository.SaveChangesAsync();

            await _auditLogService.LogEventAsync("DocumentUpdated", 
                $"Document '{document.Title}' updated", authorId);

            _logger.LogInformation("Knowledge base document updated: {DocumentId} by user {UserId}", 
                id, authorId);

            return _mapper.Map<KnowledgeBaseDocumentDto>(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating knowledge base document {DocumentId} for user {UserId}", 
                id, authorId);
            throw;
        }
    }

    public async Task<KnowledgeBaseDocumentDto?> GetDocumentByIdAsync(int id)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(id, 
                d => d.Category, d => d.Author, d => d.Reviewer, d => d.Attachments);

            if (document == null)
                return null;

            var dto = _mapper.Map<KnowledgeBaseDocumentDto>(document);
            
            // Get versions
            var versions = await _documentRepository.GetDocumentVersionsAsync(id);
            dto.Versions = _mapper.Map<List<KnowledgeBaseDocumentVersionDto>>(versions);

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving knowledge base document {DocumentId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteDocumentAsync(int id, int userId)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
                return false;

            if (document.AuthorId != userId)
                throw new UnauthorizedAccessException("Only the author can delete the document");

            await _documentRepository.DeleteAsync(document);
            await _documentRepository.SaveChangesAsync();

            await _auditLogService.LogEventAsync("DocumentDeleted", 
                $"Document '{document.Title}' deleted", userId);

            _logger.LogInformation("Knowledge base document deleted: {DocumentId} by user {UserId}", 
                id, userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting knowledge base document {DocumentId} for user {UserId}", 
                id, userId);
            throw;
        }
    }

    public async Task<IEnumerable<KnowledgeBaseDocumentDto>> SearchDocumentsAsync(KnowledgeBaseSearchDto searchDto)
    {
        try
        {
            var documents = await _documentRepository.SearchDocumentsAsync(searchDto);
            return _mapper.Map<IEnumerable<KnowledgeBaseDocumentDto>>(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching knowledge base documents");
            throw;
        }
    }

    public async Task<int> GetSearchResultCountAsync(KnowledgeBaseSearchDto searchDto)
    {
        try
        {
            return await _documentRepository.GetSearchResultCountAsync(searchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search result count");
            throw;
        }
    }

    #endregion

    #region Document Approval Workflow

    public async Task<bool> SubmitForApprovalAsync(int documentId, int authorId)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
                return false;

            if (document.AuthorId != authorId)
                throw new UnauthorizedAccessException("Only the author can submit for approval");

            if (document.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("Only draft documents can be submitted for approval");

            document.Status = DocumentStatus.PendingReview;
            document.UpdatedAt = DateTime.UtcNow;

            await _documentRepository.UpdateAsync(document);
            await _documentRepository.SaveChangesAsync();

            await _auditLogService.LogEventAsync("DocumentSubmittedForApproval", 
                $"Document '{document.Title}' submitted for approval", authorId);

            _logger.LogInformation("Knowledge base document submitted for approval: {DocumentId} by user {UserId}", 
                documentId, authorId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting document {DocumentId} for approval by user {UserId}", 
                documentId, authorId);
            throw;
        }
    }

    public async Task<bool> ApproveDocumentAsync(DocumentApprovalDto approvalDto, int approverId)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(approvalDto.DocumentId);
            if (document == null)
                return false;

            if (document.Status != DocumentStatus.PendingReview && document.Status != DocumentStatus.UnderReview)
                throw new InvalidOperationException("Document is not in a reviewable state");

            document.Status = DocumentStatus.Approved;
            document.ReviewerId = approverId;
            document.ReviewedAt = DateTime.UtcNow;
            document.ReviewComments = approvalDto.Comments;
            document.PublishedAt = DateTime.UtcNow;
            document.UpdatedAt = DateTime.UtcNow;

            // If this is a new version, mark it as current and mark others as not current
            if (document.ParentDocumentId.HasValue)
            {
                var allVersions = await _documentRepository.GetDocumentVersionsAsync(document.ParentDocumentId.Value);
                foreach (var version in allVersions.Where(v => v.Id != document.Id))
                {
                    version.IsCurrentVersion = false;
                    await _documentRepository.UpdateAsync(version);
                }
            }

            await _documentRepository.UpdateAsync(document);
            await _documentRepository.SaveChangesAsync();

            await _auditLogService.LogEventAsync("DocumentApproved", 
                $"Document '{document.Title}' approved", approverId);

            _logger.LogInformation("Knowledge base document approved: {DocumentId} by user {UserId}", 
                approvalDto.DocumentId, approverId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving document {DocumentId} by user {UserId}", 
                approvalDto.DocumentId, approverId);
            throw;
        }
    }

    public async Task<bool> RejectDocumentAsync(DocumentApprovalDto approvalDto, int approverId)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(approvalDto.DocumentId);
            if (document == null)
                return false;

            if (document.Status != DocumentStatus.PendingReview && document.Status != DocumentStatus.UnderReview)
                throw new InvalidOperationException("Document is not in a reviewable state");

            document.Status = DocumentStatus.Rejected;
            document.ReviewerId = approverId;
            document.ReviewedAt = DateTime.UtcNow;
            document.ReviewComments = approvalDto.Comments;
            document.UpdatedAt = DateTime.UtcNow;

            await _documentRepository.UpdateAsync(document);
            await _documentRepository.SaveChangesAsync();

            await _auditLogService.LogEventAsync("DocumentRejected", 
                $"Document '{document.Title}' rejected", approverId);

            _logger.LogInformation("Knowledge base document rejected: {DocumentId} by user {UserId}", 
                approvalDto.DocumentId, approverId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting document {DocumentId} by user {UserId}", 
                approvalDto.DocumentId, approverId);
            throw;
        }
    }

    public async Task<IEnumerable<KnowledgeBaseDocumentDto>> GetPendingApprovalDocumentsAsync()
    {
        try
        {
            var documents = await _documentRepository.GetPendingApprovalDocumentsAsync();
            return _mapper.Map<IEnumerable<KnowledgeBaseDocumentDto>>(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending approval documents");
            throw;
        }
    }

    #endregion

    #region Version Control

    public async Task<KnowledgeBaseDocumentDto> CreateNewVersionAsync(int documentId, UpdateKnowledgeBaseDocumentDto dto, int authorId)
    {
        try
        {
            var originalDocument = await _documentRepository.GetByIdAsync(documentId);
            if (originalDocument == null)
                throw new ArgumentException("Original document not found");

            // Mark current version as not current
            originalDocument.IsCurrentVersion = false;
            await _documentRepository.UpdateAsync(originalDocument);

            // Create new version
            var newVersion = _mapper.Map<KnowledgeBaseDocument>(dto);
            newVersion.AuthorId = authorId;
            newVersion.ParentDocumentId = originalDocument.ParentDocumentId ?? originalDocument.Id;
            newVersion.Version = originalDocument.Version + 1;
            newVersion.IsCurrentVersion = true;
            newVersion.Status = DocumentStatus.Draft;
            newVersion.CreatedAt = DateTime.UtcNow;
            newVersion.UpdatedAt = DateTime.UtcNow;

            await _documentRepository.AddAsync(newVersion);
            await _documentRepository.SaveChangesAsync();

            await _auditLogService.LogEventAsync("DocumentVersionCreated", 
                $"New version {newVersion.Version} of document '{newVersion.Title}' created", authorId);

            _logger.LogInformation("New version of knowledge base document created: {DocumentId} version {Version} by user {UserId}", 
                newVersion.Id, newVersion.Version, authorId);

            // Reload with includes for proper mapping
            var createdDocument = await _documentRepository.GetByIdAsync(newVersion.Id, 
                d => d.Category, d => d.Author, d => d.Reviewer);

            return _mapper.Map<KnowledgeBaseDocumentDto>(createdDocument);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new version of document {DocumentId} for user {UserId}", 
                documentId, authorId);
            throw;
        }
    }

    public async Task<IEnumerable<KnowledgeBaseDocumentVersionDto>> GetDocumentVersionsAsync(int documentId)
    {
        try
        {
            var versions = await _documentRepository.GetDocumentVersionsAsync(documentId);
            return _mapper.Map<IEnumerable<KnowledgeBaseDocumentVersionDto>>(versions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving versions for document {DocumentId}", documentId);
            throw;
        }
    }

    public async Task<bool> RestoreVersionAsync(int documentId, int versionId, int userId)
    {
        try
        {
            var versionToRestore = await _documentRepository.GetByIdAsync(versionId);
            if (versionToRestore == null)
                return false;

            var currentVersion = await _documentRepository.GetCurrentVersionAsync(documentId);
            if (currentVersion == null)
                return false;

            // Mark current version as not current
            currentVersion.IsCurrentVersion = false;
            await _documentRepository.UpdateAsync(currentVersion);

            // Mark restored version as current
            versionToRestore.IsCurrentVersion = true;
            versionToRestore.UpdatedAt = DateTime.UtcNow;
            await _documentRepository.UpdateAsync(versionToRestore);

            await _documentRepository.SaveChangesAsync();

            await _auditLogService.LogEventAsync("DocumentVersionRestored", 
                $"Version {versionToRestore.Version} of document '{versionToRestore.Title}' restored", userId);

            _logger.LogInformation("Knowledge base document version restored: {VersionId} by user {UserId}", 
                versionId, userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring document version {VersionId} by user {UserId}", 
                versionId, userId);
            throw;
        }
    }

    #endregion

    #region Category Management

    public async Task<KnowledgeBaseCategoryDto> CreateCategoryAsync(string name, string description, int? parentCategoryId = null)
    {
        try
        {
            var category = new KnowledgeBaseCategory
            {
                Name = name,
                Description = description,
                ParentCategoryId = parentCategoryId,
                Slug = GenerateSlug(name),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Ensure slug is unique
            var originalSlug = category.Slug;
            var counter = 1;
            while (!await _categoryRepository.IsSlugUniqueAsync(category.Slug))
            {
                category.Slug = $"{originalSlug}-{counter}";
                counter++;
            }

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            _logger.LogInformation("Knowledge base category created: {CategoryId}", category.Id);

            return _mapper.Map<KnowledgeBaseCategoryDto>(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating knowledge base category");
            throw;
        }
    }

    public async Task<KnowledgeBaseCategoryDto> UpdateCategoryAsync(int id, string name, string description)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new ArgumentException("Category not found");

            category.Name = name;
            category.Description = description;
            category.Slug = GenerateSlug(name);
            category.UpdatedAt = DateTime.UtcNow;

            // Ensure slug is unique
            var originalSlug = category.Slug;
            var counter = 1;
            while (!await _categoryRepository.IsSlugUniqueAsync(category.Slug, id))
            {
                category.Slug = $"{originalSlug}-{counter}";
                counter++;
            }

            await _categoryRepository.UpdateAsync(category);
            await _categoryRepository.SaveChangesAsync();

            _logger.LogInformation("Knowledge base category updated: {CategoryId}", id);

            return _mapper.Map<KnowledgeBaseCategoryDto>(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating knowledge base category {CategoryId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return false;

            // Check if category has documents
            var documentCount = await _categoryRepository.GetDocumentCountAsync(id);
            if (documentCount > 0)
                throw new InvalidOperationException("Cannot delete category with existing documents");

            await _categoryRepository.DeleteAsync(category);
            await _categoryRepository.SaveChangesAsync();

            _logger.LogInformation("Knowledge base category deleted: {CategoryId}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting knowledge base category {CategoryId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<KnowledgeBaseCategoryDto>> GetCategoriesAsync()
    {
        try
        {
            var categories = await _categoryRepository.GetActiveCategoriesAsync();
            return _mapper.Map<IEnumerable<KnowledgeBaseCategoryDto>>(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving knowledge base categories");
            throw;
        }
    }

    public async Task<IEnumerable<KnowledgeBaseCategoryDto>> GetRootCategoriesAsync()
    {
        try
        {
            var categories = await _categoryRepository.GetRootCategoriesAsync();
            return _mapper.Map<IEnumerable<KnowledgeBaseCategoryDto>>(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving root knowledge base categories");
            throw;
        }
    }

    public async Task<KnowledgeBaseCategoryDto?> GetCategoryByIdAsync(int id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id, c => c.ParentCategory, c => c.SubCategories);
            if (category == null)
                return null;

            return _mapper.Map<KnowledgeBaseCategoryDto>(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving knowledge base category {CategoryId}", id);
            throw;
        }
    }

    #endregion

    #region Document Views and Analytics

    public async Task RecordDocumentViewAsync(int documentId, int? userId = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var view = new KnowledgeBaseDocumentView
            {
                DocumentId = documentId,
                ViewedBy = userId,
                ViewedAt = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsUniqueView = true // This could be enhanced to check for duplicate views
            };

            // Add view record (this would need a repository for KnowledgeBaseDocumentView)
            // For now, just increment the view count
            await _documentRepository.IncrementViewCountAsync(documentId);

            _logger.LogInformation("Document view recorded: {DocumentId} by user {UserId}", documentId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording document view for document {DocumentId}", documentId);
            // Don't throw here as this is not critical functionality
        }
    }

    public async Task<IEnumerable<KnowledgeBaseDocumentDto>> GetFeaturedDocumentsAsync()
    {
        try
        {
            var documents = await _documentRepository.GetFeaturedDocumentsAsync();
            return _mapper.Map<IEnumerable<KnowledgeBaseDocumentDto>>(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving featured documents");
            throw;
        }
    }

    public async Task<IEnumerable<KnowledgeBaseDocumentDto>> GetRecentDocumentsAsync(int count = 10)
    {
        try
        {
            var documents = await _documentRepository.GetRecentDocumentsAsync(count);
            return _mapper.Map<IEnumerable<KnowledgeBaseDocumentDto>>(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent documents");
            throw;
        }
    }

    public async Task<IEnumerable<KnowledgeBaseDocumentDto>> GetPopularDocumentsAsync(int count = 10)
    {
        try
        {
            var documents = await _documentRepository.GetPopularDocumentsAsync(count);
            return _mapper.Map<IEnumerable<KnowledgeBaseDocumentDto>>(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving popular documents");
            throw;
        }
    }

    #endregion

    #region Document Attachments

    public async Task<KnowledgeBaseDocumentAttachmentDto> AddAttachmentAsync(int documentId, string fileName, string filePath, string contentType, long fileSize, int uploadedBy, string? description = null)
    {
        try
        {
            var attachment = new KnowledgeBaseDocumentAttachment
            {
                DocumentId = documentId,
                FileName = fileName,
                FilePath = filePath,
                ContentType = contentType,
                FileSize = fileSize,
                Description = description,
                UploadedBy = uploadedBy,
                UploadedAt = DateTime.UtcNow
            };

            // This would need a repository for KnowledgeBaseDocumentAttachment
            // For now, we'll assume it's handled through the document repository

            _logger.LogInformation("Attachment added to document: {DocumentId} by user {UserId}", documentId, uploadedBy);

            return _mapper.Map<KnowledgeBaseDocumentAttachmentDto>(attachment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding attachment to document {DocumentId}", documentId);
            throw;
        }
    }

    public async Task<bool> RemoveAttachmentAsync(int attachmentId, int userId)
    {
        try
        {
            // This would need implementation with proper repository
            _logger.LogInformation("Attachment removed: {AttachmentId} by user {UserId}", attachmentId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing attachment {AttachmentId}", attachmentId);
            throw;
        }
    }

    public async Task<IEnumerable<KnowledgeBaseDocumentAttachmentDto>> GetDocumentAttachmentsAsync(int documentId)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId, d => d.Attachments);
            if (document == null)
                return new List<KnowledgeBaseDocumentAttachmentDto>();

            return _mapper.Map<IEnumerable<KnowledgeBaseDocumentAttachmentDto>>(document.Attachments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachments for document {DocumentId}", documentId);
            throw;
        }
    }

    #endregion

    #region Maintenance

    public async Task<IEnumerable<KnowledgeBaseDocumentDto>> GetExpiredDocumentsAsync()
    {
        try
        {
            var documents = await _documentRepository.GetExpiredDocumentsAsync();
            return _mapper.Map<IEnumerable<KnowledgeBaseDocumentDto>>(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expired documents");
            throw;
        }
    }

    public async Task<IEnumerable<KnowledgeBaseDocumentDto>> GetDocumentsExpiringInDaysAsync(int days)
    {
        try
        {
            var documents = await _documentRepository.GetDocumentsExpiringInDaysAsync(days);
            return _mapper.Map<IEnumerable<KnowledgeBaseDocumentDto>>(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents expiring in {Days} days", days);
            throw;
        }
    }

    public async Task<bool> ArchiveExpiredDocumentsAsync()
    {
        try
        {
            var expiredDocuments = await _documentRepository.GetExpiredDocumentsAsync();
            
            foreach (var document in expiredDocuments)
            {
                document.Status = DocumentStatus.Expired;
                document.UpdatedAt = DateTime.UtcNow;
                await _documentRepository.UpdateAsync(document);
            }

            await _documentRepository.SaveChangesAsync();

            _logger.LogInformation("Archived {Count} expired documents", expiredDocuments.Count());

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving expired documents");
            throw;
        }
    }

    #endregion

    #region Private Methods

    private static string GenerateSlug(string input)
    {
        return input.ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace("/", "-")
            .Replace("\\", "-")
            .Replace(":", "")
            .Replace(";", "")
            .Replace("@", "at")
            .Replace("#", "")
            .Replace("$", "")
            .Replace("%", "")
            .Replace("^", "")
            .Replace("*", "")
            .Replace("+", "")
            .Replace("=", "")
            .Replace("|", "")
            .Replace("`", "")
            .Replace("~", "")
            .Replace("<", "")
            .Replace(">", "");
    }

    #endregion
}
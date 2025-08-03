using StrideHR.Core.Models.KnowledgeBase;

namespace StrideHR.Core.Interfaces.Services;

public interface IKnowledgeBaseService
{
    // Document Management
    Task<KnowledgeBaseDocumentDto> CreateDocumentAsync(CreateKnowledgeBaseDocumentDto dto, int authorId);
    Task<KnowledgeBaseDocumentDto> UpdateDocumentAsync(int id, UpdateKnowledgeBaseDocumentDto dto, int authorId);
    Task<KnowledgeBaseDocumentDto?> GetDocumentByIdAsync(int id);
    Task<bool> DeleteDocumentAsync(int id, int userId);
    Task<IEnumerable<KnowledgeBaseDocumentDto>> SearchDocumentsAsync(KnowledgeBaseSearchDto searchDto);
    Task<int> GetSearchResultCountAsync(KnowledgeBaseSearchDto searchDto);

    // Document Approval Workflow
    Task<bool> SubmitForApprovalAsync(int documentId, int authorId);
    Task<bool> ApproveDocumentAsync(DocumentApprovalDto approvalDto, int approverId);
    Task<bool> RejectDocumentAsync(DocumentApprovalDto approvalDto, int approverId);
    Task<IEnumerable<KnowledgeBaseDocumentDto>> GetPendingApprovalDocumentsAsync();

    // Version Control
    Task<KnowledgeBaseDocumentDto> CreateNewVersionAsync(int documentId, UpdateKnowledgeBaseDocumentDto dto, int authorId);
    Task<IEnumerable<KnowledgeBaseDocumentVersionDto>> GetDocumentVersionsAsync(int documentId);
    Task<bool> RestoreVersionAsync(int documentId, int versionId, int userId);

    // Category Management
    Task<KnowledgeBaseCategoryDto> CreateCategoryAsync(string name, string description, int? parentCategoryId = null);
    Task<KnowledgeBaseCategoryDto> UpdateCategoryAsync(int id, string name, string description);
    Task<bool> DeleteCategoryAsync(int id);
    Task<IEnumerable<KnowledgeBaseCategoryDto>> GetCategoriesAsync();
    Task<IEnumerable<KnowledgeBaseCategoryDto>> GetRootCategoriesAsync();
    Task<KnowledgeBaseCategoryDto?> GetCategoryByIdAsync(int id);

    // Document Views and Analytics
    Task RecordDocumentViewAsync(int documentId, int? userId = null, string? ipAddress = null, string? userAgent = null);
    Task<IEnumerable<KnowledgeBaseDocumentDto>> GetFeaturedDocumentsAsync();
    Task<IEnumerable<KnowledgeBaseDocumentDto>> GetRecentDocumentsAsync(int count = 10);
    Task<IEnumerable<KnowledgeBaseDocumentDto>> GetPopularDocumentsAsync(int count = 10);

    // Document Attachments
    Task<KnowledgeBaseDocumentAttachmentDto> AddAttachmentAsync(int documentId, string fileName, string filePath, string contentType, long fileSize, int uploadedBy, string? description = null);
    Task<bool> RemoveAttachmentAsync(int attachmentId, int userId);
    Task<IEnumerable<KnowledgeBaseDocumentAttachmentDto>> GetDocumentAttachmentsAsync(int documentId);

    // Maintenance
    Task<IEnumerable<KnowledgeBaseDocumentDto>> GetExpiredDocumentsAsync();
    Task<IEnumerable<KnowledgeBaseDocumentDto>> GetDocumentsExpiringInDaysAsync(int days);
    Task<bool> ArchiveExpiredDocumentsAsync();
}
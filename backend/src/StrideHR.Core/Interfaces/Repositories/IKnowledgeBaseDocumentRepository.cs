using StrideHR.Core.Entities;
using StrideHR.Core.Models.KnowledgeBase;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IKnowledgeBaseDocumentRepository : IRepository<KnowledgeBaseDocument>
{
    Task<IEnumerable<KnowledgeBaseDocument>> SearchDocumentsAsync(KnowledgeBaseSearchDto searchDto);
    Task<int> GetSearchResultCountAsync(KnowledgeBaseSearchDto searchDto);
    Task<IEnumerable<KnowledgeBaseDocument>> GetDocumentsByAuthorAsync(int authorId);
    Task<IEnumerable<KnowledgeBaseDocument>> GetDocumentsByCategoryAsync(int categoryId);
    Task<IEnumerable<KnowledgeBaseDocument>> GetFeaturedDocumentsAsync();
    Task<IEnumerable<KnowledgeBaseDocument>> GetRecentDocumentsAsync(int count = 10);
    Task<IEnumerable<KnowledgeBaseDocument>> GetPopularDocumentsAsync(int count = 10);
    Task<IEnumerable<KnowledgeBaseDocument>> GetDocumentVersionsAsync(int documentId);
    Task<KnowledgeBaseDocument?> GetCurrentVersionAsync(int parentDocumentId);
    Task<IEnumerable<KnowledgeBaseDocument>> GetPendingApprovalDocumentsAsync();
    Task<IEnumerable<KnowledgeBaseDocument>> GetExpiredDocumentsAsync();
    Task<IEnumerable<KnowledgeBaseDocument>> GetDocumentsExpiringInDaysAsync(int days);
    Task IncrementViewCountAsync(int documentId);
    Task IncrementDownloadCountAsync(int documentId);
}
using StrideHR.Core.Models.Chatbot;

namespace StrideHR.Core.Interfaces.Services;

public interface IChatbotKnowledgeBaseService
{
    Task<KnowledgeBaseDto> CreateArticleAsync(CreateKnowledgeBaseDto dto, int createdBy);
    Task<KnowledgeBaseDto> GetArticleByIdAsync(int id);
    Task<List<KnowledgeBaseDto>> SearchArticlesAsync(string searchTerm);
    Task<List<KnowledgeBaseDto>> GetArticlesByCategoryAsync(string category);
    Task<List<KnowledgeBaseDto>> GetMostViewedArticlesAsync(int count = 10);
    Task<List<KnowledgeBaseDto>> GetMostHelpfulArticlesAsync(int count = 10);
    Task<KnowledgeBaseDto> UpdateArticleAsync(int id, CreateKnowledgeBaseDto dto, int updatedBy);
    Task<bool> DeleteArticleAsync(int id);
    Task<bool> ProvideArticleFeedbackAsync(int articleId, int employeeId, bool isHelpful, string? comments = null);
    Task<List<KnowledgeBaseDto>> GetRelevantArticlesAsync(string intent, Dictionary<string, object>? entities = null);
}
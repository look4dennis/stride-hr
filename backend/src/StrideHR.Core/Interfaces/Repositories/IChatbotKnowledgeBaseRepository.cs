using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IChatbotKnowledgeBaseRepository : IRepository<ChatbotKnowledgeBase>
{
    Task<List<ChatbotKnowledgeBase>> SearchByKeywordsAsync(string[] keywords);
    Task<List<ChatbotKnowledgeBase>> SearchByCategoryAsync(string category);
    Task<List<ChatbotKnowledgeBase>> SearchByContentAsync(string searchTerm);
    Task<List<ChatbotKnowledgeBase>> GetByStatusAsync(KnowledgeBaseStatus status);
    Task<List<ChatbotKnowledgeBase>> GetMostViewedAsync(int count = 10);
    Task<List<ChatbotKnowledgeBase>> GetMostHelpfulAsync(int count = 10);
    Task IncrementViewCountAsync(int id);
    Task UpdateHelpfulnessAsync(int id, bool isHelpful);
}
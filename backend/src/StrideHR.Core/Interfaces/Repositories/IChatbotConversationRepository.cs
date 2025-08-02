using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IChatbotConversationRepository : IRepository<ChatbotConversation>
{
    Task<ChatbotConversation?> GetBySessionIdAsync(string sessionId);
    Task<ChatbotConversation?> GetWithMessagesAsync(int id);
    Task<ChatbotConversation?> GetActiveConversationByEmployeeAsync(int employeeId);
    Task<List<ChatbotConversation>> GetConversationsByEmployeeAsync(int employeeId, int page = 1, int pageSize = 20);
    Task<List<ChatbotConversation>> GetEscalatedConversationsAsync();
    Task<List<ChatbotConversation>> GetConversationsByStatusAsync(ChatbotConversationStatus status);
    Task<Dictionary<string, int>> GetConversationStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
}
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IChatbotMessageRepository : IRepository<ChatbotMessage>
{
    Task<List<ChatbotMessage>> GetMessagesByConversationAsync(int conversationId);
    Task<List<ChatbotMessage>> GetUnprocessedMessagesAsync();
    Task<List<ChatbotMessage>> GetMessagesByIntentAsync(string intent, int limit = 100);
    Task<Dictionary<string, int>> GetIntentStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<decimal> GetAverageConfidenceScoreAsync(string? intent = null);
}
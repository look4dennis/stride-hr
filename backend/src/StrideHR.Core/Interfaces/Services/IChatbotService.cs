using StrideHR.Core.Models.Chatbot;

namespace StrideHR.Core.Interfaces.Services;

public interface IChatbotService
{
    Task<ChatbotResponseDto> ProcessMessageAsync(SendMessageDto dto, int employeeId);
    Task<ChatbotConversationDto> StartConversationAsync(int employeeId);
    Task<ChatbotConversationDto> GetConversationAsync(string sessionId);
    Task<ChatbotConversationDto> EndConversationAsync(string sessionId, int? satisfactionRating = null, string? feedbackComments = null);
    Task<ChatbotConversationDto> EscalateToHumanAsync(EscalateToHumanDto dto, int employeeId);
    Task<List<ChatbotConversationDto>> GetEmployeeConversationsAsync(int employeeId, int page = 1, int pageSize = 20);
    Task<List<ChatbotConversationDto>> GetEscalatedConversationsAsync();
    Task<ChatbotAnalyticsDto> GetAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
}
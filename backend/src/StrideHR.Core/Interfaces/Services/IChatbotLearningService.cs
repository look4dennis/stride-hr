using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Services;

public interface IChatbotLearningService
{
    Task RecordInteractionAsync(string userInput, string botResponse, string intent, decimal confidenceScore, int employeeId, string sessionId);
    Task RecordFeedbackAsync(string sessionId, bool wasHelpful, string? userFeedback = null, string? correctResponse = null);
    Task ProcessLearningDataAsync();
    Task<List<ChatbotLearningData>> GetLowConfidenceInteractionsAsync(decimal threshold = 0.7m);
    Task<List<ChatbotLearningData>> GetUnhelpfulInteractionsAsync();
    Task<Dictionary<string, decimal>> GetIntentAccuracyMetricsAsync();
    Task<List<string>> GetImprovementSuggestionsAsync();
    Task RetrainModelAsync();
}
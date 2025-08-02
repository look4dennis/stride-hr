using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Services;

public interface INaturalLanguageProcessingService
{
    Task<string> DetectIntentAsync(string message, Dictionary<string, object>? context = null);
    Task<Dictionary<string, object>> ExtractEntitiesAsync(string message);
    Task<decimal> GetConfidenceScoreAsync(string message, string intent);
    Task<string> GenerateResponseAsync(string intent, Dictionary<string, object>? entities = null, Dictionary<string, object>? context = null);
    Task<List<string>> GetSuggestedResponsesAsync(string intent);
    Task<bool> ShouldEscalateAsync(string message, string intent, decimal confidenceScore);
    Task TrainModelAsync(List<ChatbotLearningData> trainingData);
    Task<bool> IsModelTrainedAsync();
}
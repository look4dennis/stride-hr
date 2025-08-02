using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IChatbotLearningDataRepository : IRepository<ChatbotLearningData>
{
    Task<List<ChatbotLearningData>> GetUnprocessedLearningDataAsync();
    Task<List<ChatbotLearningData>> GetTrainingDataAsync();
    Task<List<ChatbotLearningData>> GetByIntentAsync(string intent);
    Task<List<ChatbotLearningData>> GetLowConfidenceInteractionsAsync(decimal threshold = 0.7m);
    Task<List<ChatbotLearningData>> GetUnhelpfulInteractionsAsync();
    Task MarkAsProcessedAsync(int id);
}
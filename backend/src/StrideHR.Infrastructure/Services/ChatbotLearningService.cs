using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Chatbot;

namespace StrideHR.Infrastructure.Services;

public class ChatbotLearningService : IChatbotLearningService
{
    private readonly IChatbotLearningDataRepository _learningDataRepository;
    private readonly IChatbotMessageRepository _messageRepository;
    private readonly INaturalLanguageProcessingService _nlpService;
    private readonly IUnitOfWork _unitOfWork;

    public ChatbotLearningService(
        IChatbotLearningDataRepository learningDataRepository,
        IChatbotMessageRepository messageRepository,
        INaturalLanguageProcessingService nlpService,
        IUnitOfWork unitOfWork)
    {
        _learningDataRepository = learningDataRepository;
        _messageRepository = messageRepository;
        _nlpService = nlpService;
        _unitOfWork = unitOfWork;
    }

    public async Task RecordInteractionAsync(string userInput, string botResponse, string intent, decimal confidenceScore, int employeeId, string sessionId)
    {
        var learningData = new ChatbotLearningData
        {
            UserInput = userInput,
            BotResponse = botResponse,
            Intent = intent,
            ConfidenceScore = confidenceScore,
            EmployeeId = employeeId,
            SessionId = sessionId,
            InteractionDate = DateTime.UtcNow,
            WasHelpful = true, // Default to true, will be updated based on feedback
            IsTrainingData = false
        };

        await _learningDataRepository.AddAsync(learningData);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RecordFeedbackAsync(string sessionId, bool wasHelpful, string? userFeedback = null, string? correctResponse = null)
    {
        var learningDataList = await _learningDataRepository.GetAllAsync();
        var sessionData = learningDataList.Where(ld => ld.SessionId == sessionId).ToList();

        foreach (var data in sessionData)
        {
            data.WasHelpful = wasHelpful;
            data.UserFeedback = userFeedback;
            data.CorrectResponse = correctResponse;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ProcessLearningDataAsync()
    {
        var unprocessedData = await _learningDataRepository.GetUnprocessedLearningDataAsync();

        foreach (var data in unprocessedData)
        {
            // Mark low confidence interactions for review
            if (data.ConfidenceScore < 0.7m)
            {
                // These interactions need human review
                continue;
            }

            // Mark helpful interactions as training data
            if (data.WasHelpful && data.ConfidenceScore > 0.8m)
            {
                data.IsTrainingData = true;
            }

            await _learningDataRepository.MarkAsProcessedAsync(data.Id);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<ChatbotLearningData>> GetLowConfidenceInteractionsAsync(decimal threshold = 0.7m)
    {
        return await _learningDataRepository.GetLowConfidenceInteractionsAsync(threshold);
    }

    public async Task<List<ChatbotLearningData>> GetUnhelpfulInteractionsAsync()
    {
        return await _learningDataRepository.GetUnhelpfulInteractionsAsync();
    }

    public async Task<Dictionary<string, decimal>> GetIntentAccuracyMetricsAsync()
    {
        var allData = await _learningDataRepository.GetAllAsync();
        
        var intentMetrics = allData
            .GroupBy(ld => ld.Intent)
            .ToDictionary(
                g => g.Key,
                g => g.Count(x => x.WasHelpful) / (decimal)g.Count()
            );

        return intentMetrics;
    }

    public async Task<List<string>> GetImprovementSuggestionsAsync()
    {
        var suggestions = new List<string>();

        // Get low confidence interactions
        var lowConfidenceData = await GetLowConfidenceInteractionsAsync();
        if (lowConfidenceData.Any())
        {
            var topLowConfidenceIntents = lowConfidenceData
                .GroupBy(ld => ld.Intent)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => g.Key);

            foreach (var intent in topLowConfidenceIntents)
            {
                suggestions.Add($"Improve training data for '{intent}' intent - {lowConfidenceData.Count(ld => ld.Intent == intent)} low confidence interactions");
            }
        }

        // Get unhelpful interactions
        var unhelpfulData = await GetUnhelpfulInteractionsAsync();
        if (unhelpfulData.Any())
        {
            var topUnhelpfulIntents = unhelpfulData
                .GroupBy(ld => ld.Intent)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => g.Key);

            foreach (var intent in topUnhelpfulIntents)
            {
                suggestions.Add($"Review responses for '{intent}' intent - {unhelpfulData.Count(ld => ld.Intent == intent)} unhelpful interactions");
            }
        }

        // Check for missing knowledge base articles
        var intentStats = await _messageRepository.GetIntentStatisticsAsync();
        var topIntents = intentStats.OrderByDescending(kvp => kvp.Value).Take(5);

        foreach (var intent in topIntents)
        {
            suggestions.Add($"Consider adding more knowledge base articles for '{intent.Key}' - {intent.Value} interactions");
        }

        return suggestions;
    }

    public async Task RetrainModelAsync()
    {
        // Get all training data
        var trainingData = await _learningDataRepository.GetTrainingDataAsync();
        
        // Retrain the NLP model
        await _nlpService.TrainModelAsync(trainingData);

        // Mark all unprocessed data as processed
        var unprocessedData = await _learningDataRepository.GetUnprocessedLearningDataAsync();
        foreach (var data in unprocessedData)
        {
            await _learningDataRepository.MarkAsProcessedAsync(data.Id);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
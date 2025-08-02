using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ChatbotLearningDataRepository : Repository<ChatbotLearningData>, IChatbotLearningDataRepository
{
    public ChatbotLearningDataRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<ChatbotLearningData>> GetUnprocessedLearningDataAsync()
    {
        return await _context.ChatbotLearningData
            .Include(ld => ld.Employee)
            .Where(ld => !ld.ProcessedAt.HasValue)
            .OrderBy(ld => ld.InteractionDate)
            .ToListAsync();
    }

    public async Task<List<ChatbotLearningData>> GetTrainingDataAsync()
    {
        return await _context.ChatbotLearningData
            .Include(ld => ld.Employee)
            .Where(ld => ld.IsTrainingData)
            .OrderBy(ld => ld.InteractionDate)
            .ToListAsync();
    }

    public async Task<List<ChatbotLearningData>> GetByIntentAsync(string intent)
    {
        return await _context.ChatbotLearningData
            .Include(ld => ld.Employee)
            .Where(ld => ld.Intent == intent)
            .OrderByDescending(ld => ld.InteractionDate)
            .ToListAsync();
    }

    public async Task<List<ChatbotLearningData>> GetLowConfidenceInteractionsAsync(decimal threshold = 0.7m)
    {
        return await _context.ChatbotLearningData
            .Include(ld => ld.Employee)
            .Where(ld => ld.ConfidenceScore < threshold)
            .OrderBy(ld => ld.ConfidenceScore)
            .ToListAsync();
    }

    public async Task<List<ChatbotLearningData>> GetUnhelpfulInteractionsAsync()
    {
        return await _context.ChatbotLearningData
            .Include(ld => ld.Employee)
            .Where(ld => !ld.WasHelpful)
            .OrderByDescending(ld => ld.InteractionDate)
            .ToListAsync();
    }

    public async Task MarkAsProcessedAsync(int id)
    {
        var learningData = await _context.ChatbotLearningData.FindAsync(id);
        if (learningData != null)
        {
            learningData.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
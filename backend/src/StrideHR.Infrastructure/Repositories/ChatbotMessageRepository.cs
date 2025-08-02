using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ChatbotMessageRepository : Repository<ChatbotMessage>, IChatbotMessageRepository
{
    public ChatbotMessageRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<ChatbotMessage>> GetMessagesByConversationAsync(int conversationId)
    {
        return await _context.ChatbotMessages
            .Include(m => m.Conversation)
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<List<ChatbotMessage>> GetUnprocessedMessagesAsync()
    {
        return await _context.ChatbotMessages
            .Include(m => m.Conversation)
            .Where(m => m.RequiresAction && !m.IsProcessed)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<List<ChatbotMessage>> GetMessagesByIntentAsync(string intent, int limit = 100)
    {
        return await _context.ChatbotMessages
            .Include(m => m.Conversation)
            .Where(m => m.Intent == intent)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetIntentStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.ChatbotMessages
            .Where(m => !string.IsNullOrEmpty(m.Intent));

        if (fromDate.HasValue)
            query = query.Where(m => m.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(m => m.Timestamp <= toDate.Value);

        var stats = await query
            .GroupBy(m => m.Intent)
            .Select(g => new { Intent = g.Key!, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToDictionaryAsync(x => x.Intent, x => x.Count);

        return stats;
    }

    public async Task<decimal> GetAverageConfidenceScoreAsync(string? intent = null)
    {
        var query = _context.ChatbotMessages
            .Where(m => m.ConfidenceScore.HasValue);

        if (!string.IsNullOrEmpty(intent))
            query = query.Where(m => m.Intent == intent);

        var averageScore = await query.AverageAsync(m => m.ConfidenceScore!.Value);
        return averageScore;
    }
}
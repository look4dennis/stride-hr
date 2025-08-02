using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ChatbotKnowledgeBaseRepository : Repository<ChatbotKnowledgeBase>, IChatbotKnowledgeBaseRepository
{
    public ChatbotKnowledgeBaseRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<ChatbotKnowledgeBase>> SearchByKeywordsAsync(string[] keywords)
    {
        var query = _context.ChatbotKnowledgeBases
            .Include(kb => kb.UpdatedByEmployee)
            .Where(kb => kb.Status == KnowledgeBaseStatus.Published);

        foreach (var keyword in keywords)
        {
            query = query.Where(kb => kb.Keywords.Contains(keyword) || 
                                     kb.Title.Contains(keyword) || 
                                     kb.Content.Contains(keyword));
        }

        return await query
            .OrderByDescending(kb => kb.Priority)
            .ThenByDescending(kb => kb.ViewCount)
            .ToListAsync();
    }

    public async Task<List<ChatbotKnowledgeBase>> SearchByCategoryAsync(string category)
    {
        return await _context.ChatbotKnowledgeBases
            .Include(kb => kb.UpdatedByEmployee)
            .Where(kb => kb.Category == category && kb.Status == KnowledgeBaseStatus.Published)
            .OrderByDescending(kb => kb.Priority)
            .ThenByDescending(kb => kb.ViewCount)
            .ToListAsync();
    }

    public async Task<List<ChatbotKnowledgeBase>> SearchByContentAsync(string searchTerm)
    {
        return await _context.ChatbotKnowledgeBases
            .Include(kb => kb.UpdatedByEmployee)
            .Where(kb => (kb.Title.Contains(searchTerm) || 
                         kb.Content.Contains(searchTerm) || 
                         kb.Keywords.Any(k => k.Contains(searchTerm))) &&
                        kb.Status == KnowledgeBaseStatus.Published)
            .OrderByDescending(kb => kb.Priority)
            .ThenByDescending(kb => kb.ViewCount)
            .ToListAsync();
    }

    public async Task<List<ChatbotKnowledgeBase>> GetByStatusAsync(KnowledgeBaseStatus status)
    {
        return await _context.ChatbotKnowledgeBases
            .Include(kb => kb.UpdatedByEmployee)
            .Where(kb => kb.Status == status)
            .OrderByDescending(kb => kb.LastUpdated)
            .ToListAsync();
    }

    public async Task<List<ChatbotKnowledgeBase>> GetMostViewedAsync(int count = 10)
    {
        return await _context.ChatbotKnowledgeBases
            .Include(kb => kb.UpdatedByEmployee)
            .Where(kb => kb.Status == KnowledgeBaseStatus.Published)
            .OrderByDescending(kb => kb.ViewCount)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<ChatbotKnowledgeBase>> GetMostHelpfulAsync(int count = 10)
    {
        return await _context.ChatbotKnowledgeBases
            .Include(kb => kb.UpdatedByEmployee)
            .Where(kb => kb.Status == KnowledgeBaseStatus.Published)
            .OrderByDescending(kb => kb.HelpfulCount)
            .ThenBy(kb => kb.NotHelpfulCount)
            .Take(count)
            .ToListAsync();
    }

    public async Task IncrementViewCountAsync(int id)
    {
        var article = await _context.ChatbotKnowledgeBases.FindAsync(id);
        if (article != null)
        {
            article.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateHelpfulnessAsync(int id, bool isHelpful)
    {
        var article = await _context.ChatbotKnowledgeBases.FindAsync(id);
        if (article != null)
        {
            if (isHelpful)
                article.HelpfulCount++;
            else
                article.NotHelpfulCount++;
            
            await _context.SaveChangesAsync();
        }
    }
}
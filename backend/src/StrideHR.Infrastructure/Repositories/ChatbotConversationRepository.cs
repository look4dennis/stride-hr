using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ChatbotConversationRepository : Repository<ChatbotConversation>, IChatbotConversationRepository
{
    public ChatbotConversationRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<ChatbotConversation?> GetBySessionIdAsync(string sessionId)
    {
        return await _context.ChatbotConversations
            .Include(c => c.Employee)
            .Include(c => c.EscalatedToEmployee)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId);
    }

    public async Task<ChatbotConversation?> GetWithMessagesAsync(int id)
    {
        return await _context.ChatbotConversations
            .Include(c => c.Employee)
            .Include(c => c.EscalatedToEmployee)
            .Include(c => c.Messages.OrderBy(m => m.Timestamp))
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ChatbotConversation?> GetActiveConversationByEmployeeAsync(int employeeId)
    {
        return await _context.ChatbotConversations
            .Include(c => c.Messages.OrderBy(m => m.Timestamp))
            .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.Status == ChatbotConversationStatus.Active);
    }

    public async Task<List<ChatbotConversation>> GetConversationsByEmployeeAsync(int employeeId, int page = 1, int pageSize = 20)
    {
        return await _context.ChatbotConversations
            .Include(c => c.Employee)
            .Include(c => c.EscalatedToEmployee)
            .Where(c => c.EmployeeId == employeeId)
            .OrderByDescending(c => c.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<ChatbotConversation>> GetEscalatedConversationsAsync()
    {
        return await _context.ChatbotConversations
            .Include(c => c.Employee)
            .Include(c => c.EscalatedToEmployee)
            .Where(c => c.EscalatedToHuman)
            .OrderByDescending(c => c.StartedAt)
            .ToListAsync();
    }

    public async Task<List<ChatbotConversation>> GetConversationsByStatusAsync(ChatbotConversationStatus status)
    {
        return await _context.ChatbotConversations
            .Include(c => c.Employee)
            .Include(c => c.EscalatedToEmployee)
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.StartedAt)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetConversationStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.ChatbotConversations.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(c => c.StartedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(c => c.StartedAt <= toDate.Value);

        var stats = await query
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        return stats;
    }
}
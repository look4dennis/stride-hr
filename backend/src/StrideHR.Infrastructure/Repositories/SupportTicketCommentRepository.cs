using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class SupportTicketCommentRepository : Repository<SupportTicketComment>, ISupportTicketCommentRepository
{
    public SupportTicketCommentRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<SupportTicketComment>> GetByTicketIdAsync(int ticketId)
    {
        return await _context.SupportTicketComments
            .Include(c => c.Author)
            .Where(c => c.SupportTicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<SupportTicketComment>> GetPublicCommentsByTicketIdAsync(int ticketId)
    {
        return await _context.SupportTicketComments
            .Include(c => c.Author)
            .Where(c => c.SupportTicketId == ticketId && !c.IsInternal)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }
}
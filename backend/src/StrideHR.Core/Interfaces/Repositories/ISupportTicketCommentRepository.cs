using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ISupportTicketCommentRepository : IRepository<SupportTicketComment>
{
    Task<List<SupportTicketComment>> GetByTicketIdAsync(int ticketId);
    Task<List<SupportTicketComment>> GetPublicCommentsByTicketIdAsync(int ticketId);
}
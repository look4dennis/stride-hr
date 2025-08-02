using StrideHR.Core.Entities;
using StrideHR.Core.Models.SupportTicket;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ISupportTicketRepository : IRepository<SupportTicket>
{
    Task<SupportTicket?> GetByTicketNumberAsync(string ticketNumber);
    Task<SupportTicket?> GetWithDetailsAsync(int id);
    Task<(List<SupportTicket> Tickets, int TotalCount)> SearchAsync(SupportTicketSearchCriteria criteria);
    Task<List<SupportTicket>> GetByRequesterIdAsync(int requesterId);
    Task<List<SupportTicket>> GetByAssignedToIdAsync(int assignedToId);
    Task<List<SupportTicket>> GetOverdueTicketsAsync();
    Task<string> GenerateTicketNumberAsync();
    Task<SupportTicketAnalyticsDto> GetAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
}
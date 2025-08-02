using StrideHR.Core.Models.SupportTicket;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Services;

public interface ISupportTicketService
{
    Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketDto dto, int requesterId);
    Task<SupportTicketDto> GetTicketByIdAsync(int id);
    Task<SupportTicketDto?> GetTicketByNumberAsync(string ticketNumber);
    Task<(List<SupportTicketDto> Tickets, int TotalCount)> SearchTicketsAsync(SupportTicketSearchCriteria criteria);
    Task<SupportTicketDto> UpdateTicketAsync(int id, UpdateSupportTicketDto dto, int updatedById);
    Task<SupportTicketDto> AssignTicketAsync(int id, int assignedToId, int assignedById);
    Task<SupportTicketDto> UpdateStatusAsync(int id, SupportTicketStatus status, int updatedById, string? reason = null);
    Task<SupportTicketDto> ResolveTicketAsync(int id, string resolution, int resolvedById);
    Task<SupportTicketDto> CloseTicketAsync(int id, int closedById, int? satisfactionRating = null, string? feedbackComments = null);
    Task<SupportTicketDto> ReopenTicketAsync(int id, string reason, int reopenedById);
    Task<List<SupportTicketDto>> GetMyTicketsAsync(int employeeId);
    Task<List<SupportTicketDto>> GetAssignedTicketsAsync(int employeeId);
    Task<List<SupportTicketDto>> GetOverdueTicketsAsync();
    Task<SupportTicketCommentDto> AddCommentAsync(int ticketId, CreateSupportTicketCommentDto dto, int authorId);
    Task<List<SupportTicketCommentDto>> GetTicketCommentsAsync(int ticketId, bool includeInternal = false);
    Task<SupportTicketAnalyticsDto> GetAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task DeleteTicketAsync(int id);
}
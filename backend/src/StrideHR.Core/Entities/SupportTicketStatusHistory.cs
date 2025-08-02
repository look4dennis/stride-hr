using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class SupportTicketStatusHistory : BaseEntity
{
    public int SupportTicketId { get; set; }
    public SupportTicketStatus FromStatus { get; set; }
    public SupportTicketStatus ToStatus { get; set; }
    public int ChangedById { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Reason { get; set; }

    // Navigation Properties
    public virtual SupportTicket SupportTicket { get; set; } = null!;
    public virtual Employee ChangedBy { get; set; } = null!;
}
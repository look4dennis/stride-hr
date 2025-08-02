using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class SupportTicket : BaseEntity
{
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SupportTicketCategory Category { get; set; }
    public SupportTicketPriority Priority { get; set; }
    public SupportTicketStatus Status { get; set; }
    public int RequesterId { get; set; }
    public int? AssignedToId { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? Resolution { get; set; }
    public bool RequiresRemoteAccess { get; set; }
    public string? RemoteAccessDetails { get; set; }
    public int? AssetId { get; set; }
    public string? AttachmentPath { get; set; }
    public TimeSpan? ResolutionTime { get; set; }
    public int? SatisfactionRating { get; set; }
    public string? FeedbackComments { get; set; }

    // Navigation Properties
    public virtual Employee Requester { get; set; } = null!;
    public virtual Employee? AssignedTo { get; set; }
    public virtual Asset? Asset { get; set; }
    public virtual ICollection<SupportTicketComment> Comments { get; set; } = new List<SupportTicketComment>();
    public virtual ICollection<SupportTicketStatusHistory> StatusHistory { get; set; } = new List<SupportTicketStatusHistory>();
}
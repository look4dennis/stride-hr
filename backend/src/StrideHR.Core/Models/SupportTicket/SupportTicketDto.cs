using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.SupportTicket;

public class SupportTicketDto
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SupportTicketCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public SupportTicketPriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public SupportTicketStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int RequesterId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string RequesterEmail { get; set; } = string.Empty;
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? Resolution { get; set; }
    public bool RequiresRemoteAccess { get; set; }
    public string? RemoteAccessDetails { get; set; }
    public int? AssetId { get; set; }
    public string? AssetName { get; set; }
    public string? AttachmentPath { get; set; }
    public TimeSpan? ResolutionTime { get; set; }
    public int? SatisfactionRating { get; set; }
    public string? FeedbackComments { get; set; }
    public int CommentsCount { get; set; }
}
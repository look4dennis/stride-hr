using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.SupportTicket;

public class UpdateSupportTicketDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public SupportTicketCategory? Category { get; set; }
    public SupportTicketPriority? Priority { get; set; }
    public SupportTicketStatus? Status { get; set; }
    public int? AssignedToId { get; set; }
    public string? Resolution { get; set; }
    public bool? RequiresRemoteAccess { get; set; }
    public string? RemoteAccessDetails { get; set; }
    public int? AssetId { get; set; }
}
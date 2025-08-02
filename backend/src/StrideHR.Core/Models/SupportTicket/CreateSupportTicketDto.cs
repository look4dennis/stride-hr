using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.SupportTicket;

public class CreateSupportTicketDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SupportTicketCategory Category { get; set; }
    public SupportTicketPriority Priority { get; set; }
    public bool RequiresRemoteAccess { get; set; }
    public string? RemoteAccessDetails { get; set; }
    public int? AssetId { get; set; }
    public string? AttachmentPath { get; set; }
}
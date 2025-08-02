namespace StrideHR.Core.Entities;

public class SupportTicketComment : BaseEntity
{
    public int SupportTicketId { get; set; }
    public int AuthorId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public string? AttachmentPath { get; set; }

    // Navigation Properties
    public virtual SupportTicket SupportTicket { get; set; } = null!;
    public virtual Employee Author { get; set; } = null!;
}
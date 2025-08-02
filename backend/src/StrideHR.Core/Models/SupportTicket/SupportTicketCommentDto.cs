namespace StrideHR.Core.Models.SupportTicket;

public class SupportTicketCommentDto
{
    public int Id { get; set; }
    public int SupportTicketId { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsInternal { get; set; }
    public string? AttachmentPath { get; set; }
}

public class CreateSupportTicketCommentDto
{
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public string? AttachmentPath { get; set; }
}
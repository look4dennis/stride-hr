using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.SupportTicket;

public class SupportTicketSearchCriteria
{
    public string? SearchTerm { get; set; }
    public SupportTicketStatus? Status { get; set; }
    public SupportTicketCategory? Category { get; set; }
    public SupportTicketPriority? Priority { get; set; }
    public int? RequesterId { get; set; }
    public int? AssignedToId { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? ResolvedFrom { get; set; }
    public DateTime? ResolvedTo { get; set; }
    public bool? RequiresRemoteAccess { get; set; }
    public int? AssetId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
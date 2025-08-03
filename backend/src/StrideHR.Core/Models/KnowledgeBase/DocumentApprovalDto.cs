using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.KnowledgeBase;

public class DocumentApprovalDto
{
    public int DocumentId { get; set; }
    public ApprovalAction Action { get; set; }
    public string? Comments { get; set; }
}
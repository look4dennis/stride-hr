using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ChatbotKnowledgeBaseFeedback : BaseEntity
{
    public int KnowledgeBaseId { get; set; }
    public int EmployeeId { get; set; }
    public bool IsHelpful { get; set; }
    public string? Comments { get; set; }
    public DateTime ProvidedAt { get; set; }

    // Navigation Properties
    public virtual ChatbotKnowledgeBase KnowledgeBase { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
}
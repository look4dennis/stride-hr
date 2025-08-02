using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ChatbotKnowledgeBase : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string[] Keywords { get; set; } = Array.Empty<string>();
    public string[] Tags { get; set; } = Array.Empty<string>();
    public KnowledgeBaseStatus Status { get; set; }
    public int Priority { get; set; }
    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public new int UpdatedBy { get; set; }
    public string? RelatedArticleIds { get; set; } // JSON array of related article IDs

    // Navigation Properties
    public virtual Employee UpdatedByEmployee { get; set; } = null!;
    public virtual ICollection<ChatbotKnowledgeBaseFeedback> Feedback { get; set; } = new List<ChatbotKnowledgeBaseFeedback>();
}
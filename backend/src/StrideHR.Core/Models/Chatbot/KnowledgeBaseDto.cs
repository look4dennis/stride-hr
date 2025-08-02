using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Chatbot;

public class KnowledgeBaseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string[] Keywords { get; set; } = Array.Empty<string>();
    public string[] Tags { get; set; } = Array.Empty<string>();
    public KnowledgeBaseStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public int UpdatedBy { get; set; }
    public string UpdatedByName { get; set; } = string.Empty;
    public List<int>? RelatedArticleIds { get; set; }
    public decimal HelpfulnessRatio { get; set; }
}
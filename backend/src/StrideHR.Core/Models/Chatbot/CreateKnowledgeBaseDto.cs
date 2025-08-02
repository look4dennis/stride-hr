using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Chatbot;

public class CreateKnowledgeBaseDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string[] Keywords { get; set; } = Array.Empty<string>();
    public string[] Tags { get; set; } = Array.Empty<string>();
    public KnowledgeBaseStatus Status { get; set; } = KnowledgeBaseStatus.Draft;
    public int Priority { get; set; } = 1;
    public List<int>? RelatedArticleIds { get; set; }
}
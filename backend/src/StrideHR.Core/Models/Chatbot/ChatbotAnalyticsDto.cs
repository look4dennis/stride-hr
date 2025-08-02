namespace StrideHR.Core.Models.Chatbot;

public class ChatbotAnalyticsDto
{
    public int TotalConversations { get; set; }
    public int ActiveConversations { get; set; }
    public int CompletedConversations { get; set; }
    public int EscalatedConversations { get; set; }
    public decimal EscalationRate { get; set; }
    public decimal AverageConversationDuration { get; set; }
    public decimal AverageMessagesPerConversation { get; set; }
    public decimal AverageSatisfactionRating { get; set; }
    public Dictionary<string, int> TopIntents { get; set; } = new();
    public Dictionary<string, int> TopCategories { get; set; } = new();
    public List<string> CommonIssues { get; set; } = new();
    public Dictionary<string, decimal> IntentConfidenceScores { get; set; } = new();
}
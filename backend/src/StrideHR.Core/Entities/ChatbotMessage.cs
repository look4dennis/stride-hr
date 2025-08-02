using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ChatbotMessage : BaseEntity
{
    public int ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public ChatbotMessageType MessageType { get; set; }
    public ChatbotMessageSender Sender { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Intent { get; set; }
    public decimal? ConfidenceScore { get; set; }
    public string? Entities { get; set; } // JSON string for extracted entities
    public bool RequiresAction { get; set; }
    public string? ActionType { get; set; }
    public string? ActionData { get; set; } // JSON string for action parameters
    public bool IsProcessed { get; set; }

    // Navigation Properties
    public virtual ChatbotConversation Conversation { get; set; } = null!;
}
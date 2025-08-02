using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Chatbot;

public class ChatbotMessageDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public ChatbotMessageType MessageType { get; set; }
    public string MessageTypeName { get; set; } = string.Empty;
    public ChatbotMessageSender Sender { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Intent { get; set; }
    public decimal? ConfidenceScore { get; set; }
    public Dictionary<string, object>? Entities { get; set; }
    public bool RequiresAction { get; set; }
    public string? ActionType { get; set; }
    public Dictionary<string, object>? ActionData { get; set; }
    public bool IsProcessed { get; set; }
}
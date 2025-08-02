using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Chatbot;

public class ChatbotResponseDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public ChatbotMessageType MessageType { get; set; }
    public string? Intent { get; set; }
    public decimal ConfidenceScore { get; set; }
    public Dictionary<string, object>? Entities { get; set; }
    public bool RequiresAction { get; set; }
    public string? ActionType { get; set; }
    public Dictionary<string, object>? ActionData { get; set; }
    public List<string>? SuggestedResponses { get; set; }
    public bool ShouldEscalate { get; set; }
    public string? EscalationReason { get; set; }
}
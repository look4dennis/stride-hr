using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Chatbot;

public class SendMessageDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ChatbotMessageType MessageType { get; set; } = ChatbotMessageType.Text;
    public Dictionary<string, object>? Context { get; set; }
}
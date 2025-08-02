using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Chatbot;

public class ChatbotConversationDto
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public ChatbotConversationStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool EscalatedToHuman { get; set; }
    public int? EscalatedToEmployeeId { get; set; }
    public string? EscalatedToEmployeeName { get; set; }
    public string? EscalationReason { get; set; }
    public int? SatisfactionRating { get; set; }
    public string? FeedbackComments { get; set; }
    public int MessageCount { get; set; }
    public List<ChatbotMessageDto> Messages { get; set; } = new();
}
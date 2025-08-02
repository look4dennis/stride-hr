using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ChatbotConversation : BaseEntity
{
    public string SessionId { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public ChatbotConversationStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool EscalatedToHuman { get; set; }
    public int? EscalatedToEmployeeId { get; set; }
    public string? EscalationReason { get; set; }
    public int? SatisfactionRating { get; set; }
    public string? FeedbackComments { get; set; }

    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee? EscalatedToEmployee { get; set; }
    public virtual ICollection<ChatbotMessage> Messages { get; set; } = new List<ChatbotMessage>();
}
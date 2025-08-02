using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ChatbotLearningData : BaseEntity
{
    public string UserInput { get; set; } = string.Empty;
    public string BotResponse { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public bool WasHelpful { get; set; }
    public string? UserFeedback { get; set; }
    public string? CorrectResponse { get; set; }
    public DateTime InteractionDate { get; set; }
    public int EmployeeId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public bool IsTrainingData { get; set; }
    public DateTime? ProcessedAt { get; set; }

    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
}
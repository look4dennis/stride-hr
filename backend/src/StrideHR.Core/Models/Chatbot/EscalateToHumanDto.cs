namespace StrideHR.Core.Models.Chatbot;

public class EscalateToHumanDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int? PreferredAgentId { get; set; }
    public string? AdditionalContext { get; set; }
}
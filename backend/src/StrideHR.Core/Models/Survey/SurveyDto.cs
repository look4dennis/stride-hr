using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Survey;

public class SurveyDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SurveyType Type { get; set; }
    public SurveyStatus Status { get; set; }
    public bool IsAnonymous { get; set; }
    public bool AllowMultipleResponses { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int CreatedByEmployeeId { get; set; }
    public string CreatedByEmployeeName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public bool IsGlobal { get; set; }
    public string? Instructions { get; set; }
    public string? ThankYouMessage { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public bool RequireAuthentication { get; set; }
    public bool ShowProgressBar { get; set; }
    public bool RandomizeQuestions { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Statistics
    public int TotalQuestions { get; set; }
    public int TotalResponses { get; set; }
    public int CompletedResponses { get; set; }
    public double ResponseRate { get; set; }
    public double CompletionRate { get; set; }
    public TimeSpan? AverageCompletionTime { get; set; }

    public List<SurveyQuestionDto> Questions { get; set; } = new();
}
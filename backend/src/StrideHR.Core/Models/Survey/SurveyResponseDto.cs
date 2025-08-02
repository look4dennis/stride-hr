using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Survey;

public class SurveyResponseDto
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    public string SurveyTitle { get; set; } = string.Empty;
    public int? RespondentEmployeeId { get; set; }
    public string? RespondentEmployeeName { get; set; }
    public string? AnonymousId { get; set; }
    public SurveyResponseStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int CompletionPercentage { get; set; }
    public TimeSpan? TimeTaken { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<SurveyAnswerDto> Answers { get; set; } = new();
}
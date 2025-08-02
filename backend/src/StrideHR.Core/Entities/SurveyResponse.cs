using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class SurveyResponse : BaseEntity
{
    public int SurveyId { get; set; }
    public int? RespondentEmployeeId { get; set; }
    public string? AnonymousId { get; set; }
    public SurveyResponseStatus Status { get; set; } = SurveyResponseStatus.NotStarted;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int CompletionPercentage { get; set; } = 0;
    public TimeSpan? TimeTaken { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceInfo { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Survey Survey { get; set; } = null!;
    public virtual Employee? RespondentEmployee { get; set; }
    public virtual ICollection<SurveyAnswer> Answers { get; set; } = new List<SurveyAnswer>();
}
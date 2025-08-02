using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class Survey : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SurveyType Type { get; set; }
    public SurveyStatus Status { get; set; } = SurveyStatus.Draft;
    public bool IsAnonymous { get; set; } = false;
    public bool AllowMultipleResponses { get; set; } = false;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int CreatedByEmployeeId { get; set; }
    public int BranchId { get; set; }
    public bool IsGlobal { get; set; } = false;
    public string? Instructions { get; set; }
    public string? ThankYouMessage { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public bool RequireAuthentication { get; set; } = true;
    public bool ShowProgressBar { get; set; } = true;
    public bool RandomizeQuestions { get; set; } = false;
    public string? Tags { get; set; }

    // Navigation Properties
    public virtual Employee CreatedByEmployee { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
    public virtual ICollection<SurveyQuestion> Questions { get; set; } = new List<SurveyQuestion>();
    public virtual ICollection<SurveyResponse> Responses { get; set; } = new List<SurveyResponse>();
    public virtual ICollection<SurveyDistribution> Distributions { get; set; } = new List<SurveyDistribution>();
    public virtual ICollection<SurveyAnalytics> Analytics { get; set; } = new List<SurveyAnalytics>();
}
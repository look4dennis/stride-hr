namespace StrideHR.Core.Entities;

public class SurveyDistribution : BaseEntity
{
    public int SurveyId { get; set; }
    public int? TargetEmployeeId { get; set; }
    public int? TargetBranchId { get; set; }
    public int? TargetDepartmentId { get; set; }
    public string? TargetRole { get; set; }
    public string? TargetCriteria { get; set; } // JSON for complex targeting
    public DateTime? SentAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int ReminderCount { get; set; } = 0;
    public DateTime? LastReminderSent { get; set; }
    public bool IsActive { get; set; } = true;
    public string? InvitationMessage { get; set; }
    public string? AccessToken { get; set; }

    // Navigation Properties
    public virtual Survey Survey { get; set; } = null!;
    public virtual Employee? TargetEmployee { get; set; }
    public virtual Branch? TargetBranch { get; set; }
}
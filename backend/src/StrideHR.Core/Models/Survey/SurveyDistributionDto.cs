namespace StrideHR.Core.Models.Survey;

public class SurveyDistributionDto
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    public int? TargetEmployeeId { get; set; }
    public string? TargetEmployeeName { get; set; }
    public int? TargetBranchId { get; set; }
    public string? TargetBranchName { get; set; }
    public int? TargetDepartmentId { get; set; }
    public string? TargetRole { get; set; }
    public string? TargetCriteria { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int ReminderCount { get; set; }
    public DateTime? LastReminderSent { get; set; }
    public bool IsActive { get; set; }
    public string? InvitationMessage { get; set; }
    public string? AccessToken { get; set; }
}
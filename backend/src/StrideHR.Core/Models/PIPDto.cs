using StrideHR.Core.Enums;

namespace StrideHR.Core.Models;

public class PIPDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public int? HRId { get; set; }
    public string? HRName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PerformanceIssues { get; set; } = string.Empty;
    public string ExpectedImprovements { get; set; } = string.Empty;
    public string SupportProvided { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ReviewFrequencyDays { get; set; }
    public PIPStatus Status { get; set; }
    public string? FinalOutcome { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ManagerNotes { get; set; }
    public string? HRNotes { get; set; }
    public List<PIPGoalDto> Goals { get; set; } = new();
    public List<PIPReviewDto> Reviews { get; set; } = new();
}

public class CreatePIPDto
{
    public int EmployeeId { get; set; }
    public int ManagerId { get; set; }
    public int? HRId { get; set; }
    public int? PerformanceReviewId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PerformanceIssues { get; set; } = string.Empty;
    public string ExpectedImprovements { get; set; } = string.Empty;
    public string SupportProvided { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ReviewFrequencyDays { get; set; } = 30;
    public List<CreatePIPGoalDto> Goals { get; set; } = new();
}

public class UpdatePIPDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? PerformanceIssues { get; set; }
    public string? ExpectedImprovements { get; set; }
    public string? SupportProvided { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? ReviewFrequencyDays { get; set; }
    public PIPStatus? Status { get; set; }
    public string? FinalOutcome { get; set; }
    public bool? IsSuccessful { get; set; }
    public string? ManagerNotes { get; set; }
    public string? HRNotes { get; set; }
}

public class PIPGoalDto
{
    public int Id { get; set; }
    public int PIPId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MeasurableObjective { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public PerformanceGoalStatus Status { get; set; }
    public decimal ProgressPercentage { get; set; }
    public string? EmployeeComments { get; set; }
    public string? ManagerComments { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsAchieved { get; set; }
}

public class CreatePIPGoalDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MeasurableObjective { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
}

public class PIPReviewDto
{
    public int Id { get; set; }
    public int PIPId { get; set; }
    public int ReviewedBy { get; set; }
    public string ReviewedByName { get; set; } = string.Empty;
    public DateTime ReviewDate { get; set; }
    public string ProgressSummary { get; set; } = string.Empty;
    public string EmployeeFeedback { get; set; } = string.Empty;
    public string ManagerFeedback { get; set; } = string.Empty;
    public string? ChallengesFaced { get; set; }
    public string? SupportProvided { get; set; }
    public string? NextSteps { get; set; }
    public PerformanceRating OverallProgress { get; set; }
    public bool IsOnTrack { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public string? RecommendedActions { get; set; }
}

public class CreatePIPReviewDto
{
    public int PIPId { get; set; }
    public string ProgressSummary { get; set; } = string.Empty;
    public string EmployeeFeedback { get; set; } = string.Empty;
    public string ManagerFeedback { get; set; } = string.Empty;
    public string? ChallengesFaced { get; set; }
    public string? SupportProvided { get; set; }
    public string? NextSteps { get; set; }
    public PerformanceRating OverallProgress { get; set; }
    public bool IsOnTrack { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public string? RecommendedActions { get; set; }
}
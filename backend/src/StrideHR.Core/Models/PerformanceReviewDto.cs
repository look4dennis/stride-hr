using StrideHR.Core.Enums;

namespace StrideHR.Core.Models;

public class PerformanceReviewDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public string ReviewPeriod { get; set; } = string.Empty;
    public DateTime ReviewStartDate { get; set; }
    public DateTime ReviewEndDate { get; set; }
    public DateTime DueDate { get; set; }
    public PerformanceReviewStatus Status { get; set; }
    public PerformanceRating? OverallRating { get; set; }
    public decimal? OverallScore { get; set; }
    public string? EmployeeSelfAssessment { get; set; }
    public string? ManagerComments { get; set; }
    public string? DevelopmentPlan { get; set; }
    public string? StrengthsIdentified { get; set; }
    public string? AreasForImprovement { get; set; }
    public bool RequiresPIP { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedByName { get; set; }
    public List<PerformanceFeedbackDto> Feedbacks { get; set; } = new();
    public List<PerformanceGoalDto> Goals { get; set; } = new();
}

public class CreatePerformanceReviewDto
{
    public int EmployeeId { get; set; }
    public string ReviewPeriod { get; set; } = string.Empty;
    public DateTime ReviewStartDate { get; set; }
    public DateTime ReviewEndDate { get; set; }
    public DateTime DueDate { get; set; }
}

public class UpdatePerformanceReviewDto
{
    public PerformanceReviewStatus? Status { get; set; }
    public PerformanceRating? OverallRating { get; set; }
    public decimal? OverallScore { get; set; }
    public string? EmployeeSelfAssessment { get; set; }
    public string? ManagerComments { get; set; }
    public string? DevelopmentPlan { get; set; }
    public string? StrengthsIdentified { get; set; }
    public string? AreasForImprovement { get; set; }
    public bool? RequiresPIP { get; set; }
}
using StrideHR.Core.Enums;

namespace StrideHR.Core.Models;

public class PerformanceGoalDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SuccessCriteria { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime TargetDate { get; set; }
    public int WeightPercentage { get; set; }
    public PerformanceGoalStatus Status { get; set; }
    public decimal ProgressPercentage { get; set; }
    public string? Notes { get; set; }
    public DateTime? CompletedDate { get; set; }
    public PerformanceRating? FinalRating { get; set; }
    public string? ManagerComments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreatePerformanceGoalDto
{
    public int EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SuccessCriteria { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime TargetDate { get; set; }
    public int WeightPercentage { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePerformanceGoalDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? SuccessCriteria { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? TargetDate { get; set; }
    public int? WeightPercentage { get; set; }
    public PerformanceGoalStatus? Status { get; set; }
    public decimal? ProgressPercentage { get; set; }
    public string? Notes { get; set; }
    public PerformanceRating? FinalRating { get; set; }
    public string? ManagerComments { get; set; }
}
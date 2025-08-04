using StrideHR.Core.Enums;

namespace StrideHR.Infrastructure.DTOs;

public class SetPerformanceGoalsDto
{
    public List<PerformanceGoalDto> Goals { get; set; } = new();
    public PerformanceReviewPeriod Period { get; set; }
    public int EmployeeId { get; set; }
    public PerformanceReviewPeriodDto? ReviewPeriod { get; set; }
}

public class PerformanceReviewPeriodDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class PerformanceGoalDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public string Metrics { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public PerformanceGoalStatus Status { get; set; }
    public decimal Weight { get; set; }
}

public class CreatePerformanceReviewDto
{
    public int EmployeeId { get; set; }
    public PerformanceReviewPeriod Period { get; set; }
    public List<GoalAchievementDto> GoalAchievements { get; set; } = new();
    public string? Comments { get; set; }
    public int ReviewerId { get; set; }
    public PerformanceReviewPeriodDto? ReviewPeriod { get; set; }
    public PerformanceRating OverallRating { get; set; }
}

public class GoalAchievementDto
{
    public int GoalId { get; set; }
    public decimal AchievedValue { get; set; }
    public PerformanceRating Rating { get; set; }
    public string? Comments { get; set; }
    public string? GoalTitle { get; set; }
}

public class PerformanceReviewDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public PerformanceReviewPeriod Period { get; set; }
    public PerformanceRating OverallRating { get; set; }
    public PerformanceReviewStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
using System.ComponentModel.DataAnnotations;
using StrideHR.Core.Entities;

namespace StrideHR.API.DTOs;

// DSR Submission DTOs
public class SubmitDSRDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public int? ProjectId { get; set; }

    public int? TaskId { get; set; }

    [Required]
    [Range(0.1, 24, ErrorMessage = "Hours worked must be between 0.1 and 24")]
    public decimal HoursWorked { get; set; }

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}

public class UpdateDSRDto
{
    public int? ProjectId { get; set; }

    public int? TaskId { get; set; }

    [Required]
    [Range(0.1, 24, ErrorMessage = "Hours worked must be between 0.1 and 24")]
    public decimal HoursWorked { get; set; }

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}

public class DSRDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public int? TaskId { get; set; }
    public string? TaskTitle { get; set; }
    public decimal HoursWorked { get; set; }
    public string? Description { get; set; }
    public DSRStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int? ReviewedBy { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DSRSummaryDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string? ProjectName { get; set; }
    public string? TaskTitle { get; set; }
    public decimal HoursWorked { get; set; }
    public DSRStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

// DSR Review DTOs
public class ReviewDSRDto
{
    [Required]
    public int ReviewerId { get; set; }

    [Required]
    public DSRStatus Status { get; set; } // Must be Approved or Rejected

    [StringLength(500, ErrorMessage = "Review comments cannot exceed 500 characters")]
    public string? ReviewComments { get; set; }
}

public class BulkApproveDSRsDto
{
    [Required]
    public int ReviewerId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one DSR ID must be provided")]
    public List<int> DSRIds { get; set; } = new();

    [StringLength(500, ErrorMessage = "Review comments cannot exceed 500 characters")]
    public string? ReviewComments { get; set; }
}

public class PendingDSRDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? ProjectName { get; set; }
    public string? TaskTitle { get; set; }
    public decimal HoursWorked { get; set; }
    public string? Description { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int DaysWaiting { get; set; }
}

// Productivity DTOs
public class EmployeeProductivityDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalWorkingDays { get; set; }
    public int DSRSubmittedDays { get; set; }
    public decimal TotalHoursLogged { get; set; }
    public decimal AverageHoursPerDay { get; set; }
    public decimal ProductivityPercentage { get; set; }
    public int IdleDays { get; set; }
    public decimal IdlePercentage { get; set; }
    public List<ProjectHoursBreakdownDto> ProjectHours { get; set; } = new();
}

public class ProjectHoursBreakdownDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal HoursWorked { get; set; }
    public decimal Percentage { get; set; }
}

public class TeamProductivityDto
{
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalTeamMembers { get; set; }
    public decimal TeamAverageProductivity { get; set; }
    public int TotalIdleEmployees { get; set; }
    public decimal TotalHoursLogged { get; set; }
    public List<EmployeeProductivitySummaryDto> TeamMemberMetrics { get; set; } = new();
}

public class EmployeeProductivitySummaryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal ProductivityPercentage { get; set; }
    public decimal HoursLogged { get; set; }
    public bool IsIdle { get; set; }
}

public class IdleEmployeeDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public bool HasSubmittedDSR { get; set; }
    public decimal HoursLogged { get; set; }
    public string IdleReason { get; set; } = string.Empty;
    public string IdleDuration { get; set; } = string.Empty;
}

public class ProductivityDashboardDto
{
    public DateTime Date { get; set; }
    public int TotalEmployees { get; set; }
    public int EmployeesWithDSR { get; set; }
    public int IdleEmployees { get; set; }
    public decimal DSRSubmissionRate { get; set; }
    public decimal AverageProductivity { get; set; }
    public decimal TotalHoursLogged { get; set; }
    public List<DepartmentProductivityDto> DepartmentMetrics { get; set; } = new();
    public List<ProjectProductivityDto> ProjectMetrics { get; set; } = new();
}

public class DepartmentProductivityDto
{
    public string Department { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal AverageProductivity { get; set; }
    public decimal TotalHours { get; set; }
}

public class ProjectProductivityDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal HoursLogged { get; set; }
    public int ContributorCount { get; set; }
}

// Project Hours Tracking DTOs
public class ProjectHoursSummaryDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int EstimatedHours { get; set; }
    public decimal ActualHoursLogged { get; set; }
    public decimal HoursVariance { get; set; }
    public decimal CompletionPercentage { get; set; }
    public bool IsOverBudget { get; set; }
    public string VarianceStatus { get; set; } = string.Empty;
    public List<EmployeeProjectContributionDto> EmployeeContributions { get; set; } = new();
}

public class EmployeeProjectContributionDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal HoursContributed { get; set; }
    public decimal ContributionPercentage { get; set; }
}

public class ProjectHoursComparisonDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public decimal VarianceHours { get; set; }
    public decimal VariancePercentage { get; set; }
    public bool IsOnTrack { get; set; }
    public DateTime? ProjectedCompletionDate { get; set; }
    public List<TaskHoursComparisonDto> TaskComparisons { get; set; } = new();
}

public class TaskHoursComparisonDto
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public int EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public decimal Variance { get; set; }
    public string VarianceStatus { get; set; } = string.Empty;
}

public class EmployeeProjectHoursDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal HoursWorked { get; set; }
    public int DSRCount { get; set; }
    public DateTime FirstEntry { get; set; }
    public DateTime LastEntry { get; set; }
}

// Analytics DTOs
public class DSRAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDSRsSubmitted { get; set; }
    public int PendingReviews { get; set; }
    public int ApprovedDSRs { get; set; }
    public int RejectedDSRs { get; set; }
    public decimal ApprovalRate { get; set; }
    public decimal AverageHoursPerDSR { get; set; }
    public decimal TotalProductiveHours { get; set; }
    public List<ProjectHoursAnalyticsDto> ProjectAnalytics { get; set; } = new();
    public List<EmployeeProductivityRankingDto> ProductivityRankings { get; set; } = new();
}

public class ProjectHoursAnalyticsDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public int ContributorCount { get; set; }
    public decimal AverageHoursPerContributor { get; set; }
}

public class EmployeeProductivityRankingDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal ProductivityScore { get; set; }
    public decimal TotalHours { get; set; }
    public int Rank { get; set; }
}

// Query DTOs
public class GetEmployeeDSRsQueryDto
{
    public int EmployeeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DSRStatus? Status { get; set; }
    public int? ProjectId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetPendingDSRsQueryDto
{
    public int ReviewerId { get; set; }
    public DateTime? Date { get; set; }
    public int? EmployeeId { get; set; }
    public int? ProjectId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetProductivityQueryDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? BranchId { get; set; }
}

public class GetProjectHoursQueryDto
{
    public int ProjectId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

// Response DTOs
public class PagedDSRResponseDto
{
    public List<DSRDto> DSRs { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class PagedPendingDSRResponseDto
{
    public List<PendingDSRDto> DSRs { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class DSRSubmissionStatusDto
{
    public bool HasSubmittedToday { get; set; }
    public DSRDto? TodayDSR { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool CanSubmit { get; set; }
    public bool CanEdit { get; set; }
}

public class ProductivityAlertDto
{
    public string AlertType { get; set; } = string.Empty; // "low_productivity", "no_dsr", "overdue_review"
    public string Message { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Severity { get; set; } = string.Empty; // "low", "medium", "high"
}

// Project dropdown DTOs for DSR submission
public class ProjectDropdownDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class TaskDropdownDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public bool IsActive { get; set; }
}
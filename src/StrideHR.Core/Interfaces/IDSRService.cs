using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Service interface for Daily Status Report (DSR) operations
/// </summary>
public interface IDSRService
{
    // DSR Submission
    /// <summary>
    /// Submit a new DSR entry
    /// </summary>
    Task<DSR> SubmitDSRAsync(SubmitDSRRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing DSR entry (only if not yet reviewed)
    /// </summary>
    Task<DSR> UpdateDSRAsync(int dsrId, UpdateDSRRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get DSR by ID
    /// </summary>
    Task<DSR?> GetDSRByIdAsync(int dsrId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get DSRs for an employee with filtering
    /// </summary>
    Task<(IEnumerable<DSR> DSRs, int TotalCount)> GetEmployeeDSRsAsync(
        GetEmployeeDSRsRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get today's DSR for an employee
    /// </summary>
    Task<DSR?> GetTodayDSRAsync(int employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if employee has submitted DSR for today
    /// </summary>
    Task<bool> HasSubmittedTodayDSRAsync(int employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete DSR (only if not yet reviewed)
    /// </summary>
    Task<bool> DeleteDSRAsync(int dsrId, string deletedBy, CancellationToken cancellationToken = default);

    // DSR Review and Approval
    /// <summary>
    /// Get DSRs pending review for a manager
    /// </summary>
    Task<(IEnumerable<DSR> DSRs, int TotalCount)> GetPendingDSRsForReviewAsync(
        GetPendingDSRsRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Review and approve/reject DSR
    /// </summary>
    Task<DSR> ReviewDSRAsync(int dsrId, ReviewDSRRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk approve DSRs
    /// </summary>
    Task<IEnumerable<DSR>> BulkApproveDSRsAsync(
        BulkApproveDSRsRequest request, 
        CancellationToken cancellationToken = default);

    // Productivity Tracking
    /// <summary>
    /// Get employee productivity metrics for a date range
    /// </summary>
    Task<EmployeeProductivityMetrics> GetEmployeeProductivityAsync(
        int employeeId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get team productivity metrics
    /// </summary>
    Task<TeamProductivityMetrics> GetTeamProductivityAsync(
        int managerId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get idle employees for today
    /// </summary>
    Task<IEnumerable<IdleEmployeeInfo>> GetIdleEmployeesTodayAsync(
        int? branchId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get productivity dashboard data
    /// </summary>
    Task<ProductivityDashboard> GetProductivityDashboardAsync(
        int? branchId = null, 
        DateTime? date = null, 
        CancellationToken cancellationToken = default);

    // Project Hours Tracking
    /// <summary>
    /// Get project hours summary from DSRs
    /// </summary>
    Task<ProjectHoursSummary> GetProjectHoursSummaryAsync(
        int projectId, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get project hours vs estimates comparison
    /// </summary>
    Task<ProjectHoursComparison> GetProjectHoursComparisonAsync(
        int projectId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get employee project hours breakdown
    /// </summary>
    Task<IEnumerable<EmployeeProjectHours>> GetEmployeeProjectHoursAsync(
        int employeeId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);

    // Analytics and Reporting
    /// <summary>
    /// Get DSR analytics for management
    /// </summary>
    Task<DSRAnalytics> GetDSRAnalyticsAsync(
        int? branchId = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get employees who haven't submitted DSR today
    /// </summary>
    Task<IEnumerable<Employee>> GetEmployeesWithoutDSRTodayAsync(
        int? branchId = null, 
        CancellationToken cancellationToken = default);
}

// Request Models
public class SubmitDSRRequest
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public int? ProjectId { get; set; }
    public int? TaskId { get; set; }
    public decimal HoursWorked { get; set; }
    public string? Description { get; set; }
}

public class UpdateDSRRequest
{
    public int? ProjectId { get; set; }
    public int? TaskId { get; set; }
    public decimal HoursWorked { get; set; }
    public string? Description { get; set; }
}

public class GetEmployeeDSRsRequest
{
    public int EmployeeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DSRStatus? Status { get; set; }
    public int? ProjectId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetPendingDSRsRequest
{
    public int ReviewerId { get; set; }
    public DateTime? Date { get; set; }
    public int? EmployeeId { get; set; }
    public int? ProjectId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ReviewDSRRequest
{
    public int ReviewerId { get; set; }
    public DSRStatus Status { get; set; } // Approved or Rejected
    public string? ReviewComments { get; set; }
}

public class BulkApproveDSRsRequest
{
    public int ReviewerId { get; set; }
    public List<int> DSRIds { get; set; } = new();
    public string? ReviewComments { get; set; }
}

// Response Models
public class EmployeeProductivityMetrics
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
    public List<ProjectHoursBreakdown> ProjectHours { get; set; } = new();
}

public class TeamProductivityMetrics
{
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalTeamMembers { get; set; }
    public decimal TeamAverageProductivity { get; set; }
    public int TotalIdleEmployees { get; set; }
    public decimal TotalHoursLogged { get; set; }
    public List<EmployeeProductivitySummary> TeamMemberMetrics { get; set; } = new();
}

public class IdleEmployeeInfo
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public bool HasSubmittedDSR { get; set; }
    public decimal HoursLogged { get; set; }
    public string IdleReason { get; set; } = string.Empty;
    public TimeSpan IdleDuration { get; set; }
}

public class ProductivityDashboard
{
    public DateTime Date { get; set; }
    public int TotalEmployees { get; set; }
    public int EmployeesWithDSR { get; set; }
    public int IdleEmployees { get; set; }
    public decimal AverageProductivity { get; set; }
    public decimal TotalHoursLogged { get; set; }
    public List<DepartmentProductivity> DepartmentMetrics { get; set; } = new();
    public List<ProjectProductivity> ProjectMetrics { get; set; } = new();
}

public class ProjectHoursSummary
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int EstimatedHours { get; set; }
    public decimal ActualHoursLogged { get; set; }
    public decimal HoursVariance { get; set; }
    public decimal CompletionPercentage { get; set; }
    public bool IsOverBudget { get; set; }
    public List<EmployeeProjectContribution> EmployeeContributions { get; set; } = new();
}

public class ProjectHoursComparison
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public decimal VarianceHours { get; set; }
    public decimal VariancePercentage { get; set; }
    public bool IsOnTrack { get; set; }
    public DateTime? ProjectedCompletionDate { get; set; }
    public List<TaskHoursComparison> TaskComparisons { get; set; } = new();
}

public class EmployeeProjectHours
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal HoursWorked { get; set; }
    public int DSRCount { get; set; }
    public DateTime FirstEntry { get; set; }
    public DateTime LastEntry { get; set; }
}

public class DSRAnalytics
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDSRsSubmitted { get; set; }
    public int PendingReviews { get; set; }
    public int ApprovedDSRs { get; set; }
    public int RejectedDSRs { get; set; }
    public decimal AverageHoursPerDSR { get; set; }
    public decimal TotalProductiveHours { get; set; }
    public List<ProjectHoursAnalytics> ProjectAnalytics { get; set; } = new();
    public List<EmployeeProductivityRanking> ProductivityRankings { get; set; } = new();
}

// Supporting Models
public class ProjectHoursBreakdown
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal HoursWorked { get; set; }
    public decimal Percentage { get; set; }
}

public class EmployeeProductivitySummary
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal ProductivityPercentage { get; set; }
    public decimal HoursLogged { get; set; }
    public bool IsIdle { get; set; }
}

public class DepartmentProductivity
{
    public string Department { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal AverageProductivity { get; set; }
    public decimal TotalHours { get; set; }
}

public class ProjectProductivity
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal HoursLogged { get; set; }
    public int ContributorCount { get; set; }
}

public class EmployeeProjectContribution
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal HoursContributed { get; set; }
    public decimal ContributionPercentage { get; set; }
}

public class TaskHoursComparison
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public int EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public decimal Variance { get; set; }
}

public class ProjectHoursAnalytics
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public int ContributorCount { get; set; }
    public decimal AverageHoursPerContributor { get; set; }
}

public class EmployeeProductivityRanking
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal ProductivityScore { get; set; }
    public decimal TotalHours { get; set; }
    public int Rank { get; set; }
}
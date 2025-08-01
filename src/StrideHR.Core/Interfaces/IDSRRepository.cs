using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Repository interface for DSR data access operations
/// </summary>
public interface IDSRRepository : IRepository<DSR>
{
    // Basic DSR Operations
    /// <summary>
    /// Get DSR by employee and date
    /// </summary>
    Task<DSR?> GetByEmployeeAndDateAsync(int employeeId, DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get DSRs for an employee with filtering and pagination
    /// </summary>
    Task<(IEnumerable<DSR> DSRs, int TotalCount)> GetEmployeeDSRsAsync(
        int employeeId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        DSRStatus? status = null,
        int? projectId = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get DSRs pending review for a manager
    /// </summary>
    Task<(IEnumerable<DSR> DSRs, int TotalCount)> GetPendingDSRsForReviewAsync(
        int reviewerId,
        DateTime? date = null,
        int? employeeId = null,
        int? projectId = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get DSRs by status
    /// </summary>
    Task<IEnumerable<DSR>> GetDSRsByStatusAsync(
        DSRStatus status,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    // Productivity and Analytics
    /// <summary>
    /// Get employee productivity data for date range
    /// </summary>
    Task<IEnumerable<DSR>> GetEmployeeProductivityDataAsync(
        int employeeId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get team productivity data for a manager
    /// </summary>
    Task<IEnumerable<DSR>> GetTeamProductivityDataAsync(
        int managerId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get employees who haven't submitted DSR for a specific date
    /// </summary>
    Task<IEnumerable<Employee>> GetEmployeesWithoutDSRAsync(
        DateTime date,
        int? branchId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get idle employees (those with low productivity) for today
    /// </summary>
    Task<IEnumerable<Employee>> GetIdleEmployeesTodayAsync(
        int? branchId = null,
        decimal productivityThreshold = 6.0m, // Less than 6 hours considered idle
        CancellationToken cancellationToken = default);

    // Project Hours Tracking
    /// <summary>
    /// Get project hours summary from DSRs
    /// </summary>
    Task<IEnumerable<DSR>> GetProjectHoursDataAsync(
        int projectId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get employee project hours breakdown
    /// </summary>
    Task<IEnumerable<DSR>> GetEmployeeProjectHoursAsync(
        int employeeId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total hours logged for a project
    /// </summary>
    Task<decimal> GetTotalProjectHoursAsync(
        int projectId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total hours logged for a task
    /// </summary>
    Task<decimal> GetTotalTaskHoursAsync(
        int taskId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    // Analytics and Reporting
    /// <summary>
    /// Get DSR statistics for analytics
    /// </summary>
    Task<DSRStatistics> GetDSRStatisticsAsync(
        int? branchId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get productivity statistics by department
    /// </summary>
    Task<IEnumerable<DepartmentProductivityStats>> GetDepartmentProductivityStatsAsync(
        int? branchId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get project productivity statistics
    /// </summary>
    Task<IEnumerable<ProjectProductivityStats>> GetProjectProductivityStatsAsync(
        int? branchId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get employee productivity rankings
    /// </summary>
    Task<IEnumerable<EmployeeProductivityStats>> GetEmployeeProductivityRankingsAsync(
        int? branchId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int topCount = 10,
        CancellationToken cancellationToken = default);

    // Bulk Operations
    /// <summary>
    /// Bulk update DSR status
    /// </summary>
    Task<int> BulkUpdateStatusAsync(
        IEnumerable<int> dsrIds,
        DSRStatus status,
        int reviewerId,
        string? reviewComments = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if employee has submitted DSR for date
    /// </summary>
    Task<bool> HasEmployeeSubmittedDSRAsync(
        int employeeId,
        DateTime date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get DSR submission rate for date range
    /// </summary>
    Task<decimal> GetDSRSubmissionRateAsync(
        DateTime startDate,
        DateTime endDate,
        int? branchId = null,
        CancellationToken cancellationToken = default);
}

// Statistics Models
public class DSRStatistics
{
    public int TotalDSRs { get; set; }
    public int PendingReviews { get; set; }
    public int ApprovedDSRs { get; set; }
    public int RejectedDSRs { get; set; }
    public decimal TotalHours { get; set; }
    public decimal AverageHoursPerDSR { get; set; }
    public decimal SubmissionRate { get; set; }
}

public class DepartmentProductivityStats
{
    public string Department { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public int DSRCount { get; set; }
    public decimal TotalHours { get; set; }
    public decimal AverageHoursPerEmployee { get; set; }
    public decimal ProductivityPercentage { get; set; }
}

public class ProjectProductivityStats
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int ContributorCount { get; set; }
    public int DSRCount { get; set; }
    public decimal TotalHours { get; set; }
    public decimal AverageHoursPerContributor { get; set; }
}

public class EmployeeProductivityStats
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int DSRCount { get; set; }
    public decimal TotalHours { get; set; }
    public decimal AverageHoursPerDay { get; set; }
    public decimal ProductivityScore { get; set; }
    public int Rank { get; set; }
}
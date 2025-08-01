using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// Service implementation for Daily Status Report (DSR) operations
/// </summary>
public class DSRService : IDSRService
{
    private readonly IDSRRepository _dsrRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ILogger<DSRService> _logger;

    public DSRService(
        IDSRRepository dsrRepository,
        IEmployeeRepository employeeRepository,
        IProjectRepository projectRepository,
        IAttendanceRepository attendanceRepository,
        ILogger<DSRService> logger)
    {
        _dsrRepository = dsrRepository;
        _employeeRepository = employeeRepository;
        _projectRepository = projectRepository;
        _attendanceRepository = attendanceRepository;
        _logger = logger;
    }

    public async Task<DSR> SubmitDSRAsync(SubmitDSRRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Submitting DSR for employee {EmployeeId} on {Date}", request.EmployeeId, request.Date);

        // Validate employee exists
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
            throw new ArgumentException($"Employee with ID {request.EmployeeId} not found");

        // Check if DSR already exists for this date
        var existingDSR = await _dsrRepository.GetByEmployeeAndDateAsync(request.EmployeeId, request.Date, cancellationToken);
        if (existingDSR != null)
            throw new InvalidOperationException($"DSR already exists for employee {request.EmployeeId} on {request.Date:yyyy-MM-dd}");

        // Validate project and task if provided
        if (request.ProjectId.HasValue)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId.Value, cancellationToken);
            if (project == null)
                throw new ArgumentException($"Project with ID {request.ProjectId} not found");

            // Check if employee is assigned to the project
            var isAssigned = await _projectRepository.IsEmployeeAssignedToProjectAsync(request.ProjectId.Value, request.EmployeeId, cancellationToken);
            if (!isAssigned)
                throw new InvalidOperationException($"Employee {request.EmployeeId} is not assigned to project {request.ProjectId}");

            if (request.TaskId.HasValue)
            {
                var task = await _projectRepository.GetTaskByIdAsync(request.TaskId.Value, cancellationToken);
                if (task == null || task.ProjectId != request.ProjectId)
                    throw new ArgumentException($"Task with ID {request.TaskId} not found or doesn't belong to project {request.ProjectId}");
            }
        }

        // Validate hours worked
        if (request.HoursWorked <= 0 || request.HoursWorked > 24)
            throw new ArgumentException("Hours worked must be between 0 and 24");

        // Get employee's working hours from attendance or organization settings
        var maxAllowedHours = await GetMaxAllowedHoursForEmployeeAsync(request.EmployeeId, request.Date, cancellationToken);
        if (request.HoursWorked > maxAllowedHours)
            throw new ArgumentException($"Hours worked ({request.HoursWorked}) cannot exceed maximum allowed hours ({maxAllowedHours})");

        var dsr = new DSR
        {
            EmployeeId = request.EmployeeId,
            Date = request.Date.Date,
            ProjectId = request.ProjectId,
            TaskId = request.TaskId,
            HoursWorked = request.HoursWorked,
            Description = request.Description?.Trim(),
            Status = DSRStatus.Submitted,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.EmployeeId.ToString()
        };

        await _dsrRepository.AddAsync(dsr, cancellationToken);
        await _dsrRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("DSR submitted successfully for employee {EmployeeId} on {Date}", request.EmployeeId, request.Date);

        return dsr;
    }

    public async Task<DSR> UpdateDSRAsync(int dsrId, UpdateDSRRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating DSR {DSRId}", dsrId);

        var dsr = await _dsrRepository.GetByIdAsync(dsrId, cancellationToken);
        if (dsr == null)
            throw new ArgumentException($"DSR with ID {dsrId} not found");

        // Check if DSR can be updated (not yet reviewed)
        if (dsr.Status != DSRStatus.Draft && dsr.Status != DSRStatus.Submitted)
            throw new InvalidOperationException("DSR cannot be updated after it has been reviewed");

        // Validate project and task if provided
        if (request.ProjectId.HasValue)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId.Value, cancellationToken);
            if (project == null)
                throw new ArgumentException($"Project with ID {request.ProjectId} not found");

            var isAssigned = await _projectRepository.IsEmployeeAssignedToProjectAsync(request.ProjectId.Value, dsr.EmployeeId, cancellationToken);
            if (!isAssigned)
                throw new InvalidOperationException($"Employee {dsr.EmployeeId} is not assigned to project {request.ProjectId}");

            if (request.TaskId.HasValue)
            {
                var task = await _projectRepository.GetTaskByIdAsync(request.TaskId.Value, cancellationToken);
                if (task == null || task.ProjectId != request.ProjectId)
                    throw new ArgumentException($"Task with ID {request.TaskId} not found or doesn't belong to project {request.ProjectId}");
            }
        }

        // Validate hours worked
        if (request.HoursWorked <= 0 || request.HoursWorked > 24)
            throw new ArgumentException("Hours worked must be between 0 and 24");

        var maxAllowedHours = await GetMaxAllowedHoursForEmployeeAsync(dsr.EmployeeId, dsr.Date, cancellationToken);
        if (request.HoursWorked > maxAllowedHours)
            throw new ArgumentException($"Hours worked ({request.HoursWorked}) cannot exceed maximum allowed hours ({maxAllowedHours})");

        dsr.ProjectId = request.ProjectId;
        dsr.TaskId = request.TaskId;
        dsr.HoursWorked = request.HoursWorked;
        dsr.Description = request.Description?.Trim();
        dsr.UpdatedAt = DateTime.UtcNow;
        dsr.UpdatedBy = dsr.EmployeeId.ToString();

        await _dsrRepository.UpdateAsync(dsr, cancellationToken);
        await _dsrRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("DSR {DSRId} updated successfully", dsrId);

        return dsr;
    }

    public async Task<DSR?> GetDSRByIdAsync(int dsrId, CancellationToken cancellationToken = default)
    {
        return await _dsrRepository.GetByIdAsync(dsrId, cancellationToken);
    }

    public async Task<(IEnumerable<DSR> DSRs, int TotalCount)> GetEmployeeDSRsAsync(
        GetEmployeeDSRsRequest request, 
        CancellationToken cancellationToken = default)
    {
        return await _dsrRepository.GetEmployeeDSRsAsync(
            request.EmployeeId,
            request.StartDate,
            request.EndDate,
            request.Status,
            request.ProjectId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }

    public async Task<DSR?> GetTodayDSRAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _dsrRepository.GetByEmployeeAndDateAsync(employeeId, DateTime.Today, cancellationToken);
    }

    public async Task<bool> HasSubmittedTodayDSRAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _dsrRepository.HasEmployeeSubmittedDSRAsync(employeeId, DateTime.Today, cancellationToken);
    }

    public async Task<bool> DeleteDSRAsync(int dsrId, string deletedBy, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting DSR {DSRId}", dsrId);

        var dsr = await _dsrRepository.GetByIdAsync(dsrId, cancellationToken);
        if (dsr == null)
            return false;

        // Check if DSR can be deleted (not yet reviewed)
        if (dsr.Status != DSRStatus.Draft && dsr.Status != DSRStatus.Submitted)
            throw new InvalidOperationException("DSR cannot be deleted after it has been reviewed");

        await _dsrRepository.DeleteAsync(dsrId, deletedBy, cancellationToken);
        await _dsrRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("DSR {DSRId} deleted successfully", dsrId);

        return true;
    }

    public async Task<(IEnumerable<DSR> DSRs, int TotalCount)> GetPendingDSRsForReviewAsync(
        GetPendingDSRsRequest request, 
        CancellationToken cancellationToken = default)
    {
        return await _dsrRepository.GetPendingDSRsForReviewAsync(
            request.ReviewerId,
            request.Date,
            request.EmployeeId,
            request.ProjectId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }

    public async Task<DSR> ReviewDSRAsync(int dsrId, ReviewDSRRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reviewing DSR {DSRId} by reviewer {ReviewerId}", dsrId, request.ReviewerId);

        var dsr = await _dsrRepository.GetByIdAsync(dsrId, cancellationToken);
        if (dsr == null)
            throw new ArgumentException($"DSR with ID {dsrId} not found");

        if (dsr.Status != DSRStatus.Submitted)
            throw new InvalidOperationException("Only submitted DSRs can be reviewed");

        // Validate reviewer has authority to review this DSR
        var employee = await _employeeRepository.GetByIdAsync(dsr.EmployeeId, cancellationToken);
        if (employee?.ReportingManagerId != request.ReviewerId)
            throw new InvalidOperationException("Reviewer does not have authority to review this DSR");

        if (request.Status != DSRStatus.Approved && request.Status != DSRStatus.Rejected)
            throw new ArgumentException("Review status must be either Approved or Rejected");

        dsr.Status = request.Status;
        dsr.ReviewedBy = request.ReviewerId;
        dsr.ReviewedAt = DateTime.UtcNow;
        dsr.ReviewComments = request.ReviewComments?.Trim();
        dsr.UpdatedAt = DateTime.UtcNow;
        dsr.UpdatedBy = request.ReviewerId.ToString();

        await _dsrRepository.UpdateAsync(dsr, cancellationToken);
        await _dsrRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("DSR {DSRId} reviewed successfully with status {Status}", dsrId, request.Status);

        return dsr;
    }

    public async Task<IEnumerable<DSR>> BulkApproveDSRsAsync(
        BulkApproveDSRsRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Bulk approving {Count} DSRs by reviewer {ReviewerId}", request.DSRIds.Count, request.ReviewerId);

        var updatedCount = await _dsrRepository.BulkUpdateStatusAsync(
            request.DSRIds,
            DSRStatus.Approved,
            request.ReviewerId,
            request.ReviewComments,
            cancellationToken);

        _logger.LogInformation("Bulk approved {Count} DSRs successfully", updatedCount);

        // Return the updated DSRs
        var updatedDSRs = new List<DSR>();
        foreach (var dsrId in request.DSRIds)
        {
            var dsr = await _dsrRepository.GetByIdAsync(dsrId, cancellationToken);
            if (dsr != null)
                updatedDSRs.Add(dsr);
        }

        return updatedDSRs;
    }

    public async Task<EmployeeProductivityMetrics> GetEmployeeProductivityAsync(
        int employeeId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null)
            throw new ArgumentException($"Employee with ID {employeeId} not found");

        var dsrs = await _dsrRepository.GetEmployeeProductivityDataAsync(employeeId, startDate, endDate, cancellationToken);
        var dsrList = dsrs.ToList();

        var totalWorkingDays = GetWorkingDaysBetween(startDate, endDate);
        var dsrSubmittedDays = dsrList.Select(d => d.Date.Date).Distinct().Count();
        var totalHoursLogged = dsrList.Sum(d => d.HoursWorked);
        var averageHoursPerDay = dsrSubmittedDays > 0 ? totalHoursLogged / dsrSubmittedDays : 0;

        // Calculate productivity percentage (assuming 8 hours as 100% productive day)
        var expectedHours = dsrSubmittedDays * 8m;
        var productivityPercentage = expectedHours > 0 ? (totalHoursLogged / expectedHours) * 100 : 0;

        var idleDays = totalWorkingDays - dsrSubmittedDays;
        var idlePercentage = totalWorkingDays > 0 ? (decimal)idleDays / totalWorkingDays * 100 : 0;

        // Project hours breakdown
        var projectHours = dsrList
            .Where(d => d.ProjectId.HasValue)
            .GroupBy(d => new { d.ProjectId, d.Project?.Name })
            .Select(g => new ProjectHoursBreakdown
            {
                ProjectId = g.Key.ProjectId!.Value,
                ProjectName = g.Key.Name ?? "Unknown Project",
                HoursWorked = g.Sum(d => d.HoursWorked),
                Percentage = totalHoursLogged > 0 ? (g.Sum(d => d.HoursWorked) / totalHoursLogged) * 100 : 0
            })
            .OrderByDescending(p => p.HoursWorked)
            .ToList();

        return new EmployeeProductivityMetrics
        {
            EmployeeId = employeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            StartDate = startDate,
            EndDate = endDate,
            TotalWorkingDays = totalWorkingDays,
            DSRSubmittedDays = dsrSubmittedDays,
            TotalHoursLogged = totalHoursLogged,
            AverageHoursPerDay = averageHoursPerDay,
            ProductivityPercentage = productivityPercentage,
            IdleDays = idleDays,
            IdlePercentage = idlePercentage,
            ProjectHours = projectHours
        };
    }

    public async Task<TeamProductivityMetrics> GetTeamProductivityAsync(
        int managerId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        var manager = await _employeeRepository.GetByIdAsync(managerId, cancellationToken);
        if (manager == null)
            throw new ArgumentException($"Manager with ID {managerId} not found");

        var teamDSRs = await _dsrRepository.GetTeamProductivityDataAsync(managerId, startDate, endDate, cancellationToken);
        var teamDSRsList = teamDSRs.ToList();

        var teamMembers = teamDSRsList.Select(d => d.Employee).Distinct().ToList();
        var totalTeamMembers = teamMembers.Count;
        var totalHoursLogged = teamDSRsList.Sum(d => d.HoursWorked);

        var teamMemberMetrics = new List<EmployeeProductivitySummary>();
        var totalProductivity = 0m;
        var idleEmployees = 0;

        foreach (var member in teamMembers)
        {
            var memberDSRs = teamDSRsList.Where(d => d.EmployeeId == member.Id).ToList();
            var memberHours = memberDSRs.Sum(d => d.HoursWorked);
            var workingDays = GetWorkingDaysBetween(startDate, endDate);
            var dsrDays = memberDSRs.Select(d => d.Date.Date).Distinct().Count();
            var expectedHours = dsrDays * 8m;
            var productivity = expectedHours > 0 ? (memberHours / expectedHours) * 100 : 0;

            var isIdle = productivity < 75; // Consider below 75% as idle
            if (isIdle) idleEmployees++;

            totalProductivity += productivity;

            teamMemberMetrics.Add(new EmployeeProductivitySummary
            {
                EmployeeId = member.Id,
                EmployeeName = $"{member.FirstName} {member.LastName}",
                ProductivityPercentage = productivity,
                HoursLogged = memberHours,
                IsIdle = isIdle
            });
        }

        var teamAverageProductivity = totalTeamMembers > 0 ? totalProductivity / totalTeamMembers : 0;

        return new TeamProductivityMetrics
        {
            ManagerId = managerId,
            ManagerName = $"{manager.FirstName} {manager.LastName}",
            StartDate = startDate,
            EndDate = endDate,
            TotalTeamMembers = totalTeamMembers,
            TeamAverageProductivity = teamAverageProductivity,
            TotalIdleEmployees = idleEmployees,
            TotalHoursLogged = totalHoursLogged,
            TeamMemberMetrics = teamMemberMetrics.OrderByDescending(m => m.ProductivityPercentage).ToList()
        };
    }

    public async Task<IEnumerable<IdleEmployeeInfo>> GetIdleEmployeesTodayAsync(
        int? branchId = null, 
        CancellationToken cancellationToken = default)
    {
        var idleEmployees = await _dsrRepository.GetIdleEmployeesTodayAsync(branchId, 6.0m, cancellationToken);
        var result = new List<IdleEmployeeInfo>();

        foreach (var employee in idleEmployees)
        {
            var todayDSR = await _dsrRepository.GetByEmployeeAndDateAsync(employee.Id, DateTime.Today, cancellationToken);
            var hasSubmittedDSR = todayDSR != null;
            var hoursLogged = todayDSR?.HoursWorked ?? 0;

            var idleReason = !hasSubmittedDSR ? "No DSR submitted" : 
                           hoursLogged < 6 ? $"Low productivity ({hoursLogged} hours)" : 
                           "Unknown";

            // Calculate idle duration (simplified - could be enhanced with attendance data)
            var idleDuration = !hasSubmittedDSR ? TimeSpan.FromHours(8) : TimeSpan.FromHours((double)(8 - hoursLogged));

            result.Add(new IdleEmployeeInfo
            {
                EmployeeId = employee.Id,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                Department = employee.Department ?? "Unknown",
                ManagerName = employee.ReportingManager != null ? 
                    $"{employee.ReportingManager.FirstName} {employee.ReportingManager.LastName}" : null,
                HasSubmittedDSR = hasSubmittedDSR,
                HoursLogged = hoursLogged,
                IdleReason = idleReason,
                IdleDuration = idleDuration
            });
        }

        return result.OrderBy(i => i.Department).ThenBy(i => i.EmployeeName);
    }

    public async Task<ProductivityDashboard> GetProductivityDashboardAsync(
        int? branchId = null, 
        DateTime? date = null, 
        CancellationToken cancellationToken = default)
    {
        var targetDate = date ?? DateTime.Today;
        
        // Get basic statistics
        var statistics = await _dsrRepository.GetDSRStatisticsAsync(branchId, targetDate, targetDate, cancellationToken);
        var departmentStats = await _dsrRepository.GetDepartmentProductivityStatsAsync(branchId, targetDate, targetDate, cancellationToken);
        var projectStats = await _dsrRepository.GetProjectProductivityStatsAsync(branchId, targetDate, targetDate, cancellationToken);

        // Get total employees
        var employeeQuery = _employeeRepository.GetQueryable().Where(e => !e.IsDeleted);
        if (branchId.HasValue)
            employeeQuery = employeeQuery.Where(e => e.BranchId == branchId.Value);
        
        var totalEmployees = employeeQuery.Count();
        var employeesWithDSR = statistics.TotalDSRs;
        var idleEmployees = totalEmployees - employeesWithDSR;

        return new ProductivityDashboard
        {
            Date = targetDate,
            TotalEmployees = totalEmployees,
            EmployeesWithDSR = employeesWithDSR,
            IdleEmployees = idleEmployees,
            AverageProductivity = statistics.AverageHoursPerDSR / 8 * 100, // Assuming 8 hours as 100%
            TotalHoursLogged = statistics.TotalHours,
            DepartmentMetrics = departmentStats.Select(d => new DepartmentProductivity
            {
                Department = d.Department,
                EmployeeCount = d.EmployeeCount,
                AverageProductivity = d.ProductivityPercentage,
                TotalHours = d.TotalHours
            }).ToList(),
            ProjectMetrics = projectStats.Select(p => new ProjectProductivity
            {
                ProjectId = p.ProjectId,
                ProjectName = p.ProjectName,
                HoursLogged = p.TotalHours,
                ContributorCount = p.ContributorCount
            }).ToList()
        };
    }

    public async Task<ProjectHoursSummary> GetProjectHoursSummaryAsync(
        int projectId, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project == null)
            throw new ArgumentException($"Project with ID {projectId} not found");

        var projectDSRs = await _dsrRepository.GetProjectHoursDataAsync(projectId, startDate, endDate, cancellationToken);
        var dsrList = projectDSRs.ToList();

        var actualHoursLogged = dsrList.Sum(d => d.HoursWorked);
        var hoursVariance = actualHoursLogged - project.EstimatedHours;
        var completionPercentage = project.EstimatedHours > 0 ? (actualHoursLogged / project.EstimatedHours) * 100 : 0;
        var isOverBudget = actualHoursLogged > project.EstimatedHours;

        var employeeContributions = dsrList
            .GroupBy(d => new { d.EmployeeId, d.Employee.FirstName, d.Employee.LastName })
            .Select(g => new EmployeeProjectContribution
            {
                EmployeeId = g.Key.EmployeeId,
                EmployeeName = $"{g.Key.FirstName} {g.Key.LastName}",
                HoursContributed = g.Sum(d => d.HoursWorked),
                ContributionPercentage = actualHoursLogged > 0 ? (g.Sum(d => d.HoursWorked) / actualHoursLogged) * 100 : 0
            })
            .OrderByDescending(c => c.HoursContributed)
            .ToList();

        return new ProjectHoursSummary
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            EstimatedHours = project.EstimatedHours,
            ActualHoursLogged = actualHoursLogged,
            HoursVariance = hoursVariance,
            CompletionPercentage = completionPercentage,
            IsOverBudget = isOverBudget,
            EmployeeContributions = employeeContributions
        };
    }

    public async Task<ProjectHoursComparison> GetProjectHoursComparisonAsync(
        int projectId, 
        CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project == null)
            throw new ArgumentException($"Project with ID {projectId} not found");

        var actualHours = await _dsrRepository.GetTotalProjectHoursAsync(projectId, null, null, cancellationToken);
        var varianceHours = actualHours - project.EstimatedHours;
        var variancePercentage = project.EstimatedHours > 0 ? (varianceHours / project.EstimatedHours) * 100 : 0;
        var isOnTrack = Math.Abs(variancePercentage) <= 10; // Within 10% considered on track

        // Get task comparisons
        var tasks = await _projectRepository.GetTasksByProjectAsync(projectId, cancellationToken);
        var taskComparisons = new List<TaskHoursComparison>();

        foreach (var task in tasks)
        {
            var taskActualHours = await _dsrRepository.GetTotalTaskHoursAsync(task.Id, null, null, cancellationToken);
            var taskVariance = taskActualHours - task.EstimatedHours;

            taskComparisons.Add(new TaskHoursComparison
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                EstimatedHours = task.EstimatedHours,
                ActualHours = taskActualHours,
                Variance = taskVariance
            });
        }

        return new ProjectHoursComparison
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            EstimatedHours = project.EstimatedHours,
            ActualHours = actualHours,
            VarianceHours = varianceHours,
            VariancePercentage = variancePercentage,
            IsOnTrack = isOnTrack,
            ProjectedCompletionDate = CalculateProjectedCompletionDate(project, actualHours),
            TaskComparisons = taskComparisons.OrderByDescending(t => Math.Abs(t.Variance)).ToList()
        };
    }

    public async Task<IEnumerable<EmployeeProjectHours>> GetEmployeeProjectHoursAsync(
        int employeeId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        var dsrs = await _dsrRepository.GetEmployeeProjectHoursAsync(employeeId, startDate, endDate, cancellationToken);
        
        return dsrs
            .GroupBy(d => new { d.ProjectId, d.Project?.Name })
            .Select(g => new EmployeeProjectHours
            {
                ProjectId = g.Key.ProjectId!.Value,
                ProjectName = g.Key.Name ?? "Unknown Project",
                HoursWorked = g.Sum(d => d.HoursWorked),
                DSRCount = g.Count(),
                FirstEntry = g.Min(d => d.Date),
                LastEntry = g.Max(d => d.Date)
            })
            .OrderByDescending(p => p.HoursWorked)
            .ToList();
    }

    public async Task<DSRAnalytics> GetDSRAnalyticsAsync(
        int? branchId = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today;

        var statistics = await _dsrRepository.GetDSRStatisticsAsync(branchId, start, end, cancellationToken);
        var projectStats = await _dsrRepository.GetProjectProductivityStatsAsync(branchId, start, end, cancellationToken);
        var employeeRankings = await _dsrRepository.GetEmployeeProductivityRankingsAsync(branchId, start, end, 10, cancellationToken);

        var projectAnalytics = projectStats.Select(p => new ProjectHoursAnalytics
        {
            ProjectId = p.ProjectId,
            ProjectName = p.ProjectName,
            TotalHours = p.TotalHours,
            ContributorCount = p.ContributorCount,
            AverageHoursPerContributor = p.AverageHoursPerContributor
        }).ToList();

        var productivityRankings = employeeRankings.Select(e => new EmployeeProductivityRanking
        {
            EmployeeId = e.EmployeeId,
            EmployeeName = e.EmployeeName,
            Department = e.Department,
            ProductivityScore = e.ProductivityScore,
            TotalHours = e.TotalHours,
            Rank = e.Rank
        }).ToList();

        return new DSRAnalytics
        {
            StartDate = start,
            EndDate = end,
            TotalDSRsSubmitted = statistics.TotalDSRs,
            PendingReviews = statistics.PendingReviews,
            ApprovedDSRs = statistics.ApprovedDSRs,
            RejectedDSRs = statistics.RejectedDSRs,
            AverageHoursPerDSR = statistics.AverageHoursPerDSR,
            TotalProductiveHours = statistics.TotalHours,
            ProjectAnalytics = projectAnalytics,
            ProductivityRankings = productivityRankings
        };
    }

    public async Task<IEnumerable<Employee>> GetEmployeesWithoutDSRTodayAsync(
        int? branchId = null, 
        CancellationToken cancellationToken = default)
    {
        return await _dsrRepository.GetEmployeesWithoutDSRAsync(DateTime.Today, branchId, cancellationToken);
    }

    // Private helper methods
    private async Task<decimal> GetMaxAllowedHoursForEmployeeAsync(int employeeId, DateTime date, CancellationToken cancellationToken)
    {
        // Try to get from attendance record first
        var attendanceRecord = await _attendanceRepository.GetByEmployeeAndDateAsync(employeeId, date, cancellationToken);
        if (attendanceRecord?.TotalWorkingHours.HasValue == true)
        {
            return (decimal)attendanceRecord.TotalWorkingHours.Value.TotalHours;
        }

        // Default to 12 hours if no attendance record (allowing for some overtime)
        return 12m;
    }

    private static int GetWorkingDaysBetween(DateTime startDate, DateTime endDate)
    {
        var workingDays = 0;
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                workingDays++;
        }
        return workingDays;
    }

    private static DateTime? CalculateProjectedCompletionDate(Project project, decimal actualHours)
    {
        if (actualHours <= 0 || project.EstimatedHours <= 0)
            return null;

        var progressPercentage = actualHours / project.EstimatedHours;
        if (progressPercentage <= 0)
            return null;

        var daysSinceStart = (DateTime.Today - project.StartDate).Days;
        if (daysSinceStart <= 0)
            return null;

        var projectedTotalDays = daysSinceStart / (double)progressPercentage;
        return project.StartDate.AddDays(projectedTotalDays);
    }
}
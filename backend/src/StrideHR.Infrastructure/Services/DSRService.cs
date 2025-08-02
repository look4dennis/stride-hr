using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.DSR;

namespace StrideHR.Infrastructure.Services;

public class DSRService : IDSRService
{
    private readonly IDSRRepository _dsrRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectTaskRepository _taskRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<AttendanceRecord> _attendanceRepository;
    private readonly IRepository<Organization> _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DSRService(
        IDSRRepository dsrRepository,
        IProjectRepository projectRepository,
        IProjectTaskRepository taskRepository,
        IRepository<Employee> employeeRepository,
        IRepository<AttendanceRecord> attendanceRepository,
        IRepository<Organization> organizationRepository,
        IUnitOfWork unitOfWork)
    {
        _dsrRepository = dsrRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _employeeRepository = employeeRepository;
        _attendanceRepository = attendanceRepository;
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DSR> CreateDSRAsync(CreateDSRRequest request)
    {
        // Validate employee exists
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee == null)
            throw new ArgumentException("Employee not found");

        // Check if DSR already exists for this date
        var existingDSR = await _dsrRepository.GetDSRByEmployeeAndDateAsync(request.EmployeeId, request.Date);
        if (existingDSR != null)
            throw new InvalidOperationException("DSR already exists for this date");

        // Validate project and task if provided
        if (request.ProjectId.HasValue)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId.Value);
            if (project == null)
                throw new ArgumentException("Project not found");

            // Check if employee is assigned to the project
            var isAssigned = await _projectRepository.IsEmployeeAssignedToProjectAsync(request.ProjectId.Value, request.EmployeeId);
            if (!isAssigned)
                throw new InvalidOperationException("Employee is not assigned to this project");

            if (request.TaskId.HasValue)
            {
                var task = await _taskRepository.GetByIdAsync(request.TaskId.Value);
                if (task == null || task.ProjectId != request.ProjectId.Value)
                    throw new ArgumentException("Task not found or does not belong to the specified project");
            }
        }

        var dsr = new DSR
        {
            EmployeeId = request.EmployeeId,
            Date = request.Date.Date,
            ProjectId = request.ProjectId,
            TaskId = request.TaskId,
            HoursWorked = request.HoursWorked,
            Description = request.Description,
            Status = DSRStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        await _dsrRepository.AddAsync(dsr);
        await _unitOfWork.SaveChangesAsync();

        return await _dsrRepository.GetByIdAsync(dsr.Id) ?? dsr;
    }

    public async Task<DSR> UpdateDSRAsync(int dsrId, UpdateDSRRequest request)
    {
        var dsr = await _dsrRepository.GetByIdAsync(dsrId);
        if (dsr == null)
            throw new ArgumentException("DSR not found");

        if (dsr.Status != DSRStatus.Draft)
            throw new InvalidOperationException("Only draft DSRs can be updated");

        // Validate project and task if provided
        if (request.ProjectId.HasValue)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId.Value);
            if (project == null)
                throw new ArgumentException("Project not found");

            var isAssigned = await _projectRepository.IsEmployeeAssignedToProjectAsync(request.ProjectId.Value, dsr.EmployeeId);
            if (!isAssigned)
                throw new InvalidOperationException("Employee is not assigned to this project");

            if (request.TaskId.HasValue)
            {
                var task = await _taskRepository.GetByIdAsync(request.TaskId.Value);
                if (task == null || task.ProjectId != request.ProjectId.Value)
                    throw new ArgumentException("Task not found or does not belong to the specified project");
            }
        }

        // Update fields if provided
        if (request.ProjectId.HasValue)
            dsr.ProjectId = request.ProjectId.Value;
        
        if (request.TaskId.HasValue)
            dsr.TaskId = request.TaskId.Value;
        
        if (request.HoursWorked.HasValue)
            dsr.HoursWorked = request.HoursWorked.Value;
        
        if (!string.IsNullOrEmpty(request.Description))
            dsr.Description = request.Description;

        dsr.UpdatedAt = DateTime.UtcNow;

        await _dsrRepository.UpdateAsync(dsr);
        await _unitOfWork.SaveChangesAsync();

        return await _dsrRepository.GetByIdAsync(dsr.Id) ?? dsr;
    }

    public async Task<DSR> SubmitDSRAsync(int dsrId, int employeeId)
    {
        var dsr = await _dsrRepository.GetByIdAsync(dsrId);
        if (dsr == null)
            throw new ArgumentException("DSR not found");

        if (dsr.EmployeeId != employeeId)
            throw new UnauthorizedAccessException("You can only submit your own DSRs");

        if (dsr.Status != DSRStatus.Draft)
            throw new InvalidOperationException("Only draft DSRs can be submitted");

        dsr.Status = DSRStatus.Submitted;
        dsr.SubmittedAt = DateTime.UtcNow;
        dsr.UpdatedAt = DateTime.UtcNow;

        await _dsrRepository.UpdateAsync(dsr);
        await _unitOfWork.SaveChangesAsync();

        return await _dsrRepository.GetByIdAsync(dsr.Id) ?? dsr;
    }

    public async Task<bool> DeleteDSRAsync(int dsrId, int employeeId)
    {
        var dsr = await _dsrRepository.GetByIdAsync(dsrId);
        if (dsr == null)
            return false;

        if (dsr.EmployeeId != employeeId)
            throw new UnauthorizedAccessException("You can only delete your own DSRs");

        if (dsr.Status != DSRStatus.Draft)
            throw new InvalidOperationException("Only draft DSRs can be deleted");

        await _dsrRepository.DeleteAsync(dsr);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<DSR?> GetDSRByIdAsync(int dsrId)
    {
        return await _dsrRepository.GetByIdAsync(dsrId);
    }

    public async Task<DSR?> GetDSRByEmployeeAndDateAsync(int employeeId, DateTime date)
    {
        return await _dsrRepository.GetDSRByEmployeeAndDateAsync(employeeId, date);
    }

    public async Task<IEnumerable<DSR>> GetDSRsByEmployeeAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _dsrRepository.GetDSRsByEmployeeAsync(employeeId, startDate, endDate);
    }

    public async Task<IEnumerable<DSR>> GetDSRsByProjectAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _dsrRepository.GetDSRsByProjectAsync(projectId, startDate, endDate);
    }

    public async Task<DSR> ReviewDSRAsync(int dsrId, ReviewDSRRequest request)
    {
        var dsr = await _dsrRepository.GetByIdAsync(dsrId);
        if (dsr == null)
            throw new ArgumentException("DSR not found");

        if (dsr.Status != DSRStatus.Submitted && dsr.Status != DSRStatus.UnderReview)
            throw new InvalidOperationException("Only submitted or under review DSRs can be reviewed");

        if (request.Status != DSRStatus.Approved && request.Status != DSRStatus.Rejected)
            throw new ArgumentException("Review status must be either Approved or Rejected");

        dsr.Status = request.Status;
        dsr.ReviewedBy = request.ReviewerId;
        dsr.ReviewedAt = DateTime.UtcNow;
        dsr.ReviewComments = request.ReviewComments;
        dsr.UpdatedAt = DateTime.UtcNow;

        await _dsrRepository.UpdateAsync(dsr);
        await _unitOfWork.SaveChangesAsync();

        return await _dsrRepository.GetByIdAsync(dsr.Id) ?? dsr;
    }

    public async Task<IEnumerable<DSR>> GetPendingDSRsForReviewAsync(int reviewerId)
    {
        return await _dsrRepository.GetPendingDSRsForReviewAsync(reviewerId);
    }

    public async Task<IEnumerable<DSR>> GetDSRsByStatusAsync(DSRStatus status)
    {
        return await _dsrRepository.GetDSRsByStatusAsync(status);
    }

    public async Task<ProductivityReport> GetEmployeeProductivityAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
            throw new ArgumentException("Employee not found");

        var organizations = await _organizationRepository.GetAllAsync();
        var organization = organizations.FirstOrDefault();
        var productiveHoursThreshold = organization?.ProductiveHoursThreshold ?? 6;
        var normalWorkingHours = organization?.NormalWorkingHours.TotalHours ?? 8;

        var dsrs = await _dsrRepository.GetDSRsByEmployeeAsync(employeeId, startDate, endDate);
        var approvedDSRs = dsrs.Where(d => d.Status == DSRStatus.Approved).ToList();

        var attendanceRecords = await _attendanceRepository.FindAsync(a => a.EmployeeId == employeeId && 
                       a.Date >= startDate && 
                       a.Date <= endDate &&
                       !a.IsDeleted);

        var totalHoursWorked = approvedDSRs.Sum(d => d.HoursWorked);
        var totalWorkingDays = GetWorkingDays(startDate, endDate);
        var totalWorkingHours = totalWorkingDays * (decimal)normalWorkingHours;
        var productiveHours = approvedDSRs.Where(d => d.HoursWorked >= productiveHoursThreshold).Sum(d => d.HoursWorked);
        var idleHours = Math.Max(0, totalWorkingHours - totalHoursWorked);

        var dailyBreakdown = new List<DailyProductivity>();
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            if (IsWorkingDay(date))
            {
                var dayDSR = approvedDSRs.FirstOrDefault(d => d.Date.Date == date);
                var attendance = attendanceRecords.FirstOrDefault(a => a.Date.Date == date);
                
                var dayHoursWorked = dayDSR?.HoursWorked ?? 0;
                var dayWorkingHours = (decimal)normalWorkingHours;
                var dayProductivity = dayWorkingHours > 0 ? (dayHoursWorked / dayWorkingHours) * 100 : 0;

                dailyBreakdown.Add(new DailyProductivity
                {
                    Date = date,
                    HoursWorked = dayHoursWorked,
                    WorkingHours = dayWorkingHours,
                    ProductivityPercentage = Math.Min(100, dayProductivity),
                    HasDSR = dayDSR != null,
                    Status = GetDayStatus(attendance, dayDSR)
                });
            }
        }

        return new ProductivityReport
        {
            EmployeeId = employeeId,
            EmployeeName = employee.FullName,
            StartDate = startDate,
            EndDate = endDate,
            TotalHoursWorked = totalHoursWorked,
            TotalWorkingHours = totalWorkingHours,
            ProductiveHours = productiveHours,
            IdleHours = idleHours,
            ProductivityPercentage = totalWorkingHours > 0 ? (totalHoursWorked / totalWorkingHours) * 100 : 0,
            TotalDSRsSubmitted = approvedDSRs.Count,
            TotalWorkingDays = totalWorkingDays,
            DailyBreakdown = dailyBreakdown
        };
    }

    public async Task<IEnumerable<IdleEmployeeInfo>> GetIdleEmployeesAsync(DateTime date)
    {
        var organizations = await _organizationRepository.GetAllAsync();
        var organization = organizations.FirstOrDefault();
        var productiveHoursThreshold = organization?.ProductiveHoursThreshold ?? 6;
        var normalWorkingHours = organization?.NormalWorkingHours.TotalHours ?? 8;

        var employees = await _employeeRepository.FindAsync(e => e.Status == EmployeeStatus.Active && !e.IsDeleted, e => e.ReportingManager);

        var idleEmployees = new List<IdleEmployeeInfo>();

        foreach (var employee in employees)
        {
            var dsr = await _dsrRepository.GetDSRByEmployeeAndDateAsync(employee.Id, date);
            var attendance = await _attendanceRepository.FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date.Date == date.Date && !a.IsDeleted);

            var hoursWorked = dsr?.HoursWorked ?? 0;
            var workingHours = (decimal)normalWorkingHours;
            var idleHours = Math.Max(0, workingHours - hoursWorked);
            var idlePercentage = workingHours > 0 ? (idleHours / workingHours) * 100 : 0;

            // Consider employee idle if they have no DSR or insufficient productive hours
            if (dsr == null || hoursWorked < productiveHoursThreshold)
            {
                var reason = dsr == null ? "No DSR submitted" : "Insufficient productive hours";
                
                idleEmployees.Add(new IdleEmployeeInfo
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employee.FullName,
                    Department = employee.Department,
                    Designation = employee.Designation,
                    Date = date,
                    WorkingHours = workingHours,
                    HoursWorked = hoursWorked,
                    IdleHours = idleHours,
                    IdlePercentage = idlePercentage,
                    Reason = reason,
                    HasDSR = dsr != null,
                    ManagerId = employee.ReportingManagerId,
                    ManagerName = employee.ReportingManager?.FullName
                });
            }
        }

        return idleEmployees.OrderByDescending(e => e.IdlePercentage);
    }

    public async Task<ProductivitySummary> GetTeamProductivityAsync(int managerId, DateTime startDate, DateTime endDate)
    {
        var manager = await _employeeRepository.GetByIdAsync(managerId);
        if (manager == null)
            throw new ArgumentException("Manager not found");

        var teamMembers = await _employeeRepository.FindAsync(e => e.ReportingManagerId == managerId && e.Status == EmployeeStatus.Active && !e.IsDeleted);

        var organizations = await _organizationRepository.GetAllAsync();
        var organization = organizations.FirstOrDefault();
        var normalWorkingHours = organization?.NormalWorkingHours.TotalHours ?? 8;
        var workingDays = GetWorkingDays(startDate, endDate);

        var teamProductivity = new List<EmployeeProductivitySummary>();
        decimal totalHoursWorked = 0;
        decimal totalWorkingHours = 0;
        int totalDSRsSubmitted = 0;
        int expectedDSRs = teamMembers.Count() * workingDays;

        foreach (var employee in teamMembers)
        {
            var dsrs = await _dsrRepository.GetDSRsByEmployeeAsync(employee.Id, startDate, endDate);
            var approvedDSRs = dsrs.Where(d => d.Status == DSRStatus.Approved).ToList();
            
            var employeeHoursWorked = approvedDSRs.Sum(d => d.HoursWorked);
            var employeeWorkingHours = workingDays * (decimal)normalWorkingHours;
            var employeeProductivity = employeeWorkingHours > 0 ? (employeeHoursWorked / employeeWorkingHours) * 100 : 0;
            
            totalHoursWorked += employeeHoursWorked;
            totalWorkingHours += employeeWorkingHours;
            totalDSRsSubmitted += approvedDSRs.Count;

            var status = employeeProductivity >= 80 ? "High" : employeeProductivity >= 60 ? "Medium" : "Low";

            teamProductivity.Add(new EmployeeProductivitySummary
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                Department = employee.Department,
                HoursWorked = employeeHoursWorked,
                WorkingHours = employeeWorkingHours,
                ProductivityPercentage = employeeProductivity,
                DSRsSubmitted = approvedDSRs.Count,
                ExpectedDSRs = workingDays,
                Status = status
            });
        }

        var averageProductivity = totalWorkingHours > 0 ? (totalHoursWorked / totalWorkingHours) * 100 : 0;
        var dsrSubmissionRate = expectedDSRs > 0 ? ((decimal)totalDSRsSubmitted / expectedDSRs) * 100 : 0;

        return new ProductivitySummary
        {
            ManagerId = managerId,
            ManagerName = manager.FullName,
            StartDate = startDate,
            EndDate = endDate,
            TotalTeamMembers = teamMembers.Count(),
            AverageProductivity = averageProductivity,
            TotalHoursWorked = totalHoursWorked,
            TotalWorkingHours = totalWorkingHours,
            TotalDSRsSubmitted = totalDSRsSubmitted,
            ExpectedDSRs = expectedDSRs,
            DSRSubmissionRate = dsrSubmissionRate,
            TeamMemberProductivity = teamProductivity
        };
    }

    public async Task<ProjectHoursReport> GetProjectHoursReportAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new ArgumentException("Project not found");

        var dsrs = await _dsrRepository.GetDSRsByProjectAsync(projectId, startDate, endDate);
        var approvedDSRs = dsrs.Where(d => d.Status == DSRStatus.Approved).ToList();

        var actualHoursWorked = approvedDSRs.Sum(d => d.HoursWorked);
        var hoursVariance = actualHoursWorked - project.EstimatedHours;
        var variancePercentage = project.EstimatedHours > 0 ? (hoursVariance / project.EstimatedHours) * 100 : 0;
        var isOnTrack = actualHoursWorked <= project.EstimatedHours;
        var remainingHours = Math.Max(0, project.EstimatedHours - actualHoursWorked);

        // Employee contributions
        var employeeContributions = approvedDSRs
            .GroupBy(d => d.EmployeeId)
            .Select(g => new EmployeeHoursContribution
            {
                EmployeeId = g.Key,
                EmployeeName = g.First().Employee?.FullName ?? "Unknown",
                HoursWorked = g.Sum(d => d.HoursWorked),
                ContributionPercentage = actualHoursWorked > 0 ? (g.Sum(d => d.HoursWorked) / actualHoursWorked) * 100 : 0,
                DSRsSubmitted = g.Count()
            })
            .OrderByDescending(e => e.HoursWorked)
            .ToList();

        // Task breakdown
        var taskBreakdown = approvedDSRs
            .Where(d => d.TaskId.HasValue)
            .GroupBy(d => d.TaskId)
            .Select(g => new TaskHoursBreakdown
            {
                TaskId = g.Key,
                TaskTitle = g.First().Task?.Title ?? "Unknown Task",
                EstimatedHours = g.First().Task?.EstimatedHours ?? 0,
                ActualHours = g.Sum(d => d.HoursWorked),
                Variance = g.Sum(d => d.HoursWorked) - (g.First().Task?.EstimatedHours ?? 0),
                Status = g.First().Task?.Status.ToString() ?? "Unknown"
            })
            .ToList();

        return new ProjectHoursReport
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            StartDate = startDate,
            EndDate = endDate,
            EstimatedHours = project.EstimatedHours,
            ActualHoursWorked = actualHoursWorked,
            HoursVariance = hoursVariance,
            VariancePercentage = variancePercentage,
            IsOnTrack = isOnTrack,
            RemainingHours = remainingHours,
            EmployeeContributions = employeeContributions,
            TaskBreakdown = taskBreakdown
        };
    }

    public async Task<IEnumerable<ProjectHoursComparison>> GetProjectHoursVsEstimatesAsync(int teamLeaderId)
    {
        var projects = await _projectRepository.GetProjectsByTeamLeaderAsync(teamLeaderId);
        var comparisons = new List<ProjectHoursComparison>();

        foreach (var project in projects)
        {
            var dsrs = await _dsrRepository.GetDSRsByProjectAsync(project.Id);
            var approvedDSRs = dsrs.Where(d => d.Status == DSRStatus.Approved).ToList();
            
            var actualHoursWorked = approvedDSRs.Sum(d => d.HoursWorked);
            var hoursVariance = actualHoursWorked - project.EstimatedHours;
            var variancePercentage = project.EstimatedHours > 0 ? (hoursVariance / project.EstimatedHours) * 100 : 0;
            
            var completionPercentage = CalculateProjectCompletionPercentage(project);
            var isOverBudget = actualHoursWorked > project.EstimatedHours;
            var isDelayed = project.EndDate < DateTime.Now && project.Status != ProjectStatus.Completed;
            
            var riskLevel = GetProjectRiskLevel(variancePercentage, isDelayed, completionPercentage);
            var projectedTotalHours = EstimateProjectedTotalHours(project, actualHoursWorked, completionPercentage);
            
            var teamMembersCount = await _projectRepository.GetProjectTeamMembersCountAsync(project.Id);
            var projectDays = (project.EndDate - project.StartDate).Days + 1;
            var averageHoursPerDay = projectDays > 0 ? actualHoursWorked / projectDays : 0;

            comparisons.Add(new ProjectHoursComparison
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                ProjectStatus = project.Status.ToString(),
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                EstimatedHours = project.EstimatedHours,
                ActualHoursWorked = actualHoursWorked,
                HoursVariance = hoursVariance,
                VariancePercentage = variancePercentage,
                CompletionPercentage = completionPercentage,
                IsOverBudget = isOverBudget,
                IsDelayed = isDelayed,
                RiskLevel = riskLevel,
                ProjectedTotalHours = projectedTotalHours,
                TeamMembersCount = teamMembersCount,
                AverageHoursPerDay = averageHoursPerDay
            });
        }

        return comparisons.OrderByDescending(c => c.VariancePercentage);
    }

    public async Task<decimal> GetTotalHoursByProjectAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _dsrRepository.GetTotalHoursByProjectAsync(projectId, startDate, endDate);
    }

    public async Task<decimal> GetTotalHoursByEmployeeAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _dsrRepository.GetTotalHoursByEmployeeAsync(employeeId, startDate, endDate);
    }

    public async Task<bool> CanEmployeeSubmitDSRAsync(int employeeId, DateTime date)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null || employee.Status != EmployeeStatus.Active)
            return false;

        var existingDSR = await _dsrRepository.GetDSRByEmployeeAndDateAsync(employeeId, date);
        return existingDSR == null;
    }

    public async Task<IEnumerable<Project>> GetAssignedProjectsAsync(int employeeId)
    {
        return await _projectRepository.GetProjectsByEmployeeAsync(employeeId);
    }

    public async Task<IEnumerable<ProjectTask>> GetAssignedTasksAsync(int employeeId, int? projectId = null)
    {
        return await _taskRepository.GetTasksByEmployeeAsync(employeeId, projectId);
    }

    // Helper methods
    private static int GetWorkingDays(DateTime startDate, DateTime endDate)
    {
        int workingDays = 0;
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            if (IsWorkingDay(date))
                workingDays++;
        }
        return workingDays;
    }

    private static bool IsWorkingDay(DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
    }

    private static string GetDayStatus(AttendanceRecord? attendance, DSR? dsr)
    {
        if (attendance == null)
            return "Absent";
        
        return attendance.Status switch
        {
            AttendanceStatus.Present => dsr != null ? "Productive" : "Present",
            AttendanceStatus.Late => "Late",
            AttendanceStatus.OnLeave => "On Leave",
            AttendanceStatus.HalfDay => "Half Day",
            _ => "Absent"
        };
    }

    private static decimal CalculateProjectCompletionPercentage(Project project)
    {
        // This is a simplified calculation - in reality, you might want to calculate based on completed tasks
        var totalDays = (project.EndDate - project.StartDate).Days + 1;
        var elapsedDays = (DateTime.Now - project.StartDate).Days + 1;
        
        if (totalDays <= 0) return 0;
        if (elapsedDays <= 0) return 0;
        if (elapsedDays >= totalDays) return 100;
        
        return Math.Min(100, (decimal)elapsedDays / totalDays * 100);
    }

    private static string GetProjectRiskLevel(decimal variancePercentage, bool isDelayed, decimal completionPercentage)
    {
        if (isDelayed || variancePercentage > 50 || (completionPercentage > 80 && variancePercentage > 20))
            return "High";
        
        if (variancePercentage > 20 || (completionPercentage > 60 && variancePercentage > 10))
            return "Medium";
        
        return "Low";
    }

    private static decimal EstimateProjectedTotalHours(Project project, decimal actualHoursWorked, decimal completionPercentage)
    {
        if (completionPercentage <= 0) return project.EstimatedHours;
        if (completionPercentage >= 100) return actualHoursWorked;
        
        return actualHoursWorked / (completionPercentage / 100);
    }
}
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.DSR;

namespace StrideHR.Core.Interfaces.Services;

public interface IDSRService
{
    // DSR Submission
    Task<DSR> CreateDSRAsync(CreateDSRRequest request);
    Task<DSR> UpdateDSRAsync(int dsrId, UpdateDSRRequest request);
    Task<DSR> SubmitDSRAsync(int dsrId, int employeeId);
    Task<bool> DeleteDSRAsync(int dsrId, int employeeId);
    
    // DSR Retrieval
    Task<DSR?> GetDSRByIdAsync(int dsrId);
    Task<DSR?> GetDSRByEmployeeAndDateAsync(int employeeId, DateTime date);
    Task<IEnumerable<DSR>> GetDSRsByEmployeeAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<DSR>> GetDSRsByProjectAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
    
    // DSR Review and Approval
    Task<DSR> ReviewDSRAsync(int dsrId, ReviewDSRRequest request);
    Task<IEnumerable<DSR>> GetPendingDSRsForReviewAsync(int reviewerId);
    Task<IEnumerable<DSR>> GetDSRsByStatusAsync(DSRStatus status);
    
    // Productivity Tracking
    Task<ProductivityReport> GetEmployeeProductivityAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<IdleEmployeeInfo>> GetIdleEmployeesAsync(DateTime date);
    Task<ProductivitySummary> GetTeamProductivityAsync(int managerId, DateTime startDate, DateTime endDate);
    
    // Project Hours Tracking
    Task<ProjectHoursReport> GetProjectHoursReportAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<ProjectHoursComparison>> GetProjectHoursVsEstimatesAsync(int teamLeaderId);
    Task<decimal> GetTotalHoursByProjectAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetTotalHoursByEmployeeAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
    
    // Validation
    Task<bool> CanEmployeeSubmitDSRAsync(int employeeId, DateTime date);
    Task<IEnumerable<Project>> GetAssignedProjectsAsync(int employeeId);
    Task<IEnumerable<ProjectTask>> GetAssignedTasksAsync(int employeeId, int? projectId = null);
}
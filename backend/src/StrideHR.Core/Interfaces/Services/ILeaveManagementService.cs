using StrideHR.Core.Models.Leave;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Services;

public interface ILeaveManagementService
{
    // Leave Request Management
    Task<LeaveRequestDto> CreateLeaveRequestAsync(int employeeId, CreateLeaveRequestDto request);
    Task<LeaveRequestDto> GetLeaveRequestAsync(int id);
    Task<IEnumerable<LeaveRequestDto>> GetEmployeeLeaveRequestsAsync(int employeeId);
    Task<IEnumerable<LeaveRequestDto>> GetPendingRequestsAsync(int branchId);
    Task<IEnumerable<LeaveRequestDto>> GetRequestsForApprovalAsync(int approverId);
    Task<LeaveRequestDto> UpdateLeaveRequestAsync(int id, CreateLeaveRequestDto request);
    Task<bool> CancelLeaveRequestAsync(int id, int employeeId);
    
    // Leave Approval Workflow
    Task<LeaveRequestDto> ApproveLeaveRequestAsync(int requestId, LeaveApprovalDto approval, int approverId);
    Task<LeaveRequestDto> RejectLeaveRequestAsync(int requestId, LeaveApprovalDto rejection, int approverId);
    Task<LeaveRequestDto> EscalateLeaveRequestAsync(int requestId, int escalateToId, int approverId, string? comments = null);
    
    // Leave Balance Management
    Task<IEnumerable<LeaveBalanceDto>> GetEmployeeLeaveBalancesAsync(int employeeId);
    Task<LeaveBalanceDto> GetLeaveBalanceAsync(int employeeId, int leavePolicyId, int year);
    Task<bool> ValidateLeaveBalanceAsync(int employeeId, int leavePolicyId, decimal requestedDays, int year);
    
    // Leave Policy Management
    Task<IEnumerable<LeavePolicyDto>> GetBranchLeavePoliciesAsync(int branchId);
    Task<LeavePolicyDto> GetLeavePolicyAsync(int id);
    
    // Leave Calendar and Conflict Detection
    Task<IEnumerable<LeaveCalendarDto>> GetLeaveCalendarAsync(DateTime startDate, DateTime endDate, int branchId);
    Task<IEnumerable<LeaveCalendarDto>> GetEmployeeLeaveCalendarAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<LeaveCalendarDto>> GetTeamLeaveCalendarAsync(int managerId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<LeaveConflictDto>> DetectLeaveConflictsAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<LeaveConflictDto>> GetTeamLeaveConflictsAsync(int managerId, DateTime startDate, DateTime endDate);
    
    // Utility Methods
    Task<decimal> CalculateLeaveDaysAsync(DateTime startDate, DateTime endDate, int branchId);
    Task<bool> IsWorkingDayAsync(DateTime date, int branchId);
}
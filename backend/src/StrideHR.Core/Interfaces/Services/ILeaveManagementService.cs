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
    Task<LeaveBalanceDto> UpdateEmployeeLeaveBalanceAsync(int employeeId, int leavePolicyId, decimal usedDays, int year);
    Task<IEnumerable<LeaveBalanceDto>> RecalculateLeaveBalancesAsync(int employeeId, int year);
    
    // Leave Accrual Management
    Task<IEnumerable<LeaveAccrualDto>> GetEmployeeAccrualsAsync(int employeeId, int year);
    Task<LeaveAccrualDto> CreateAccrualAsync(CreateLeaveAccrualDto accrual);
    Task<IEnumerable<LeaveAccrualDto>> ProcessMonthlyAccrualsAsync(int year, int month);
    Task<IEnumerable<LeaveAccrualDto>> ProcessEmployeeAccrualsAsync(int employeeId, int year);
    
    // Leave Accrual Rules Management
    Task<IEnumerable<LeaveAccrualRuleDto>> GetAccrualRulesAsync(int leavePolicyId);
    Task<LeaveAccrualRuleDto> CreateAccrualRuleAsync(CreateLeaveAccrualRuleDto rule);
    Task<LeaveAccrualRuleDto> UpdateAccrualRuleAsync(int id, CreateLeaveAccrualRuleDto rule);
    Task<bool> DeleteAccrualRuleAsync(int id);
    
    // Leave Encashment Management
    Task<IEnumerable<LeaveEncashmentDto>> GetEmployeeEncashmentsAsync(int employeeId, int year);
    Task<LeaveEncashmentDto> CreateEncashmentRequestAsync(CreateLeaveEncashmentDto request);
    Task<LeaveEncashmentDto> ApproveEncashmentAsync(int id, ApproveLeaveEncashmentDto approval, int approverId);
    Task<LeaveEncashmentDto> RejectEncashmentAsync(int id, string reason, int approverId);
    Task<IEnumerable<LeaveEncashmentDto>> GetPendingEncashmentsAsync(int branchId);
    Task<decimal> CalculateEncashmentAmountAsync(int employeeId, int leavePolicyId, decimal days);
    
    // Leave History and Analytics
    Task<LeaveHistoryDto> GetEmployeeLeaveHistoryAsync(int employeeId, int year);
    Task<IEnumerable<LeaveHistoryDto>> GetBranchLeaveHistoryAsync(int branchId, int year);
    Task<LeaveAnalyticsDto> GetLeaveAnalyticsAsync(int branchId, int year);
    Task<IEnumerable<LeaveTransactionDto>> GetLeaveTransactionsAsync(int employeeId, int leavePolicyId, int year);
    
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
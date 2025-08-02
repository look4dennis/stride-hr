using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;

namespace StrideHR.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Organization> Organizations { get; }
    IRepository<Branch> Branches { get; }
    IRepository<Employee> Employees { get; }
    IRepository<User> Users { get; }
    IRepository<AttendanceRecord> AttendanceRecords { get; }
    IRepository<BreakRecord> BreakRecords { get; }
    IRepository<Role> Roles { get; }
    IRepository<Permission> Permissions { get; }
    IRepository<RolePermission> RolePermissions { get; }
    IRepository<EmployeeRole> EmployeeRoles { get; }
    IRepository<Shift> Shifts { get; }
    IRepository<ShiftAssignment> ShiftAssignments { get; }
    IRepository<EmployeeOnboarding> EmployeeOnboardings { get; }
    IRepository<EmployeeOnboardingTask> EmployeeOnboardingTasks { get; }
    IRepository<EmployeeExit> EmployeeExits { get; }
    IRepository<EmployeeExitTask> EmployeeExitTasks { get; }
    
    // Leave Management Repositories
    ILeaveRequestRepository LeaveRequests { get; }
    ILeaveBalanceRepository LeaveBalances { get; }
    ILeavePolicyRepository LeavePolicies { get; }
    ILeaveApprovalHistoryRepository LeaveApprovalHistory { get; }
    ILeaveCalendarRepository LeaveCalendar { get; }
    ILeaveAccrualRepository LeaveAccruals { get; }
    ILeaveEncashmentRepository LeaveEncashments { get; }
    ILeaveAccrualRuleRepository LeaveAccrualRules { get; }
    
    // Performance Management Repositories
    IPerformanceGoalRepository PerformanceGoals { get; }
    IRepository<PerformanceGoalCheckIn> PerformanceGoalCheckIns { get; }
    IPerformanceReviewRepository PerformanceReviews { get; }
    IPerformanceFeedbackRepository PerformanceFeedbacks { get; }
    IPerformanceImprovementPlanRepository PerformanceImprovementPlans { get; }
    IRepository<PIPGoal> PIPGoals { get; }
    IRepository<PIPReview> PIPReviews { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
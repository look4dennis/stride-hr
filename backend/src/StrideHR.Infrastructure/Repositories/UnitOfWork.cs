using Microsoft.EntityFrameworkCore.Storage;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly StrideHRDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    // Repository instances
    private IRepository<Organization>? _organizations;
    private IRepository<Branch>? _branches;
    private IRepository<Employee>? _employees;
    private IRepository<User>? _users;
    private IRepository<AttendanceRecord>? _attendanceRecords;
    private IRepository<BreakRecord>? _breakRecords;
    private IRepository<Role>? _roles;
    private IRepository<Permission>? _permissions;
    private IRepository<RolePermission>? _rolePermissions;
    private IRepository<EmployeeRole>? _employeeRoles;
    private IRepository<Shift>? _shifts;
    private IRepository<ShiftAssignment>? _shiftAssignments;
    private IRepository<EmployeeOnboarding>? _employeeOnboardings;
    private IRepository<EmployeeOnboardingTask>? _employeeOnboardingTasks;
    private IRepository<EmployeeExit>? _employeeExits;
    private IRepository<EmployeeExitTask>? _employeeExitTasks;
    
    // Leave Management Repositories
    private ILeaveRequestRepository? _leaveRequests;
    private ILeaveBalanceRepository? _leaveBalances;
    private ILeavePolicyRepository? _leavePolicies;
    private ILeaveApprovalHistoryRepository? _leaveApprovalHistory;
    private ILeaveCalendarRepository? _leaveCalendar;
    private ILeaveAccrualRepository? _leaveAccruals;
    private ILeaveEncashmentRepository? _leaveEncashments;
    private ILeaveAccrualRuleRepository? _leaveAccrualRules;

    public UnitOfWork(StrideHRDbContext context)
    {
        _context = context;
    }

    public IRepository<Organization> Organizations =>
        _organizations ??= new Repository<Organization>(_context);

    public IRepository<Branch> Branches =>
        _branches ??= new Repository<Branch>(_context);

    public IRepository<Employee> Employees =>
        _employees ??= new Repository<Employee>(_context);

    public IRepository<User> Users =>
        _users ??= new Repository<User>(_context);

    public IRepository<AttendanceRecord> AttendanceRecords =>
        _attendanceRecords ??= new Repository<AttendanceRecord>(_context);

    public IRepository<BreakRecord> BreakRecords =>
        _breakRecords ??= new Repository<BreakRecord>(_context);

    public IRepository<Role> Roles =>
        _roles ??= new Repository<Role>(_context);

    public IRepository<Permission> Permissions =>
        _permissions ??= new Repository<Permission>(_context);

    public IRepository<RolePermission> RolePermissions =>
        _rolePermissions ??= new Repository<RolePermission>(_context);

    public IRepository<EmployeeRole> EmployeeRoles =>
        _employeeRoles ??= new Repository<EmployeeRole>(_context);

    public IRepository<Shift> Shifts =>
        _shifts ??= new Repository<Shift>(_context);

    public IRepository<ShiftAssignment> ShiftAssignments =>
        _shiftAssignments ??= new Repository<ShiftAssignment>(_context);

    public IRepository<EmployeeOnboarding> EmployeeOnboardings =>
        _employeeOnboardings ??= new Repository<EmployeeOnboarding>(_context);

    public IRepository<EmployeeOnboardingTask> EmployeeOnboardingTasks =>
        _employeeOnboardingTasks ??= new Repository<EmployeeOnboardingTask>(_context);

    public IRepository<EmployeeExit> EmployeeExits =>
        _employeeExits ??= new Repository<EmployeeExit>(_context);

    public IRepository<EmployeeExitTask> EmployeeExitTasks =>
        _employeeExitTasks ??= new Repository<EmployeeExitTask>(_context);

    // Leave Management Repositories
    public ILeaveRequestRepository LeaveRequests =>
        _leaveRequests ??= new LeaveRequestRepository(_context);

    public ILeaveBalanceRepository LeaveBalances =>
        _leaveBalances ??= new LeaveBalanceRepository(_context);

    public ILeavePolicyRepository LeavePolicies =>
        _leavePolicies ??= new LeavePolicyRepository(_context);

    public ILeaveApprovalHistoryRepository LeaveApprovalHistory =>
        _leaveApprovalHistory ??= new LeaveApprovalHistoryRepository(_context);

    public ILeaveCalendarRepository LeaveCalendar =>
        _leaveCalendar ??= new LeaveCalendarRepository(_context);

    public ILeaveAccrualRepository LeaveAccruals =>
        _leaveAccruals ??= new LeaveAccrualRepository(_context);

    public ILeaveEncashmentRepository LeaveEncashments =>
        _leaveEncashments ??= new LeaveEncashmentRepository(_context);

    public ILeaveAccrualRuleRepository LeaveAccrualRules =>
        _leaveAccrualRules ??= new LeaveAccrualRuleRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
            _disposed = true;
        }
    }
}
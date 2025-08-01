using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Organization> Organizations { get; }
    IRepository<Branch> Branches { get; }
    IRepository<Employee> Employees { get; }
    IRepository<AttendanceRecord> AttendanceRecords { get; }
    IRepository<BreakRecord> BreakRecords { get; }
    IRepository<Role> Roles { get; }
    IRepository<Permission> Permissions { get; }
    IRepository<RolePermission> RolePermissions { get; }
    IRepository<EmployeeRole> EmployeeRoles { get; }
    IRepository<Shift> Shifts { get; }
    IRepository<ShiftAssignment> ShiftAssignments { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
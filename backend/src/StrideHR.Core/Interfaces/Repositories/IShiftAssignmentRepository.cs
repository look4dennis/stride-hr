using StrideHR.Core.Entities;
using StrideHR.Core.Models.Shift;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IShiftAssignmentRepository : IRepository<ShiftAssignment>
{
    Task<IEnumerable<ShiftAssignment>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<ShiftAssignment>> GetByShiftIdAsync(int shiftId);
    Task<IEnumerable<ShiftAssignment>> GetActiveAssignmentsAsync(int employeeId);
    Task<ShiftAssignment?> GetCurrentAssignmentAsync(int employeeId, DateTime date);
    Task<IEnumerable<ShiftAssignment>> GetAssignmentsByDateRangeAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<ShiftAssignment>> SearchAssignmentsAsync(ShiftAssignmentSearchCriteria criteria);
    Task<int> GetTotalCountAsync(ShiftAssignmentSearchCriteria criteria);
    Task<IEnumerable<ShiftAssignment>> GetConflictingAssignmentsAsync(int employeeId, int shiftId, DateTime startDate, DateTime? endDate);
    Task<IEnumerable<ShiftAssignment>> GetAssignmentsByBranchAsync(int branchId, DateTime? date = null);
    Task<bool> HasActiveAssignmentAsync(int employeeId, int shiftId);
    Task<IEnumerable<ShiftAssignment>> GetUpcomingAssignmentsAsync(int employeeId, int days = 7);
    Task<int> GetAssignedEmployeesCountAsync(int shiftId, DateTime? date = null);
    Task<bool> HasConflictingAssignmentAsync(int employeeId, DateTime date);
}
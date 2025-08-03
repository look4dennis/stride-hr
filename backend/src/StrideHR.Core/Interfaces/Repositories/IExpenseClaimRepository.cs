using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IExpenseClaimRepository : IRepository<ExpenseClaim>
{
    Task<ExpenseClaim?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<ExpenseClaim>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<ExpenseClaim>> GetPendingApprovalsAsync(int approverId);
    Task<IEnumerable<ExpenseClaim>> GetByStatusAsync(ExpenseClaimStatus status);
    Task<IEnumerable<ExpenseClaim>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<string> GenerateClaimNumberAsync();
    Task<decimal> GetTotalExpensesByEmployeeAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<ExpenseClaim>> GetExpensesForReimbursementAsync();
    Task<bool> HasPendingClaimsAsync(int employeeId);
}
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IBudgetRepository : IRepository<Budget>
{
    Task<List<Budget>> GetByBranchAndPeriodAsync(int branchId, int year, int? month = null);
    Task<List<Budget>> GetByDepartmentAndPeriodAsync(string department, int year, int? month = null);
    Task<Budget?> GetBudgetAsync(int branchId, string department, string category, int year, int month);
    Task<List<Budget>> GetBudgetsByStatusAsync(BudgetStatus status);
    Task<List<Budget>> GetBudgetsByCategoryAsync(string category, int year, int? month = null);
    Task<decimal> GetTotalBudgetedAmountAsync(int branchId, int year, int? month = null);
    Task<decimal> GetTotalActualAmountAsync(int branchId, int year, int? month = null);
    Task<List<Budget>> GetBudgetsWithVarianceAsync(decimal varianceThreshold);
    Task UpdateActualAmountAsync(int budgetId, decimal actualAmount);
    Task<List<Budget>> GetBudgetsForApprovalAsync(int approverId);
}
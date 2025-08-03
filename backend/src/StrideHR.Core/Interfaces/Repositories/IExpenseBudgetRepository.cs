using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IExpenseBudgetRepository : IRepository<ExpenseBudget>
{
    Task<IEnumerable<ExpenseBudget>> GetByOrganizationIdAsync(int organizationId);
    Task<IEnumerable<ExpenseBudget>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<ExpenseBudget>> GetByCategoryIdAsync(int categoryId);
    Task<ExpenseBudget?> GetActiveBudgetAsync(int organizationId, int? departmentId, int? employeeId, int? categoryId, DateTime date);
    Task<IEnumerable<ExpenseBudget>> GetActiveBudgetsAsync(DateTime date);
    Task<decimal> GetBudgetUtilizationAsync(int budgetId, DateTime? asOfDate = null);
    Task<IEnumerable<ExpenseBudget>> GetBudgetsExceedingThresholdAsync(decimal thresholdPercentage);
    Task<bool> IsBudgetExceededAsync(int budgetId, decimal additionalAmount = 0);
}
using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ExpenseBudgetRepository : Repository<ExpenseBudget>, IExpenseBudgetRepository
{
    public ExpenseBudgetRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ExpenseBudget>> GetByOrganizationIdAsync(int organizationId)
    {
        return await _context.ExpenseBudgets
            .Include(eb => eb.Organization)
            .Include(eb => eb.Employee)
            .Include(eb => eb.Category)
            .Include(eb => eb.BudgetAlerts)
            .Where(eb => eb.OrganizationId == organizationId)
            .OrderBy(eb => eb.BudgetName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExpenseBudget>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.ExpenseBudgets
            .Include(eb => eb.Organization)
            .Include(eb => eb.Employee)
            .Include(eb => eb.Category)
            .Include(eb => eb.BudgetAlerts)
            .Where(eb => eb.EmployeeId == employeeId && eb.IsActive)
            .OrderBy(eb => eb.BudgetName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExpenseBudget>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.ExpenseBudgets
            .Include(eb => eb.Organization)
            .Include(eb => eb.Employee)
            .Include(eb => eb.Category)
            .Include(eb => eb.BudgetAlerts)
            .Where(eb => eb.CategoryId == categoryId && eb.IsActive)
            .OrderBy(eb => eb.BudgetName)
            .ToListAsync();
    }

    public async Task<ExpenseBudget?> GetActiveBudgetAsync(int organizationId, int? departmentId, int? employeeId, int? categoryId, DateTime date)
    {
        var query = _context.ExpenseBudgets
            .Include(eb => eb.Organization)
            .Include(eb => eb.Employee)
            .Include(eb => eb.Category)
            .Include(eb => eb.BudgetAlerts)
            .Where(eb => eb.OrganizationId == organizationId &&
                        eb.IsActive &&
                        eb.StartDate <= date &&
                        eb.EndDate >= date);

        if (employeeId.HasValue)
        {
            query = query.Where(eb => eb.EmployeeId == employeeId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(eb => eb.CategoryId == categoryId.Value);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ExpenseBudget>> GetActiveBudgetsAsync(DateTime date)
    {
        return await _context.ExpenseBudgets
            .Include(eb => eb.Organization)
            .Include(eb => eb.Employee)
            .Include(eb => eb.Category)
            .Include(eb => eb.BudgetAlerts)
            .Where(eb => eb.IsActive &&
                        eb.StartDate <= date &&
                        eb.EndDate >= date)
            .OrderBy(eb => eb.BudgetName)
            .ToListAsync();
    }

    public async Task<decimal> GetBudgetUtilizationAsync(int budgetId, DateTime? asOfDate = null)
    {
        var budget = await _context.ExpenseBudgets.FindAsync(budgetId);
        if (budget == null) return 0;

        var endDate = asOfDate ?? DateTime.Today;
        if (endDate > budget.EndDate) endDate = budget.EndDate;

        var query = _context.ExpenseClaims
            .Where(ec => ec.Employee.Branch.OrganizationId == budget.OrganizationId &&
                        ec.ExpenseDate >= budget.StartDate &&
                        ec.ExpenseDate <= endDate &&
                        (ec.Status == ExpenseClaimStatus.Approved || ec.Status == ExpenseClaimStatus.Reimbursed));

        if (budget.EmployeeId.HasValue)
        {
            query = query.Where(ec => ec.EmployeeId == budget.EmployeeId.Value);
        }

        if (budget.CategoryId.HasValue)
        {
            query = query.Where(ec => ec.ExpenseItems.Any(ei => ei.ExpenseCategoryId == budget.CategoryId.Value));
            var totalAmount = await query
                .SelectMany(ec => ec.ExpenseItems)
                .Where(ei => ei.ExpenseCategoryId == budget.CategoryId.Value)
                .SumAsync(ei => ei.Amount);
            return totalAmount;
        }

        return await query.SumAsync(ec => ec.TotalAmount);
    }

    public async Task<IEnumerable<ExpenseBudget>> GetBudgetsExceedingThresholdAsync(decimal thresholdPercentage)
    {
        var budgets = await GetActiveBudgetsAsync(DateTime.Today);
        var exceedingBudgets = new List<ExpenseBudget>();

        foreach (var budget in budgets)
        {
            var utilization = await GetBudgetUtilizationAsync(budget.Id);
            var utilizationPercentage = budget.BudgetLimit > 0 ? (utilization / budget.BudgetLimit) * 100 : 0;

            if (utilizationPercentage >= thresholdPercentage)
            {
                exceedingBudgets.Add(budget);
            }
        }

        return exceedingBudgets;
    }

    public async Task<bool> IsBudgetExceededAsync(int budgetId, decimal additionalAmount = 0)
    {
        var budget = await _context.ExpenseBudgets.FindAsync(budgetId);
        if (budget == null) return false;

        var currentUtilization = await GetBudgetUtilizationAsync(budgetId);
        return (currentUtilization + additionalAmount) > budget.BudgetLimit;
    }
}
using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class BudgetRepository : Repository<Budget>, IBudgetRepository
{
    public BudgetRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<Budget>> GetByBranchAndPeriodAsync(int branchId, int year, int? month = null)
    {
        var query = _context.Budgets
            .Include(b => b.Branch)
            .Include(b => b.CreatedByEmployee)
            .Include(b => b.ApprovedByEmployee)
            .Where(b => b.BranchId == branchId && b.Year == year);

        if (month.HasValue)
        {
            query = query.Where(b => b.Month == month.Value);
        }

        return await query.OrderBy(b => b.Month).ThenBy(b => b.Department).ToListAsync();
    }

    public async Task<List<Budget>> GetByDepartmentAndPeriodAsync(string department, int year, int? month = null)
    {
        var query = _context.Budgets
            .Include(b => b.Branch)
            .Include(b => b.CreatedByEmployee)
            .Include(b => b.ApprovedByEmployee)
            .Where(b => b.Department == department && b.Year == year);

        if (month.HasValue)
        {
            query = query.Where(b => b.Month == month.Value);
        }

        return await query.OrderBy(b => b.Month).ThenBy(b => b.BranchId).ToListAsync();
    }

    public async Task<Budget?> GetBudgetAsync(int branchId, string department, string category, int year, int month)
    {
        return await _context.Budgets
            .Include(b => b.Branch)
            .Include(b => b.CreatedByEmployee)
            .Include(b => b.ApprovedByEmployee)
            .FirstOrDefaultAsync(b => 
                b.BranchId == branchId && 
                b.Department == department && 
                b.Category == category && 
                b.Year == year && 
                b.Month == month);
    }

    public async Task<List<Budget>> GetBudgetsByStatusAsync(BudgetStatus status)
    {
        return await _context.Budgets
            .Include(b => b.Branch)
            .Include(b => b.CreatedByEmployee)
            .Include(b => b.ApprovedByEmployee)
            .Where(b => b.Status == status)
            .OrderBy(b => b.Year).ThenBy(b => b.Month)
            .ToListAsync();
    }

    public async Task<List<Budget>> GetBudgetsByCategoryAsync(string category, int year, int? month = null)
    {
        var query = _context.Budgets
            .Include(b => b.Branch)
            .Include(b => b.CreatedByEmployee)
            .Include(b => b.ApprovedByEmployee)
            .Where(b => b.Category == category && b.Year == year);

        if (month.HasValue)
        {
            query = query.Where(b => b.Month == month.Value);
        }

        return await query.OrderBy(b => b.Month).ThenBy(b => b.BranchId).ToListAsync();
    }

    public async Task<decimal> GetTotalBudgetedAmountAsync(int branchId, int year, int? month = null)
    {
        var query = _context.Budgets
            .Where(b => b.BranchId == branchId && b.Year == year);

        if (month.HasValue)
        {
            query = query.Where(b => b.Month == month.Value);
        }

        return await query.SumAsync(b => b.BudgetedAmount);
    }

    public async Task<decimal> GetTotalActualAmountAsync(int branchId, int year, int? month = null)
    {
        var query = _context.Budgets
            .Where(b => b.BranchId == branchId && b.Year == year);

        if (month.HasValue)
        {
            query = query.Where(b => b.Month == month.Value);
        }

        return await query.SumAsync(b => b.ActualAmount);
    }

    public async Task<List<Budget>> GetBudgetsWithVarianceAsync(decimal varianceThreshold)
    {
        return await _context.Budgets
            .Include(b => b.Branch)
            .Include(b => b.CreatedByEmployee)
            .Include(b => b.ApprovedByEmployee)
            .Where(b => Math.Abs(b.VariancePercentage) >= varianceThreshold)
            .OrderByDescending(b => Math.Abs(b.VariancePercentage))
            .ToListAsync();
    }

    public async Task UpdateActualAmountAsync(int budgetId, decimal actualAmount)
    {
        var budget = await _context.Budgets.FindAsync(budgetId);
        if (budget != null)
        {
            budget.ActualAmount = actualAmount;
            budget.Variance = actualAmount - budget.BudgetedAmount;
            budget.VariancePercentage = budget.BudgetedAmount != 0 
                ? (budget.Variance / budget.BudgetedAmount) * 100 
                : 0;
            
            // Update status based on variance
            if (budget.VariancePercentage > 10)
            {
                budget.Status = BudgetStatus.Exceeded;
            }

            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Budget>> GetBudgetsForApprovalAsync(int approverId)
    {
        return await _context.Budgets
            .Include(b => b.Branch)
            .Include(b => b.CreatedByEmployee)
            .Where(b => b.Status == BudgetStatus.PendingApproval)
            .OrderBy(b => b.CreatedAt)
            .ToListAsync();
    }
}
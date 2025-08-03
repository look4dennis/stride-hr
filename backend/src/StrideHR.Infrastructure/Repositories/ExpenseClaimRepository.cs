using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ExpenseClaimRepository : Repository<ExpenseClaim>, IExpenseClaimRepository
{
    public ExpenseClaimRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<ExpenseClaim?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(e => e.Employee)
            .Include(e => e.ApprovedByEmployee)
            .Include(e => e.ExpenseItems)
                .ThenInclude(ei => ei.ExpenseCategory)
            .Include(e => e.ExpenseItems)
                .ThenInclude(ei => ei.Project)
            .Include(e => e.ExpenseItems)
                .ThenInclude(ei => ei.Documents)
            .Include(e => e.ApprovalHistory)
                .ThenInclude(ah => ah.Approver)
            .Include(e => e.Documents)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<ExpenseClaim>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _dbSet
            .Include(e => e.Employee)
            .Include(e => e.ExpenseItems)
                .ThenInclude(ei => ei.ExpenseCategory)
            .Include(e => e.Documents)
            .Where(e => e.EmployeeId == employeeId)
            .OrderByDescending(e => e.SubmissionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExpenseClaim>> GetPendingApprovalsAsync(int approverId)
    {
        // Get employee to determine their role and approval level
        var approver = await _context.Set<Employee>()
            .Include(e => e.Branch)
            .FirstOrDefaultAsync(e => e.Id == approverId);

        if (approver == null)
            return new List<ExpenseClaim>();

        var query = _dbSet
            .Include(e => e.Employee)
            .Include(e => e.ExpenseItems)
                .ThenInclude(ei => ei.ExpenseCategory)
            .Where(e => e.Status == ExpenseClaimStatus.Submitted || 
                       e.Status == ExpenseClaimStatus.UnderReview ||
                       e.Status == ExpenseClaimStatus.ManagerApproved);

        // Filter based on branch isolation if enabled
        if (approver.Branch.Organization.BranchIsolationEnabled)
        {
            query = query.Where(e => e.Employee.BranchId == approver.BranchId);
        }

        return await query
            .OrderBy(e => e.SubmissionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExpenseClaim>> GetByStatusAsync(ExpenseClaimStatus status)
    {
        return await _dbSet
            .Include(e => e.Employee)
            .Include(e => e.ExpenseItems)
                .ThenInclude(ei => ei.ExpenseCategory)
            .Where(e => e.Status == status)
            .OrderByDescending(e => e.SubmissionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExpenseClaim>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(e => e.Employee)
            .Include(e => e.ExpenseItems)
                .ThenInclude(ei => ei.ExpenseCategory)
            .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<string> GenerateClaimNumberAsync()
    {
        var year = DateTime.Now.Year;
        var month = DateTime.Now.Month;
        
        var lastClaim = await _dbSet
            .Where(e => e.ClaimNumber.StartsWith($"EXP-{year:D4}-{month:D2}"))
            .OrderByDescending(e => e.ClaimNumber)
            .FirstOrDefaultAsync();

        int nextSequence = 1;
        if (lastClaim != null)
        {
            var lastSequence = lastClaim.ClaimNumber.Split('-').LastOrDefault();
            if (int.TryParse(lastSequence, out int sequence))
            {
                nextSequence = sequence + 1;
            }
        }

        return $"EXP-{year:D4}-{month:D2}-{nextSequence:D4}";
    }

    public async Task<decimal> GetTotalExpensesByEmployeeAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(e => e.EmployeeId == employeeId && 
                       e.ExpenseDate >= startDate && 
                       e.ExpenseDate <= endDate &&
                       (e.Status == ExpenseClaimStatus.Approved || e.Status == ExpenseClaimStatus.Reimbursed))
            .SumAsync(e => e.TotalAmount);
    }

    public async Task<IEnumerable<ExpenseClaim>> GetExpensesForReimbursementAsync()
    {
        return await _dbSet
            .Include(e => e.Employee)
            .Include(e => e.ExpenseItems)
                .ThenInclude(ei => ei.ExpenseCategory)
            .Where(e => e.Status == ExpenseClaimStatus.Approved)
            .OrderBy(e => e.ApprovedDate)
            .ToListAsync();
    }

    public async Task<bool> HasPendingClaimsAsync(int employeeId)
    {
        return await _dbSet
            .AnyAsync(e => e.EmployeeId == employeeId && 
                          (e.Status == ExpenseClaimStatus.Submitted ||
                           e.Status == ExpenseClaimStatus.UnderReview ||
                           e.Status == ExpenseClaimStatus.ManagerApproved ||
                           e.Status == ExpenseClaimStatus.RequiresMoreInfo));
    }
}
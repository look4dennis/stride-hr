using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ExpenseComplianceViolationRepository : Repository<ExpenseComplianceViolation>, IExpenseComplianceViolationRepository
{
    public ExpenseComplianceViolationRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ExpenseComplianceViolation>> GetByExpenseClaimIdAsync(int expenseClaimId)
    {
        return await _context.ExpenseComplianceViolations
            .Include(ecv => ecv.ExpenseClaim)
            .Include(ecv => ecv.ExpenseItem)
            .Include(ecv => ecv.PolicyRule)
            .Include(ecv => ecv.ResolvedByEmployee)
            .Include(ecv => ecv.WaivedByEmployee)
            .Where(ecv => ecv.ExpenseClaimId == expenseClaimId)
            .OrderByDescending(ecv => ecv.ViolationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExpenseComplianceViolation>> GetByEmployeeIdAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ExpenseComplianceViolations
            .Include(ecv => ecv.ExpenseClaim)
            .Include(ecv => ecv.ExpenseItem)
            .Include(ecv => ecv.PolicyRule)
            .Include(ecv => ecv.ResolvedByEmployee)
            .Include(ecv => ecv.WaivedByEmployee)
            .Where(ecv => ecv.ExpenseClaim.EmployeeId == employeeId);

        if (startDate.HasValue)
        {
            query = query.Where(ecv => ecv.ViolationDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(ecv => ecv.ViolationDate <= endDate.Value);
        }

        return await query.OrderByDescending(ecv => ecv.ViolationDate).ToListAsync();
    }

    public async Task<IEnumerable<ExpenseComplianceViolation>> GetUnresolvedViolationsAsync()
    {
        return await _context.ExpenseComplianceViolations
            .Include(ecv => ecv.ExpenseClaim)
            .Include(ecv => ecv.ExpenseItem)
            .Include(ecv => ecv.PolicyRule)
            .Include(ecv => ecv.ResolvedByEmployee)
            .Include(ecv => ecv.WaivedByEmployee)
            .Where(ecv => !ecv.IsResolved && !ecv.IsWaived)
            .OrderByDescending(ecv => ecv.Severity)
            .ThenByDescending(ecv => ecv.ViolationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExpenseComplianceViolation>> GetBySeverityAsync(ExpenseViolationSeverity severity)
    {
        return await _context.ExpenseComplianceViolations
            .Include(ecv => ecv.ExpenseClaim)
            .Include(ecv => ecv.ExpenseItem)
            .Include(ecv => ecv.PolicyRule)
            .Include(ecv => ecv.ResolvedByEmployee)
            .Include(ecv => ecv.WaivedByEmployee)
            .Where(ecv => ecv.Severity == severity)
            .OrderByDescending(ecv => ecv.ViolationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExpenseComplianceViolation>> GetByPolicyRuleIdAsync(int policyRuleId)
    {
        return await _context.ExpenseComplianceViolations
            .Include(ecv => ecv.ExpenseClaim)
            .Include(ecv => ecv.ExpenseItem)
            .Include(ecv => ecv.PolicyRule)
            .Include(ecv => ecv.ResolvedByEmployee)
            .Include(ecv => ecv.WaivedByEmployee)
            .Where(ecv => ecv.PolicyRuleId == policyRuleId)
            .OrderByDescending(ecv => ecv.ViolationDate)
            .ToListAsync();
    }

    public async Task<decimal> GetComplianceRateAsync(DateTime startDate, DateTime endDate, int? employeeId = null)
    {
        var claimsQuery = _context.ExpenseClaims
            .Where(ec => ec.ExpenseDate >= startDate && ec.ExpenseDate <= endDate);

        if (employeeId.HasValue)
        {
            claimsQuery = claimsQuery.Where(ec => ec.EmployeeId == employeeId.Value);
        }

        var totalClaims = await claimsQuery.CountAsync();
        if (totalClaims == 0) return 100; // 100% compliance if no claims

        var violationsQuery = _context.ExpenseComplianceViolations
            .Where(ecv => ecv.ViolationDate >= startDate && ecv.ViolationDate <= endDate);

        if (employeeId.HasValue)
        {
            violationsQuery = violationsQuery.Where(ecv => ecv.ExpenseClaim.EmployeeId == employeeId.Value);
        }

        var claimsWithViolations = await violationsQuery
            .Select(ecv => ecv.ExpenseClaimId)
            .Distinct()
            .CountAsync();

        var compliantClaims = totalClaims - claimsWithViolations;
        return totalClaims > 0 ? (decimal)compliantClaims / totalClaims * 100 : 100;
    }

    public async Task<Dictionary<string, int>> GetViolationTypeCountsAsync(DateTime startDate, DateTime endDate)
    {
        var violations = await _context.ExpenseComplianceViolations
            .Where(ecv => ecv.ViolationDate >= startDate && ecv.ViolationDate <= endDate)
            .GroupBy(ecv => ecv.ViolationType)
            .Select(g => new { ViolationType = g.Key, Count = g.Count() })
            .ToListAsync();

        return violations.ToDictionary(v => v.ViolationType, v => v.Count);
    }

    public async Task<bool> HasUnresolvedViolationsAsync(int expenseClaimId)
    {
        return await _context.ExpenseComplianceViolations
            .AnyAsync(ecv => ecv.ExpenseClaimId == expenseClaimId && 
                           !ecv.IsResolved && 
                           !ecv.IsWaived);
    }
}
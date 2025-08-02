using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class PayrollFormulaRepository : Repository<PayrollFormula>, IPayrollFormulaRepository
{
    public PayrollFormulaRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<PayrollFormula>> GetActiveFormulasAsync()
    {
        return await _context.PayrollFormulas
            .Include(f => f.Organization)
            .Include(f => f.Branch)
            .Where(f => f.IsActive)
            .OrderBy(f => f.Priority)
            .ToListAsync();
    }

    public async Task<List<PayrollFormula>> GetFormulasByTypeAsync(PayrollFormulaType type)
    {
        return await _context.PayrollFormulas
            .Include(f => f.Organization)
            .Include(f => f.Branch)
            .Where(f => f.IsActive && f.Type == type)
            .OrderBy(f => f.Priority)
            .ToListAsync();
    }

    public async Task<List<PayrollFormula>> GetFormulasForEmployeeAsync(int employeeId)
    {
        var employee = await _context.Employees
            .Include(e => e.Branch)
            .ThenInclude(b => b.Organization)
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee == null)
            return new List<PayrollFormula>();

        return await _context.PayrollFormulas
            .Include(f => f.Organization)
            .Include(f => f.Branch)
            .Where(f => f.IsActive &&
                       (f.OrganizationId == null || f.OrganizationId == employee.Branch.OrganizationId) &&
                       (f.BranchId == null || f.BranchId == employee.BranchId) &&
                       (string.IsNullOrEmpty(f.Department) || f.Department == employee.Department) &&
                       (string.IsNullOrEmpty(f.Designation) || f.Designation == employee.Designation))
            .OrderBy(f => f.Priority)
            .ToListAsync();
    }

    public async Task<List<PayrollFormula>> GetFormulasByBranchAsync(int branchId)
    {
        return await _context.PayrollFormulas
            .Include(f => f.Organization)
            .Include(f => f.Branch)
            .Where(f => f.IsActive && (f.BranchId == null || f.BranchId == branchId))
            .OrderBy(f => f.Priority)
            .ToListAsync();
    }

    public async Task<List<PayrollFormula>> GetFormulasByOrganizationAsync(int organizationId)
    {
        return await _context.PayrollFormulas
            .Include(f => f.Organization)
            .Include(f => f.Branch)
            .Where(f => f.IsActive && (f.OrganizationId == null || f.OrganizationId == organizationId))
            .OrderBy(f => f.Priority)
            .ToListAsync();
    }
}
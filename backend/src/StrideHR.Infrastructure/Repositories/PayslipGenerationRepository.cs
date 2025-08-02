using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class PayslipGenerationRepository : Repository<PayslipGeneration>, IPayslipGenerationRepository
{
    public PayslipGenerationRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<PayslipGeneration?> GetByPayrollRecordAsync(int payrollRecordId)
    {
        return await _context.PayslipGenerations
            .Include(p => p.PayrollRecord)
                .ThenInclude(pr => pr.Employee)
            .Include(p => p.PayslipTemplate)
            .Include(p => p.GeneratedByEmployee)
            .Include(p => p.HRApprovedByEmployee)
            .Include(p => p.FinanceApprovedByEmployee)
            .Include(p => p.ReleasedByEmployee)
            .Include(p => p.ApprovalHistory)
                .ThenInclude(ah => ah.ActionByEmployee)
            .Where(p => p.PayrollRecordId == payrollRecordId)
            .OrderByDescending(p => p.Version)
            .FirstOrDefaultAsync();
    }

    public async Task<List<PayslipGeneration>> GetByStatusAsync(PayslipStatus status)
    {
        return await _context.PayslipGenerations
            .Include(p => p.PayrollRecord)
                .ThenInclude(pr => pr.Employee)
            .Include(p => p.PayslipTemplate)
            .Include(p => p.GeneratedByEmployee)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.GeneratedAt)
            .ToListAsync();
    }

    public async Task<List<PayslipGeneration>> GetPendingApprovalsAsync(PayslipApprovalLevel approvalLevel)
    {
        var status = approvalLevel == PayslipApprovalLevel.HR 
            ? PayslipStatus.PendingHRApproval 
            : PayslipStatus.PendingFinanceApproval;

        return await _context.PayslipGenerations
            .Include(p => p.PayrollRecord)
                .ThenInclude(pr => pr.Employee)
                    .ThenInclude(e => e.Branch)
            .Include(p => p.PayslipTemplate)
            .Include(p => p.GeneratedByEmployee)
            .Where(p => p.Status == status)
            .OrderBy(p => p.GeneratedAt)
            .ToListAsync();
    }

    public async Task<List<PayslipGeneration>> GetByBranchAndPeriodAsync(int branchId, int year, int month)
    {
        return await _context.PayslipGenerations
            .Include(p => p.PayrollRecord)
                .ThenInclude(pr => pr.Employee)
            .Include(p => p.PayslipTemplate)
            .Include(p => p.GeneratedByEmployee)
            .Include(p => p.HRApprovedByEmployee)
            .Include(p => p.FinanceApprovedByEmployee)
            .Where(p => p.PayrollRecord.Employee.BranchId == branchId &&
                       p.PayrollRecord.PayrollYear == year &&
                       p.PayrollRecord.PayrollMonth == month)
            .OrderBy(p => p.PayrollRecord.Employee.FullName)
            .ToListAsync();
    }

    public async Task<List<PayslipGeneration>> GetByEmployeeAsync(int employeeId, int year, int? month = null)
    {
        var query = _context.PayslipGenerations
            .Include(p => p.PayrollRecord)
            .Include(p => p.PayslipTemplate)
            .Include(p => p.GeneratedByEmployee)
            .Where(p => p.PayrollRecord.EmployeeId == employeeId &&
                       p.PayrollRecord.PayrollYear == year);

        if (month.HasValue)
        {
            query = query.Where(p => p.PayrollRecord.PayrollMonth == month.Value);
        }

        return await query
            .OrderByDescending(p => p.PayrollRecord.PayrollMonth)
            .ThenByDescending(p => p.Version)
            .ToListAsync();
    }

    public async Task<bool> UpdateStatusAsync(int payslipGenerationId, PayslipStatus status)
    {
        var payslipGeneration = await _context.PayslipGenerations
            .FindAsync(payslipGenerationId);

        if (payslipGeneration == null)
            return false;

        payslipGeneration.Status = status;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<PayslipGeneration>> GetReleasedPayslipsAsync(int branchId, DateTime fromDate, DateTime toDate)
    {
        return await _context.PayslipGenerations
            .Include(p => p.PayrollRecord)
                .ThenInclude(pr => pr.Employee)
            .Include(p => p.PayslipTemplate)
            .Where(p => p.PayrollRecord.Employee.BranchId == branchId &&
                       p.Status == PayslipStatus.Released &&
                       p.ReleasedAt >= fromDate &&
                       p.ReleasedAt <= toDate)
            .OrderByDescending(p => p.ReleasedAt)
            .ToListAsync();
    }
}
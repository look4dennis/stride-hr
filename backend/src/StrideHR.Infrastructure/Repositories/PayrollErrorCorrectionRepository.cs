using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Payroll;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class PayrollErrorCorrectionRepository : Repository<PayrollErrorCorrection>, IPayrollErrorCorrectionRepository
{
    public PayrollErrorCorrectionRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<PayrollErrorCorrection>> GetByPayrollRecordIdAsync(int payrollRecordId)
    {
        return await _context.Set<PayrollErrorCorrection>()
            .Where(ec => ec.PayrollRecordId == payrollRecordId)
            .Include(ec => ec.PayrollRecord)
            .Include(ec => ec.RequestedByUser)
            .Include(ec => ec.ApprovedByUser)
            .Include(ec => ec.ProcessedByUser)
            .OrderByDescending(ec => ec.RequestedAt)
            .ToListAsync();
    }

    public async Task<List<PayrollErrorCorrection>> GetPendingCorrectionsAsync(int? branchId = null)
    {
        var query = _context.Set<PayrollErrorCorrection>()
            .Where(ec => ec.Status == PayrollCorrectionStatus.Pending || ec.Status == PayrollCorrectionStatus.UnderReview)
            .Include(ec => ec.PayrollRecord)
                .ThenInclude(pr => pr.Employee)
            .Include(ec => ec.RequestedByUser)
            .AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(ec => ec.PayrollRecord.Employee.BranchId == branchId.Value);
        }

        return await query
            .OrderBy(ec => ec.RequestedAt)
            .ToListAsync();
    }

    public async Task<List<PayrollErrorCorrection>> GetByStatusAsync(PayrollCorrectionStatus status, int? branchId = null)
    {
        var query = _context.Set<PayrollErrorCorrection>()
            .Where(ec => ec.Status == status)
            .Include(ec => ec.PayrollRecord)
                .ThenInclude(pr => pr.Employee)
            .Include(ec => ec.RequestedByUser)
            .AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(ec => ec.PayrollRecord.Employee.BranchId == branchId.Value);
        }

        return await query
            .OrderByDescending(ec => ec.RequestedAt)
            .ToListAsync();
    }

    public async Task<List<PayrollErrorCorrection>> GetByEmployeeIdAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Set<PayrollErrorCorrection>()
            .Where(ec => ec.PayrollRecord.EmployeeId == employeeId)
            .Include(ec => ec.PayrollRecord)
            .Include(ec => ec.RequestedByUser)
            .Include(ec => ec.ApprovedByUser)
            .Include(ec => ec.ProcessedByUser)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(ec => ec.RequestedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(ec => ec.RequestedAt <= endDate.Value);
        }

        return await query
            .OrderByDescending(ec => ec.RequestedAt)
            .ToListAsync();
    }
}
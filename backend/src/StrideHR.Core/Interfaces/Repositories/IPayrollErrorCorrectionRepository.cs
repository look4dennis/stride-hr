using StrideHR.Core.Entities;
using StrideHR.Core.Models.Payroll;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IPayrollErrorCorrectionRepository : IRepository<PayrollErrorCorrection>
{
    Task<List<PayrollErrorCorrection>> GetByPayrollRecordIdAsync(int payrollRecordId);
    Task<List<PayrollErrorCorrection>> GetPendingCorrectionsAsync(int? branchId = null);
    Task<List<PayrollErrorCorrection>> GetByStatusAsync(PayrollCorrectionStatus status, int? branchId = null);
    Task<List<PayrollErrorCorrection>> GetByEmployeeIdAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
}
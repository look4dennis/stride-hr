using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IPayslipGenerationRepository : IRepository<PayslipGeneration>
{
    Task<PayslipGeneration?> GetByPayrollRecordAsync(int payrollRecordId);
    Task<List<PayslipGeneration>> GetByStatusAsync(PayslipStatus status);
    Task<List<PayslipGeneration>> GetPendingApprovalsAsync(PayslipApprovalLevel approvalLevel);
    Task<List<PayslipGeneration>> GetByBranchAndPeriodAsync(int branchId, int year, int month);
    Task<List<PayslipGeneration>> GetByEmployeeAsync(int employeeId, int year, int? month = null);
    Task<bool> UpdateStatusAsync(int payslipGenerationId, PayslipStatus status);
    Task<List<PayslipGeneration>> GetReleasedPayslipsAsync(int branchId, DateTime fromDate, DateTime toDate);
}
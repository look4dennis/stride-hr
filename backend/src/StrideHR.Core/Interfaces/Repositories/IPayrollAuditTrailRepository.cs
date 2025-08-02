using StrideHR.Core.Entities;
using StrideHR.Core.Models.Payroll;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IPayrollAuditTrailRepository : IRepository<PayrollAuditTrail>
{
    Task<List<PayrollAuditTrail>> GetByPayrollRecordIdAsync(int payrollRecordId);
    Task<List<PayrollAuditTrail>> GetByEmployeeIdAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<PayrollAuditTrail>> GetByActionAsync(PayrollAuditAction action, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<PayrollAuditTrail>> GetByUserIdAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<(List<PayrollAuditTrail> items, int totalCount)> GetPagedAsync(PayrollAuditTrailRequest request);
}
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IExpenseComplianceViolationRepository : IRepository<ExpenseComplianceViolation>
{
    Task<IEnumerable<ExpenseComplianceViolation>> GetByExpenseClaimIdAsync(int expenseClaimId);
    Task<IEnumerable<ExpenseComplianceViolation>> GetByEmployeeIdAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<ExpenseComplianceViolation>> GetUnresolvedViolationsAsync();
    Task<IEnumerable<ExpenseComplianceViolation>> GetBySeverityAsync(ExpenseViolationSeverity severity);
    Task<IEnumerable<ExpenseComplianceViolation>> GetByPolicyRuleIdAsync(int policyRuleId);
    Task<decimal> GetComplianceRateAsync(DateTime startDate, DateTime endDate, int? employeeId = null);
    Task<Dictionary<string, int>> GetViolationTypeCountsAsync(DateTime startDate, DateTime endDate);
    Task<bool> HasUnresolvedViolationsAsync(int expenseClaimId);
}
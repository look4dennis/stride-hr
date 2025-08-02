using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ILeaveAccrualRuleRepository : IRepository<LeaveAccrualRule>
{
    Task<IEnumerable<LeaveAccrualRule>> GetByLeavePolicyIdAsync(int leavePolicyId);
    Task<LeaveAccrualRule?> GetActiveRuleAsync(int leavePolicyId, DateTime effectiveDate);
    Task<IEnumerable<LeaveAccrualRule>> GetActiveRulesAsync(DateTime effectiveDate);
    Task<IEnumerable<LeaveAccrualRule>> GetByFrequencyAsync(AccrualFrequency frequency);
}
using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Leave;

public class LeavePolicyDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public LeaveType LeaveType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int AnnualAllocation { get; set; }
    public int MaxConsecutiveDays { get; set; }
    public int MinAdvanceNoticeDays { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsCarryForwardAllowed { get; set; }
    public int MaxCarryForwardDays { get; set; }
    public bool IsEncashmentAllowed { get; set; }
    public decimal EncashmentRate { get; set; }
    public bool IsActive { get; set; }
}
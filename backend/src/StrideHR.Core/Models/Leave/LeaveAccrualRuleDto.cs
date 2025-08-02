using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Leave;

public class LeaveAccrualRuleDto
{
    public int Id { get; set; }
    public int LeavePolicyId { get; set; }
    public LeaveType LeaveType { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public AccrualFrequency AccrualFrequency { get; set; }
    public decimal AccrualRate { get; set; }
    public int MaxAccrualDays { get; set; }
    public bool IsProRated { get; set; }
    public int MinServiceMonths { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

public class CreateLeaveAccrualRuleDto
{
    public int LeavePolicyId { get; set; }
    public AccrualFrequency AccrualFrequency { get; set; }
    public decimal AccrualRate { get; set; }
    public int MaxAccrualDays { get; set; }
    public bool IsProRated { get; set; } = true;
    public int MinServiceMonths { get; set; } = 0;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
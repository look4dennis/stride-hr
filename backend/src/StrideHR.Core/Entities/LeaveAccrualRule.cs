using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class LeaveAccrualRule : BaseEntity
{
    public int LeavePolicyId { get; set; }
    public AccrualFrequency AccrualFrequency { get; set; }
    public decimal AccrualRate { get; set; }
    public int MaxAccrualDays { get; set; }
    public bool IsProRated { get; set; } = true;
    public int MinServiceMonths { get; set; } = 0;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual LeavePolicy LeavePolicy { get; set; } = null!;
}
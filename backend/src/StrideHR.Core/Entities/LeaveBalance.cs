using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class LeaveBalance : BaseEntity
{
    public int EmployeeId { get; set; }
    public int LeavePolicyId { get; set; }
    public int Year { get; set; }
    public decimal AllocatedDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal CarriedForwardDays { get; set; }
    public decimal EncashedDays { get; set; }
    public decimal RemainingDays => AllocatedDays + CarriedForwardDays - UsedDays - EncashedDays;
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual LeavePolicy LeavePolicy { get; set; } = null!;
}
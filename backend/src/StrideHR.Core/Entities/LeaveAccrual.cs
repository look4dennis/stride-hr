using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class LeaveAccrual : BaseEntity
{
    public int EmployeeId { get; set; }
    public int LeavePolicyId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal AccruedDays { get; set; }
    public decimal AccrualRate { get; set; }
    public AccrualType AccrualType { get; set; }
    public DateTime AccrualDate { get; set; }
    public string? Notes { get; set; }
    public bool IsProcessed { get; set; } = false;
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual LeavePolicy LeavePolicy { get; set; } = null!;
}
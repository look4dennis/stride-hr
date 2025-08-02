using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class PayrollAdjustment : BaseEntity
{
    public int PayrollRecordId { get; set; }
    public PayrollAdjustmentType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int AdjustedBy { get; set; }
    public DateTime AdjustedAt { get; set; }
    public bool IsApproved { get; set; } = false;
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    // Navigation Properties
    public virtual PayrollRecord PayrollRecord { get; set; } = null!;
    public virtual Employee AdjustedByEmployee { get; set; } = null!;
    public virtual Employee? ApprovedByEmployee { get; set; }
}
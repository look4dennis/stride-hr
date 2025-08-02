using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class LeaveEncashment : BaseEntity
{
    public int EmployeeId { get; set; }
    public int LeavePolicyId { get; set; }
    public int Year { get; set; }
    public decimal EncashedDays { get; set; }
    public decimal EncashmentRate { get; set; }
    public decimal EncashmentAmount { get; set; }
    public DateTime EncashmentDate { get; set; }
    public EncashmentStatus Status { get; set; } = EncashmentStatus.Pending;
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Reason { get; set; }
    public string? Comments { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual LeavePolicy LeavePolicy { get; set; } = null!;
    public virtual Employee? ApprovedByEmployee { get; set; }
}
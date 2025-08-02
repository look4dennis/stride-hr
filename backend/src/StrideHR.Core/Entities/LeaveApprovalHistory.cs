using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class LeaveApprovalHistory : BaseEntity
{
    public int LeaveRequestId { get; set; }
    public int ApproverId { get; set; }
    public ApprovalLevel Level { get; set; }
    public ApprovalAction Action { get; set; }
    public string? Comments { get; set; }
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;
    public int? EscalatedToId { get; set; }
    
    // Navigation Properties
    public virtual LeaveRequest LeaveRequest { get; set; } = null!;
    public virtual Employee Approver { get; set; } = null!;
    public virtual Employee? EscalatedTo { get; set; }
}
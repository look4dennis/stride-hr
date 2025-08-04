using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class LeaveRequest : BaseEntity
{
    public int EmployeeId { get; set; }
    public int LeavePolicyId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal RequestedDays { get; set; }
    public decimal ApprovedDays { get; set; }
    public decimal TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public string? Notes { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedBy { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsEmergency { get; set; }
    public string? AttachmentPath { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual LeavePolicy LeavePolicy { get; set; } = null!;
    public virtual Employee? ApprovedByEmployee { get; set; }
    public virtual ICollection<LeaveApprovalHistory> ApprovalHistory { get; set; } = new List<LeaveApprovalHistory>();
}
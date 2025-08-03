using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ShiftSwapRequest : BaseEntity
{
    public int RequesterId { get; set; }
    public int RequesterShiftAssignmentId { get; set; }
    public int? TargetEmployeeId { get; set; }
    public int? TargetShiftAssignmentId { get; set; }
    public DateTime RequestedDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ShiftSwapStatus Status { get; set; } = ShiftSwapStatus.Pending;
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsEmergency { get; set; } = false;
    public DateTime? ExpiresAt { get; set; }
    
    // Navigation Properties
    public virtual Employee Requester { get; set; } = null!;
    public virtual ShiftAssignment RequesterShiftAssignment { get; set; } = null!;
    public virtual Employee? TargetEmployee { get; set; }
    public virtual ShiftAssignment? TargetShiftAssignment { get; set; }
    public virtual Employee? ApprovedByEmployee { get; set; }
    public virtual ICollection<ShiftSwapResponse> SwapResponses { get; set; } = new List<ShiftSwapResponse>();
}
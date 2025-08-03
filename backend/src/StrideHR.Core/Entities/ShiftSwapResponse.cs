using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ShiftSwapResponse : BaseEntity
{
    public int ShiftSwapRequestId { get; set; }
    public int ResponderId { get; set; }
    public int ResponderShiftAssignmentId { get; set; }
    public bool IsAccepted { get; set; }
    public string? Notes { get; set; }
    public DateTime RespondedAt { get; set; }
    
    // Navigation Properties
    public virtual ShiftSwapRequest ShiftSwapRequest { get; set; } = null!;
    public virtual Employee Responder { get; set; } = null!;
    public virtual ShiftAssignment ResponderShiftAssignment { get; set; } = null!;
}
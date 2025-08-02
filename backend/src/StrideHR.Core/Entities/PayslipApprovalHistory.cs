using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class PayslipApprovalHistory : BaseEntity
{
    public int PayslipGenerationId { get; set; }
    public PayslipApprovalLevel ApprovalLevel { get; set; }
    public PayslipApprovalAction Action { get; set; }
    public int ActionBy { get; set; }
    public DateTime ActionAt { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    
    // Previous and New Status
    public PayslipStatus PreviousStatus { get; set; }
    public PayslipStatus NewStatus { get; set; }
    
    // Navigation Properties
    public virtual PayslipGeneration PayslipGeneration { get; set; } = null!;
    public virtual Employee ActionByEmployee { get; set; } = null!;
}
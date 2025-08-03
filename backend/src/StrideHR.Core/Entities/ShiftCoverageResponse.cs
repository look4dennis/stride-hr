namespace StrideHR.Core.Entities;

public class ShiftCoverageResponse : BaseEntity
{
    public int ShiftCoverageRequestId { get; set; }
    public int ResponderId { get; set; }
    public bool IsAccepted { get; set; }
    public string? Notes { get; set; }
    public DateTime RespondedAt { get; set; }
    
    // Navigation Properties
    public virtual ShiftCoverageRequest ShiftCoverageRequest { get; set; } = null!;
    public virtual Employee Responder { get; set; } = null!;
}
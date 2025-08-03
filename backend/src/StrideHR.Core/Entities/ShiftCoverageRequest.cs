using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ShiftCoverageRequest : BaseEntity
{
    public int RequesterId { get; set; }
    public int ShiftAssignmentId { get; set; }
    public DateTime ShiftDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ShiftCoverageRequestStatus Status { get; set; } = ShiftCoverageRequestStatus.Open;
    public bool IsEmergency { get; set; } = false;
    public DateTime? ExpiresAt { get; set; }
    public int? AcceptedBy { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string? AcceptanceNotes { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    
    // Navigation Properties
    public virtual Employee Requester { get; set; } = null!;
    public virtual ShiftAssignment ShiftAssignment { get; set; } = null!;
    public virtual Employee? AcceptedByEmployee { get; set; }
    public virtual Employee? ApprovedByEmployee { get; set; }
    public virtual ICollection<ShiftCoverageResponse> CoverageResponses { get; set; } = new List<ShiftCoverageResponse>();
}
using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class AssetHandover : BaseEntity
{
    public int AssetId { get; set; }
    public int EmployeeId { get; set; }
    public int? EmployeeExitId { get; set; }
    public HandoverStatus Status { get; set; } = HandoverStatus.Pending;
    public DateTime InitiatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public AssetCondition? ReturnedCondition { get; set; }
    public string? HandoverNotes { get; set; }
    public string? DamageNotes { get; set; }
    public decimal? DamageCharges { get; set; }
    public string? Currency { get; set; }
    public int InitiatedBy { get; set; }
    public int? CompletedBy { get; set; }
    public bool IsApproved { get; set; } = false;
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }

    // Navigation Properties
    public virtual Asset Asset { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    public virtual EmployeeExit? EmployeeExit { get; set; }
    public virtual Employee InitiatedByEmployee { get; set; } = null!;
    public virtual Employee? CompletedByEmployee { get; set; }
    public virtual Employee? ApprovedByEmployee { get; set; }
}
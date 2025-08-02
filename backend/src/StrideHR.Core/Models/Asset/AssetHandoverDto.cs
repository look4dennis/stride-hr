using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Asset;

public class AssetHandoverDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string AssetTag { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int? EmployeeExitId { get; set; }
    public HandoverStatus Status { get; set; }
    public DateTime InitiatedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public AssetCondition? ReturnedCondition { get; set; }
    public string? HandoverNotes { get; set; }
    public string? DamageNotes { get; set; }
    public decimal? DamageCharges { get; set; }
    public string? Currency { get; set; }
    public string InitiatedByName { get; set; } = string.Empty;
    public string? CompletedByName { get; set; }
    public bool IsApproved { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Calculated properties
    public bool IsOverdue => Status != HandoverStatus.Completed && DueDate.HasValue && DueDate.Value < DateTime.UtcNow;
    public int? DaysUntilDue => DueDate.HasValue && Status != HandoverStatus.Completed ? (int?)(DueDate.Value - DateTime.UtcNow).TotalDays : null;
}
using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Asset;

public class AssetAssignmentDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string AssetTag { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public AssetCondition AssignedCondition { get; set; }
    public AssetCondition? ReturnedCondition { get; set; }
    public string? AssignmentNotes { get; set; }
    public string? ReturnNotes { get; set; }
    public bool IsActive { get; set; }
    public string AssignedByName { get; set; } = string.Empty;
    public string? ReturnedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}
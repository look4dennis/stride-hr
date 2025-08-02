using StrideHR.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Asset;

public class CreateAssetAssignmentDto
{
    [Required]
    public int AssetId { get; set; }
    
    public int? EmployeeId { get; set; }
    
    public int? ProjectId { get; set; }
    
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    
    [Required]
    public AssetCondition AssignedCondition { get; set; }
    
    [StringLength(1000)]
    public string? AssignmentNotes { get; set; }
    
    [Required]
    public int AssignedBy { get; set; }
}

public class ReturnAssetDto
{
    [Required]
    public AssetCondition ReturnedCondition { get; set; }
    
    [StringLength(1000)]
    public string? ReturnNotes { get; set; }
    
    [Required]
    public int ReturnedBy { get; set; }
}
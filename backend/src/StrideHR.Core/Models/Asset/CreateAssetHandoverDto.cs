using StrideHR.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Asset;

public class CreateAssetHandoverDto
{
    [Required]
    public int AssetId { get; set; }
    
    [Required]
    public int EmployeeId { get; set; }
    
    public int? EmployeeExitId { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    [StringLength(1000)]
    public string? HandoverNotes { get; set; }
    
    [Required]
    public int InitiatedBy { get; set; }
}

public class CompleteAssetHandoverDto
{
    [Required]
    public AssetCondition ReturnedCondition { get; set; }
    
    [StringLength(1000)]
    public string? HandoverNotes { get; set; }
    
    [StringLength(1000)]
    public string? DamageNotes { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? DamageCharges { get; set; }
    
    [StringLength(3)]
    public string? Currency { get; set; }
    
    [Required]
    public int CompletedBy { get; set; }
}

public class ApproveAssetHandoverDto
{
    [Required]
    public int ApprovedBy { get; set; }
    
    [StringLength(500)]
    public string? ApprovalNotes { get; set; }
}
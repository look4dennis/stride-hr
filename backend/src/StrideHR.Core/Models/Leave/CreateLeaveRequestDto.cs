using StrideHR.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Leave;

public class CreateLeaveRequestDto
{
    [Required]
    public int LeavePolicyId { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Comments { get; set; }
    
    public bool IsEmergency { get; set; }
    
    public string? AttachmentPath { get; set; }
}
using StrideHR.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Grievance;

public class CreateGrievanceDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public GrievanceCategory Category { get; set; }
    
    [Required]
    public GrievancePriority Priority { get; set; }
    
    public bool IsAnonymous { get; set; } = false;
    
    public bool RequiresInvestigation { get; set; } = false;
    
    public string? AttachmentPath { get; set; }
    
    public EscalationLevel? PreferredEscalationLevel { get; set; }
}
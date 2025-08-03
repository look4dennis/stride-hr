using StrideHR.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Grievance;

public class UpdateGrievanceDto
{
    [StringLength(200)]
    public string? Title { get; set; }
    
    [StringLength(2000)]
    public string? Description { get; set; }
    
    public GrievanceCategory? Category { get; set; }
    
    public GrievancePriority? Priority { get; set; }
    
    public bool? RequiresInvestigation { get; set; }
    
    public string? InvestigationNotes { get; set; }
    
    public string? AttachmentPath { get; set; }
}
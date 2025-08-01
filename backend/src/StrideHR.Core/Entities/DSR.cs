using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class DSR : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public int? ProjectId { get; set; }
    public int? TaskId { get; set; }
    public decimal HoursWorked { get; set; }
    public string Description { get; set; } = string.Empty;
    public DSRStatus Status { get; set; } = DSRStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComments { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Project? Project { get; set; }
    public virtual ProjectTask? Task { get; set; }
    public virtual Employee? Reviewer { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

/// <summary>
/// Daily Status Report entity - placeholder for DSR management task
/// </summary>
public class DSR : AuditableEntity
{
    public int EmployeeId { get; set; }
    
    /// <summary>
    /// DSR date
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Project ID (optional - can be null for "No Task Assigned")
    /// </summary>
    public int? ProjectId { get; set; }
    
    /// <summary>
    /// Task ID (optional - can be null for "No Task Assigned")
    /// </summary>
    public int? TaskId { get; set; }
    
    /// <summary>
    /// Hours worked on this task/project
    /// </summary>
    public decimal HoursWorked { get; set; }
    
    /// <summary>
    /// Work description/activities performed
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// DSR status
    /// </summary>
    public DSRStatus Status { get; set; } = DSRStatus.Submitted;
    
    /// <summary>
    /// Submission timestamp
    /// </summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Reviewed by (manager/team lead)
    /// </summary>
    public int? ReviewedBy { get; set; }
    
    /// <summary>
    /// Review date
    /// </summary>
    public DateTime? ReviewedAt { get; set; }
    
    /// <summary>
    /// Review comments
    /// </summary>
    [MaxLength(500)]
    public string? ReviewComments { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Project? Project { get; set; }
    public virtual ProjectTask? Task { get; set; }
    public virtual Employee? Reviewer { get; set; }
}

/// <summary>
/// DSR status enumeration
/// </summary>
public enum DSRStatus
{
    Draft = 1,
    Submitted = 2,
    UnderReview = 3,
    Approved = 4,
    Rejected = 5
}
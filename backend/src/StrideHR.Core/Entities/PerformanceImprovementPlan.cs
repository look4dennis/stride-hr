using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class PerformanceImprovementPlan : BaseEntity
{
    public int EmployeeId { get; set; }
    public int ManagerId { get; set; }
    public int? HRId { get; set; }
    public int? PerformanceReviewId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PerformanceIssues { get; set; } = string.Empty;
    public string ExpectedImprovements { get; set; } = string.Empty;
    public string SupportProvided { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ReviewFrequencyDays { get; set; } = 30; // Review every X days
    public PIPStatus Status { get; set; } = PIPStatus.Draft;
    public string? FinalOutcome { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsSuccessful { get; set; } = false;
    public string? ManagerNotes { get; set; }
    public string? HRNotes { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee Manager { get; set; } = null!;
    public virtual Employee? HR { get; set; }
    public virtual PerformanceReview? PerformanceReview { get; set; }
    public virtual ICollection<PIPGoal> Goals { get; set; } = new List<PIPGoal>();
    public virtual ICollection<PIPReview> Reviews { get; set; } = new List<PIPReview>();
}
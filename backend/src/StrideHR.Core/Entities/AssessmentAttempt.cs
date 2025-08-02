using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

public class AssessmentAttempt : BaseEntity
{
    public int AssessmentId { get; set; }
    
    public int EmployeeId { get; set; }
    
    public int TrainingProgressId { get; set; }
    
    public int AttemptNumber { get; set; }
    
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedAt { get; set; }
    
    public decimal? Score { get; set; }
    
    public decimal? MaxScore { get; set; }
    
    public bool IsPassed { get; set; }
    
    public AssessmentAttemptStatus Status { get; set; } = AssessmentAttemptStatus.InProgress;
    
    public int TimeSpentMinutes { get; set; }
    
    public string? Notes { get; set; }
    
    // Navigation Properties
    public virtual Assessment Assessment { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    public virtual TrainingProgress TrainingProgress { get; set; } = null!;
    public virtual ICollection<AssessmentAnswer> Answers { get; set; } = new List<AssessmentAnswer>();
}

public enum AssessmentAttemptStatus
{
    InProgress,
    Completed,
    TimedOut,
    Abandoned
}
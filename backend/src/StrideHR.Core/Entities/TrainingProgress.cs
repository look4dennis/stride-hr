using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

public class TrainingProgress : BaseEntity
{
    public int TrainingAssignmentId { get; set; }
    
    public int EmployeeId { get; set; }
    
    public int TrainingModuleId { get; set; }
    
    public decimal ProgressPercentage { get; set; } = 0;
    
    public int TimeSpentMinutes { get; set; } = 0;
    
    public DateTime? LastAccessedAt { get; set; }
    
    public DateTime? StartedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public TrainingProgressStatus Status { get; set; } = TrainingProgressStatus.NotStarted;
    
    // Tracking data
    public Dictionary<string, object> ProgressData { get; set; } = new();
    
    // Navigation Properties
    public virtual TrainingAssignment TrainingAssignment { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    public virtual TrainingModule TrainingModule { get; set; } = null!;
    public virtual ICollection<AssessmentAttempt> AssessmentAttempts { get; set; } = new List<AssessmentAttempt>();
}

public enum TrainingProgressStatus
{
    NotStarted,
    InProgress,
    Completed,
    Failed,
    Expired
}
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

public class Assessment : BaseEntity
{
    public int TrainingModuleId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public AssessmentType Type { get; set; }
    
    public int TimeLimit { get; set; } // in minutes
    
    public decimal PassingScore { get; set; }
    
    public int MaxAttempts { get; set; } = 3;
    
    public int RetakeWaitingPeriodHours { get; set; } = 24;
    
    public bool IsActive { get; set; } = true;
    
    public int CreatedByEmployeeId { get; set; }
    
    // Navigation Properties
    public virtual TrainingModule TrainingModule { get; set; } = null!;
    public virtual Employee CreatedByEmployee { get; set; } = null!;
    public virtual ICollection<AssessmentQuestion> Questions { get; set; } = new List<AssessmentQuestion>();
    public virtual ICollection<AssessmentAttempt> Attempts { get; set; } = new List<AssessmentAttempt>();
}

public enum AssessmentType
{
    Quiz,
    Exam,
    PracticalTest,
    Survey
}
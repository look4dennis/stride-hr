using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

public class TrainingModule : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public string Content { get; set; } = string.Empty;
    
    public TrainingType Type { get; set; }
    
    public TrainingLevel Level { get; set; }
    
    public int EstimatedDurationMinutes { get; set; }
    
    public bool IsMandatory { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int CreatedByEmployeeId { get; set; }
    
    // Prerequisites
    public List<int> PrerequisiteModuleIds { get; set; } = new();
    
    // Content files
    public List<string> ContentFiles { get; set; } = new();
    
    // Navigation Properties
    public virtual Employee CreatedByEmployee { get; set; } = null!;
    public virtual ICollection<TrainingAssignment> TrainingAssignments { get; set; } = new List<TrainingAssignment>();
    public virtual ICollection<TrainingProgress> TrainingProgresses { get; set; } = new List<TrainingProgress>();
    public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    public virtual ICollection<Certification> Certifications { get; set; } = new List<Certification>();
}

public enum TrainingType
{
    OnlineModule,
    Video,
    Document,
    InteractiveContent,
    ExternalLink,
    InPersonTraining
}

public enum TrainingLevel
{
    Beginner,
    Intermediate,
    Advanced,
    Expert
}
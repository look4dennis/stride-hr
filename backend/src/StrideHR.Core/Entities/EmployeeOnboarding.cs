namespace StrideHR.Core.Entities;

public class EmployeeOnboarding : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime OnboardingDate { get; set; }
    public string OnboardingManager { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedDate { get; set; }
    public string? Notes { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual ICollection<EmployeeOnboardingTask> Tasks { get; set; } = new List<EmployeeOnboardingTask>();
}

public class EmployeeOnboardingTask : BaseEntity
{
    public int EmployeeOnboardingId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedDate { get; set; }
    public int? AssignedTo { get; set; }
    public string? CompletionNotes { get; set; }
    
    // Navigation Properties
    public virtual EmployeeOnboarding EmployeeOnboarding { get; set; } = null!;
    public virtual Employee? AssignedToEmployee { get; set; }
}
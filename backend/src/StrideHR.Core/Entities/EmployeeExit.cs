namespace StrideHR.Core.Entities;

public class EmployeeExit : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime ExitDate { get; set; }
    public string ExitReason { get; set; } = string.Empty;
    public string? ExitNotes { get; set; }
    public string ProcessedBy { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedDate { get; set; }
    public bool IsExitInterviewCompleted { get; set; } = false;
    public DateTime? ExitInterviewDate { get; set; }
    public string? ExitInterviewNotes { get; set; }
    public bool AreAssetsReturned { get; set; } = false;
    public string? AssetReturnNotes { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual ICollection<EmployeeExitTask> Tasks { get; set; } = new List<EmployeeExitTask>();
}

public class EmployeeExitTask : BaseEntity
{
    public int EmployeeExitId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedDate { get; set; }
    public string? CompletedBy { get; set; }
    public string? CompletionNotes { get; set; }
    
    // Navigation Properties
    public virtual EmployeeExit EmployeeExit { get; set; } = null!;
}
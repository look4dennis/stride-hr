using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Employee;

public class EmployeeExitDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime ExitDate { get; set; }

    [Required]
    [StringLength(100)]
    public string ExitReason { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? ExitNotes { get; set; }

    [Required]
    [StringLength(100)]
    public string ProcessedBy { get; set; } = string.Empty;

    public List<ExitTask> Tasks { get; set; } = new List<ExitTask>();

    public bool IsExitInterviewCompleted { get; set; } = false;

    public DateTime? ExitInterviewDate { get; set; }

    [StringLength(1000)]
    public string? ExitInterviewNotes { get; set; }

    public bool AreAssetsReturned { get; set; } = false;

    [StringLength(500)]
    public string? AssetReturnNotes { get; set; }
}

public class ExitTask
{
    [Required]
    [StringLength(200)]
    public string TaskName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedDate { get; set; }

    [StringLength(100)]
    public string? CompletedBy { get; set; }

    [StringLength(500)]
    public string? CompletionNotes { get; set; }
}
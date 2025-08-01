using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Employee;

public class EmployeeOnboardingDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime OnboardingDate { get; set; }

    [Required]
    [StringLength(100)]
    public string OnboardingManager { get; set; } = string.Empty;

    public List<OnboardingTask> Tasks { get; set; } = new List<OnboardingTask>();

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class OnboardingTask
{
    [Required]
    [StringLength(200)]
    public string TaskName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedDate { get; set; }

    [StringLength(100)]
    public string? AssignedTo { get; set; }

    [StringLength(500)]
    public string? CompletionNotes { get; set; }
}
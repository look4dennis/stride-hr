using StrideHR.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models;

public class TrainingAssignmentDto
{
    public int Id { get; set; }
    public int TrainingModuleId { get; set; }
    public string TrainingModuleTitle { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public int AssignedBy { get; set; }
    public string AssignedByName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public TrainingAssignmentStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public decimal ProgressPercentage { get; set; }
    public int TimeSpentMinutes { get; set; }
    public bool IsOverdue { get; set; }
}

public class CreateTrainingAssignmentDto
{
    [Required]
    public int TrainingModuleId { get; set; }
    
    [Required]
    public List<int> EmployeeIds { get; set; } = new();
    
    public DateTime? DueDate { get; set; }
    
    public string? Notes { get; set; }
}

public class BulkTrainingAssignmentDto
{
    [Required]
    public int TrainingModuleId { get; set; }
    
    public List<int> EmployeeIds { get; set; } = new();
    
    public List<int> DepartmentIds { get; set; } = new();
    
    public List<int> RoleIds { get; set; } = new();
    
    public bool AssignToAllEmployees { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public string? Notes { get; set; }
}
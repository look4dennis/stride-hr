using StrideHR.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models;

public class TrainingModuleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public TrainingType Type { get; set; }
    public TrainingLevel Level { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsActive { get; set; }
    public int CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<int> PrerequisiteModuleIds { get; set; } = new();
    public List<string> PrerequisiteModuleNames { get; set; } = new();
    public List<string> ContentFiles { get; set; } = new();
    public int AssignedEmployeesCount { get; set; }
    public int CompletedEmployeesCount { get; set; }
    public decimal CompletionRate { get; set; }
}

public class CreateTrainingModuleDto
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
    
    public List<int> PrerequisiteModuleIds { get; set; } = new();
    
    public List<string> ContentFiles { get; set; } = new();
}

public class UpdateTrainingModuleDto
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
    
    public bool IsActive { get; set; }
    
    public List<int> PrerequisiteModuleIds { get; set; } = new();
    
    public List<string> ContentFiles { get; set; } = new();
}
using StrideHR.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models;

public class AssessmentDto
{
    public int Id { get; set; }
    public int TrainingModuleId { get; set; }
    public string TrainingModuleTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AssessmentType Type { get; set; }
    public int TimeLimit { get; set; }
    public decimal PassingScore { get; set; }
    public int MaxAttempts { get; set; }
    public int RetakeWaitingPeriodHours { get; set; }
    public bool IsActive { get; set; }
    public int CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int QuestionCount { get; set; }
    public decimal TotalPoints { get; set; }
    public List<AssessmentQuestionDto> Questions { get; set; } = new();
}

public class CreateAssessmentDto
{
    [Required]
    public int TrainingModuleId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public AssessmentType Type { get; set; }
    
    [Range(1, 480)]
    public int TimeLimit { get; set; } = 60;
    
    [Range(0, 100)]
    public decimal PassingScore { get; set; } = 70;
    
    [Range(1, 10)]
    public int MaxAttempts { get; set; } = 3;
    
    [Range(0, 168)]
    public int RetakeWaitingPeriodHours { get; set; } = 24;
    
    public List<CreateAssessmentQuestionDto> Questions { get; set; } = new();
}

public class AssessmentQuestionDto
{
    public int Id { get; set; }
    public int AssessmentId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public List<string> Options { get; set; } = new();
    public List<string> CorrectAnswers { get; set; } = new();
    public decimal Points { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
    public string? Explanation { get; set; }
}

public class CreateAssessmentQuestionDto
{
    [Required]
    public string QuestionText { get; set; } = string.Empty;
    
    public QuestionType Type { get; set; }
    
    public List<string> Options { get; set; } = new();
    
    public List<string> CorrectAnswers { get; set; } = new();
    
    [Range(0.1, 10)]
    public decimal Points { get; set; } = 1;
    
    public int OrderIndex { get; set; }
    
    public string? Explanation { get; set; }
}

public class SubmitAnswerDto
{
    [Required]
    public int QuestionId { get; set; }
    
    [Required]
    public List<string> Answers { get; set; } = new();
}
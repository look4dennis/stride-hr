using System.ComponentModel.DataAnnotations;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class AssessmentQuestion : BaseEntity
{
    public int AssessmentId { get; set; }
    
    [Required]
    public string QuestionText { get; set; } = string.Empty;
    
    public QuestionType Type { get; set; }
    
    public List<string> Options { get; set; } = new();
    
    public List<string> CorrectAnswers { get; set; } = new();
    
    public decimal Points { get; set; } = 1;
    
    public int OrderIndex { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public string? Explanation { get; set; }
    
    // Navigation Properties
    public virtual Assessment Assessment { get; set; } = null!;
    public virtual ICollection<AssessmentAnswer> Answers { get; set; } = new List<AssessmentAnswer>();
}
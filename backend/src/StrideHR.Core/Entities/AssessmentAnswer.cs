using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

public class AssessmentAnswer : BaseEntity
{
    public int AssessmentAttemptId { get; set; }
    
    public int AssessmentQuestionId { get; set; }
    
    public List<string> SelectedAnswers { get; set; } = new();
    
    public string? TextAnswer { get; set; }
    
    public bool IsCorrect { get; set; }
    
    public decimal PointsEarned { get; set; }
    
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual AssessmentAttempt AssessmentAttempt { get; set; } = null!;
    public virtual AssessmentQuestion AssessmentQuestion { get; set; } = null!;
}
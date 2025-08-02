namespace StrideHR.Core.Entities;

public class SurveyAnswer : BaseEntity
{
    public int ResponseId { get; set; }
    public int QuestionId { get; set; }
    public int? SelectedOptionId { get; set; }
    public string? TextAnswer { get; set; }
    public int? NumericAnswer { get; set; }
    public DateTime? DateAnswer { get; set; }
    public bool? BooleanAnswer { get; set; }
    public string? OtherAnswer { get; set; }
    public string? MultipleSelections { get; set; } // JSON array for multiple choice
    public int? RatingValue { get; set; }
    public bool IsSkipped { get; set; } = false;

    // Navigation Properties
    public virtual SurveyResponse Response { get; set; } = null!;
    public virtual SurveyQuestion Question { get; set; } = null!;
    public virtual SurveyQuestionOption? SelectedOption { get; set; }
}
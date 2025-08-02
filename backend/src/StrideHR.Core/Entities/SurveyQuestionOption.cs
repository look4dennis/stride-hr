namespace StrideHR.Core.Entities;

public class SurveyQuestionOption : BaseEntity
{
    public int QuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int? Value { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual SurveyQuestion Question { get; set; } = null!;
    public virtual ICollection<SurveyAnswer> Answers { get; set; } = new List<SurveyAnswer>();
}
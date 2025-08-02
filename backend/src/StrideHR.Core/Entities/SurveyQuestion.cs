using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class SurveyQuestion : BaseEntity
{
    public int SurveyId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public int OrderIndex { get; set; }
    public bool IsRequired { get; set; } = false;
    public string? HelpText { get; set; }
    public string? ValidationRules { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public string? PlaceholderText { get; set; }
    public bool AllowOther { get; set; } = false;
    public string? ConditionalLogic { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual Survey Survey { get; set; } = null!;
    public virtual ICollection<SurveyQuestionOption> Options { get; set; } = new List<SurveyQuestionOption>();
    public virtual ICollection<SurveyAnswer> Answers { get; set; } = new List<SurveyAnswer>();
}
using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Survey;

public class SurveyQuestionDto
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public int OrderIndex { get; set; }
    public bool IsRequired { get; set; }
    public string? HelpText { get; set; }
    public string? ValidationRules { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public string? PlaceholderText { get; set; }
    public bool AllowOther { get; set; }
    public string? ConditionalLogic { get; set; }
    public bool IsActive { get; set; }

    public List<SurveyQuestionOptionDto> Options { get; set; } = new();
}
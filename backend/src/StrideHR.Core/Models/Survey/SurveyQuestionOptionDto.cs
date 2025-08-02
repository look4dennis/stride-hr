namespace StrideHR.Core.Models.Survey;

public class SurveyQuestionOptionDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int? Value { get; set; }
    public bool IsActive { get; set; }
}
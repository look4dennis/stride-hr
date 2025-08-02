namespace StrideHR.Core.Models.Survey;

public class SurveyAnswerDto
{
    public int Id { get; set; }
    public int ResponseId { get; set; }
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int? SelectedOptionId { get; set; }
    public string? SelectedOptionText { get; set; }
    public string? TextAnswer { get; set; }
    public int? NumericAnswer { get; set; }
    public DateTime? DateAnswer { get; set; }
    public bool? BooleanAnswer { get; set; }
    public string? OtherAnswer { get; set; }
    public List<string>? MultipleSelections { get; set; }
    public int? RatingValue { get; set; }
    public bool IsSkipped { get; set; }
}
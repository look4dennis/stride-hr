using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Survey;

public class SubmitSurveyResponseDto
{
    [Required]
    public int SurveyId { get; set; }

    public int? RespondentEmployeeId { get; set; }

    public string? AnonymousId { get; set; }

    [Required]
    public List<SubmitSurveyAnswerDto> Answers { get; set; } = new();

    public string? Notes { get; set; }

    public string? Location { get; set; }
}

public class SubmitSurveyAnswerDto
{
    [Required]
    public int QuestionId { get; set; }

    public int? SelectedOptionId { get; set; }

    public string? TextAnswer { get; set; }

    public int? NumericAnswer { get; set; }

    public DateTime? DateAnswer { get; set; }

    public bool? BooleanAnswer { get; set; }

    public string? OtherAnswer { get; set; }

    public List<int>? MultipleSelections { get; set; }

    public int? RatingValue { get; set; }

    public bool IsSkipped { get; set; } = false;
}
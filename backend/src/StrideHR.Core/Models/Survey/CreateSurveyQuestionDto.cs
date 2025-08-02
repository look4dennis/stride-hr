using System.ComponentModel.DataAnnotations;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Survey;

public class CreateSurveyQuestionDto
{
    [Required]
    [StringLength(1000)]
    public string QuestionText { get; set; } = string.Empty;

    [Required]
    public QuestionType Type { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int OrderIndex { get; set; }

    public bool IsRequired { get; set; } = false;

    [StringLength(500)]
    public string? HelpText { get; set; }

    [StringLength(1000)]
    public string? ValidationRules { get; set; }

    public int? MinLength { get; set; }

    public int? MaxLength { get; set; }

    public int? MinValue { get; set; }

    public int? MaxValue { get; set; }

    [StringLength(200)]
    public string? PlaceholderText { get; set; }

    public bool AllowOther { get; set; } = false;

    [StringLength(2000)]
    public string? ConditionalLogic { get; set; }

    public List<CreateSurveyQuestionOptionDto> Options { get; set; } = new();
}
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Survey;

public class CreateSurveyQuestionOptionDto
{
    [Required]
    [StringLength(500)]
    public string OptionText { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int OrderIndex { get; set; }

    public int? Value { get; set; }
}
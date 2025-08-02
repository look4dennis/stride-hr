using System.ComponentModel.DataAnnotations;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Survey;

public class CreateSurveyDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public SurveyType Type { get; set; }

    public bool IsAnonymous { get; set; } = false;

    public bool AllowMultipleResponses { get; set; } = false;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public int BranchId { get; set; }

    public bool IsGlobal { get; set; } = false;

    [StringLength(2000)]
    public string? Instructions { get; set; }

    [StringLength(1000)]
    public string? ThankYouMessage { get; set; }

    [Range(1, 300)]
    public int EstimatedDurationMinutes { get; set; } = 10;

    public bool RequireAuthentication { get; set; } = true;

    public bool ShowProgressBar { get; set; } = true;

    public bool RandomizeQuestions { get; set; } = false;

    [StringLength(500)]
    public string? Tags { get; set; }

    public List<CreateSurveyQuestionDto> Questions { get; set; } = new();
}
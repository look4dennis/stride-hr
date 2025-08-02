using System.ComponentModel.DataAnnotations;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Survey;

public class UpdateSurveyDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public SurveyType Type { get; set; }

    [Required]
    public SurveyStatus Status { get; set; }

    public bool IsAnonymous { get; set; }

    public bool AllowMultipleResponses { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsGlobal { get; set; }

    [StringLength(2000)]
    public string? Instructions { get; set; }

    [StringLength(1000)]
    public string? ThankYouMessage { get; set; }

    [Range(1, 300)]
    public int EstimatedDurationMinutes { get; set; }

    public bool RequireAuthentication { get; set; }

    public bool ShowProgressBar { get; set; }

    public bool RandomizeQuestions { get; set; }

    [StringLength(500)]
    public string? Tags { get; set; }
}
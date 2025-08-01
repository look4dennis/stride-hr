using System.ComponentModel.DataAnnotations;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Attendance;

public class StartBreakRequest
{
    [Required]
    public BreakType BreakType { get; set; }
    
    public string? Location { get; set; }
    
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double? Latitude { get; set; }
    
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double? Longitude { get; set; }
    
    public string? Notes { get; set; }
}
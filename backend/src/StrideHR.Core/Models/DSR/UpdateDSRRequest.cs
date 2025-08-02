using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.DSR;

public class UpdateDSRRequest
{
    public int? ProjectId { get; set; }
    
    public int? TaskId { get; set; }
    
    [Range(0.1, 24, ErrorMessage = "Hours worked must be between 0.1 and 24")]
    public decimal? HoursWorked { get; set; }
    
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}
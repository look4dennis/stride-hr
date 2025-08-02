using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.DSR;

public class CreateDSRRequest
{
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    public int? ProjectId { get; set; }
    
    public int? TaskId { get; set; }
    
    [Required]
    [Range(0.1, 24, ErrorMessage = "Hours worked must be between 0.1 and 24")]
    public decimal HoursWorked { get; set; }
    
    [Required]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;
}
using System.ComponentModel.DataAnnotations;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.DSR;

public class ReviewDSRRequest
{
    [Required]
    public int ReviewerId { get; set; }
    
    [Required]
    public DSRStatus Status { get; set; } // Should be Approved or Rejected
    
    [StringLength(500, ErrorMessage = "Review comments cannot exceed 500 characters")]
    public string? ReviewComments { get; set; }
}
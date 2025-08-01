using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Employee;

public class ProfilePhotoUploadDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public byte[] PhotoData { get; set; } = Array.Empty<byte>();

    [Required]
    [StringLength(100)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ContentType { get; set; } = string.Empty;

    [Range(1, 5 * 1024 * 1024)] // Max 5MB
    public long FileSize { get; set; }
}
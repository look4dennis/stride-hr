using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Organization;

public class OrganizationLogoUploadDto
{
    [Required]
    public int OrganizationId { get; set; }

    [Required]
    public byte[] LogoData { get; set; } = Array.Empty<byte>();

    [Required]
    [StringLength(100)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ContentType { get; set; } = string.Empty;

    [Range(1, 10 * 1024 * 1024)] // Max 10MB
    public long FileSize { get; set; }
}
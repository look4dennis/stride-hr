using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

public class Certification : BaseEntity
{
    public int TrainingModuleId { get; set; }
    
    public int EmployeeId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string CertificationName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string CertificationNumber { get; set; } = string.Empty;
    
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpiryDate { get; set; }
    
    public CertificationStatus Status { get; set; } = CertificationStatus.Active;
    
    public string? CertificateFilePath { get; set; }
    
    public int IssuedBy { get; set; }
    
    public decimal? Score { get; set; }
    
    public string? Notes { get; set; }
    
    // External certification tracking
    public bool IsExternalCertification { get; set; }
    
    public string? ExternalProvider { get; set; }
    
    public string? VerificationUrl { get; set; }
    
    // Navigation Properties
    public virtual TrainingModule TrainingModule { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee IssuedByEmployee { get; set; } = null!;
}

public enum CertificationStatus
{
    Active,
    Expired,
    Revoked,
    Suspended
}
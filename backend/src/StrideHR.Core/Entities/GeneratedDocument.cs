using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class GeneratedDocument : BaseEntity
{
    public int DocumentTemplateId { get; set; }
    public int EmployeeId { get; set; } // Employee for whom the document was generated
    public string DocumentNumber { get; set; } = string.Empty; // Unique document number
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Final generated content
    public DocumentStatus Status { get; set; }
    public string FilePath { get; set; } = string.Empty; // Path to generated PDF/document
    public string FileHash { get; set; } = string.Empty; // For integrity verification
    public Dictionary<string, object> MergeData { get; set; } = new(); // Data used for generation
    public int GeneratedBy { get; set; }
    public DateTime GeneratedAt { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool RequiresSignature { get; set; } = false;
    public string? SignatureWorkflow { get; set; }
    public bool IsDigitallySigned { get; set; } = false;
    public DateTime? SignedAt { get; set; }
    public string? SignedBy { get; set; }
    public string? SignatureHash { get; set; }
    public int DownloadCount { get; set; } = 0;
    public DateTime? LastDownloadedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual DocumentTemplate DocumentTemplate { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee GeneratedByEmployee { get; set; } = null!;
    public virtual ICollection<DocumentSignature> Signatures { get; set; } = new List<DocumentSignature>();
    public virtual ICollection<DocumentApproval> Approvals { get; set; } = new List<DocumentApproval>();
    public virtual ICollection<DocumentAuditLog> AuditLogs { get; set; } = new List<DocumentAuditLog>();
}
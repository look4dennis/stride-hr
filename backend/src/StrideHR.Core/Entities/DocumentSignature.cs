using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class DocumentSignature : BaseEntity
{
    public int GeneratedDocumentId { get; set; }
    public int SignerId { get; set; }
    public string SignerRole { get; set; } = string.Empty; // Employee, Manager, HR, etc.
    public string SignatureType { get; set; } = string.Empty; // Digital, Electronic, Wet
    public string SignatureData { get; set; } = string.Empty; // Base64 signature image or certificate
    public string SignatureHash { get; set; } = string.Empty;
    public DateTime SignedAt { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public ApprovalAction Action { get; set; }
    public string? Comments { get; set; }
    public bool IsValid { get; set; } = true;
    public DateTime? InvalidatedAt { get; set; }
    public string? InvalidationReason { get; set; }
    public int SignatureOrder { get; set; } // For sequential signing
    public bool IsRequired { get; set; } = true;

    // Navigation Properties
    public virtual GeneratedDocument GeneratedDocument { get; set; } = null!;
    public virtual Employee Signer { get; set; } = null!;
}
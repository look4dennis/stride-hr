using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.DocumentTemplate;

public class GeneratedDocumentDto
{
    public int Id { get; set; }
    public int DocumentTemplateId { get; set; }
    public string DocumentTemplateName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int GeneratedBy { get; set; }
    public string GeneratedByName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool RequiresSignature { get; set; }
    public bool IsDigitallySigned { get; set; }
    public DateTime? SignedAt { get; set; }
    public string? SignedBy { get; set; }
    public int DownloadCount { get; set; }
    public DateTime? LastDownloadedAt { get; set; }
    public string? Notes { get; set; }
    public List<DocumentSignatureDto> Signatures { get; set; } = new();
    public List<DocumentApprovalDto> Approvals { get; set; } = new();
}

public class GenerateDocumentDto
{
    public int DocumentTemplateId { get; set; }
    public int EmployeeId { get; set; }
    public Dictionary<string, object> MergeData { get; set; } = new();
    public DateTime? ExpiryDate { get; set; }
    public bool RequiresSignature { get; set; }
    public string? SignatureWorkflow { get; set; }
    public string? Notes { get; set; }
}

public class DocumentSignatureDto
{
    public int Id { get; set; }
    public int SignerId { get; set; }
    public string SignerName { get; set; } = string.Empty;
    public string SignerRole { get; set; } = string.Empty;
    public string SignatureType { get; set; } = string.Empty;
    public DateTime SignedAt { get; set; }
    public ApprovalAction Action { get; set; }
    public string? Comments { get; set; }
    public bool IsValid { get; set; }
    public int SignatureOrder { get; set; }
    public bool IsRequired { get; set; }
}

public class DocumentApprovalDto
{
    public int Id { get; set; }
    public int ApproverId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public ApprovalLevel Level { get; set; }
    public ApprovalAction Action { get; set; }
    public string? Comments { get; set; }
    public DateTime? ActionDate { get; set; }
    public bool IsRequired { get; set; }
    public int ApprovalOrder { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsOverdue { get; set; }
}

public class SignDocumentDto
{
    public string SignatureData { get; set; } = string.Empty; // Base64 signature
    public string SignatureType { get; set; } = "Electronic";
    public string? Comments { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class ApproveDocumentDto
{
    public ApprovalAction Action { get; set; }
    public string? Comments { get; set; }
}
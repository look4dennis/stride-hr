using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class PayslipGeneration : BaseEntity
{
    public int PayrollRecordId { get; set; }
    public int PayslipTemplateId { get; set; }
    public string PayslipPath { get; set; } = string.Empty; // File path to generated payslip
    public string PayslipFileName { get; set; } = string.Empty;
    
    // Generation Metadata
    public DateTime GeneratedAt { get; set; }
    public int GeneratedBy { get; set; }
    public PayslipStatus Status { get; set; } = PayslipStatus.Generated;
    
    // Approval Workflow
    public int? HRApprovedBy { get; set; }
    public DateTime? HRApprovedAt { get; set; }
    public string? HRApprovalNotes { get; set; }
    
    public int? FinanceApprovedBy { get; set; }
    public DateTime? FinanceApprovedAt { get; set; }
    public string? FinanceApprovalNotes { get; set; }
    
    // Release Information
    public DateTime? ReleasedAt { get; set; }
    public int? ReleasedBy { get; set; }
    public bool IsNotificationSent { get; set; } = false;
    public DateTime? NotificationSentAt { get; set; }
    
    // Version Control
    public int Version { get; set; } = 1;
    public string? RegenerationReason { get; set; }
    
    // Navigation Properties
    public virtual PayrollRecord PayrollRecord { get; set; } = null!;
    public virtual PayslipTemplate PayslipTemplate { get; set; } = null!;
    public virtual Employee GeneratedByEmployee { get; set; } = null!;
    public virtual Employee? HRApprovedByEmployee { get; set; }
    public virtual Employee? FinanceApprovedByEmployee { get; set; }
    public virtual Employee? ReleasedByEmployee { get; set; }
    public virtual ICollection<PayslipApprovalHistory> ApprovalHistory { get; set; } = new List<PayslipApprovalHistory>();
}
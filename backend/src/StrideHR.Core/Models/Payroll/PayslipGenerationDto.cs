using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Payroll;

public class PayslipGenerationDto
{
    public int Id { get; set; }
    public int PayrollRecordId { get; set; }
    public int PayslipTemplateId { get; set; }
    public string PayslipPath { get; set; } = string.Empty;
    public string PayslipFileName { get; set; } = string.Empty;
    public PayslipStatus Status { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string GeneratedByName { get; set; } = string.Empty;
    
    // Employee Information
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    
    // Payroll Information
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }
    public decimal NetSalary { get; set; }
    public string Currency { get; set; } = string.Empty;
    
    // Approval Information
    public PayslipApprovalInfo? HRApproval { get; set; }
    public PayslipApprovalInfo? FinanceApproval { get; set; }
    
    // Release Information
    public DateTime? ReleasedAt { get; set; }
    public string? ReleasedByName { get; set; }
    public bool IsNotificationSent { get; set; }
    
    public int Version { get; set; }
    public string? RegenerationReason { get; set; }
}

public class PayslipApprovalInfo
{
    public string ApprovedByName { get; set; } = string.Empty;
    public DateTime ApprovedAt { get; set; }
    public string? Notes { get; set; }
}

public class CreatePayslipGenerationRequest
{
    public int PayrollRecordId { get; set; }
    public int PayslipTemplateId { get; set; }
    public bool AutoSubmitForApproval { get; set; } = true;
}

public class BulkPayslipGenerationRequest
{
    public List<int> PayrollRecordIds { get; set; } = new();
    public int PayslipTemplateId { get; set; }
    public bool AutoSubmitForApproval { get; set; } = true;
}

public class PayslipApprovalRequest
{
    public int PayslipGenerationId { get; set; }
    public PayslipApprovalLevel ApprovalLevel { get; set; }
    public PayslipApprovalAction Action { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
}

public class PayslipReleaseRequest
{
    public List<int> PayslipGenerationIds { get; set; } = new();
    public bool SendNotifications { get; set; } = true;
    public string? ReleaseNotes { get; set; }
}

public class PayslipApprovalSummary
{
    public int TotalPayslips { get; set; }
    public int PendingHRApproval { get; set; }
    public int PendingFinanceApproval { get; set; }
    public int Approved { get; set; }
    public int Released { get; set; }
    public int Rejected { get; set; }
    public decimal TotalPayrollAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

/// <summary>
/// Leave request entity - placeholder for leave management task
/// </summary>
public class LeaveRequest : AuditableEntity
{
    public int EmployeeId { get; set; }
    
    /// <summary>
    /// Leave type
    /// </summary>
    public LeaveType Type { get; set; }
    
    /// <summary>
    /// Leave start date
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Leave end date
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Number of leave days
    /// </summary>
    public decimal Days { get; set; }
    
    /// <summary>
    /// Leave reason
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    /// <summary>
    /// Leave request status
    /// </summary>
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    
    /// <summary>
    /// Approved/rejected by
    /// </summary>
    public int? ApprovedBy { get; set; }
    
    /// <summary>
    /// Approval/rejection date
    /// </summary>
    public DateTime? ApprovalDate { get; set; }
    
    /// <summary>
    /// Approval/rejection comments
    /// </summary>
    [MaxLength(500)]
    public string? ApprovalComments { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee? Approver { get; set; }
}

/// <summary>
/// Leave type enumeration
/// </summary>
public enum LeaveType
{
    Annual = 1,
    Sick = 2,
    Personal = 3,
    Maternity = 4,
    Paternity = 5,
    Emergency = 6,
    Unpaid = 7
}

/// <summary>
/// Leave status enumeration
/// </summary>
public enum LeaveStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}
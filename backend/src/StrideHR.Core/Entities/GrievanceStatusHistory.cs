using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class GrievanceStatusHistory : BaseEntity
{
    public int GrievanceId { get; set; }
    public GrievanceStatus FromStatus { get; set; }
    public GrievanceStatus ToStatus { get; set; }
    public int ChangedById { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    
    // Navigation Properties
    public virtual Grievance Grievance { get; set; } = null!;
    public virtual Employee ChangedBy { get; set; } = null!;
}
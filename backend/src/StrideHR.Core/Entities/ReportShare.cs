using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ReportShare : BaseEntity
{
    public int ReportId { get; set; }
    public int SharedWith { get; set; }
    public int SharedBy { get; set; }
    public ReportPermission Permission { get; set; }
    public DateTime SharedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public virtual Report Report { get; set; } = null!;
    public virtual Employee SharedWithEmployee { get; set; } = null!;
    public virtual Employee SharedByEmployee { get; set; } = null!;
}
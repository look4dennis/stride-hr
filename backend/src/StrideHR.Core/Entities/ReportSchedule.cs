using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ReportSchedule : BaseEntity
{
    public int ReportId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? NextRunTime { get; set; }
    public DateTime? LastRunTime { get; set; }
    public string? Parameters { get; set; } // JSON parameters
    public string Recipients { get; set; } = string.Empty; // JSON array of email addresses
    public ReportExportFormat ExportFormat { get; set; }
    public string? EmailSubject { get; set; }
    public string? EmailBody { get; set; }
    public new int CreatedBy { get; set; }

    // Navigation Properties
    public virtual Report Report { get; set; } = null!;
    public virtual Employee CreatedByEmployee { get; set; } = null!;
}
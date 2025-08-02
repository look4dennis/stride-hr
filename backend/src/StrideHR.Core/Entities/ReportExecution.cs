using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ReportExecution : BaseEntity
{
    public int ReportId { get; set; }
    public int ExecutedBy { get; set; }
    public DateTime ExecutedAt { get; set; }
    public ReportExecutionStatus Status { get; set; }
    public string? Parameters { get; set; } // JSON parameters
    public string? ResultData { get; set; } // JSON result data
    public string? ErrorMessage { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public int RecordCount { get; set; }
    public string? ExportFormat { get; set; }
    public string? ExportPath { get; set; }

    // Navigation Properties
    public virtual Report Report { get; set; } = null!;
    public virtual Employee ExecutedByEmployee { get; set; } = null!;
}
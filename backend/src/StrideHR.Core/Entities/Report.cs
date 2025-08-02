using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class Report : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public string DataSource { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty; // JSON configuration
    public string Filters { get; set; } = string.Empty; // JSON filters
    public string Columns { get; set; } = string.Empty; // JSON column definitions
    public string ChartConfiguration { get; set; } = string.Empty; // JSON chart config
    public bool IsPublic { get; set; }
    public bool IsScheduled { get; set; }
    public string? ScheduleCron { get; set; }
    public DateTime? LastExecuted { get; set; }
    public ReportStatus Status { get; set; }
    public new int CreatedBy { get; set; }
    public int? BranchId { get; set; }

    // Navigation Properties
    public virtual Employee CreatedByEmployee { get; set; } = null!;
    public virtual Branch? Branch { get; set; }
    public virtual ICollection<ReportExecution> ReportExecutions { get; set; } = new List<ReportExecution>();
    public virtual ICollection<ReportSchedule> ReportSchedules { get; set; } = new List<ReportSchedule>();
    public virtual ICollection<ReportShare> ReportShares { get; set; } = new List<ReportShare>();
}
using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.DataImportExport;

public class ExportRequestDto
{
    public string EntityType { get; set; } = string.Empty;
    public ReportExportFormat Format { get; set; } = ReportExportFormat.Excel;
    public Dictionary<string, object> Filters { get; set; } = new();
    public List<string> SelectedFields { get; set; } = new();
    public int? BranchId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IncludeDeleted { get; set; } = false;
}

public class ExportResultDto
{
    public bool Success { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public byte[]? FileContent { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
    public string ExportedBy { get; set; } = string.Empty;
    public string? Message { get; set; }
}
namespace StrideHR.Core.Models.DataImportExport;

public class ImportResultDto
{
    public bool Success { get; set; }
    public int TotalRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public List<ImportErrorDto> Errors { get; set; } = new();
    public string? Message { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    public string ImportedBy { get; set; } = string.Empty;
}

public class ImportErrorDto
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
}
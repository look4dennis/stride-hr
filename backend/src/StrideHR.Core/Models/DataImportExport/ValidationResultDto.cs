namespace StrideHR.Core.Models.DataImportExport;

public class ValidationResultDto
{
    public bool IsValid { get; set; }
    public List<ValidationErrorDto> Errors { get; set; } = new();
    public List<ValidationWarningDto> Warnings { get; set; } = new();
    public int TotalRecords { get; set; }
    public int ValidRecords { get; set; }
    public int InvalidRecords { get; set; }
}

public class ValidationErrorDto
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
}

public class ValidationWarningDto
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string WarningMessage { get; set; } = string.Empty;
    public string WarningCode { get; set; } = string.Empty;
}

public enum ValidationSeverity
{
    Warning,
    Error,
    Critical
}
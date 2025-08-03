namespace StrideHR.Core.Models.Integrations;

public class ExternalIntegration
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public IntegrationType Type { get; set; }
    public string SystemType { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty; // JSON configuration
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public int CreatedBy { get; set; }
    
    public virtual List<IntegrationLog> Logs { get; set; } = new();
}

public class IntegrationLog
{
    public int Id { get; set; }
    public int ExternalIntegrationId { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RequestData { get; set; }
    public string? ResponseData { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public TimeSpan Duration { get; set; }
    
    public virtual ExternalIntegration ExternalIntegration { get; set; } = null!;
}

// Payroll Integration Models
public class PayrollSystemConfig
{
    public string ApiUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Dictionary<string, string> CustomFields { get; set; } = new();
}

public class PayrollIntegrationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ExternalIntegration? Integration { get; set; }
    public string? ErrorCode { get; set; }
}

public class PayrollExportRequest
{
    public DateTime PayrollPeriodStart { get; set; }
    public DateTime PayrollPeriodEnd { get; set; }
    public List<int>? EmployeeIds { get; set; }
    public List<int>? BranchIds { get; set; }
    public PayrollExportFormat Format { get; set; }
    public Dictionary<string, object> CustomParameters { get; set; } = new();
}

public class PayrollExportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ExportedData { get; set; }
    public string? FileName { get; set; }
    public int RecordsExported { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class PayrollImportRequest
{
    public string Data { get; set; } = string.Empty;
    public PayrollImportFormat Format { get; set; }
    public bool ValidateOnly { get; set; }
    public Dictionary<string, object> CustomParameters { get; set; } = new();
}

public class PayrollImportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public int RecordsImported { get; set; }
    public int RecordsSkipped { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

// Accounting Integration Models
public class AccountingSystemConfig
{
    public string ApiUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Dictionary<string, string> AccountMappings { get; set; } = new();
}

public class AccountingIntegrationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ExternalIntegration? Integration { get; set; }
    public string? ErrorCode { get; set; }
}

public class AccountingExportRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AccountingDataType DataType { get; set; }
    public List<int>? BranchIds { get; set; }
    public AccountingExportFormat Format { get; set; }
    public Dictionary<string, object> CustomParameters { get; set; } = new();
}

public class AccountingExportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ExportedData { get; set; }
    public string? FileName { get; set; }
    public int RecordsExported { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class AccountingImportRequest
{
    public string Data { get; set; } = string.Empty;
    public AccountingImportFormat Format { get; set; }
    public bool ValidateOnly { get; set; }
    public Dictionary<string, object> CustomParameters { get; set; } = new();
}

public class AccountingImportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public int RecordsImported { get; set; }
    public int RecordsSkipped { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class IntegrationSyncResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RecordsSynced { get; set; }
    public DateTime SyncedAt { get; set; }
    public SyncDirection Direction { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class IntegrationHealthStatus
{
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime LastChecked { get; set; }
    public DateTime? LastSuccessfulSync { get; set; }
    public int ConsecutiveFailures { get; set; }
    public List<string> Issues { get; set; } = new();
}

public class IntegrationMetrics
{
    public int TotalOperations { get; set; }
    public int SuccessfulOperations { get; set; }
    public int FailedOperations { get; set; }
    public double SuccessRate { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Dictionary<string, int> OperationCounts { get; set; } = new();
}

// Enums
public enum IntegrationType
{
    Payroll,
    Accounting,
    Calendar,
    Email,
    Other
}

public enum PayrollSystemType
{
    ADP,
    Paychex,
    QuickBooks,
    SAP,
    Workday,
    BambooHR,
    Custom
}

public enum AccountingSystemType
{
    QuickBooks,
    Xero,
    SAP,
    Oracle,
    NetSuite,
    Sage,
    Custom
}

public enum PayrollExportFormat
{
    Json,
    Xml,
    Csv,
    Excel,
    Custom
}

public enum PayrollImportFormat
{
    Json,
    Xml,
    Csv,
    Excel,
    Custom
}

public enum AccountingDataType
{
    Payroll,
    Expenses,
    Assets,
    All
}

public enum AccountingExportFormat
{
    Json,
    Xml,
    Csv,
    Excel,
    Custom
}

public enum AccountingImportFormat
{
    Json,
    Xml,
    Csv,
    Excel,
    Custom
}

public enum SyncDirection
{
    Import,
    Export,
    Bidirectional
}
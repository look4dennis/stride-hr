namespace StrideHR.API.DTOs.Organization;

/// <summary>
/// Organization data transfer object for API responses
/// </summary>
public class OrganizationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? LogoPath { get; set; }
    public decimal NormalWorkingHours { get; set; }
    public decimal OvertimeRate { get; set; }
    public int ProductiveHoursThreshold { get; set; }
    public bool BranchIsolationEnabled { get; set; }
    public Dictionary<string, object>? Settings { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<BranchDto>? Branches { get; set; }
}

/// <summary>
/// Branch data transfer object for API responses
/// </summary>
public class BranchDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public List<string>? LocalHolidays { get; set; }
    public Dictionary<string, object>? ComplianceSettings { get; set; }
    public string? EmployeeIdPattern { get; set; }
    public TimeSpan? WorkingHoursStart { get; set; }
    public TimeSpan? WorkingHoursEnd { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int EmployeeCount { get; set; }
}

/// <summary>
/// Create organization request DTO
/// </summary>
public class CreateOrganizationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public decimal NormalWorkingHours { get; set; } = 8.0m;
    public decimal OvertimeRate { get; set; } = 1.5m;
    public int ProductiveHoursThreshold { get; set; } = 6;
    public bool BranchIsolationEnabled { get; set; } = false;
    public Dictionary<string, object>? Settings { get; set; }
}

/// <summary>
/// Update organization request DTO
/// </summary>
public class UpdateOrganizationDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public decimal? NormalWorkingHours { get; set; }
    public decimal? OvertimeRate { get; set; }
    public int? ProductiveHoursThreshold { get; set; }
    public bool? BranchIsolationEnabled { get; set; }
    public Dictionary<string, object>? Settings { get; set; }
}

/// <summary>
/// Create branch request DTO
/// </summary>
public class CreateBranchDto
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string TimeZone { get; set; } = "UTC";
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public List<string>? LocalHolidays { get; set; }
    public Dictionary<string, object>? ComplianceSettings { get; set; }
    public string? EmployeeIdPattern { get; set; }
    public TimeSpan? WorkingHoursStart { get; set; }
    public TimeSpan? WorkingHoursEnd { get; set; }
}

/// <summary>
/// Update branch request DTO
/// </summary>
public class UpdateBranchDto
{
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? Currency { get; set; }
    public string? TimeZone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public List<string>? LocalHolidays { get; set; }
    public Dictionary<string, object>? ComplianceSettings { get; set; }
    public string? EmployeeIdPattern { get; set; }
    public TimeSpan? WorkingHoursStart { get; set; }
    public TimeSpan? WorkingHoursEnd { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Organization configuration request DTO
/// </summary>
public class OrganizationConfigurationDto
{
    public decimal? NormalWorkingHours { get; set; }
    public decimal? OvertimeRate { get; set; }
    public int? ProductiveHoursThreshold { get; set; }
    public bool? BranchIsolationEnabled { get; set; }
    public Dictionary<string, object>? CustomSettings { get; set; }
}

/// <summary>
/// Compliance settings request DTO
/// </summary>
public class ComplianceSettingsDto
{
    public Dictionary<string, object> TaxSettings { get; set; } = new();
    public Dictionary<string, object> LaborLawSettings { get; set; } = new();
    public Dictionary<string, object> StatutorySettings { get; set; } = new();
    public Dictionary<string, object> ReportingSettings { get; set; } = new();
    public List<string> RequiredDocuments { get; set; } = new();
    public Dictionary<string, object> CustomCompliance { get; set; } = new();
}

/// <summary>
/// Currency conversion request DTO
/// </summary>
public class CurrencyConversionDto
{
    public decimal Amount { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
}

/// <summary>
/// Currency conversion response DTO
/// </summary>
public class CurrencyConversionResponseDto
{
    public decimal OriginalAmount { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public decimal ConvertedAmount { get; set; }
    public string ToCurrency { get; set; } = string.Empty;
    public DateTime ConversionDate { get; set; }
}

/// <summary>
/// Logo upload response DTO
/// </summary>
public class LogoUploadResponseDto
{
    public string FilePath { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
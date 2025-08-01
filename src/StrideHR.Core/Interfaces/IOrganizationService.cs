using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Interface for organization management operations
/// </summary>
public interface IOrganizationService
{
    // Organization CRUD Operations
    Task<Organization> CreateOrganizationAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default);
    Task<Organization?> GetOrganizationByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Organization> UpdateOrganizationAsync(int id, UpdateOrganizationRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteOrganizationAsync(int id, string? deletedBy = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Organization>> GetAllOrganizationsAsync(CancellationToken cancellationToken = default);
    
    // Logo Management
    Task<string> UploadLogoAsync(int organizationId, Stream logoStream, string fileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteLogoAsync(int organizationId, CancellationToken cancellationToken = default);
    
    // Configuration Management
    Task<Organization> UpdateConfigurationAsync(int organizationId, OrganizationConfigurationRequest request, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetConfigurationAsync(int organizationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for branch management operations
/// </summary>
public interface IBranchService
{
    // Branch CRUD Operations
    Task<Branch> CreateBranchAsync(CreateBranchRequest request, CancellationToken cancellationToken = default);
    Task<Branch?> GetBranchByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Branch> UpdateBranchAsync(int id, UpdateBranchRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteBranchAsync(int id, string? deletedBy = null, CancellationToken cancellationToken = default);
    
    // Branch Queries
    Task<IEnumerable<Branch>> GetBranchesByOrganizationAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Branch>> GetActiveBranchesAsync(int? organizationId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Branch>> GetBranchesByCountryAsync(string country, CancellationToken cancellationToken = default);
    
    // Multi-Country Support
    Task<IEnumerable<string>> GetSupportedCountriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetSupportedCurrenciesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetSupportedTimeZonesAsync(CancellationToken cancellationToken = default);
    
    // Currency and Timezone Handling
    Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken = default);
    Task<DateTime> ConvertToLocalTimeAsync(DateTime utcDateTime, string timeZone, CancellationToken cancellationToken = default);
    Task<DateTime> ConvertToUtcAsync(DateTime localDateTime, string timeZone, CancellationToken cancellationToken = default);
    
    // Branch-based Data Isolation
    Task<bool> ValidateBranchAccessAsync(int branchId, string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<int>> GetUserAccessibleBranchesAsync(string userId, CancellationToken cancellationToken = default);
    
    // Compliance Management
    Task<Branch> UpdateComplianceSettingsAsync(int branchId, ComplianceSettingsRequest request, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetComplianceSettingsAsync(int branchId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request model for creating an organization
/// </summary>
public class CreateOrganizationRequest
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
/// Request model for updating an organization
/// </summary>
public class UpdateOrganizationRequest
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
/// Request model for organization configuration
/// </summary>
public class OrganizationConfigurationRequest
{
    public decimal? NormalWorkingHours { get; set; }
    public decimal? OvertimeRate { get; set; }
    public int? ProductiveHoursThreshold { get; set; }
    public bool? BranchIsolationEnabled { get; set; }
    public Dictionary<string, object>? CustomSettings { get; set; }
}

/// <summary>
/// Request model for creating a branch
/// </summary>
public class CreateBranchRequest
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
/// Request model for updating a branch
/// </summary>
public class UpdateBranchRequest
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
/// Request model for compliance settings
/// </summary>
public class ComplianceSettingsRequest
{
    public Dictionary<string, object> TaxSettings { get; set; } = new();
    public Dictionary<string, object> LaborLawSettings { get; set; } = new();
    public Dictionary<string, object> StatutorySettings { get; set; } = new();
    public Dictionary<string, object> ReportingSettings { get; set; } = new();
    public List<string> RequiredDocuments { get; set; } = new();
    public Dictionary<string, object> CustomCompliance { get; set; } = new();
}
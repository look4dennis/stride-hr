using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using System.Text.Json;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// Service implementation for branch management operations
/// </summary>
public class BranchService : IBranchService
{
    private readonly IBranchRepository _branchRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ILogger<BranchService> _logger;
    private readonly IAuditService _auditService;
    private readonly ICurrencyService _currencyService;
    private readonly ITimeZoneService _timeZoneService;

    // Static data for supported countries, currencies, and timezones
    private static readonly Dictionary<string, string> SupportedCountries = new()
    {
        { "US", "United States" },
        { "CA", "Canada" },
        { "GB", "United Kingdom" },
        { "IN", "India" },
        { "AU", "Australia" },
        { "DE", "Germany" },
        { "FR", "France" },
        { "JP", "Japan" },
        { "SG", "Singapore" },
        { "AE", "United Arab Emirates" }
    };

    private static readonly string[] SupportedCurrencies = 
    {
        "USD", "CAD", "GBP", "EUR", "INR", "AUD", "JPY", "SGD", "AED"
    };

    private static readonly string[] SupportedTimeZones = 
    {
        "UTC", "America/New_York", "America/Chicago", "America/Denver", "America/Los_Angeles",
        "America/Toronto", "Europe/London", "Europe/Paris", "Europe/Berlin", "Asia/Kolkata",
        "Asia/Tokyo", "Asia/Singapore", "Australia/Sydney", "Asia/Dubai"
    };

    // Simple exchange rates (in production, this would come from an external service)
    private static readonly Dictionary<string, decimal> ExchangeRates = new()
    {
        { "USD", 1.0m },
        { "EUR", 0.85m },
        { "GBP", 0.73m },
        { "INR", 83.0m },
        { "CAD", 1.35m },
        { "AUD", 1.50m },
        { "JPY", 150.0m },
        { "SGD", 1.35m },
        { "AED", 3.67m }
    };

    public BranchService(
        IBranchRepository branchRepository,
        IOrganizationRepository organizationRepository,
        ILogger<BranchService> logger,
        IAuditService auditService,
        ICurrencyService currencyService,
        ITimeZoneService timeZoneService)
    {
        _branchRepository = branchRepository;
        _organizationRepository = organizationRepository;
        _logger = logger;
        _auditService = auditService;
        _currencyService = currencyService;
        _timeZoneService = timeZoneService;
    }

    public async Task<Branch> CreateBranchAsync(CreateBranchRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new branch: {Name} in {Country}", request.Name, request.Country);

        // Validate organization exists
        var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
        if (organization == null)
        {
            throw new ArgumentException($"Organization with ID {request.OrganizationId} not found");
        }

        // Validate branch name uniqueness within organization
        if (!await _branchRepository.IsNameUniqueAsync(request.Name, request.OrganizationId, null, cancellationToken))
        {
            throw new InvalidOperationException($"Branch name '{request.Name}' already exists in this organization");
        }

        var branch = new Branch
        {
            OrganizationId = request.OrganizationId,
            Name = request.Name,
            Country = request.Country,
            Currency = request.Currency,
            TimeZone = request.TimeZone,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Phone = request.Phone,
            Email = request.Email,
            LocalHolidays = request.LocalHolidays != null ? JsonSerializer.Serialize(request.LocalHolidays) : null,
            ComplianceSettings = request.ComplianceSettings != null ? JsonSerializer.Serialize(request.ComplianceSettings) : null,
            EmployeeIdPattern = request.EmployeeIdPattern,
            WorkingHoursStart = request.WorkingHoursStart,
            WorkingHoursEnd = request.WorkingHoursEnd,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdBranch = await _branchRepository.AddAsync(branch, cancellationToken);
        await _branchRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Branch", createdBranch.Id, "CREATE", 
            $"Branch '{request.Name}' created in {request.Country}", cancellationToken);

        _logger.LogInformation("Branch created successfully: {Name}", request.Name);
        return createdBranch;
    }

    public async Task<Branch?> GetBranchByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _branchRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Branch> UpdateBranchAsync(int id, UpdateBranchRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating branch: {Id}", id);

        var branch = await _branchRepository.GetByIdAsync(id, cancellationToken);
        if (branch == null)
        {
            throw new ArgumentException($"Branch with ID {id} not found");
        }

        // Validate branch name uniqueness if name is being changed
        if (!string.IsNullOrEmpty(request.Name) && request.Name != branch.Name)
        {
            if (!await _branchRepository.IsNameUniqueAsync(request.Name, branch.OrganizationId, id, cancellationToken))
            {
                throw new InvalidOperationException($"Branch name '{request.Name}' already exists in this organization");
            }
        }

        // Update fields that are provided
        if (!string.IsNullOrEmpty(request.Name)) branch.Name = request.Name;
        if (!string.IsNullOrEmpty(request.Country)) branch.Country = request.Country;
        if (!string.IsNullOrEmpty(request.Currency)) branch.Currency = request.Currency;
        if (!string.IsNullOrEmpty(request.TimeZone)) branch.TimeZone = request.TimeZone;
        if (request.Address != null) branch.Address = request.Address;
        if (request.City != null) branch.City = request.City;
        if (request.State != null) branch.State = request.State;
        if (request.PostalCode != null) branch.PostalCode = request.PostalCode;
        if (request.Phone != null) branch.Phone = request.Phone;
        if (request.Email != null) branch.Email = request.Email;
        if (request.LocalHolidays != null) branch.LocalHolidays = JsonSerializer.Serialize(request.LocalHolidays);
        if (request.ComplianceSettings != null) branch.ComplianceSettings = JsonSerializer.Serialize(request.ComplianceSettings);
        if (request.EmployeeIdPattern != null) branch.EmployeeIdPattern = request.EmployeeIdPattern;
        if (request.WorkingHoursStart.HasValue) branch.WorkingHoursStart = request.WorkingHoursStart;
        if (request.WorkingHoursEnd.HasValue) branch.WorkingHoursEnd = request.WorkingHoursEnd;
        if (request.IsActive.HasValue) branch.IsActive = request.IsActive.Value;

        branch.UpdatedAt = DateTime.UtcNow;

        var updatedBranch = await _branchRepository.UpdateAsync(branch, cancellationToken);
        await _branchRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Branch", id, "UPDATE", 
            $"Branch '{branch.Name}' updated", cancellationToken);

        _logger.LogInformation("Branch updated successfully: {Name}", branch.Name);
        return updatedBranch;
    }

    public async Task<bool> DeleteBranchAsync(int id, string? deletedBy = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting branch: {Id}", id);

        var branch = await _branchRepository.GetByIdAsync(id, cancellationToken);
        if (branch == null)
        {
            return false;
        }

        await _branchRepository.SoftDeleteAsync(id, deletedBy, cancellationToken);
        await _branchRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Branch", id, "DELETE", 
            $"Branch '{branch.Name}' deleted", cancellationToken);

        _logger.LogInformation("Branch deleted successfully: {Name}", branch.Name);
        return true;
    }

    public async Task<IEnumerable<Branch>> GetBranchesByOrganizationAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        return await _branchRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);
    }

    public async Task<IEnumerable<Branch>> GetActiveBranchesAsync(int? organizationId = null, CancellationToken cancellationToken = default)
    {
        return await _branchRepository.GetActiveBranchesAsync(organizationId, cancellationToken);
    }

    public async Task<IEnumerable<Branch>> GetBranchesByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        return await _branchRepository.GetByCountryAsync(country, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetSupportedCountriesAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(SupportedCountries.Values);
    }

    public async Task<IEnumerable<string>> GetSupportedCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        var currencies = await _currencyService.GetSupportedCurrenciesAsync(cancellationToken);
        return currencies.Select(c => c.Code);
    }

    public async Task<IEnumerable<string>> GetSupportedTimeZonesAsync(CancellationToken cancellationToken = default)
    {
        var timeZones = await _timeZoneService.GetSupportedTimeZonesAsync(cancellationToken);
        return timeZones.Select(tz => tz.Id);
    }

    public async Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken = default)
    {
        return await _currencyService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency, cancellationToken);
    }

    public async Task<DateTime> ConvertToLocalTimeAsync(DateTime utcDateTime, string timeZone, CancellationToken cancellationToken = default)
    {
        return await _timeZoneService.ConvertToLocalTimeAsync(utcDateTime, timeZone, cancellationToken);
    }

    public async Task<DateTime> ConvertToUtcAsync(DateTime localDateTime, string timeZone, CancellationToken cancellationToken = default)
    {
        return await _timeZoneService.ConvertToUtcAsync(localDateTime, timeZone, cancellationToken);
    }

    public async Task<bool> ValidateBranchAccessAsync(int branchId, string userId, CancellationToken cancellationToken = default)
    {
        // This is a simplified implementation
        // In a real application, this would check user permissions and branch assignments
        var branch = await _branchRepository.GetByIdAsync(branchId, cancellationToken);
        return branch != null && branch.IsActive;
    }

    public async Task<IEnumerable<int>> GetUserAccessibleBranchesAsync(string userId, CancellationToken cancellationToken = default)
    {
        // This is a simplified implementation
        // In a real application, this would return branches based on user permissions
        var activeBranches = await _branchRepository.GetActiveBranchesAsync(null, cancellationToken);
        return activeBranches.Select(b => b.Id);
    }

    public async Task<Branch> UpdateComplianceSettingsAsync(int branchId, ComplianceSettingsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating compliance settings for branch: {BranchId}", branchId);

        var branch = await _branchRepository.GetByIdAsync(branchId, cancellationToken);
        if (branch == null)
        {
            throw new ArgumentException($"Branch with ID {branchId} not found");
        }

        var complianceSettings = new Dictionary<string, object>
        {
            ["TaxSettings"] = request.TaxSettings,
            ["LaborLawSettings"] = request.LaborLawSettings,
            ["StatutorySettings"] = request.StatutorySettings,
            ["ReportingSettings"] = request.ReportingSettings,
            ["RequiredDocuments"] = request.RequiredDocuments,
            ["CustomCompliance"] = request.CustomCompliance,
            ["LastUpdated"] = DateTime.UtcNow
        };

        branch.ComplianceSettings = JsonSerializer.Serialize(complianceSettings);
        branch.UpdatedAt = DateTime.UtcNow;

        var updatedBranch = await _branchRepository.UpdateAsync(branch, cancellationToken);
        await _branchRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Branch", branchId, "COMPLIANCE_UPDATE", 
            $"Compliance settings updated for branch '{branch.Name}'", cancellationToken);

        _logger.LogInformation("Compliance settings updated successfully for branch: {Name}", branch.Name);
        return updatedBranch;
    }

    public async Task<Dictionary<string, object>> GetComplianceSettingsAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var branch = await _branchRepository.GetByIdAsync(branchId, cancellationToken);
        if (branch == null)
        {
            throw new ArgumentException($"Branch with ID {branchId} not found");
        }

        if (string.IsNullOrEmpty(branch.ComplianceSettings))
        {
            return new Dictionary<string, object>();
        }

        var settings = JsonSerializer.Deserialize<Dictionary<string, object>>(branch.ComplianceSettings);
        return settings ?? new Dictionary<string, object>();
    }
}
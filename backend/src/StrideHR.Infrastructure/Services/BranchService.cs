using System.Text.Json;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Branch;

namespace StrideHR.Infrastructure.Services;

public class BranchService : IBranchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrencyService _currencyService;
    private readonly ITimeZoneService _timeZoneService;
    private readonly ILogger<BranchService> _logger;

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

    private static readonly Dictionary<string, string> SupportedCurrencies = new()
    {
        { "USD", "$" },
        { "CAD", "C$" },
        { "GBP", "£" },
        { "INR", "₹" },
        { "AUD", "A$" },
        { "EUR", "€" },
        { "JPY", "¥" },
        { "SGD", "S$" },
        { "AED", "د.إ" }
    };

    private static readonly List<string> SupportedTimeZones = new()
    {
        "UTC",
        "America/New_York",
        "America/Chicago",
        "America/Denver",
        "America/Los_Angeles",
        "America/Toronto",
        "Europe/London",
        "Europe/Paris",
        "Europe/Berlin",
        "Asia/Kolkata",
        "Asia/Tokyo",
        "Asia/Singapore",
        "Asia/Dubai",
        "Australia/Sydney"
    };

    public BranchService(
        IUnitOfWork unitOfWork, 
        ICurrencyService currencyService,
        ITimeZoneService timeZoneService,
        ILogger<BranchService> logger)
    {
        _unitOfWork = unitOfWork;
        _currencyService = currencyService;
        _timeZoneService = timeZoneService;
        _logger = logger;
    }

    #region Basic CRUD Operations

    public async Task<Branch?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Branches.GetByIdAsync(id, b => b.Organization, b => b.Employees);
    }

    public async Task<IEnumerable<Branch>> GetAllAsync()
    {
        var branches = await _unitOfWork.Branches.GetAllAsync(b => b.Organization);
        return branches ?? Enumerable.Empty<Branch>();
    }

    public async Task<IEnumerable<Branch>> GetByOrganizationAsync(int organizationId)
    {
        var branches = await _unitOfWork.Branches.FindAsync(
            b => b.OrganizationId == organizationId,
            b => b.Organization);
        return branches ?? Enumerable.Empty<Branch>();
    }

    public async Task<Branch> CreateAsync(CreateBranchDto dto)
    {
        if (!await ValidateBranchDataAsync(dto))
        {
            throw new ArgumentException("Invalid branch data");
        }

        var branch = new Branch
        {
            OrganizationId = dto.OrganizationId,
            Name = dto.Name,
            Country = dto.Country,
            CountryCode = dto.CountryCode,
            Currency = dto.Currency,
            CurrencySymbol = dto.CurrencySymbol,
            TimeZone = dto.TimeZone,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Phone = dto.Phone,
            Email = dto.Email,
            LocalHolidays = JsonSerializer.Serialize(dto.LocalHolidays),
            ComplianceSettings = JsonSerializer.Serialize(dto.ComplianceSettings ?? new BranchComplianceDto()),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Branches.AddAsync(branch);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Branch created successfully: {BranchName} in {Country}", branch.Name, branch.Country);
        return branch;
    }

    public async Task UpdateAsync(int id, UpdateBranchDto dto)
    {
        if (!await ValidateBranchUpdateAsync(id, dto))
        {
            throw new ArgumentException("Invalid branch update data");
        }

        var branch = await GetByIdAsync(id);
        if (branch == null)
        {
            throw new ArgumentException("Branch not found");
        }

        branch.Name = dto.Name;
        branch.Country = dto.Country;
        branch.CountryCode = dto.CountryCode;
        branch.Currency = dto.Currency;
        branch.CurrencySymbol = dto.CurrencySymbol;
        branch.TimeZone = dto.TimeZone;
        branch.Address = dto.Address;
        branch.City = dto.City;
        branch.State = dto.State;
        branch.PostalCode = dto.PostalCode;
        branch.Phone = dto.Phone;
        branch.Email = dto.Email;
        branch.IsActive = dto.IsActive;
        branch.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Branches.UpdateAsync(branch);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Branch updated successfully: {BranchName}", branch.Name);
    }

    public async Task DeleteAsync(int id)
    {
        var branch = await GetByIdAsync(id);
        if (branch == null)
        {
            throw new ArgumentException("Branch not found");
        }

        // Check if branch has employees
        if (branch.Employees.Any())
        {
            throw new InvalidOperationException("Cannot delete branch with existing employees");
        }

        await _unitOfWork.Branches.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Branch deleted successfully: {BranchName}", branch.Name);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _unitOfWork.Branches.AnyAsync(b => b.Id == id);
    }

    #endregion

    #region Enhanced Functionality

    public async Task<BranchDto?> GetBranchDtoAsync(int id)
    {
        var branch = await GetByIdAsync(id);
        return branch != null ? await MapToBranchDtoAsync(branch) : null;
    }

    public async Task<IEnumerable<BranchDto>> GetBranchDtosAsync()
    {
        var branches = await GetAllAsync();
        var branchDtos = new List<BranchDto>();

        foreach (var branch in branches)
        {
            branchDtos.Add(await MapToBranchDtoAsync(branch));
        }

        return branchDtos;
    }

    public async Task<IEnumerable<BranchDto>> GetBranchDtosByOrganizationAsync(int organizationId)
    {
        var branches = await GetByOrganizationAsync(organizationId);
        var branchDtos = new List<BranchDto>();

        foreach (var branch in branches)
        {
            branchDtos.Add(await MapToBranchDtoAsync(branch));
        }

        return branchDtos;
    }

    #endregion

    #region Multi-country Support

    public async Task<IEnumerable<string>> GetSupportedCountriesAsync()
    {
        return await Task.FromResult(SupportedCountries.Values);
    }

    public async Task<IEnumerable<string>> GetSupportedCurrenciesAsync()
    {
        return await _currencyService.GetSupportedCurrenciesAsync();
    }

    public async Task<IEnumerable<string>> GetSupportedTimeZonesAsync()
    {
        return await _timeZoneService.GetSupportedTimeZonesAsync();
    }

    #endregion

    #region Currency and Timezone Handling

    public async Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        return await _currencyService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency);
    }

    public async Task<DateTime> ConvertTimeZoneAsync(DateTime dateTime, string fromTimeZone, string toTimeZone)
    {
        return await _timeZoneService.ConvertTimeZoneAsync(dateTime, fromTimeZone, toTimeZone);
    }

    #endregion

    #region Branch-based Data Isolation

    public async Task<bool> HasAccessToBranchAsync(int userId, int branchId)
    {
        // This is a simplified implementation. In a real-world scenario,
        // you would check user permissions and branch assignments
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return false;
        }

        // For now, assume all users have access to all branches
        // This should be implemented based on your specific business rules
        return await ExistsAsync(branchId);
    }

    public async Task<IEnumerable<Branch>> GetAccessibleBranchesAsync(int userId)
    {
        // This is a simplified implementation. In a real-world scenario,
        // you would filter branches based on user permissions
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return Enumerable.Empty<Branch>();
        }

        // For now, return all branches
        // This should be implemented based on your specific business rules
        return await GetAllAsync();
    }

    #endregion

    #region Compliance and Configuration

    public async Task UpdateComplianceSettingsAsync(int id, BranchComplianceDto dto)
    {
        var branch = await GetByIdAsync(id);
        if (branch == null)
        {
            throw new ArgumentException("Branch not found");
        }

        branch.ComplianceSettings = JsonSerializer.Serialize(dto);
        branch.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Branches.UpdateAsync(branch);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Compliance settings updated for branch: {BranchName}", branch.Name);
    }

    public async Task<BranchComplianceDto?> GetComplianceSettingsAsync(int id)
    {
        var branch = await GetByIdAsync(id);
        if (branch == null)
        {
            return null;
        }

        try
        {
            if (!string.IsNullOrEmpty(branch.ComplianceSettings))
            {
                return JsonSerializer.Deserialize<BranchComplianceDto>(branch.ComplianceSettings);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize compliance settings for branch {BranchId}", id);
        }

        return new BranchComplianceDto();
    }

    public async Task UpdateLocalHolidaysAsync(int id, List<LocalHolidayDto> holidays)
    {
        var branch = await GetByIdAsync(id);
        if (branch == null)
        {
            throw new ArgumentException("Branch not found");
        }

        branch.LocalHolidays = JsonSerializer.Serialize(holidays);
        branch.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Branches.UpdateAsync(branch);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Local holidays updated for branch: {BranchName}", branch.Name);
    }

    public async Task<List<LocalHolidayDto>> GetLocalHolidaysAsync(int id)
    {
        var branch = await GetByIdAsync(id);
        if (branch == null)
        {
            return new List<LocalHolidayDto>();
        }

        try
        {
            if (!string.IsNullOrEmpty(branch.LocalHolidays))
            {
                return JsonSerializer.Deserialize<List<LocalHolidayDto>>(branch.LocalHolidays) 
                       ?? new List<LocalHolidayDto>();
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize local holidays for branch {BranchId}", id);
        }

        return new List<LocalHolidayDto>();
    }

    #endregion

    #region Validation

    public async Task<bool> ValidateBranchDataAsync(CreateBranchDto dto)
    {
        // Check if organization exists
        var organization = await _unitOfWork.Organizations.GetByIdAsync(dto.OrganizationId);
        if (organization == null)
        {
            return false;
        }

        // Check if branch name already exists in the same organization
        var existingBranch = await _unitOfWork.Branches.FirstOrDefaultAsync(
            b => b.Name == dto.Name && b.OrganizationId == dto.OrganizationId);
        if (existingBranch != null)
        {
            return false;
        }

        // Validate country code
        if (!SupportedCountries.ContainsKey(dto.CountryCode))
        {
            return false;
        }

        // Validate currency
        if (!await _currencyService.IsCurrencySupportedAsync(dto.Currency))
        {
            return false;
        }

        // Validate timezone
        if (!await _timeZoneService.IsTimeZoneSupportedAsync(dto.TimeZone))
        {
            return false;
        }

        return true;
    }

    public async Task<bool> ValidateBranchUpdateAsync(int id, UpdateBranchDto dto)
    {
        // Check if branch name already exists in the same organization (excluding current branch)
        var currentBranch = await GetByIdAsync(id);
        if (currentBranch == null)
        {
            return false;
        }

        var existingBranch = await _unitOfWork.Branches.FirstOrDefaultAsync(
            b => b.Name == dto.Name && b.OrganizationId == currentBranch.OrganizationId && b.Id != id);
        if (existingBranch != null)
        {
            return false;
        }

        // Validate country code
        if (!SupportedCountries.ContainsKey(dto.CountryCode))
        {
            return false;
        }

        // Validate currency
        if (!await _currencyService.IsCurrencySupportedAsync(dto.Currency))
        {
            return false;
        }

        // Validate timezone
        if (!await _timeZoneService.IsTimeZoneSupportedAsync(dto.TimeZone))
        {
            return false;
        }

        return true;
    }

    #endregion

    #region Private Helper Methods

    private async Task<BranchDto> MapToBranchDtoAsync(Branch branch)
    {
        var employeeCount = 0;
        if (branch.Employees.Any())
        {
            employeeCount = branch.Employees.Count;
        }
        else
        {
            var employees = await _unitOfWork.Employees.FindAsync(e => e.BranchId == branch.Id);
            employeeCount = employees?.Count() ?? 0;
        }

        var localHolidays = new List<LocalHolidayDto>();
        var complianceSettings = new BranchComplianceDto();

        try
        {
            if (!string.IsNullOrEmpty(branch.LocalHolidays))
            {
                localHolidays = JsonSerializer.Deserialize<List<LocalHolidayDto>>(branch.LocalHolidays) 
                               ?? new List<LocalHolidayDto>();
            }

            if (!string.IsNullOrEmpty(branch.ComplianceSettings))
            {
                complianceSettings = JsonSerializer.Deserialize<BranchComplianceDto>(branch.ComplianceSettings) 
                                   ?? new BranchComplianceDto();
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize branch data for branch {BranchId}", branch.Id);
        }

        return new BranchDto
        {
            Id = branch.Id,
            OrganizationId = branch.OrganizationId,
            OrganizationName = branch.Organization?.Name ?? string.Empty,
            Name = branch.Name,
            Country = branch.Country,
            CountryCode = branch.CountryCode,
            Currency = branch.Currency,
            CurrencySymbol = branch.CurrencySymbol,
            TimeZone = branch.TimeZone,
            Address = branch.Address,
            City = branch.City,
            State = branch.State,
            PostalCode = branch.PostalCode,
            Phone = branch.Phone,
            Email = branch.Email,
            LocalHolidays = localHolidays,
            ComplianceSettings = complianceSettings,
            IsActive = branch.IsActive,
            EmployeeCount = employeeCount,
            CreatedAt = branch.CreatedAt,
            UpdatedAt = branch.UpdatedAt
        };
    }

    #endregion
}
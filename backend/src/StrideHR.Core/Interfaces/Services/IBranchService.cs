using StrideHR.Core.Entities;
using StrideHR.Core.Models.Branch;

namespace StrideHR.Core.Interfaces.Services;

public interface IBranchService
{
    // Basic CRUD operations
    Task<Branch?> GetByIdAsync(int id);
    Task<IEnumerable<Branch>> GetAllAsync();
    Task<IEnumerable<Branch>> GetByOrganizationAsync(int organizationId);
    Task<Branch> CreateAsync(CreateBranchDto dto);
    Task UpdateAsync(int id, UpdateBranchDto dto);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);

    // Enhanced functionality
    Task<BranchDto?> GetBranchDtoAsync(int id);
    Task<IEnumerable<BranchDto>> GetBranchDtosAsync();
    Task<IEnumerable<BranchDto>> GetBranchDtosByOrganizationAsync(int organizationId);

    // Multi-country support
    Task<IEnumerable<string>> GetSupportedCountriesAsync();
    Task<IEnumerable<string>> GetSupportedCurrenciesAsync();
    Task<IEnumerable<string>> GetSupportedTimeZonesAsync();

    // Currency and timezone handling
    Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency);
    Task<DateTime> ConvertTimeZoneAsync(DateTime dateTime, string fromTimeZone, string toTimeZone);

    // Branch-based data isolation
    Task<bool> HasAccessToBranchAsync(int userId, int branchId);
    Task<IEnumerable<Branch>> GetAccessibleBranchesAsync(int userId);

    // Compliance and configuration
    Task UpdateComplianceSettingsAsync(int id, BranchComplianceDto dto);
    Task<BranchComplianceDto?> GetComplianceSettingsAsync(int id);
    Task UpdateLocalHolidaysAsync(int id, List<LocalHolidayDto> holidays);
    Task<List<LocalHolidayDto>> GetLocalHolidaysAsync(int id);

    // Validation
    Task<bool> ValidateBranchDataAsync(CreateBranchDto dto);
    Task<bool> ValidateBranchUpdateAsync(int id, UpdateBranchDto dto);
}
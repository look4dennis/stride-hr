using StrideHR.Core.Entities;
using StrideHR.Core.Models.Organization;

namespace StrideHR.Core.Interfaces.Services;

public interface IOrganizationService
{
    // Basic CRUD operations
    Task<Organization?> GetByIdAsync(int id);
    Task<IEnumerable<Organization>> GetAllAsync();
    Task<Organization> CreateAsync(CreateOrganizationDto dto);
    Task UpdateAsync(int id, UpdateOrganizationDto dto);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);

    // Enhanced functionality
    Task<OrganizationDto?> GetOrganizationDtoAsync(int id);
    Task<IEnumerable<OrganizationDto>> GetOrganizationDtosAsync();

    // Logo management
    Task<string> UploadLogoAsync(OrganizationLogoUploadDto dto);
    Task<byte[]?> GetLogoAsync(int organizationId);
    Task DeleteLogoAsync(int organizationId);

    // Configuration management
    Task UpdateConfigurationAsync(int id, OrganizationConfigurationDto dto);
    Task<OrganizationConfigurationDto?> GetConfigurationAsync(int id);

    // Validation
    Task<bool> ValidateOrganizationDataAsync(CreateOrganizationDto dto);
    Task<bool> ValidateOrganizationUpdateAsync(int id, UpdateOrganizationDto dto);
}
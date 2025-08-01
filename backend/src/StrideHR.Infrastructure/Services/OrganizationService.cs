using System.Text.Json;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Organization;

namespace StrideHR.Infrastructure.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<OrganizationService> _logger;

    public OrganizationService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ILogger<OrganizationService> logger)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    #region Basic CRUD Operations

    public async Task<Organization?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Organizations.GetByIdAsync(id, o => o.Branches);
    }

    public async Task<IEnumerable<Organization>> GetAllAsync()
    {
        var organizations = await _unitOfWork.Organizations.GetAllAsync(o => o.Branches);
        return organizations ?? Enumerable.Empty<Organization>();
    }

    public async Task<Organization> CreateAsync(CreateOrganizationDto dto)
    {
        if (!await ValidateOrganizationDataAsync(dto))
        {
            throw new ArgumentException("Invalid organization data");
        }

        var organization = new Organization
        {
            Name = dto.Name,
            Address = dto.Address,
            Email = dto.Email,
            Phone = dto.Phone,
            Website = dto.Website,
            TaxId = dto.TaxId,
            RegistrationNumber = dto.RegistrationNumber,
            NormalWorkingHours = TimeSpan.FromHours(dto.NormalWorkingHours),
            OvertimeRate = dto.OvertimeRate,
            ProductiveHoursThreshold = dto.ProductiveHoursThreshold,
            BranchIsolationEnabled = dto.BranchIsolationEnabled,
            ConfigurationSettings = dto.ConfigurationSettings ?? "{}",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Organizations.AddAsync(organization);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Organization created successfully: {OrganizationName}", organization.Name);
        return organization;
    }

    public async Task UpdateAsync(int id, UpdateOrganizationDto dto)
    {
        if (!await ValidateOrganizationUpdateAsync(id, dto))
        {
            throw new ArgumentException("Invalid organization update data");
        }

        var organization = await GetByIdAsync(id);
        if (organization == null)
        {
            throw new ArgumentException("Organization not found");
        }

        organization.Name = dto.Name;
        organization.Address = dto.Address;
        organization.Email = dto.Email;
        organization.Phone = dto.Phone;
        organization.Website = dto.Website;
        organization.TaxId = dto.TaxId;
        organization.RegistrationNumber = dto.RegistrationNumber;
        organization.NormalWorkingHours = TimeSpan.FromHours(dto.NormalWorkingHours);
        organization.OvertimeRate = dto.OvertimeRate;
        organization.ProductiveHoursThreshold = dto.ProductiveHoursThreshold;
        organization.BranchIsolationEnabled = dto.BranchIsolationEnabled;
        organization.ConfigurationSettings = dto.ConfigurationSettings ?? "{}";
        organization.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Organizations.UpdateAsync(organization);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Organization updated successfully: {OrganizationName}", organization.Name);
    }

    public async Task DeleteAsync(int id)
    {
        var organization = await GetByIdAsync(id);
        if (organization == null)
        {
            throw new ArgumentException("Organization not found");
        }

        // Check if organization has branches
        if (organization.Branches.Any())
        {
            throw new InvalidOperationException("Cannot delete organization with existing branches");
        }

        await _unitOfWork.Organizations.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Organization deleted successfully: {OrganizationName}", organization.Name);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _unitOfWork.Organizations.AnyAsync(o => o.Id == id);
    }

    #endregion

    #region Enhanced Functionality

    public async Task<OrganizationDto?> GetOrganizationDtoAsync(int id)
    {
        var organization = await GetByIdAsync(id);
        if (organization == null)
        {
            return null;
        }

        var employeeCount = 0;
        foreach (var branch in organization.Branches)
        {
            var branchEmployees = await _unitOfWork.Employees.FindAsync(e => e.BranchId == branch.Id);
            employeeCount += branchEmployees?.Count() ?? 0;
        }

        return MapToOrganizationDto(organization, employeeCount);
    }

    public async Task<IEnumerable<OrganizationDto>> GetOrganizationDtosAsync()
    {
        var organizations = await GetAllAsync();
        var organizationDtos = new List<OrganizationDto>();

        foreach (var organization in organizations)
        {
            var employeeCount = 0;
            foreach (var branch in organization.Branches)
            {
                var branchEmployees = await _unitOfWork.Employees.FindAsync(e => e.BranchId == branch.Id);
                employeeCount += branchEmployees?.Count() ?? 0;
            }

            organizationDtos.Add(MapToOrganizationDto(organization, employeeCount));
        }

        return organizationDtos;
    }

    #endregion

    #region Logo Management

    public async Task<string> UploadLogoAsync(OrganizationLogoUploadDto dto)
    {
        var organization = await GetByIdAsync(dto.OrganizationId);
        if (organization == null)
        {
            throw new ArgumentException("Organization not found");
        }

        // Delete existing logo if exists
        if (!string.IsNullOrEmpty(organization.Logo))
        {
            await _fileStorageService.DeleteFileAsync(organization.Logo);
        }

        // Save new logo
        var filePath = await _fileStorageService.SaveFileAsync(
            dto.LogoData,
            dto.FileName,
            "organization-logos");

        // Update organization record
        organization.Logo = filePath;
        organization.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Organizations.UpdateAsync(organization);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Logo uploaded for organization: {OrganizationName}", organization.Name);
        return filePath;
    }

    public async Task<byte[]?> GetLogoAsync(int organizationId)
    {
        var organization = await GetByIdAsync(organizationId);
        if (organization?.Logo == null)
        {
            return null;
        }

        return await _fileStorageService.GetFileAsync(organization.Logo);
    }

    public async Task DeleteLogoAsync(int organizationId)
    {
        var organization = await GetByIdAsync(organizationId);
        if (organization?.Logo == null)
        {
            return;
        }

        await _fileStorageService.DeleteFileAsync(organization.Logo);

        organization.Logo = null;
        organization.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Organizations.UpdateAsync(organization);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Logo deleted for organization: {OrganizationName}", organization.Name);
    }

    #endregion

    #region Configuration Management

    public async Task UpdateConfigurationAsync(int id, OrganizationConfigurationDto dto)
    {
        var organization = await GetByIdAsync(id);
        if (organization == null)
        {
            throw new ArgumentException("Organization not found");
        }

        organization.NormalWorkingHours = TimeSpan.FromHours(dto.NormalWorkingHours);
        organization.OvertimeRate = dto.OvertimeRate;
        organization.ProductiveHoursThreshold = dto.ProductiveHoursThreshold;
        organization.BranchIsolationEnabled = dto.BranchIsolationEnabled;
        organization.ConfigurationSettings = JsonSerializer.Serialize(dto.AdditionalSettings);
        organization.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Organizations.UpdateAsync(organization);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Configuration updated for organization: {OrganizationName}", organization.Name);
    }

    public async Task<OrganizationConfigurationDto?> GetConfigurationAsync(int id)
    {
        var organization = await GetByIdAsync(id);
        if (organization == null)
        {
            return null;
        }

        var additionalSettings = new Dictionary<string, object>();
        try
        {
            if (!string.IsNullOrEmpty(organization.ConfigurationSettings))
            {
                additionalSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(organization.ConfigurationSettings) 
                                   ?? new Dictionary<string, object>();
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize configuration settings for organization {OrganizationId}", id);
        }

        return new OrganizationConfigurationDto
        {
            NormalWorkingHours = (int)organization.NormalWorkingHours.TotalHours,
            OvertimeRate = organization.OvertimeRate,
            ProductiveHoursThreshold = organization.ProductiveHoursThreshold,
            BranchIsolationEnabled = organization.BranchIsolationEnabled,
            AdditionalSettings = additionalSettings
        };
    }

    #endregion

    #region Validation

    public async Task<bool> ValidateOrganizationDataAsync(CreateOrganizationDto dto)
    {
        // Check if organization name already exists
        var existingOrganization = await _unitOfWork.Organizations.FirstOrDefaultAsync(o => o.Name == dto.Name);
        if (existingOrganization != null)
        {
            return false;
        }

        // Check if email already exists
        var existingEmail = await _unitOfWork.Organizations.FirstOrDefaultAsync(o => o.Email == dto.Email);
        if (existingEmail != null)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> ValidateOrganizationUpdateAsync(int id, UpdateOrganizationDto dto)
    {
        // Check if organization name already exists for another organization
        var existingOrganization = await _unitOfWork.Organizations.FirstOrDefaultAsync(
            o => o.Name == dto.Name && o.Id != id);
        if (existingOrganization != null)
        {
            return false;
        }

        // Check if email already exists for another organization
        var existingEmail = await _unitOfWork.Organizations.FirstOrDefaultAsync(
            o => o.Email == dto.Email && o.Id != id);
        if (existingEmail != null)
        {
            return false;
        }

        return true;
    }

    #endregion

    #region Private Helper Methods

    private static OrganizationDto MapToOrganizationDto(Organization organization, int employeeCount)
    {
        return new OrganizationDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Address = organization.Address,
            Email = organization.Email,
            Phone = organization.Phone,
            Logo = organization.Logo,
            Website = organization.Website,
            TaxId = organization.TaxId,
            RegistrationNumber = organization.RegistrationNumber,
            NormalWorkingHours = (int)organization.NormalWorkingHours.TotalHours,
            OvertimeRate = organization.OvertimeRate,
            ProductiveHoursThreshold = organization.ProductiveHoursThreshold,
            BranchIsolationEnabled = organization.BranchIsolationEnabled,
            ConfigurationSettings = organization.ConfigurationSettings,
            BranchCount = organization.Branches.Count,
            EmployeeCount = employeeCount,
            CreatedAt = organization.CreatedAt,
            UpdatedAt = organization.UpdatedAt
        };
    }

    #endregion
}
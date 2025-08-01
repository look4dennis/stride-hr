using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using System.Text.Json;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// Service implementation for organization management operations
/// </summary>
public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ILogger<OrganizationService> _logger;
    private readonly IAuditService _auditService;

    public OrganizationService(
        IOrganizationRepository organizationRepository,
        ILogger<OrganizationService> logger,
        IAuditService auditService)
    {
        _organizationRepository = organizationRepository;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<Organization> CreateOrganizationAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new organization: {Name}", request.Name);

        // Validate name uniqueness
        if (!await _organizationRepository.IsNameUniqueAsync(request.Name, null, cancellationToken))
        {
            throw new InvalidOperationException($"Organization name '{request.Name}' is already in use");
        }

        var organization = new Organization
        {
            Name = request.Name,
            Address = request.Address,
            Email = request.Email,
            Phone = request.Phone,
            NormalWorkingHours = request.NormalWorkingHours,
            OvertimeRate = request.OvertimeRate,
            ProductiveHoursThreshold = request.ProductiveHoursThreshold,
            BranchIsolationEnabled = request.BranchIsolationEnabled,
            Settings = request.Settings != null ? JsonSerializer.Serialize(request.Settings) : null,
            CreatedAt = DateTime.UtcNow
        };

        var createdOrganization = await _organizationRepository.AddAsync(organization, cancellationToken);
        await _organizationRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogDataModificationAsync(null, "Organization", createdOrganization.Id, "CREATE", 
            null, $"Organization '{request.Name}' created");

        _logger.LogInformation("Organization created successfully: {Name}", request.Name);
        return createdOrganization;
    }

    public async Task<Organization?> GetOrganizationByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _organizationRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Organization> UpdateOrganizationAsync(int id, UpdateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating organization: {Id}", id);

        var organization = await _organizationRepository.GetByIdAsync(id, cancellationToken);
        if (organization == null)
        {
            throw new ArgumentException($"Organization with ID {id} not found");
        }

        // Validate name uniqueness if name is being changed
        if (!string.IsNullOrEmpty(request.Name) && request.Name != organization.Name)
        {
            if (!await _organizationRepository.IsNameUniqueAsync(request.Name, id, cancellationToken))
            {
                throw new InvalidOperationException($"Organization name '{request.Name}' is already in use");
            }
        }

        // Update fields that are provided
        if (!string.IsNullOrEmpty(request.Name)) organization.Name = request.Name;
        if (request.Address != null) organization.Address = request.Address;
        if (request.Email != null) organization.Email = request.Email;
        if (request.Phone != null) organization.Phone = request.Phone;
        if (request.NormalWorkingHours.HasValue) organization.NormalWorkingHours = request.NormalWorkingHours.Value;
        if (request.OvertimeRate.HasValue) organization.OvertimeRate = request.OvertimeRate.Value;
        if (request.ProductiveHoursThreshold.HasValue) organization.ProductiveHoursThreshold = request.ProductiveHoursThreshold.Value;
        if (request.BranchIsolationEnabled.HasValue) organization.BranchIsolationEnabled = request.BranchIsolationEnabled.Value;
        if (request.Settings != null) organization.Settings = JsonSerializer.Serialize(request.Settings);

        organization.UpdatedAt = DateTime.UtcNow;

        var updatedOrganization = await _organizationRepository.UpdateAsync(organization, cancellationToken);
        await _organizationRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogDataModificationAsync(null, "Organization", id, "UPDATE", 
            null, $"Organization '{organization.Name}' updated");

        _logger.LogInformation("Organization updated successfully: {Name}", organization.Name);
        return updatedOrganization;
    }

    public async Task<bool> DeleteOrganizationAsync(int id, string? deletedBy = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting organization: {Id}", id);

        var organization = await _organizationRepository.GetByIdAsync(id, cancellationToken);
        if (organization == null)
        {
            return false;
        }

        await _organizationRepository.SoftDeleteAsync(id, deletedBy, cancellationToken);
        await _organizationRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogDataModificationAsync(null, "Organization", id, "DELETE", 
            null, $"Organization '{organization.Name}' deleted");

        _logger.LogInformation("Organization deleted successfully: {Name}", organization.Name);
        return true;
    }

    public async Task<IEnumerable<Organization>> GetAllOrganizationsAsync(CancellationToken cancellationToken = default)
    {
        return await _organizationRepository.GetAllAsync(cancellationToken);
    }

    public async Task<string> UploadLogoAsync(int organizationId, Stream logoStream, string fileName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading logo for organization: {OrganizationId}", organizationId);

        var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);
        if (organization == null)
        {
            throw new ArgumentException($"Organization with ID {organizationId} not found");
        }

        // Create uploads directory if it doesn't exist
        var uploadsPath = Path.Combine("uploads", "organization-logos");
        Directory.CreateDirectory(uploadsPath);

        // Generate unique filename
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"org_{organizationId}_{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsPath, uniqueFileName);

        // Save file
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await logoStream.CopyToAsync(fileStream, cancellationToken);
        }

        // Delete old logo if exists
        if (!string.IsNullOrEmpty(organization.LogoPath) && File.Exists(organization.LogoPath))
        {
            File.Delete(organization.LogoPath);
        }

        // Update organization record
        organization.LogoPath = filePath;
        organization.UpdatedAt = DateTime.UtcNow;

        await _organizationRepository.UpdateAsync(organization, cancellationToken);
        await _organizationRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogDataModificationAsync(null, "Organization", organizationId, "UPDATE", 
            null, $"Logo uploaded for organization '{organization.Name}'");

        _logger.LogInformation("Logo uploaded successfully for organization: {Name}", organization.Name);
        return filePath;
    }

    public async Task<bool> DeleteLogoAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);
        if (organization == null || string.IsNullOrEmpty(organization.LogoPath))
        {
            return false;
        }

        // Delete file if exists
        if (File.Exists(organization.LogoPath))
        {
            File.Delete(organization.LogoPath);
        }

        // Update organization record
        organization.LogoPath = null;
        organization.UpdatedAt = DateTime.UtcNow;

        await _organizationRepository.UpdateAsync(organization, cancellationToken);
        await _organizationRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogDataModificationAsync(null, "Organization", organizationId, "UPDATE", 
            null, $"Logo deleted for organization '{organization.Name}'");

        return true;
    }

    public async Task<Organization> UpdateConfigurationAsync(int organizationId, OrganizationConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating configuration for organization: {OrganizationId}", organizationId);

        var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);
        if (organization == null)
        {
            throw new ArgumentException($"Organization with ID {organizationId} not found");
        }

        // Update configuration fields
        if (request.NormalWorkingHours.HasValue) organization.NormalWorkingHours = request.NormalWorkingHours.Value;
        if (request.OvertimeRate.HasValue) organization.OvertimeRate = request.OvertimeRate.Value;
        if (request.ProductiveHoursThreshold.HasValue) organization.ProductiveHoursThreshold = request.ProductiveHoursThreshold.Value;
        if (request.BranchIsolationEnabled.HasValue) organization.BranchIsolationEnabled = request.BranchIsolationEnabled.Value;

        // Merge custom settings
        if (request.CustomSettings != null)
        {
            var existingSettings = string.IsNullOrEmpty(organization.Settings) 
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(organization.Settings) ?? new Dictionary<string, object>();

            foreach (var setting in request.CustomSettings)
            {
                existingSettings[setting.Key] = setting.Value;
            }

            organization.Settings = JsonSerializer.Serialize(existingSettings);
        }

        organization.UpdatedAt = DateTime.UtcNow;

        var updatedOrganization = await _organizationRepository.UpdateAsync(organization, cancellationToken);
        await _organizationRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogDataModificationAsync(null, "Organization", organizationId, "CONFIG_UPDATE", 
            null, $"Configuration updated for organization '{organization.Name}'");

        _logger.LogInformation("Configuration updated successfully for organization: {Name}", organization.Name);
        return updatedOrganization;
    }

    public async Task<Dictionary<string, object>> GetConfigurationAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);
        if (organization == null)
        {
            throw new ArgumentException($"Organization with ID {organizationId} not found");
        }

        var configuration = new Dictionary<string, object>
        {
            ["NormalWorkingHours"] = organization.NormalWorkingHours,
            ["OvertimeRate"] = organization.OvertimeRate,
            ["ProductiveHoursThreshold"] = organization.ProductiveHoursThreshold,
            ["BranchIsolationEnabled"] = organization.BranchIsolationEnabled
        };

        // Add custom settings if they exist
        if (!string.IsNullOrEmpty(organization.Settings))
        {
            var customSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(organization.Settings);
            if (customSettings != null)
            {
                foreach (var setting in customSettings)
                {
                    configuration[setting.Key] = setting.Value;
                }
            }
        }

        return configuration;
    }
}
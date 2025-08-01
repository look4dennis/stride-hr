using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.DTOs.Organization;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using System.Text.Json;

namespace StrideHR.API.Controllers;

/// <summary>
/// Controller for organization management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationService _organizationService;
    private readonly ILogger<OrganizationController> _logger;

    public OrganizationController(IOrganizationService organizationService, ILogger<OrganizationController> logger)
    {
        _organizationService = organizationService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new organization
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrganizationDto>> CreateOrganization([FromBody] CreateOrganizationDto dto)
    {
        try
        {
            var request = new CreateOrganizationRequest
            {
                Name = dto.Name,
                Address = dto.Address,
                Email = dto.Email,
                Phone = dto.Phone,
                NormalWorkingHours = dto.NormalWorkingHours,
                OvertimeRate = dto.OvertimeRate,
                ProductiveHoursThreshold = dto.ProductiveHoursThreshold,
                BranchIsolationEnabled = dto.BranchIsolationEnabled,
                Settings = dto.Settings
            };

            var organization = await _organizationService.CreateOrganizationAsync(request);
            var responseDto = MapToDto(organization);

            return CreatedAtAction(nameof(GetOrganization), new { id = organization.Id }, responseDto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organization");
            return StatusCode(500, new { message = "An error occurred while creating the organization" });
        }
    }

    /// <summary>
    /// Get organization by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrganizationDto>> GetOrganization(int id)
    {
        try
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(id);
            if (organization == null)
            {
                return NotFound(new { message = $"Organization with ID {id} not found" });
            }

            var responseDto = MapToDto(organization);
            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organization {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the organization" });
        }
    }

    /// <summary>
    /// Update organization
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<OrganizationDto>> UpdateOrganization(int id, [FromBody] UpdateOrganizationDto dto)
    {
        try
        {
            var request = new UpdateOrganizationRequest
            {
                Name = dto.Name,
                Address = dto.Address,
                Email = dto.Email,
                Phone = dto.Phone,
                NormalWorkingHours = dto.NormalWorkingHours,
                OvertimeRate = dto.OvertimeRate,
                ProductiveHoursThreshold = dto.ProductiveHoursThreshold,
                BranchIsolationEnabled = dto.BranchIsolationEnabled,
                Settings = dto.Settings
            };

            var organization = await _organizationService.UpdateOrganizationAsync(id, request);
            var responseDto = MapToDto(organization);

            return Ok(responseDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the organization" });
        }
    }

    /// <summary>
    /// Delete organization (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteOrganization(int id)
    {
        try
        {
            var result = await _organizationService.DeleteOrganizationAsync(id, User.Identity?.Name);
            if (!result)
            {
                return NotFound(new { message = $"Organization with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organization {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the organization" });
        }
    }

    /// <summary>
    /// Get all organizations
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetAllOrganizations()
    {
        try
        {
            var organizations = await _organizationService.GetAllOrganizationsAsync();
            var organizationDtos = organizations.Select(MapToDto);

            return Ok(organizationDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organizations");
            return StatusCode(500, new { message = "An error occurred while retrieving organizations" });
        }
    }

    /// <summary>
    /// Upload organization logo
    /// </summary>
    [HttpPost("{id}/logo")]
    public async Task<ActionResult<LogoUploadResponseDto>> UploadLogo(int id, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file provided" });
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new { message = "Invalid file type. Only JPEG, PNG, and GIF files are allowed." });
            }

            // Validate file size (5MB max)
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { message = "File size cannot exceed 5MB" });
            }

            using var stream = file.OpenReadStream();
            var filePath = await _organizationService.UploadLogoAsync(id, stream, file.FileName);

            var response = new LogoUploadResponseDto
            {
                FilePath = filePath,
                Message = "Logo uploaded successfully"
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading logo for organization {Id}", id);
            return StatusCode(500, new { message = "An error occurred while uploading the logo" });
        }
    }

    /// <summary>
    /// Delete organization logo
    /// </summary>
    [HttpDelete("{id}/logo")]
    public async Task<ActionResult> DeleteLogo(int id)
    {
        try
        {
            var result = await _organizationService.DeleteLogoAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Organization not found or no logo to delete" });
            }

            return Ok(new { message = "Logo deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting logo for organization {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the logo" });
        }
    }

    /// <summary>
    /// Update organization configuration
    /// </summary>
    [HttpPut("{id}/configuration")]
    public async Task<ActionResult<OrganizationDto>> UpdateConfiguration(int id, [FromBody] OrganizationConfigurationDto dto)
    {
        try
        {
            var request = new OrganizationConfigurationRequest
            {
                NormalWorkingHours = dto.NormalWorkingHours,
                OvertimeRate = dto.OvertimeRate,
                ProductiveHoursThreshold = dto.ProductiveHoursThreshold,
                BranchIsolationEnabled = dto.BranchIsolationEnabled,
                CustomSettings = dto.CustomSettings
            };

            var organization = await _organizationService.UpdateConfigurationAsync(id, request);
            var responseDto = MapToDto(organization);

            return Ok(responseDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration for organization {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the configuration" });
        }
    }

    /// <summary>
    /// Get organization configuration
    /// </summary>
    [HttpGet("{id}/configuration")]
    public async Task<ActionResult<Dictionary<string, object>>> GetConfiguration(int id)
    {
        try
        {
            var configuration = await _organizationService.GetConfigurationAsync(id);
            return Ok(configuration);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration for organization {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the configuration" });
        }
    }

    private static OrganizationDto MapToDto(Organization organization)
    {
        Dictionary<string, object>? settings = null;
        if (!string.IsNullOrEmpty(organization.Settings))
        {
            try
            {
                settings = JsonSerializer.Deserialize<Dictionary<string, object>>(organization.Settings);
            }
            catch
            {
                // If deserialization fails, leave settings as null
            }
        }

        return new OrganizationDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Address = organization.Address,
            Email = organization.Email,
            Phone = organization.Phone,
            LogoPath = organization.LogoPath,
            NormalWorkingHours = organization.NormalWorkingHours,
            OvertimeRate = organization.OvertimeRate,
            ProductiveHoursThreshold = organization.ProductiveHoursThreshold,
            BranchIsolationEnabled = organization.BranchIsolationEnabled,
            Settings = settings,
            CreatedAt = organization.CreatedAt,
            UpdatedAt = organization.UpdatedAt,
            Branches = organization.Branches?.Select(MapBranchToDto).ToList()
        };
    }

    private static BranchDto MapBranchToDto(Branch branch)
    {
        List<string>? localHolidays = null;
        if (!string.IsNullOrEmpty(branch.LocalHolidays))
        {
            try
            {
                localHolidays = JsonSerializer.Deserialize<List<string>>(branch.LocalHolidays);
            }
            catch
            {
                // If deserialization fails, leave as null
            }
        }

        Dictionary<string, object>? complianceSettings = null;
        if (!string.IsNullOrEmpty(branch.ComplianceSettings))
        {
            try
            {
                complianceSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(branch.ComplianceSettings);
            }
            catch
            {
                // If deserialization fails, leave as null
            }
        }

        return new BranchDto
        {
            Id = branch.Id,
            OrganizationId = branch.OrganizationId,
            OrganizationName = branch.Organization?.Name ?? string.Empty,
            Name = branch.Name,
            Country = branch.Country,
            Currency = branch.Currency,
            TimeZone = branch.TimeZone,
            Address = branch.Address,
            City = branch.City,
            State = branch.State,
            PostalCode = branch.PostalCode,
            Phone = branch.Phone,
            Email = branch.Email,
            LocalHolidays = localHolidays,
            ComplianceSettings = complianceSettings,
            EmployeeIdPattern = branch.EmployeeIdPattern,
            WorkingHoursStart = branch.WorkingHoursStart,
            WorkingHoursEnd = branch.WorkingHoursEnd,
            IsActive = branch.IsActive,
            CreatedAt = branch.CreatedAt,
            UpdatedAt = branch.UpdatedAt,
            EmployeeCount = branch.Employees?.Count ?? 0
        };
    }
}
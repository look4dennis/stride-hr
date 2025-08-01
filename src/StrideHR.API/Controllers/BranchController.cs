using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.DTOs.Organization;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using System.Text.Json;

namespace StrideHR.API.Controllers;

/// <summary>
/// Controller for branch management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchController : ControllerBase
{
    private readonly IBranchService _branchService;
    private readonly ILogger<BranchController> _logger;

    public BranchController(IBranchService branchService, ILogger<BranchController> logger)
    {
        _branchService = branchService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new branch
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BranchDto>> CreateBranch([FromBody] CreateBranchDto dto)
    {
        try
        {
            var request = new CreateBranchRequest
            {
                OrganizationId = dto.OrganizationId,
                Name = dto.Name,
                Country = dto.Country,
                Currency = dto.Currency,
                TimeZone = dto.TimeZone,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Phone = dto.Phone,
                Email = dto.Email,
                LocalHolidays = dto.LocalHolidays,
                ComplianceSettings = dto.ComplianceSettings,
                EmployeeIdPattern = dto.EmployeeIdPattern,
                WorkingHoursStart = dto.WorkingHoursStart,
                WorkingHoursEnd = dto.WorkingHoursEnd
            };

            var branch = await _branchService.CreateBranchAsync(request);
            var responseDto = MapToDto(branch);

            return CreatedAtAction(nameof(GetBranch), new { id = branch.Id }, responseDto);
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
            _logger.LogError(ex, "Error creating branch");
            return StatusCode(500, new { message = "An error occurred while creating the branch" });
        }
    }

    /// <summary>
    /// Get branch by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BranchDto>> GetBranch(int id)
    {
        try
        {
            var branch = await _branchService.GetBranchByIdAsync(id);
            if (branch == null)
            {
                return NotFound(new { message = $"Branch with ID {id} not found" });
            }

            var responseDto = MapToDto(branch);
            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branch {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the branch" });
        }
    }

    /// <summary>
    /// Update branch
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<BranchDto>> UpdateBranch(int id, [FromBody] UpdateBranchDto dto)
    {
        try
        {
            var request = new UpdateBranchRequest
            {
                Name = dto.Name,
                Country = dto.Country,
                Currency = dto.Currency,
                TimeZone = dto.TimeZone,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Phone = dto.Phone,
                Email = dto.Email,
                LocalHolidays = dto.LocalHolidays,
                ComplianceSettings = dto.ComplianceSettings,
                EmployeeIdPattern = dto.EmployeeIdPattern,
                WorkingHoursStart = dto.WorkingHoursStart,
                WorkingHoursEnd = dto.WorkingHoursEnd,
                IsActive = dto.IsActive
            };

            var branch = await _branchService.UpdateBranchAsync(id, request);
            var responseDto = MapToDto(branch);

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
            _logger.LogError(ex, "Error updating branch {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the branch" });
        }
    }

    /// <summary>
    /// Delete branch (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBranch(int id)
    {
        try
        {
            var result = await _branchService.DeleteBranchAsync(id, User.Identity?.Name);
            if (!result)
            {
                return NotFound(new { message = $"Branch with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting branch {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the branch" });
        }
    }

    /// <summary>
    /// Get branches by organization
    /// </summary>
    [HttpGet("organization/{organizationId}")]
    public async Task<ActionResult<IEnumerable<BranchDto>>> GetBranchesByOrganization(int organizationId)
    {
        try
        {
            var branches = await _branchService.GetBranchesByOrganizationAsync(organizationId);
            var branchDtos = branches.Select(MapToDto);

            return Ok(branchDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branches for organization {OrganizationId}", organizationId);
            return StatusCode(500, new { message = "An error occurred while retrieving branches" });
        }
    }

    /// <summary>
    /// Get active branches
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<BranchDto>>> GetActiveBranches([FromQuery] int? organizationId = null)
    {
        try
        {
            var branches = await _branchService.GetActiveBranchesAsync(organizationId);
            var branchDtos = branches.Select(MapToDto);

            return Ok(branchDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active branches");
            return StatusCode(500, new { message = "An error occurred while retrieving active branches" });
        }
    }

    /// <summary>
    /// Get branches by country
    /// </summary>
    [HttpGet("country/{country}")]
    public async Task<ActionResult<IEnumerable<BranchDto>>> GetBranchesByCountry(string country)
    {
        try
        {
            var branches = await _branchService.GetBranchesByCountryAsync(country);
            var branchDtos = branches.Select(MapToDto);

            return Ok(branchDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branches for country {Country}", country);
            return StatusCode(500, new { message = "An error occurred while retrieving branches" });
        }
    }

    /// <summary>
    /// Get supported countries
    /// </summary>
    [HttpGet("supported-countries")]
    public async Task<ActionResult<IEnumerable<string>>> GetSupportedCountries()
    {
        try
        {
            var countries = await _branchService.GetSupportedCountriesAsync();
            return Ok(countries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported countries");
            return StatusCode(500, new { message = "An error occurred while retrieving supported countries" });
        }
    }

    /// <summary>
    /// Get supported currencies
    /// </summary>
    [HttpGet("supported-currencies")]
    public async Task<ActionResult<IEnumerable<string>>> GetSupportedCurrencies()
    {
        try
        {
            var currencies = await _branchService.GetSupportedCurrenciesAsync();
            return Ok(currencies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported currencies");
            return StatusCode(500, new { message = "An error occurred while retrieving supported currencies" });
        }
    }

    /// <summary>
    /// Get supported time zones
    /// </summary>
    [HttpGet("supported-timezones")]
    public async Task<ActionResult<IEnumerable<string>>> GetSupportedTimeZones()
    {
        try
        {
            var timeZones = await _branchService.GetSupportedTimeZonesAsync();
            return Ok(timeZones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported time zones");
            return StatusCode(500, new { message = "An error occurred while retrieving supported time zones" });
        }
    }

    /// <summary>
    /// Convert currency
    /// </summary>
    [HttpPost("convert-currency")]
    public async Task<ActionResult<CurrencyConversionResponseDto>> ConvertCurrency([FromBody] CurrencyConversionDto dto)
    {
        try
        {
            var convertedAmount = await _branchService.ConvertCurrencyAsync(dto.Amount, dto.FromCurrency, dto.ToCurrency);

            var response = new CurrencyConversionResponseDto
            {
                OriginalAmount = dto.Amount,
                FromCurrency = dto.FromCurrency,
                ConvertedAmount = convertedAmount,
                ToCurrency = dto.ToCurrency,
                ConversionDate = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting currency");
            return StatusCode(500, new { message = "An error occurred while converting currency" });
        }
    }

    /// <summary>
    /// Update compliance settings
    /// </summary>
    [HttpPut("{id}/compliance")]
    public async Task<ActionResult<BranchDto>> UpdateComplianceSettings(int id, [FromBody] ComplianceSettingsDto dto)
    {
        try
        {
            var request = new ComplianceSettingsRequest
            {
                TaxSettings = dto.TaxSettings,
                LaborLawSettings = dto.LaborLawSettings,
                StatutorySettings = dto.StatutorySettings,
                ReportingSettings = dto.ReportingSettings,
                RequiredDocuments = dto.RequiredDocuments,
                CustomCompliance = dto.CustomCompliance
            };

            var branch = await _branchService.UpdateComplianceSettingsAsync(id, request);
            var responseDto = MapToDto(branch);

            return Ok(responseDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating compliance settings for branch {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating compliance settings" });
        }
    }

    /// <summary>
    /// Get compliance settings
    /// </summary>
    [HttpGet("{id}/compliance")]
    public async Task<ActionResult<Dictionary<string, object>>> GetComplianceSettings(int id)
    {
        try
        {
            var settings = await _branchService.GetComplianceSettingsAsync(id);
            return Ok(settings);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving compliance settings for branch {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving compliance settings" });
        }
    }

    /// <summary>
    /// Validate branch access for user
    /// </summary>
    [HttpGet("{id}/validate-access")]
    public async Task<ActionResult<bool>> ValidateBranchAccess(int id)
    {
        try
        {
            var userId = User.Identity?.Name ?? string.Empty;
            var hasAccess = await _branchService.ValidateBranchAccessAsync(id, userId);
            return Ok(new { hasAccess });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating branch access for branch {Id}", id);
            return StatusCode(500, new { message = "An error occurred while validating branch access" });
        }
    }

    /// <summary>
    /// Get user accessible branches
    /// </summary>
    [HttpGet("accessible")]
    public async Task<ActionResult<IEnumerable<int>>> GetUserAccessibleBranches()
    {
        try
        {
            var userId = User.Identity?.Name ?? string.Empty;
            var accessibleBranches = await _branchService.GetUserAccessibleBranchesAsync(userId);
            return Ok(accessibleBranches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accessible branches");
            return StatusCode(500, new { message = "An error occurred while retrieving accessible branches" });
        }
    }

    private static BranchDto MapToDto(Branch branch)
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
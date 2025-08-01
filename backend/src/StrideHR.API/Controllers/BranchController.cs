using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Models;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Branch;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchController : ControllerBase
{
    private readonly IBranchService _branchService;
    private readonly ILogger<BranchController> _logger;

    public BranchController(IBranchService branchService, ILogger<BranchController> logger)
    {
        _branchService = branchService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<BranchDto>>>> GetAllBranches()
    {
        try
        {
            var branches = await _branchService.GetBranchDtosAsync();
            return Ok(new ApiResponse<IEnumerable<BranchDto>>
            {
                Success = true,
                Data = branches,
                Message = "Branches retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branches");
            return StatusCode(500, new ApiResponse<IEnumerable<BranchDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving branches"
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BranchDto>>> GetBranch(int id)
    {
        try
        {
            var branch = await _branchService.GetBranchDtoAsync(id);
            if (branch == null)
            {
                return NotFound(new ApiResponse<BranchDto>
                {
                    Success = false,
                    Message = "Branch not found"
                });
            }

            return Ok(new ApiResponse<BranchDto>
            {
                Success = true,
                Data = branch,
                Message = "Branch retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branch {Id}", id);
            return StatusCode(500, new ApiResponse<BranchDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the branch"
            });
        }
    }

    [HttpGet("organization/{organizationId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BranchDto>>>> GetBranchesByOrganization(int organizationId)
    {
        try
        {
            var branches = await _branchService.GetBranchDtosByOrganizationAsync(organizationId);
            return Ok(new ApiResponse<IEnumerable<BranchDto>>
            {
                Success = true,
                Data = branches,
                Message = "Branches retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branches for organization {OrganizationId}", organizationId);
            return StatusCode(500, new ApiResponse<IEnumerable<BranchDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving branches"
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BranchDto>>> CreateBranch([FromBody] CreateBranchDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<BranchDto>
                {
                    Success = false,
                    Message = "Invalid branch data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var branch = await _branchService.CreateAsync(dto);
            var branchDto = await _branchService.GetBranchDtoAsync(branch.Id);

            return CreatedAtAction(nameof(GetBranch), new { id = branch.Id }, new ApiResponse<BranchDto>
            {
                Success = true,
                Data = branchDto,
                Message = "Branch created successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<BranchDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch");
            return StatusCode(500, new ApiResponse<BranchDto>
            {
                Success = false,
                Message = "An error occurred while creating the branch"
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<BranchDto>>> UpdateBranch(int id, [FromBody] UpdateBranchDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<BranchDto>
                {
                    Success = false,
                    Message = "Invalid branch data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            await _branchService.UpdateAsync(id, dto);
            var branchDto = await _branchService.GetBranchDtoAsync(id);

            return Ok(new ApiResponse<BranchDto>
            {
                Success = true,
                Data = branchDto,
                Message = "Branch updated successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<BranchDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch {Id}", id);
            return StatusCode(500, new ApiResponse<BranchDto>
            {
                Success = false,
                Message = "An error occurred while updating the branch"
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteBranch(int id)
    {
        try
        {
            var exists = await _branchService.ExistsAsync(id);
            if (!exists)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Branch not found"
                });
            }

            await _branchService.DeleteAsync(id);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Branch deleted successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting branch {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the branch"
            });
        }
    }

    [HttpGet("supported-countries")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetSupportedCountries()
    {
        try
        {
            var countries = await _branchService.GetSupportedCountriesAsync();
            return Ok(new ApiResponse<IEnumerable<string>>
            {
                Success = true,
                Data = countries,
                Message = "Supported countries retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported countries");
            return StatusCode(500, new ApiResponse<IEnumerable<string>>
            {
                Success = false,
                Message = "An error occurred while retrieving supported countries"
            });
        }
    }

    [HttpGet("supported-currencies")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetSupportedCurrencies()
    {
        try
        {
            var currencies = await _branchService.GetSupportedCurrenciesAsync();
            return Ok(new ApiResponse<IEnumerable<string>>
            {
                Success = true,
                Data = currencies,
                Message = "Supported currencies retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported currencies");
            return StatusCode(500, new ApiResponse<IEnumerable<string>>
            {
                Success = false,
                Message = "An error occurred while retrieving supported currencies"
            });
        }
    }

    [HttpGet("supported-timezones")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetSupportedTimeZones()
    {
        try
        {
            var timeZones = await _branchService.GetSupportedTimeZonesAsync();
            return Ok(new ApiResponse<IEnumerable<string>>
            {
                Success = true,
                Data = timeZones,
                Message = "Supported time zones retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported time zones");
            return StatusCode(500, new ApiResponse<IEnumerable<string>>
            {
                Success = false,
                Message = "An error occurred while retrieving supported time zones"
            });
        }
    }

    [HttpPost("convert-currency")]
    public async Task<ActionResult<ApiResponse<decimal>>> ConvertCurrency([FromBody] CurrencyConversionRequest request)
    {
        try
        {
            var convertedAmount = await _branchService.ConvertCurrencyAsync(
                request.Amount, request.FromCurrency, request.ToCurrency);

            return Ok(new ApiResponse<decimal>
            {
                Success = true,
                Data = convertedAmount,
                Message = "Currency converted successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<decimal>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting currency");
            return StatusCode(500, new ApiResponse<decimal>
            {
                Success = false,
                Message = "An error occurred while converting currency"
            });
        }
    }

    [HttpGet("{id}/compliance")]
    public async Task<ActionResult<ApiResponse<BranchComplianceDto>>> GetComplianceSettings(int id)
    {
        try
        {
            var compliance = await _branchService.GetComplianceSettingsAsync(id);
            if (compliance == null)
            {
                return NotFound(new ApiResponse<BranchComplianceDto>
                {
                    Success = false,
                    Message = "Branch not found"
                });
            }

            return Ok(new ApiResponse<BranchComplianceDto>
            {
                Success = true,
                Data = compliance,
                Message = "Compliance settings retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving compliance settings for branch {Id}", id);
            return StatusCode(500, new ApiResponse<BranchComplianceDto>
            {
                Success = false,
                Message = "An error occurred while retrieving compliance settings"
            });
        }
    }

    [HttpPut("{id}/compliance")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateComplianceSettings(int id, [FromBody] BranchComplianceDto dto)
    {
        try
        {
            await _branchService.UpdateComplianceSettingsAsync(id, dto);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Compliance settings updated successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating compliance settings for branch {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating compliance settings"
            });
        }
    }

    [HttpGet("{id}/holidays")]
    public async Task<ActionResult<ApiResponse<List<LocalHolidayDto>>>> GetLocalHolidays(int id)
    {
        try
        {
            var holidays = await _branchService.GetLocalHolidaysAsync(id);
            return Ok(new ApiResponse<List<LocalHolidayDto>>
            {
                Success = true,
                Data = holidays,
                Message = "Local holidays retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving local holidays for branch {Id}", id);
            return StatusCode(500, new ApiResponse<List<LocalHolidayDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving local holidays"
            });
        }
    }

    [HttpPut("{id}/holidays")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateLocalHolidays(int id, [FromBody] List<LocalHolidayDto> holidays)
    {
        try
        {
            await _branchService.UpdateLocalHolidaysAsync(id, holidays);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Local holidays updated successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating local holidays for branch {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating local holidays"
            });
        }
    }
}

public class CurrencyConversionRequest
{
    public decimal Amount { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
}
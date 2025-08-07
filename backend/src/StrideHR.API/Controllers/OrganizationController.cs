using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Models;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Organization;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationService _organizationService;
    private readonly ILogger<OrganizationController> _logger;

    public OrganizationController(IOrganizationService organizationService, ILogger<OrganizationController> logger)
    {
        _organizationService = organizationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrganizationDto>>>> GetAllOrganizations()
    {
        try
        {
            var organizations = await _organizationService.GetOrganizationDtosAsync();
            return Ok(new ApiResponse<IEnumerable<OrganizationDto>>
            {
                Success = true,
                Data = organizations,
                Message = "Organizations retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organizations");
            return StatusCode(500, new ApiResponse<IEnumerable<OrganizationDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving organizations"
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrganizationDto>>> GetOrganization(int id)
    {
        try
        {
            var organization = await _organizationService.GetOrganizationDtoAsync(id);
            if (organization == null)
            {
                return NotFound(new ApiResponse<OrganizationDto>
                {
                    Success = false,
                    Message = "Organization not found"
                });
            }

            return Ok(new ApiResponse<OrganizationDto>
            {
                Success = true,
                Data = organization,
                Message = "Organization retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organization {Id}", id);
            return StatusCode(500, new ApiResponse<OrganizationDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the organization"
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrganizationDto>>> CreateOrganization([FromBody] CreateOrganizationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<OrganizationDto>
                {
                    Success = false,
                    Message = "Invalid organization data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var organization = await _organizationService.CreateAsync(dto);
            var organizationDto = await _organizationService.GetOrganizationDtoAsync(organization.Id);

            return CreatedAtAction(nameof(GetOrganization), new { id = organization.Id }, new ApiResponse<OrganizationDto>
            {
                Success = true,
                Data = organizationDto,
                Message = "Organization created successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<OrganizationDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organization");
            return StatusCode(500, new ApiResponse<OrganizationDto>
            {
                Success = false,
                Message = "An error occurred while creating the organization"
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<OrganizationDto>>> UpdateOrganization(int id, [FromBody] UpdateOrganizationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<OrganizationDto>
                {
                    Success = false,
                    Message = "Invalid organization data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            await _organizationService.UpdateAsync(id, dto);
            var organizationDto = await _organizationService.GetOrganizationDtoAsync(id);

            return Ok(new ApiResponse<OrganizationDto>
            {
                Success = true,
                Data = organizationDto,
                Message = "Organization updated successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<OrganizationDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization {Id}", id);
            return StatusCode(500, new ApiResponse<OrganizationDto>
            {
                Success = false,
                Message = "An error occurred while updating the organization"
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteOrganization(int id)
    {
        try
        {
            var exists = await _organizationService.ExistsAsync(id);
            if (!exists)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Organization not found"
                });
            }

            await _organizationService.DeleteAsync(id);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Organization deleted successfully"
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
            _logger.LogError(ex, "Error deleting organization {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the organization"
            });
        }
    }

    [HttpPost("{id}/logo")]
    [ApiExplorerSettings(IgnoreApi = true)] // Temporarily exclude from Swagger due to IFormFile issue
    public async Task<ActionResult<ApiResponse<string>>> UploadLogo(int id, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No file provided"
                });
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid file type. Only JPEG, PNG, and GIF files are allowed."
                });
            }

            // Validate file size (10MB max)
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "File size exceeds 10MB limit"
                });
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var dto = new OrganizationLogoUploadDto
            {
                OrganizationId = id,
                LogoData = memoryStream.ToArray(),
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length
            };

            var filePath = await _organizationService.UploadLogoAsync(dto);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Data = filePath,
                Message = "Logo uploaded successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading logo for organization {Id}", id);
            return StatusCode(500, new ApiResponse<string>
            {
                Success = false,
                Message = "An error occurred while uploading the logo"
            });
        }
    }

    [HttpGet("{id}/logo")]
    public async Task<IActionResult> GetLogo(int id)
    {
        try
        {
            var logoData = await _organizationService.GetLogoAsync(id);
            if (logoData == null)
            {
                return NotFound();
            }

            return File(logoData, "image/jpeg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logo for organization {Id}", id);
            return StatusCode(500);
        }
    }

    [HttpDelete("{id}/logo")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteLogo(int id)
    {
        try
        {
            await _organizationService.DeleteLogoAsync(id);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Logo deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting logo for organization {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the logo"
            });
        }
    }

    [HttpGet("{id}/configuration")]
    public async Task<ActionResult<ApiResponse<OrganizationConfigurationDto>>> GetConfiguration(int id)
    {
        try
        {
            var configuration = await _organizationService.GetConfigurationAsync(id);
            if (configuration == null)
            {
                return NotFound(new ApiResponse<OrganizationConfigurationDto>
                {
                    Success = false,
                    Message = "Organization not found"
                });
            }

            return Ok(new ApiResponse<OrganizationConfigurationDto>
            {
                Success = true,
                Data = configuration,
                Message = "Configuration retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration for organization {Id}", id);
            return StatusCode(500, new ApiResponse<OrganizationConfigurationDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the configuration"
            });
        }
    }

    [HttpPut("{id}/configuration")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateConfiguration(int id, [FromBody] OrganizationConfigurationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid configuration data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            await _organizationService.UpdateConfigurationAsync(id, dto);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Configuration updated successfully"
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
            _logger.LogError(ex, "Error updating configuration for organization {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating the configuration"
            });
        }
    }
}
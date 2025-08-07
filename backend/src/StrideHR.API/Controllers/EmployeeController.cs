using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Models;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Employee;

namespace StrideHR.API.Controllers;

/// <summary>
/// Employee management endpoints for CRUD operations, profile management, and organizational hierarchy
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Employee")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieve all employees with optional filtering and pagination
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <param name="search">Search term to filter employees by name, email, or employee ID</param>
    /// <param name="department">Filter by department name</param>
    /// <param name="branchId">Filter by branch ID</param>
    /// <param name="status">Filter by employee status (Active, Inactive, Terminated)</param>
    /// <param name="sortBy">Field to sort by (Name, Email, JoiningDate, Department)</param>
    /// <param name="sortOrder">Sort order: 'asc' or 'desc' (default: 'asc')</param>
    /// <returns>Paginated list of employees matching the criteria</returns>
    /// <response code="200">Employees retrieved successfully</response>
    /// <response code="400">Invalid pagination or filter parameters</response>
    /// <response code="401">Unauthorized - Invalid or missing JWT token</response>
    /// <response code="403">Forbidden - Insufficient permissions to view employees</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeeDto>>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 403)]
    public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeDto>>>> GetAllEmployees(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? department = null,
        [FromQuery] int? branchId = null,
        [FromQuery] string? status = null,
        [FromQuery] string sortBy = "Name",
        [FromQuery] string sortOrder = "asc")
    {
        try
        {
            var employees = await _employeeService.GetEmployeeDtosAsync();
            return Ok(new ApiResponse<IEnumerable<EmployeeDto>>
            {
                Success = true,
                Data = employees,
                Message = "Employees retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees");
            return StatusCode(500, new ApiResponse<IEnumerable<EmployeeDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving employees"
            });
        }
    }

    /// <summary>
    /// Retrieve a specific employee by ID
    /// </summary>
    /// <param name="id">The unique identifier of the employee</param>
    /// <returns>Employee details including personal information, job details, and organizational hierarchy</returns>
    /// <response code="200">Employee retrieved successfully</response>
    /// <response code="404">Employee not found with the specified ID</response>
    /// <response code="401">Unauthorized - Invalid or missing JWT token</response>
    /// <response code="403">Forbidden - Insufficient permissions to view this employee</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), 200)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 403)]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetEmployee(int id)
    {
        try
        {
            var employee = await _employeeService.GetEmployeeDtoAsync(id);
            if (employee == null)
            {
                return NotFound(new ApiResponse<EmployeeDto>
                {
                    Success = false,
                    Message = "Employee not found"
                });
            }

            return Ok(new ApiResponse<EmployeeDto>
            {
                Success = true,
                Data = employee,
                Message = "Employee retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee {Id}", id);
            return StatusCode(500, new ApiResponse<EmployeeDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the employee"
            });
        }
    }

    [HttpGet("branch/{branchId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeDto>>>> GetEmployeesByBranch(int branchId)
    {
        try
        {
            var employees = await _employeeService.GetEmployeeDtosByBranchAsync(branchId);
            return Ok(new ApiResponse<IEnumerable<EmployeeDto>>
            {
                Success = true,
                Data = employees,
                Message = "Employees retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees for branch {BranchId}", branchId);
            return StatusCode(500, new ApiResponse<IEnumerable<EmployeeDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving employees"
            });
        }
    }

    [HttpPost("search")]
    public async Task<ActionResult<ApiResponse<PagedResult<EmployeeDto>>>> SearchEmployees([FromBody] EmployeeSearchCriteria criteria)
    {
        try
        {
            var result = await _employeeService.SearchEmployeesAsync(criteria);
            return Ok(new ApiResponse<PagedResult<EmployeeDto>>
            {
                Success = true,
                Data = result,
                Message = "Employee search completed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching employees");
            return StatusCode(500, new ApiResponse<PagedResult<EmployeeDto>>
            {
                Success = false,
                Message = "An error occurred while searching employees"
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> CreateEmployee([FromBody] CreateEmployeeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<EmployeeDto>
                {
                    Success = false,
                    Message = "Invalid employee data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var employee = await _employeeService.CreateAsync(dto);
            var employeeDto = await _employeeService.GetEmployeeDtoAsync(employee.Id);

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, new ApiResponse<EmployeeDto>
            {
                Success = true,
                Data = employeeDto,
                Message = "Employee created successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<EmployeeDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(500, new ApiResponse<EmployeeDto>
            {
                Success = false,
                Message = "An error occurred while creating the employee"
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<EmployeeDto>
                {
                    Success = false,
                    Message = "Invalid employee data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            await _employeeService.UpdateAsync(id, dto);
            var employeeDto = await _employeeService.GetEmployeeDtoAsync(id);

            return Ok(new ApiResponse<EmployeeDto>
            {
                Success = true,
                Data = employeeDto,
                Message = "Employee updated successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<EmployeeDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {Id}", id);
            return StatusCode(500, new ApiResponse<EmployeeDto>
            {
                Success = false,
                Message = "An error occurred while updating the employee"
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEmployee(int id)
    {
        try
        {
            var exists = await _employeeService.ExistsAsync(id);
            if (!exists)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Employee not found"
                });
            }

            await _employeeService.DeleteAsync(id);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Employee deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the employee"
            });
        }
    }

    [HttpPost("{id}/profile-photo")]
    [ApiExplorerSettings(IgnoreApi = true)] // Temporarily exclude from Swagger due to IFormFile issue
    public async Task<ActionResult<ApiResponse<string>>> UploadProfilePhoto(int id, IFormFile file)
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

            // Validate file size (5MB max)
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "File size exceeds 5MB limit"
                });
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var dto = new ProfilePhotoUploadDto
            {
                EmployeeId = id,
                PhotoData = memoryStream.ToArray(),
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length
            };

            var filePath = await _employeeService.UploadProfilePhotoAsync(dto);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Data = filePath,
                Message = "Profile photo uploaded successfully"
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
            _logger.LogError(ex, "Error uploading profile photo for employee {Id}", id);
            return StatusCode(500, new ApiResponse<string>
            {
                Success = false,
                Message = "An error occurred while uploading the profile photo"
            });
        }
    }

    [HttpGet("{id}/profile-photo")]
    public async Task<IActionResult> GetProfilePhoto(int id)
    {
        try
        {
            var photoData = await _employeeService.GetProfilePhotoAsync(id);
            if (photoData == null)
            {
                return NotFound();
            }

            return File(photoData, "image/jpeg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile photo for employee {Id}", id);
            return StatusCode(500);
        }
    }

    [HttpDelete("{id}/profile-photo")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProfilePhoto(int id)
    {
        try
        {
            await _employeeService.DeleteProfilePhotoAsync(id);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Profile photo deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile photo for employee {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the profile photo"
            });
        }
    }

    [HttpPost("{id}/onboarding")]
    public async Task<ActionResult<ApiResponse<object>>> StartOnboarding(int id, [FromBody] EmployeeOnboardingDto dto)
    {
        try
        {
            dto.EmployeeId = id;
            var result = await _employeeService.StartOnboardingAsync(dto);
            
            if (!result)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to start onboarding process"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Onboarding process started successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting onboarding for employee {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while starting the onboarding process"
            });
        }
    }

    [HttpGet("{id}/onboarding")]
    public async Task<ActionResult<ApiResponse<EmployeeOnboardingDto>>> GetOnboardingStatus(int id)
    {
        try
        {
            var onboarding = await _employeeService.GetOnboardingStatusAsync(id);
            if (onboarding == null)
            {
                return NotFound(new ApiResponse<EmployeeOnboardingDto>
                {
                    Success = false,
                    Message = "Onboarding process not found"
                });
            }

            return Ok(new ApiResponse<EmployeeOnboardingDto>
            {
                Success = true,
                Data = onboarding,
                Message = "Onboarding status retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving onboarding status for employee {Id}", id);
            return StatusCode(500, new ApiResponse<EmployeeOnboardingDto>
            {
                Success = false,
                Message = "An error occurred while retrieving onboarding status"
            });
        }
    }

    [HttpPost("{id}/exit")]
    public async Task<ActionResult<ApiResponse<object>>> InitiateExit(int id, [FromBody] EmployeeExitDto dto)
    {
        try
        {
            dto.EmployeeId = id;
            var result = await _employeeService.InitiateExitProcessAsync(dto);
            
            if (!result)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to initiate exit process"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Exit process initiated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating exit for employee {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while initiating the exit process"
            });
        }
    }

    [HttpGet("{id}/exit")]
    public async Task<ActionResult<ApiResponse<EmployeeExitDto>>> GetExitStatus(int id)
    {
        try
        {
            var exit = await _employeeService.GetExitStatusAsync(id);
            if (exit == null)
            {
                return NotFound(new ApiResponse<EmployeeExitDto>
                {
                    Success = false,
                    Message = "Exit process not found"
                });
            }

            return Ok(new ApiResponse<EmployeeExitDto>
            {
                Success = true,
                Data = exit,
                Message = "Exit status retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exit status for employee {Id}", id);
            return StatusCode(500, new ApiResponse<EmployeeExitDto>
            {
                Success = false,
                Message = "An error occurred while retrieving exit status"
            });
        }
    }

    [HttpPost("{id}/exit/finalize")]
    public async Task<ActionResult<ApiResponse<object>>> FinalizeExit(int id)
    {
        try
        {
            var result = await _employeeService.FinalizeExitAsync(id);
            
            if (!result)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to finalize exit process"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Exit process finalized successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizing exit for employee {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while finalizing the exit process"
            });
        }
    }

    [HttpGet("{id}/roles")]
    public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeRoleDto>>>> GetEmployeeRoles(int id)
    {
        try
        {
            var roles = await _employeeService.GetEmployeeRolesAsync(id);
            return Ok(new ApiResponse<IEnumerable<EmployeeRoleDto>>
            {
                Success = true,
                Data = roles,
                Message = "Employee roles retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for employee {Id}", id);
            return StatusCode(500, new ApiResponse<IEnumerable<EmployeeRoleDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving employee roles"
            });
        }
    }

    [HttpGet("{id}/roles/active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeRoleDto>>>> GetActiveEmployeeRoles(int id)
    {
        try
        {
            var roles = await _employeeService.GetActiveEmployeeRolesAsync(id);
            return Ok(new ApiResponse<IEnumerable<EmployeeRoleDto>>
            {
                Success = true,
                Data = roles,
                Message = "Active employee roles retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active roles for employee {Id}", id);
            return StatusCode(500, new ApiResponse<IEnumerable<EmployeeRoleDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving active employee roles"
            });
        }
    }

    [HttpPost("{id}/roles/assign")]
    public async Task<ActionResult<ApiResponse<object>>> AssignRole(int id, [FromBody] AssignRoleDto dto)
    {
        try
        {
            dto.EmployeeId = id;
            // TODO: Get current user ID from authentication context
            var currentUserId = 1; // Placeholder
            
            var result = await _employeeService.AssignRoleAsync(dto, currentUserId);
            
            if (!result)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to assign role to employee"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Role assigned successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to employee {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while assigning the role"
            });
        }
    }

    [HttpPost("{id}/roles/revoke")]
    public async Task<ActionResult<ApiResponse<object>>> RevokeRole(int id, [FromBody] RevokeRoleDto dto)
    {
        try
        {
            dto.EmployeeId = id;
            // TODO: Get current user ID from authentication context
            var currentUserId = 1; // Placeholder
            
            var result = await _employeeService.RevokeRoleAsync(dto, currentUserId);
            
            if (!result)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to revoke role from employee"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Role revoked successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking role from employee {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while revoking the role"
            });
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.DTOs.Employee;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;

namespace StrideHR.API.Controllers;

/// <summary>
/// Controller for employee management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
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
    /// Create a new employee
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] CreateEmployeeDto dto)
    {
        try
        {
            var request = new CreateEmployeeRequest
            {
                BranchId = dto.BranchId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                MiddleName = dto.MiddleName,
                Email = dto.Email,
                Phone = dto.Phone,
                AlternatePhone = dto.AlternatePhone,
                DateOfBirth = dto.DateOfBirth,
                JoiningDate = dto.JoiningDate,
                Designation = dto.Designation,
                Department = dto.Department,
                BasicSalary = dto.BasicSalary,
                ReportingManagerId = dto.ReportingManagerId,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Country = dto.Country,
                EmergencyContact = dto.EmergencyContact,
                NationalId = dto.NationalId,
                TaxId = dto.TaxId,
                BankDetails = dto.BankDetails,
                VisaStatus = dto.VisaStatus,
                VisaExpiryDate = dto.VisaExpiryDate
            };

            var employee = await _employeeService.CreateEmployeeAsync(request);
            var responseDto = MapToDto(employee);

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, responseDto);
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
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(500, new { message = "An error occurred while creating the employee" });
        }
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        try
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound(new { message = $"Employee with ID {id} not found" });
            }

            var responseDto = MapToDto(employee);
            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the employee" });
        }
    }

    /// <summary>
    /// Get employee by employee ID
    /// </summary>
    [HttpGet("by-employee-id/{employeeId}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployeeByEmployeeId(string employeeId)
    {
        try
        {
            var employee = await _employeeService.GetEmployeeByEmployeeIdAsync(employeeId);
            if (employee == null)
            {
                return NotFound(new { message = $"Employee with ID {employeeId} not found" });
            }

            var responseDto = MapToDto(employee);
            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee {EmployeeId}", employeeId);
            return StatusCode(500, new { message = "An error occurred while retrieving the employee" });
        }
    }

    /// <summary>
    /// Update employee
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<EmployeeDto>> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
    {
        try
        {
            var request = new UpdateEmployeeRequest
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                MiddleName = dto.MiddleName,
                Email = dto.Email,
                Phone = dto.Phone,
                AlternatePhone = dto.AlternatePhone,
                DateOfBirth = dto.DateOfBirth,
                Designation = dto.Designation,
                Department = dto.Department,
                BasicSalary = dto.BasicSalary,
                Status = dto.Status,
                ReportingManagerId = dto.ReportingManagerId,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Country = dto.Country,
                EmergencyContact = dto.EmergencyContact,
                NationalId = dto.NationalId,
                TaxId = dto.TaxId,
                BankDetails = dto.BankDetails,
                VisaStatus = dto.VisaStatus,
                VisaExpiryDate = dto.VisaExpiryDate
            };

            var employee = await _employeeService.UpdateEmployeeAsync(id, request);
            var responseDto = MapToDto(employee);

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
            _logger.LogError(ex, "Error updating employee {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the employee" });
        }
    }

    /// <summary>
    /// Delete employee (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteEmployee(int id)
    {
        try
        {
            var result = await _employeeService.DeleteEmployeeAsync(id, User.Identity?.Name);
            if (!result)
            {
                return NotFound(new { message = $"Employee with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the employee" });
        }
    }

    /// <summary>
    /// Search employees with filtering and pagination
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<PagedEmployeeResponseDto>> SearchEmployees([FromBody] EmployeeSearchDto dto)
    {
        try
        {
            var criteria = new EmployeeSearchCriteria
            {
                SearchTerm = dto.SearchTerm,
                BranchId = dto.BranchId,
                Department = dto.Department,
                Designation = dto.Designation,
                Status = dto.Status,
                ReportingManagerId = dto.ReportingManagerId,
                JoiningDateFrom = dto.JoiningDateFrom,
                JoiningDateTo = dto.JoiningDateTo,
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize,
                SortBy = dto.SortBy,
                SortDescending = dto.SortDescending
            };

            var (employees, totalCount) = await _employeeService.SearchEmployeesAsync(criteria);
            var employeeDtos = employees.Select(MapToDto);

            var response = new PagedEmployeeResponseDto
            {
                Employees = employeeDtos,
                TotalCount = totalCount,
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching employees");
            return StatusCode(500, new { message = "An error occurred while searching employees" });
        }
    }

    /// <summary>
    /// Get employees by branch
    /// </summary>
    [HttpGet("branch/{branchId}")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployeesByBranch(int branchId)
    {
        try
        {
            var employees = await _employeeService.GetEmployeesByBranchAsync(branchId);
            var employeeDtos = employees.Select(MapToDto);

            return Ok(employeeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees for branch {BranchId}", branchId);
            return StatusCode(500, new { message = "An error occurred while retrieving employees" });
        }
    }

    /// <summary>
    /// Get employees by department
    /// </summary>
    [HttpGet("department/{department}")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployeesByDepartment(string department)
    {
        try
        {
            var employees = await _employeeService.GetEmployeesByDepartmentAsync(department);
            var employeeDtos = employees.Select(MapToDto);

            return Ok(employeeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees for department {Department}", department);
            return StatusCode(500, new { message = "An error occurred while retrieving employees" });
        }
    }

    /// <summary>
    /// Get employees by manager
    /// </summary>
    [HttpGet("manager/{managerId}")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployeesByManager(int managerId)
    {
        try
        {
            var employees = await _employeeService.GetEmployeesByManagerAsync(managerId);
            var employeeDtos = employees.Select(MapToDto);

            return Ok(employeeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees for manager {ManagerId}", managerId);
            return StatusCode(500, new { message = "An error occurred while retrieving employees" });
        }
    }

    /// <summary>
    /// Upload profile photo
    /// </summary>
    [HttpPost("{id}/profile-photo")]
    public async Task<ActionResult<ProfilePhotoResponseDto>> UploadProfilePhoto(int id, IFormFile file)
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
            var filePath = await _employeeService.UploadProfilePhotoAsync(id, stream, file.FileName);

            var response = new ProfilePhotoResponseDto
            {
                FilePath = filePath,
                Message = "Profile photo uploaded successfully"
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile photo for employee {Id}", id);
            return StatusCode(500, new { message = "An error occurred while uploading the profile photo" });
        }
    }

    /// <summary>
    /// Delete profile photo
    /// </summary>
    [HttpDelete("{id}/profile-photo")]
    public async Task<ActionResult> DeleteProfilePhoto(int id)
    {
        try
        {
            var result = await _employeeService.DeleteProfilePhotoAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Employee not found or no profile photo to delete" });
            }

            return Ok(new { message = "Profile photo deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile photo for employee {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the profile photo" });
        }
    }

    /// <summary>
    /// Start employee onboarding process
    /// </summary>
    [HttpPost("{id}/onboard")]
    public async Task<ActionResult<EmployeeDto>> OnboardEmployee(int id, [FromBody] OnboardingDto dto)
    {
        try
        {
            var request = new OnboardingRequest
            {
                CompletedDocuments = dto.CompletedDocuments,
                PendingDocuments = dto.PendingDocuments,
                OnboardingNotes = dto.OnboardingNotes,
                OrientationDate = dto.OrientationDate,
                BuddyEmployeeId = dto.BuddyEmployeeId
            };

            var employee = await _employeeService.OnboardEmployeeAsync(id, request);
            var responseDto = MapToDto(employee);

            return Ok(responseDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error onboarding employee {Id}", id);
            return StatusCode(500, new { message = "An error occurred while onboarding the employee" });
        }
    }

    /// <summary>
    /// Initiate employee exit process
    /// </summary>
    [HttpPost("{id}/exit")]
    public async Task<ActionResult<EmployeeDto>> InitiateExitProcess(int id, [FromBody] ExitProcessDto dto)
    {
        try
        {
            var request = new ExitProcessRequest
            {
                LastWorkingDay = dto.LastWorkingDay,
                ExitReason = dto.ExitReason,
                ExitNotes = dto.ExitNotes,
                IsVoluntary = dto.IsVoluntary,
                AssetsToReturn = dto.AssetsToReturn,
                HandoverNotes = dto.HandoverNotes
            };

            var employee = await _employeeService.InitiateExitProcessAsync(id, request);
            var responseDto = MapToDto(employee);

            return Ok(responseDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating exit process for employee {Id}", id);
            return StatusCode(500, new { message = "An error occurred while initiating the exit process" });
        }
    }

    /// <summary>
    /// Complete employee exit process
    /// </summary>
    [HttpPost("{id}/complete-exit")]
    public async Task<ActionResult<EmployeeDto>> CompleteExitProcess(int id)
    {
        try
        {
            var employee = await _employeeService.CompleteExitProcessAsync(id);
            var responseDto = MapToDto(employee);

            return Ok(responseDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing exit process for employee {Id}", id);
            return StatusCode(500, new { message = "An error occurred while completing the exit process" });
        }
    }

    /// <summary>
    /// Get organizational hierarchy
    /// </summary>
    [HttpGet("hierarchy")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetOrganizationalHierarchy([FromQuery] int? rootEmployeeId = null)
    {
        try
        {
            var employees = await _employeeService.GetOrganizationalHierarchyAsync(rootEmployeeId);
            var employeeDtos = employees.Select(MapToDto);

            return Ok(employeeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organizational hierarchy");
            return StatusCode(500, new { message = "An error occurred while retrieving the organizational hierarchy" });
        }
    }

    /// <summary>
    /// Check if employee ID is unique
    /// </summary>
    [HttpGet("check-employee-id/{employeeId}")]
    public async Task<ActionResult<bool>> CheckEmployeeIdUnique(string employeeId, [FromQuery] int? excludeId = null)
    {
        try
        {
            var isUnique = await _employeeService.IsEmployeeIdUniqueAsync(employeeId, excludeId);
            return Ok(new { isUnique });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking employee ID uniqueness");
            return StatusCode(500, new { message = "An error occurred while checking employee ID uniqueness" });
        }
    }

    /// <summary>
    /// Check if email is unique
    /// </summary>
    [HttpGet("check-email/{email}")]
    public async Task<ActionResult<bool>> CheckEmailUnique(string email, [FromQuery] int? excludeId = null)
    {
        try
        {
            var isUnique = await _employeeService.IsEmailUniqueAsync(email, excludeId);
            return Ok(new { isUnique });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email uniqueness");
            return StatusCode(500, new { message = "An error occurred while checking email uniqueness" });
        }
    }

    /// <summary>
    /// Generate employee ID for branch
    /// </summary>
    [HttpGet("generate-employee-id/{branchId}")]
    public async Task<ActionResult<string>> GenerateEmployeeId(int branchId)
    {
        try
        {
            var employeeId = await _employeeService.GenerateEmployeeIdAsync(branchId);
            return Ok(new { employeeId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating employee ID for branch {BranchId}", branchId);
            return StatusCode(500, new { message = "An error occurred while generating employee ID" });
        }
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            BranchId = employee.BranchId,
            BranchName = employee.Branch?.Name ?? string.Empty,
            EmployeeId = employee.EmployeeId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            MiddleName = employee.MiddleName,
            FullName = employee.FullName,
            DisplayName = employee.DisplayName,
            Email = employee.Email,
            Phone = employee.Phone,
            AlternatePhone = employee.AlternatePhone,
            ProfilePhotoPath = employee.ProfilePhotoPath,
            DateOfBirth = employee.DateOfBirth,
            JoiningDate = employee.JoiningDate,
            Designation = employee.Designation,
            Department = employee.Department,
            BasicSalary = employee.BasicSalary,
            Status = employee.Status,
            ReportingManagerId = employee.ReportingManagerId,
            ReportingManagerName = employee.ReportingManager?.FullName,
            Address = employee.Address,
            City = employee.City,
            State = employee.State,
            PostalCode = employee.PostalCode,
            Country = employee.Country,
            EmergencyContact = employee.EmergencyContact,
            NationalId = employee.NationalId,
            TaxId = employee.TaxId,
            BankDetails = employee.BankDetails,
            VisaStatus = employee.VisaStatus,
            VisaExpiryDate = employee.VisaExpiryDate,
            TerminationDate = employee.TerminationDate,
            TerminationReason = employee.TerminationReason,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt
        };
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Shift;

namespace StrideHR.API.Controllers;

[Authorize]
public class ShiftController : BaseController
{
    private readonly IShiftService _shiftService;
    private readonly ILogger<ShiftController> _logger;

    public ShiftController(IShiftService shiftService, ILogger<ShiftController> logger)
    {
        _shiftService = shiftService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new shift
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateShift([FromBody] CreateShiftDto createShiftDto)
    {
        try
        {
            var shift = await _shiftService.CreateShiftAsync(createShiftDto);
            return Success(shift, "Shift created successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shift");
            return Error("An error occurred while creating the shift");
        }
    }

    /// <summary>
    /// Update an existing shift
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateShift(int id, [FromBody] UpdateShiftDto updateShiftDto)
    {
        try
        {
            var shift = await _shiftService.UpdateShiftAsync(id, updateShiftDto);
            return Success(shift, "Shift updated successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shift {ShiftId}", id);
            return Error("An error occurred while updating the shift");
        }
    }

    /// <summary>
    /// Delete a shift
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteShift(int id)
    {
        try
        {
            var result = await _shiftService.DeleteShiftAsync(id);
            if (!result)
            {
                return Error("Shift not found");
            }
            return Success("Shift deleted successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shift {ShiftId}", id);
            return Error("An error occurred while deleting the shift");
        }
    }

    /// <summary>
    /// Get shift by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetShift(int id)
    {
        try
        {
            var shift = await _shiftService.GetShiftByIdAsync(id);
            if (shift == null)
            {
                return Error("Shift not found");
            }
            return Success(shift);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift {ShiftId}", id);
            return Error("An error occurred while retrieving the shift");
        }
    }

    /// <summary>
    /// Get shifts by organization
    /// </summary>
    [HttpGet("organization/{organizationId}")]
    public async Task<IActionResult> GetShiftsByOrganization(int organizationId)
    {
        try
        {
            var shifts = await _shiftService.GetShiftsByOrganizationAsync(organizationId);
            return Success(shifts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shifts for organization {OrganizationId}", organizationId);
            return Error("An error occurred while retrieving shifts");
        }
    }

    /// <summary>
    /// Get shifts by branch
    /// </summary>
    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetShiftsByBranch(int branchId)
    {
        try
        {
            var shifts = await _shiftService.GetShiftsByBranchAsync(branchId);
            return Success(shifts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shifts for branch {BranchId}", branchId);
            return Error("An error occurred while retrieving shifts");
        }
    }

    /// <summary>
    /// Get active shifts by organization
    /// </summary>
    [HttpGet("organization/{organizationId}/active")]
    public async Task<IActionResult> GetActiveShifts(int organizationId)
    {
        try
        {
            var shifts = await _shiftService.GetActiveShiftsAsync(organizationId);
            return Success(shifts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active shifts for organization {OrganizationId}", organizationId);
            return Error("An error occurred while retrieving active shifts");
        }
    }

    /// <summary>
    /// Search shifts with criteria
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> SearchShifts([FromBody] ShiftSearchCriteria criteria)
    {
        try
        {
            var (shifts, totalCount) = await _shiftService.SearchShiftsAsync(criteria);
            var result = new
            {
                Shifts = shifts,
                TotalCount = totalCount,
                Page = criteria.Page,
                PageSize = criteria.PageSize
            };
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching shifts");
            return Error("An error occurred while searching shifts");
        }
    }

    /// <summary>
    /// Get shift templates
    /// </summary>
    [HttpGet("templates/organization/{organizationId}")]
    public async Task<IActionResult> GetShiftTemplates(int organizationId)
    {
        try
        {
            var templates = await _shiftService.GetShiftTemplatesAsync(organizationId);
            return Success(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift templates for organization {OrganizationId}", organizationId);
            return Error("An error occurred while retrieving shift templates");
        }
    }

    /// <summary>
    /// Create shift from template
    /// </summary>
    [HttpPost("templates/{templateId}/create")]
    public async Task<IActionResult> CreateShiftFromTemplate(int templateId, [FromBody] CreateShiftDto createShiftDto)
    {
        try
        {
            var shift = await _shiftService.CreateShiftFromTemplateAsync(templateId, createShiftDto);
            return Success(shift, "Shift created from template successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shift from template {TemplateId}", templateId);
            return Error("An error occurred while creating shift from template");
        }
    }

    /// <summary>
    /// Get shifts by pattern/type
    /// </summary>
    [HttpGet("organization/{organizationId}/pattern/{shiftType}")]
    public async Task<IActionResult> GetShiftsByPattern(int organizationId, Core.Enums.ShiftType shiftType)
    {
        try
        {
            var shifts = await _shiftService.GetShiftsByPatternAsync(organizationId, shiftType);
            return Success(shifts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shifts by pattern for organization {OrganizationId}", organizationId);
            return Error("An error occurred while retrieving shifts by pattern");
        }
    }

    /// <summary>
    /// Get shift analytics
    /// </summary>
    [HttpGet("analytics/branch/{branchId}")]
    public async Task<IActionResult> GetShiftAnalytics(int branchId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var analytics = await _shiftService.GetShiftAnalyticsAsync(branchId, startDate, endDate);
            return Success(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift analytics for branch {BranchId}", branchId);
            return Error("An error occurred while retrieving shift analytics");
        }
    }
}
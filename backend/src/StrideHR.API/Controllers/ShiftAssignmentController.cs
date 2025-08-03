using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Shift;

namespace StrideHR.API.Controllers;

[Authorize]
public class ShiftAssignmentController : BaseController
{
    private readonly IShiftService _shiftService;
    private readonly ILogger<ShiftAssignmentController> _logger;

    public ShiftAssignmentController(IShiftService shiftService, ILogger<ShiftAssignmentController> logger)
    {
        _shiftService = shiftService;
        _logger = logger;
    }

    /// <summary>
    /// Assign employee to shift
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AssignEmployeeToShift([FromBody] CreateShiftAssignmentDto assignmentDto)
    {
        try
        {
            var assignment = await _shiftService.AssignEmployeeToShiftAsync(assignmentDto);
            return Success(assignment, "Employee assigned to shift successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning employee {EmployeeId} to shift {ShiftId}", 
                assignmentDto.EmployeeId, assignmentDto.ShiftId);
            return Error("An error occurred while assigning employee to shift");
        }
    }

    /// <summary>
    /// Bulk assign employees to shift
    /// </summary>
    [HttpPost("bulk")]
    public async Task<IActionResult> BulkAssignEmployeesToShift([FromBody] BulkShiftAssignmentDto bulkAssignmentDto)
    {
        try
        {
            var assignments = await _shiftService.BulkAssignEmployeesToShiftAsync(bulkAssignmentDto);
            return Success(assignments, $"{assignments.Count()} employees assigned to shift successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk assigning employees to shift {ShiftId}", bulkAssignmentDto.ShiftId);
            return Error("An error occurred while bulk assigning employees to shift");
        }
    }

    /// <summary>
    /// Update shift assignment
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateShiftAssignment(int id, [FromBody] UpdateShiftAssignmentDto updateDto)
    {
        try
        {
            var assignment = await _shiftService.UpdateShiftAssignmentAsync(id, updateDto);
            return Success(assignment, "Shift assignment updated successfully");
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
            _logger.LogError(ex, "Error updating shift assignment {AssignmentId}", id);
            return Error("An error occurred while updating shift assignment");
        }
    }

    /// <summary>
    /// Remove employee from shift
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveEmployeeFromShift(int id)
    {
        try
        {
            var result = await _shiftService.RemoveEmployeeFromShiftAsync(id);
            if (!result)
            {
                return Error("Shift assignment not found");
            }
            return Success("Employee removed from shift successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing employee from shift assignment {AssignmentId}", id);
            return Error("An error occurred while removing employee from shift");
        }
    }

    /// <summary>
    /// Get employee shift assignments
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeeShiftAssignments(int employeeId)
    {
        try
        {
            var assignments = await _shiftService.GetEmployeeShiftAssignmentsAsync(employeeId);
            return Success(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift assignments for employee {EmployeeId}", employeeId);
            return Error("An error occurred while retrieving employee shift assignments");
        }
    }

    /// <summary>
    /// Get shift assignments
    /// </summary>
    [HttpGet("shift/{shiftId}")]
    public async Task<IActionResult> GetShiftAssignments(int shiftId)
    {
        try
        {
            var assignments = await _shiftService.GetShiftAssignmentsAsync(shiftId);
            return Success(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments for shift {ShiftId}", shiftId);
            return Error("An error occurred while retrieving shift assignments");
        }
    }

    /// <summary>
    /// Get current employee shift for a specific date
    /// </summary>
    [HttpGet("employee/{employeeId}/current")]
    public async Task<IActionResult> GetCurrentEmployeeShift(int employeeId, [FromQuery] DateTime? date = null)
    {
        try
        {
            var currentDate = date ?? DateTime.Today;
            var assignment = await _shiftService.GetCurrentEmployeeShiftAsync(employeeId, currentDate);
            return Success(assignment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current shift for employee {EmployeeId}", employeeId);
            return Error("An error occurred while retrieving current employee shift");
        }
    }

    /// <summary>
    /// Get upcoming shift assignments for employee
    /// </summary>
    [HttpGet("employee/{employeeId}/upcoming")]
    public async Task<IActionResult> GetUpcomingShiftAssignments(int employeeId, [FromQuery] int days = 7)
    {
        try
        {
            var assignments = await _shiftService.GetUpcomingShiftAssignmentsAsync(employeeId, days);
            return Success(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming shift assignments for employee {EmployeeId}", employeeId);
            return Error("An error occurred while retrieving upcoming shift assignments");
        }
    }

    /// <summary>
    /// Get shift coverage for a branch on a specific date
    /// </summary>
    [HttpGet("coverage/branch/{branchId}")]
    public async Task<IActionResult> GetShiftCoverage(int branchId, [FromQuery] DateTime? date = null)
    {
        try
        {
            var currentDate = date ?? DateTime.Today;
            var coverage = await _shiftService.GetShiftCoverageAsync(branchId, currentDate);
            return Success(coverage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift coverage for branch {BranchId}", branchId);
            return Error("An error occurred while retrieving shift coverage");
        }
    }

    /// <summary>
    /// Detect shift conflicts for an employee
    /// </summary>
    [HttpPost("conflicts/detect")]
    public async Task<IActionResult> DetectShiftConflicts([FromBody] CreateShiftAssignmentDto assignmentDto)
    {
        try
        {
            var conflicts = await _shiftService.DetectShiftConflictsAsync(
                assignmentDto.EmployeeId, 
                assignmentDto.ShiftId, 
                assignmentDto.StartDate, 
                assignmentDto.EndDate);
            return Success(conflicts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting shift conflicts for employee {EmployeeId}", assignmentDto.EmployeeId);
            return Error("An error occurred while detecting shift conflicts");
        }
    }

    /// <summary>
    /// Get all shift conflicts for a branch within a date range
    /// </summary>
    [HttpGet("conflicts/branch/{branchId}")]
    public async Task<IActionResult> GetAllShiftConflicts(int branchId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var conflicts = await _shiftService.GetAllShiftConflictsAsync(branchId, startDate, endDate);
            return Success(conflicts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift conflicts for branch {BranchId}", branchId);
            return Error("An error occurred while retrieving shift conflicts");
        }
    }

    /// <summary>
    /// Validate shift assignment
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateShiftAssignment([FromBody] CreateShiftAssignmentDto assignmentDto)
    {
        try
        {
            var isValid = await _shiftService.ValidateShiftAssignmentAsync(
                assignmentDto.EmployeeId, 
                assignmentDto.ShiftId, 
                assignmentDto.StartDate, 
                assignmentDto.EndDate);
            
            var result = new { IsValid = isValid };
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating shift assignment for employee {EmployeeId}", assignmentDto.EmployeeId);
            return Error("An error occurred while validating shift assignment");
        }
    }

    /// <summary>
    /// Generate rotating shift schedule
    /// </summary>
    [HttpPost("rotating-schedule")]
    public async Task<IActionResult> GenerateRotatingShiftSchedule([FromBody] GenerateRotatingScheduleDto scheduleDto)
    {
        try
        {
            var assignments = await _shiftService.GenerateRotatingShiftScheduleAsync(
                scheduleDto.BranchId, 
                scheduleDto.EmployeeIds, 
                scheduleDto.ShiftIds, 
                scheduleDto.StartDate, 
                scheduleDto.Weeks);
            
            return Success(assignments, $"Generated {assignments.Count()} rotating shift assignments");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating rotating shift schedule for branch {BranchId}", scheduleDto.BranchId);
            return Error("An error occurred while generating rotating shift schedule");
        }
    }
}

public class GenerateRotatingScheduleDto
{
    public int BranchId { get; set; }
    public List<int> EmployeeIds { get; set; } = new();
    public List<int> ShiftIds { get; set; } = new();
    public DateTime StartDate { get; set; }
    public int Weeks { get; set; }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Expense;
using StrideHR.Core.Enums;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TravelExpenseController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<TravelExpenseController> _logger;

    public TravelExpenseController(IExpenseService expenseService, ILogger<TravelExpenseController> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new travel expense for an expense claim
    /// </summary>
    [HttpPost("claims/{expenseClaimId}/travel")]
    public async Task<ActionResult<TravelExpenseDto>> CreateTravelExpense(int expenseClaimId, [FromBody] CreateTravelExpenseDto dto)
    {
        try
        {
            var travelExpense = await _expenseService.CreateTravelExpenseAsync(expenseClaimId, dto);
            return CreatedAtAction(nameof(GetTravelExpense), new { id = travelExpense.Id }, travelExpense);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating travel expense for claim {ExpenseClaimId}", expenseClaimId);
            return StatusCode(500, "An error occurred while creating the travel expense");
        }
    }

    /// <summary>
    /// Update an existing travel expense
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TravelExpenseDto>> UpdateTravelExpense(int id, [FromBody] CreateTravelExpenseDto dto)
    {
        try
        {
            var travelExpense = await _expenseService.UpdateTravelExpenseAsync(id, dto);
            return Ok(travelExpense);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating travel expense {Id}", id);
            return StatusCode(500, "An error occurred while updating the travel expense");
        }
    }

    /// <summary>
    /// Get a travel expense by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TravelExpenseDto>> GetTravelExpense(int id)
    {
        try
        {
            var travelExpense = await _expenseService.GetTravelExpenseByIdAsync(id);
            if (travelExpense == null)
                return NotFound();

            return Ok(travelExpense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving travel expense {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the travel expense");
        }
    }

    /// <summary>
    /// Get travel expense by expense claim ID
    /// </summary>
    [HttpGet("claims/{expenseClaimId}")]
    public async Task<ActionResult<TravelExpenseDto>> GetTravelExpenseByClaimId(int expenseClaimId)
    {
        try
        {
            var travelExpense = await _expenseService.GetTravelExpenseByClaimIdAsync(expenseClaimId);
            if (travelExpense == null)
                return NotFound();

            return Ok(travelExpense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving travel expense for claim {ExpenseClaimId}", expenseClaimId);
            return StatusCode(500, "An error occurred while retrieving the travel expense");
        }
    }

    /// <summary>
    /// Get travel expenses for an employee
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<TravelExpenseDto>>> GetTravelExpensesByEmployee(
        int employeeId, 
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var travelExpenses = await _expenseService.GetTravelExpensesByEmployeeAsync(employeeId, startDate, endDate);
            return Ok(travelExpenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving travel expenses for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving travel expenses");
        }
    }

    /// <summary>
    /// Delete a travel expense
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTravelExpense(int id)
    {
        try
        {
            var result = await _expenseService.DeleteTravelExpenseAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting travel expense {Id}", id);
            return StatusCode(500, "An error occurred while deleting the travel expense");
        }
    }

    /// <summary>
    /// Calculate mileage amount
    /// </summary>
    [HttpPost("mileage/calculate")]
    public async Task<ActionResult<MileageCalculationResultDto>> CalculateMileage([FromBody] MileageCalculationDto dto)
    {
        try
        {
            var result = await _expenseService.CalculateMileageAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating mileage");
            return StatusCode(500, "An error occurred while calculating mileage");
        }
    }

    /// <summary>
    /// Get mileage rate for organization and travel mode
    /// </summary>
    [HttpGet("mileage/rate")]
    public async Task<ActionResult<decimal>> GetMileageRate([FromQuery] int organizationId, [FromQuery] TravelMode travelMode)
    {
        try
        {
            var rate = await _expenseService.GetMileageRateAsync(organizationId, travelMode);
            return Ok(rate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving mileage rate for organization {OrganizationId} and travel mode {TravelMode}", organizationId, travelMode);
            return StatusCode(500, "An error occurred while retrieving the mileage rate");
        }
    }

    /// <summary>
    /// Update mileage rate for organization and travel mode
    /// </summary>
    [HttpPut("mileage/rate")]
    public async Task<ActionResult> UpdateMileageRate([FromQuery] int organizationId, [FromQuery] TravelMode travelMode, [FromQuery] decimal rate)
    {
        try
        {
            var result = await _expenseService.UpdateMileageRateAsync(organizationId, travelMode, rate);
            if (!result)
                return BadRequest("Failed to update mileage rate");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating mileage rate for organization {OrganizationId} and travel mode {TravelMode}", organizationId, travelMode);
            return StatusCode(500, "An error occurred while updating the mileage rate");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;

namespace StrideHR.API.Controllers;

public class EmployeesController : BaseController
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEmployees()
    {
        try
        {
            _logger.LogInformation("Fetching all employees");
            var employees = await _employeeService.GetAllAsync();
            return Success(employees, "Employees retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching employees");
            return Error("Failed to retrieve employees");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(int id)
    {
        try
        {
            _logger.LogInformation("Fetching employee with ID: {EmployeeId}", id);
            var employee = await _employeeService.GetByIdAsync(id);
            
            if (employee == null)
            {
                return NotFound(new { Success = false, Message = "Employee not found" });
            }

            return Success(employee, "Employee retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching employee with ID: {EmployeeId}", id);
            return Error("Failed to retrieve employee");
        }
    }

    [HttpGet("by-employee-id/{employeeId}")]
    public async Task<IActionResult> GetEmployeeByEmployeeId(string employeeId)
    {
        try
        {
            _logger.LogInformation("Fetching employee with Employee ID: {EmployeeId}", employeeId);
            var employee = await _employeeService.GetByEmployeeIdAsync(employeeId);
            
            if (employee == null)
            {
                return NotFound(new { Success = false, Message = "Employee not found" });
            }

            return Success(employee, "Employee retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching employee with Employee ID: {EmployeeId}", employeeId);
            return Error("Failed to retrieve employee");
        }
    }

    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetEmployeesByBranch(int branchId)
    {
        try
        {
            _logger.LogInformation("Fetching employees for branch ID: {BranchId}", branchId);
            var employees = await _employeeService.GetByBranchAsync(branchId);
            return Success(employees, "Branch employees retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching employees for branch ID: {BranchId}", branchId);
            return Error("Failed to retrieve branch employees");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Models;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult Success<T>(T data, string message = "Success")
    {
        var response = new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
        return Ok(response);
    }

    protected IActionResult Success(string message = "Success")
    {
        var response = new ApiResponse<object>
        {
            Success = true,
            Message = message,
            Data = null
        };
        return Ok(response);
    }

    protected IActionResult Error(string message, List<string>? errors = null)
    {
        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Data = null,
            Errors = errors ?? new List<string>()
        };
        return BadRequest(response);
    }

    protected IActionResult NotFoundError(string message, List<string>? errors = null)
    {
        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Data = null,
            Errors = errors ?? new List<string>()
        };
        return NotFound(response);
    }

    /// <summary>
    /// Gets the current employee ID from JWT claims
    /// </summary>
    /// <returns>The employee ID of the currently authenticated user</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when employee ID claim is not found or invalid</exception>
    protected int GetCurrentEmployeeId()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        
        if (string.IsNullOrEmpty(employeeIdClaim) || !int.TryParse(employeeIdClaim, out var employeeId))
        {
            throw new UnauthorizedAccessException("Employee ID not found in token claims");
        }
        
        return employeeId;
    }

    /// <summary>
    /// Gets the current user ID from JWT claims
    /// </summary>
    /// <returns>The user ID of the currently authenticated user</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user ID claim is not found or invalid</exception>
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token claims");
        }
        
        return userId;
    }
}
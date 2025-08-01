using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Models;

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
}
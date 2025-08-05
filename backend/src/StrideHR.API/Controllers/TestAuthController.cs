using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestAuthController : ControllerBase
{
    [HttpPost("simple-login")]
    [AllowAnonymous]
    public IActionResult SimpleLogin([FromBody] SimpleLoginRequest request)
    {
        // Simple test login - just check if credentials match our test user
        if (request.Email == "admin@demo.com" && request.Password == "test123")
        {
            return Ok(new
            {
                success = true,
                message = "Login successful",
                data = new
                {
                    token = "test-token-123",
                    refreshToken = "test-refresh-token-456",
                    user = new
                    {
                        id = 1,
                        employeeId = "ADMIN-001",
                        firstName = "System",
                        lastName = "Administrator",
                        email = "admin@demo.com",
                        branchId = 1,
                        roles = new[] { "SuperAdmin" },
                        profilePhoto = (string?)null
                    },
                    expiresAt = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                }
            });
        }

        return Unauthorized(new
        {
            success = false,
            message = "Invalid credentials"
        });
    }
}

public class SimpleLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
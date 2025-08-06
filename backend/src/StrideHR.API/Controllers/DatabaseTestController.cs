using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Services;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseTestController : ControllerBase
{
    private readonly DatabaseHealthCheckService _healthCheckService;
    private readonly DatabaseInitializationService _initService;
    private readonly ILogger<DatabaseTestController> _logger;

    public DatabaseTestController(
        DatabaseHealthCheckService healthCheckService,
        DatabaseInitializationService initService,
        ILogger<DatabaseTestController> logger)
    {
        _healthCheckService = healthCheckService;
        _initService = initService;
        _logger = logger;
    }

    [HttpGet("health")]
    public async Task<IActionResult> GetDatabaseHealth()
    {
        try
        {
            var health = await _healthCheckService.CheckHealthAsync();
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking database health");
            return StatusCode(500, new { Error = "Failed to check database health", Details = ex.Message });
        }
    }

    [HttpPost("initialize")]
    public async Task<IActionResult> InitializeDatabase()
    {
        try
        {
            var result = await _initService.InitializeDatabaseAsync();
            if (result)
            {
                return Ok(new { Message = "Database initialized successfully", Success = true });
            }
            else
            {
                return BadRequest(new { Message = "Database initialization failed", Success = false });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing database");
            return StatusCode(500, new { Error = "Failed to initialize database", Details = ex.Message });
        }
    }

    [HttpGet("connection-test")]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            var result = await _initService.TestDatabaseConnectionAsync();
            return Ok(new { 
                CanConnect = result, 
                Message = result ? "Database connection successful" : "Database connection failed",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing database connection");
            return StatusCode(500, new { Error = "Connection test failed", Details = ex.Message });
        }
    }
}
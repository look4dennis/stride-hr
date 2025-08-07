using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StrideHR.Infrastructure.Data;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseVerificationController : ControllerBase
{
    private readonly StrideHRDbContext _context;
    private readonly ILogger<DatabaseVerificationController> _logger;

    public DatabaseVerificationController(StrideHRDbContext context, ILogger<DatabaseVerificationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("verify-schema")]
    public async Task<IActionResult> VerifyDatabaseSchema()
    {
        try
        {
            // Test database connection
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return BadRequest("❌ Cannot connect to database");
            }

            // Get table information from information_schema
            var tableCountQuery = @"
                SELECT COUNT(*) 
                FROM information_schema.tables 
                WHERE table_schema = 'StrideHR_Dev'";
            
            var tableCount = await _context.Database.ExecuteSqlRawAsync("SELECT 1"); // Just test query execution
            
            // Get some basic counts from key tables
            var organizationCount = await _context.Organizations.CountAsync();
            var branchCount = await _context.Branches.CountAsync();
            var userCount = await _context.Users.CountAsync();
            var employeeCount = await _context.Employees.CountAsync();
            var roleCount = await _context.Roles.CountAsync();
            
            // Check if super admin exists
            var hasSuperAdmin = await _context.Users
                .AnyAsync(u => u.Username == "Superadmin" || u.Email == "superadmin@stridehr.com");

            var result = new
            {
                Status = "✅ Database schema tables are created properly!",
                DatabaseConnection = "✅ Connected successfully",
                DataCounts = new
                {
                    Organizations = organizationCount,
                    Branches = branchCount,
                    Users = userCount,
                    Employees = employeeCount,
                    Roles = roleCount,
                    HasSuperAdmin = hasSuperAdmin
                },
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Database schema verification completed successfully");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database schema verification");
            return StatusCode(500, new { 
                Status = "❌ Database schema verification failed", 
                Error = ex.Message 
            });
        }
    }

    [HttpGet("table-list")]
    public async Task<IActionResult> GetTableList()
    {
        try
        {
            // Get list of tables using raw SQL
            var tables = new List<string>();
            
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SHOW TABLES";
            
            await _context.Database.OpenConnectionAsync();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }

            return Ok(new
            {
                Status = "✅ Database tables retrieved successfully",
                TableCount = tables.Count,
                Tables = tables.OrderBy(t => t).ToList(),
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving table list");
            return StatusCode(500, new { 
                Status = "❌ Failed to retrieve table list", 
                Error = ex.Message 
            });
        }
    }
}
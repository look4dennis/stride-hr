using Microsoft.EntityFrameworkCore;
using StrideHR.Infrastructure.Data;

namespace StrideHR.API.Services;

public class DatabaseHealthCheckService
{
    private readonly StrideHRDbContext _context;
    private readonly ILogger<DatabaseHealthCheckService> _logger;

    public DatabaseHealthCheckService(StrideHRDbContext context, ILogger<DatabaseHealthCheckService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DatabaseHealthStatus> CheckHealthAsync()
    {
        var healthStatus = new DatabaseHealthStatus
        {
            IsHealthy = false,
            CheckedAt = DateTime.UtcNow,
            Details = new Dictionary<string, object>()
        };

        try
        {
            // Test basic connectivity
            var canConnect = await _context.Database.CanConnectAsync();
            healthStatus.Details["CanConnect"] = canConnect;

            if (!canConnect)
            {
                healthStatus.ErrorMessage = "Cannot connect to database";
                return healthStatus;
            }

            // Test query execution
            var startTime = DateTime.UtcNow;
            var userCount = await _context.Users.CountAsync();
            var queryTime = DateTime.UtcNow - startTime;

            healthStatus.Details["UserCount"] = userCount;
            healthStatus.Details["QueryResponseTime"] = $"{queryTime.TotalMilliseconds}ms";

            // Check if response time is acceptable (under 5 seconds)
            if (queryTime.TotalSeconds > 5)
            {
                healthStatus.ErrorMessage = "Database response time is too slow";
                healthStatus.Details["Warning"] = "Slow database response";
            }

            // Test database version
            var connectionString = _context.Database.GetConnectionString();
            healthStatus.Details["ConnectionString"] = MaskConnectionString(connectionString);

            healthStatus.IsHealthy = true;
            _logger.LogInformation("Database health check passed");
        }
        catch (Exception ex)
        {
            healthStatus.ErrorMessage = ex.Message;
            healthStatus.Details["Exception"] = ex.GetType().Name;
            _logger.LogError(ex, "Database health check failed");
        }

        return healthStatus;
    }

    private string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "Not available";

        // Mask password in connection string for security
        return System.Text.RegularExpressions.Regex.Replace(
            connectionString, 
            @"Password=([^;]+)", 
            "Password=***", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}

public class DatabaseHealthStatus
{
    public bool IsHealthy { get; set; }
    public DateTime CheckedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using StrideHR.Infrastructure.Data;
using System.Diagnostics;
using System.Text.Json;

namespace StrideHR.API.Services;

/// <summary>
/// Comprehensive health check service for monitoring all system components
/// including database connectivity, Redis cache, and external service health
/// </summary>
public class HealthCheckService
{
    private readonly StrideHRDbContext _dbContext;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<HealthCheckService> _logger;
    private readonly IConfiguration _configuration;

    public HealthCheckService(
        StrideHRDbContext dbContext,
        ILogger<HealthCheckService> logger,
        IConfiguration configuration,
        IConnectionMultiplexer? redis = null)
    {
        _dbContext = dbContext;
        _redis = redis;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> GetSystemHealthAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        var healthChecks = new List<ComponentHealthCheck>();

        try
        {
            // Check database connectivity
            var dbHealth = await CheckDatabaseHealthAsync();
            healthChecks.Add(dbHealth);

            // Check Redis cache
            var redisHealth = await CheckRedisHealthAsync();
            healthChecks.Add(redisHealth);

            // Check external services
            var externalServicesHealth = await CheckExternalServicesHealthAsync();
            healthChecks.AddRange(externalServicesHealth);

            // Check system resources
            var systemHealth = CheckSystemResourcesHealth();
            healthChecks.Add(systemHealth);

            // Check application configuration
            var configHealth = CheckConfigurationHealth();
            healthChecks.Add(configHealth);

            stopwatch.Stop();

            var overallStatus = DetermineOverallStatus(healthChecks);
            
            return new HealthCheckResult
            {
                Status = overallStatus,
                Timestamp = DateTime.UtcNow,
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Components = healthChecks,
                Version = GetApplicationVersion(),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing system health check");
            
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Timestamp = DateTime.UtcNow,
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Components = healthChecks,
                Error = ex.Message,
                Version = GetApplicationVersion(),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            };
        }
    }

    private async Task<ComponentHealthCheck> CheckDatabaseHealthAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Test basic connectivity
            await _dbContext.Database.CanConnectAsync();
            
            // Test a simple query
            var userCount = await _dbContext.Users.CountAsync();
            
            stopwatch.Stop();

            return new ComponentHealthCheck
            {
                Name = "Database",
                Status = HealthStatus.Healthy,
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Details = new Dictionary<string, object>
                {
                    ["ConnectionString"] = MaskConnectionString(_dbContext.Database.GetConnectionString()),
                    ["Provider"] = _dbContext.Database.ProviderName ?? "Unknown",
                    ["UserCount"] = userCount,
                    ["CanConnect"] = true
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Database health check failed");
            
            return new ComponentHealthCheck
            {
                Name = "Database",
                Status = HealthStatus.Unhealthy,
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Error = ex.Message,
                Details = new Dictionary<string, object>
                {
                    ["ConnectionString"] = MaskConnectionString(_dbContext.Database.GetConnectionString()),
                    ["Provider"] = _dbContext.Database.ProviderName ?? "Unknown",
                    ["CanConnect"] = false
                }
            };
        }
    }

    private async Task<ComponentHealthCheck> CheckRedisHealthAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            if (_redis == null)
            {
                return new ComponentHealthCheck
                {
                    Name = "Redis Cache",
                    Status = HealthStatus.Degraded,
                    ResponseTime = 0,
                    Details = new Dictionary<string, object>
                    {
                        ["Status"] = "Not configured",
                        ["Message"] = "Redis is not configured for this environment"
                    }
                };
            }

            var database = _redis.GetDatabase();
            
            // Test basic connectivity with a ping
            var pingResult = await database.PingAsync();
            
            // Test set/get operation
            var testKey = $"health_check_{Guid.NewGuid()}";
            var testValue = DateTime.UtcNow.ToString();
            
            await database.StringSetAsync(testKey, testValue, TimeSpan.FromSeconds(10));
            var retrievedValue = await database.StringGetAsync(testKey);
            await database.KeyDeleteAsync(testKey);
            
            stopwatch.Stop();

            var isHealthy = pingResult.TotalMilliseconds < 1000 && retrievedValue == testValue;
            
            return new ComponentHealthCheck
            {
                Name = "Redis Cache",
                Status = isHealthy ? HealthStatus.Healthy : HealthStatus.Degraded,
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Details = new Dictionary<string, object>
                {
                    ["PingTime"] = $"{pingResult.TotalMilliseconds:F2}ms",
                    ["CanReadWrite"] = retrievedValue == testValue,
                    ["ConnectionString"] = MaskConnectionString(_redis.Configuration),
                    ["IsConnected"] = _redis.IsConnected
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Redis health check failed");
            
            return new ComponentHealthCheck
            {
                Name = "Redis Cache",
                Status = HealthStatus.Unhealthy,
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Error = ex.Message,
                Details = new Dictionary<string, object>
                {
                    ["IsConnected"] = _redis?.IsConnected ?? false
                }
            };
        }
    }

    private async Task<List<ComponentHealthCheck>> CheckExternalServicesHealthAsync()
    {
        var checks = new List<ComponentHealthCheck>();
        
        // Check email service (if configured)
        var emailHealth = await CheckEmailServiceHealthAsync();
        checks.Add(emailHealth);
        
        // Check file storage service
        var fileStorageHealth = CheckFileStorageHealthAsync();
        checks.Add(fileStorageHealth);
        
        return checks;
    }

    private async Task<ComponentHealthCheck> CheckEmailServiceHealthAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = _configuration.GetValue<int>("EmailSettings:SmtpPort");
            
            if (string.IsNullOrEmpty(smtpHost))
            {
                return new ComponentHealthCheck
                {
                    Name = "Email Service",
                    Status = HealthStatus.Degraded,
                    ResponseTime = 0,
                    Details = new Dictionary<string, object>
                    {
                        ["Status"] = "Not configured",
                        ["Message"] = "Email service is not configured"
                    }
                };
            }

            // Simple TCP connection test to SMTP server
            using var tcpClient = new System.Net.Sockets.TcpClient();
            await tcpClient.ConnectAsync(smtpHost, smtpPort);
            
            stopwatch.Stop();
            
            return new ComponentHealthCheck
            {
                Name = "Email Service",
                Status = HealthStatus.Healthy,
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Details = new Dictionary<string, object>
                {
                    ["SmtpHost"] = smtpHost,
                    ["SmtpPort"] = smtpPort,
                    ["CanConnect"] = true
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(ex, "Email service health check failed");
            
            return new ComponentHealthCheck
            {
                Name = "Email Service",
                Status = HealthStatus.Degraded,
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Error = ex.Message,
                Details = new Dictionary<string, object>
                {
                    ["CanConnect"] = false
                }
            };
        }
    }

    private ComponentHealthCheck CheckFileStorageHealthAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            
            // Check if uploads directory exists and is writable
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }
            
            // Test write permissions
            var testFile = Path.Combine(uploadsPath, $"health_check_{Guid.NewGuid()}.tmp");
            File.WriteAllText(testFile, "health check test");
            File.Delete(testFile);
            
            stopwatch.Stop();
            
            return new ComponentHealthCheck
            {
                Name = "File Storage",
                Status = HealthStatus.Healthy,
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Details = new Dictionary<string, object>
                {
                    ["UploadsPath"] = uploadsPath,
                    ["CanWrite"] = true,
                    ["FreeSpace"] = GetFreeSpaceInGB(uploadsPath)
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "File storage health check failed");
            
            return new ComponentHealthCheck
            {
                Name = "File Storage",
                Status = HealthStatus.Unhealthy,
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Error = ex.Message,
                Details = new Dictionary<string, object>
                {
                    ["CanWrite"] = false
                }
            };
        }
    }

    private ComponentHealthCheck CheckSystemResourcesHealth()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64 / (1024 * 1024); // MB
            var totalMemory = GC.GetTotalMemory(false) / (1024 * 1024); // MB
            
            var memoryStatus = workingSet < 500 ? HealthStatus.Healthy : 
                              workingSet < 1000 ? HealthStatus.Degraded : HealthStatus.Unhealthy;
            
            return new ComponentHealthCheck
            {
                Name = "System Resources",
                Status = memoryStatus,
                ResponseTime = 0,
                Details = new Dictionary<string, object>
                {
                    ["WorkingSetMB"] = workingSet,
                    ["ManagedMemoryMB"] = totalMemory,
                    ["ProcessorCount"] = Environment.ProcessorCount,
                    ["MachineName"] = Environment.MachineName,
                    ["OSVersion"] = Environment.OSVersion.ToString(),
                    ["Uptime"] = DateTime.UtcNow - process.StartTime.ToUniversalTime()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System resources health check failed");
            
            return new ComponentHealthCheck
            {
                Name = "System Resources",
                Status = HealthStatus.Degraded,
                ResponseTime = 0,
                Error = ex.Message
            };
        }
    }

    private ComponentHealthCheck CheckConfigurationHealth()
    {
        try
        {
            var issues = new List<string>();
            
            // Check critical configuration values
            if (string.IsNullOrEmpty(_configuration.GetConnectionString("DefaultConnection")))
                issues.Add("Database connection string is missing");
            
            if (string.IsNullOrEmpty(_configuration["JwtSettings:SecretKey"]))
                issues.Add("JWT secret key is missing");
            
            var status = issues.Any() ? HealthStatus.Unhealthy : HealthStatus.Healthy;
            
            return new ComponentHealthCheck
            {
                Name = "Configuration",
                Status = status,
                ResponseTime = 0,
                Details = new Dictionary<string, object>
                {
                    ["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    ["Issues"] = issues,
                    ["HasDatabaseConnection"] = !string.IsNullOrEmpty(_configuration.GetConnectionString("DefaultConnection")),
                    ["HasJwtSettings"] = !string.IsNullOrEmpty(_configuration["JwtSettings:SecretKey"])
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration health check failed");
            
            return new ComponentHealthCheck
            {
                Name = "Configuration",
                Status = HealthStatus.Unhealthy,
                ResponseTime = 0,
                Error = ex.Message
            };
        }
    }

    private static HealthStatus DetermineOverallStatus(List<ComponentHealthCheck> checks)
    {
        if (checks.Any(c => c.Status == HealthStatus.Unhealthy))
            return HealthStatus.Unhealthy;
        
        if (checks.Any(c => c.Status == HealthStatus.Degraded))
            return HealthStatus.Degraded;
        
        return HealthStatus.Healthy;
    }

    private static string MaskConnectionString(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "Not configured";
        
        // Mask sensitive information in connection strings
        var masked = connectionString;
        var sensitiveKeys = new[] { "password", "pwd", "user id", "uid" };
        
        foreach (var key in sensitiveKeys)
        {
            var pattern = $@"{key}=([^;]+)";
            masked = System.Text.RegularExpressions.Regex.Replace(
                masked, pattern, $"{key}=***", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        
        return masked;
    }

    private static string GetApplicationVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString() ?? "Unknown";
    }

    private static double GetFreeSpaceInGB(string path)
    {
        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(path) ?? "C:");
            return Math.Round(drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0), 2);
        }
        catch
        {
            return -1;
        }
    }
}

public class HealthCheckResult
{
    public HealthStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    public long ResponseTime { get; set; }
    public List<ComponentHealthCheck> Components { get; set; } = new();
    public string? Error { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
}

public class ComponentHealthCheck
{
    public string Name { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public long ResponseTime { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}

public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}
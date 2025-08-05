using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Services;
using System.Net;

namespace StrideHR.API.Controllers;

/// <summary>
/// Health check endpoints for monitoring system status and component health
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Health")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Get basic system health status
    /// </summary>
    /// <returns>Simple health status for load balancers and monitoring tools</returns>
    /// <response code="200">System is healthy</response>
    /// <response code="503">System is unhealthy or degraded</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 503)]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            var healthResult = await _healthCheckService.GetSystemHealthAsync();
            
            var response = new
            {
                status = healthResult.Status.ToString().ToLower(),
                timestamp = healthResult.Timestamp,
                version = healthResult.Version,
                environment = healthResult.Environment
            };

            return healthResult.Status == HealthStatus.Healthy 
                ? Ok(response) 
                : StatusCode((int)HttpStatusCode.ServiceUnavailable, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check endpoint failed");
            
            return StatusCode((int)HttpStatusCode.ServiceUnavailable, new
            {
                status = "unhealthy",
                timestamp = DateTime.UtcNow,
                error = "Health check failed"
            });
        }
    }

    /// <summary>
    /// Get detailed system health information
    /// </summary>
    /// <returns>Comprehensive health status including all system components</returns>
    /// <response code="200">Detailed health information retrieved successfully</response>
    /// <response code="503">System is unhealthy with detailed component status</response>
    [HttpGet("detailed")]
    [ProducesResponseType(typeof(HealthCheckResult), 200)]
    [ProducesResponseType(typeof(HealthCheckResult), 503)]
    public async Task<IActionResult> GetDetailedHealth()
    {
        try
        {
            var healthResult = await _healthCheckService.GetSystemHealthAsync();
            
            return healthResult.Status == HealthStatus.Healthy 
                ? Ok(healthResult) 
                : StatusCode((int)HttpStatusCode.ServiceUnavailable, healthResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Detailed health check endpoint failed");
            
            var errorResult = new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Timestamp = DateTime.UtcNow,
                Error = "Health check failed",
                Components = new List<ComponentHealthCheck>()
            };
            
            return StatusCode((int)HttpStatusCode.ServiceUnavailable, errorResult);
        }
    }

    /// <summary>
    /// Get health status for a specific component
    /// </summary>
    /// <param name="component">Component name (database, redis, email, filestorage, system, configuration)</param>
    /// <returns>Health status for the specified component</returns>
    /// <response code="200">Component health information retrieved successfully</response>
    /// <response code="404">Component not found</response>
    /// <response code="503">Component is unhealthy</response>
    [HttpGet("component/{component}")]
    [ProducesResponseType(typeof(ComponentHealthCheck), 200)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(ComponentHealthCheck), 503)]
    public async Task<IActionResult> GetComponentHealth(string component)
    {
        try
        {
            var healthResult = await _healthCheckService.GetSystemHealthAsync();
            var componentHealth = healthResult.Components
                .FirstOrDefault(c => c.Name.Replace(" ", "").ToLower() == component.ToLower());

            if (componentHealth == null)
            {
                return NotFound(new
                {
                    error = "Component not found",
                    availableComponents = healthResult.Components.Select(c => c.Name.Replace(" ", "").ToLower()).ToList()
                });
            }

            return componentHealth.Status == HealthStatus.Healthy 
                ? Ok(componentHealth) 
                : StatusCode((int)HttpStatusCode.ServiceUnavailable, componentHealth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Component health check failed for component: {Component}", component);
            
            return StatusCode((int)HttpStatusCode.ServiceUnavailable, new
            {
                error = "Component health check failed",
                component = component
            });
        }
    }

    /// <summary>
    /// Get system readiness status
    /// </summary>
    /// <returns>Indicates if the system is ready to accept traffic</returns>
    /// <response code="200">System is ready to accept traffic</response>
    /// <response code="503">System is not ready (starting up or shutting down)</response>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 503)]
    public async Task<IActionResult> GetReadiness()
    {
        try
        {
            var healthResult = await _healthCheckService.GetSystemHealthAsync();
            
            // System is ready if database and configuration are healthy
            var criticalComponents = healthResult.Components
                .Where(c => c.Name == "Database" || c.Name == "Configuration")
                .ToList();

            var isReady = criticalComponents.All(c => c.Status == HealthStatus.Healthy);
            
            var response = new
            {
                ready = isReady,
                timestamp = DateTime.UtcNow,
                criticalComponents = criticalComponents.Select(c => new
                {
                    name = c.Name,
                    status = c.Status.ToString().ToLower(),
                    responseTime = c.ResponseTime
                }).ToList()
            };

            return isReady 
                ? Ok(response) 
                : StatusCode((int)HttpStatusCode.ServiceUnavailable, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            
            return StatusCode((int)HttpStatusCode.ServiceUnavailable, new
            {
                ready = false,
                timestamp = DateTime.UtcNow,
                error = "Readiness check failed"
            });
        }
    }

    /// <summary>
    /// Get system liveness status
    /// </summary>
    /// <returns>Indicates if the system is alive and responding</returns>
    /// <response code="200">System is alive and responding</response>
    /// <response code="503">System is not responding properly</response>
    [HttpGet("live")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 503)]
    public IActionResult GetLiveness()
    {
        try
        {
            // Simple liveness check - if we can respond, we're alive
            return Ok(new
            {
                alive = true,
                timestamp = DateTime.UtcNow,
                uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Liveness check failed");
            
            return StatusCode((int)HttpStatusCode.ServiceUnavailable, new
            {
                alive = false,
                timestamp = DateTime.UtcNow,
                error = "Liveness check failed"
            });
        }
    }

    /// <summary>
    /// Get system metrics and performance information
    /// </summary>
    /// <returns>System performance metrics</returns>
    /// <response code="200">System metrics retrieved successfully</response>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult GetMetrics()
    {
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var gcInfo = GC.GetTotalMemory(false);
            
            var metrics = new
            {
                timestamp = DateTime.UtcNow,
                memory = new
                {
                    workingSetMB = Math.Round(process.WorkingSet64 / (1024.0 * 1024.0), 2),
                    managedMemoryMB = Math.Round(gcInfo / (1024.0 * 1024.0), 2),
                    gen0Collections = GC.CollectionCount(0),
                    gen1Collections = GC.CollectionCount(1),
                    gen2Collections = GC.CollectionCount(2)
                },
                process = new
                {
                    id = process.Id,
                    startTime = process.StartTime.ToUniversalTime(),
                    uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime(),
                    threadCount = process.Threads.Count,
                    handleCount = process.HandleCount
                },
                system = new
                {
                    processorCount = Environment.ProcessorCount,
                    machineName = Environment.MachineName,
                    osVersion = Environment.OSVersion.ToString(),
                    clrVersion = Environment.Version.ToString()
                }
            };

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Metrics collection failed");
            
            return StatusCode((int)HttpStatusCode.InternalServerError, new
            {
                error = "Metrics collection failed",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
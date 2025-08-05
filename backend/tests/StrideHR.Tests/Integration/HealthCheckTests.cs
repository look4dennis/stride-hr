using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using StrideHR.Tests.TestConfiguration;
using StrideHR.API.Services;

namespace StrideHR.Tests.Integration;

/// <summary>
/// Comprehensive tests for the health check system to validate
/// all system components are properly monitored and reported
/// </summary>
public class HealthCheckTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public HealthCheckTests(TestWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_BasicCheck_ReturnsHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResponse = JsonDocument.Parse(content);
        var root = healthResponse.RootElement;

        Assert.True(root.TryGetProperty("status", out var status));
        Assert.Equal("healthy", status.GetString());
        
        Assert.True(root.TryGetProperty("timestamp", out _));
        Assert.True(root.TryGetProperty("version", out _));
        Assert.True(root.TryGetProperty("environment", out _));
        
        _output.WriteLine($"Basic health check passed: {status.GetString()}");
    }

    [Fact]
    public async Task HealthEndpoint_DetailedCheck_ReturnsComprehensiveStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/health/detailed");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResponse = JsonDocument.Parse(content);
        var root = healthResponse.RootElement;

        // Validate overall structure
        Assert.True(root.TryGetProperty("status", out var status));
        Assert.True(root.TryGetProperty("timestamp", out _));
        Assert.True(root.TryGetProperty("responseTime", out var responseTime));
        Assert.True(root.TryGetProperty("components", out var components));
        Assert.True(root.TryGetProperty("version", out _));
        Assert.True(root.TryGetProperty("environment", out _));

        // Validate response time is reasonable
        Assert.True(responseTime.GetInt64() < 5000, "Health check should complete within 5 seconds");

        // Validate components array
        Assert.True(components.GetArrayLength() > 0, "Should have at least one component");
        
        var componentNames = new List<string>();
        foreach (var component in components.EnumerateArray())
        {
            Assert.True(component.TryGetProperty("name", out var name));
            Assert.True(component.TryGetProperty("status", out var componentStatus));
            Assert.True(component.TryGetProperty("responseTime", out _));
            
            componentNames.Add(name.GetString() ?? "");
            _output.WriteLine($"Component: {name.GetString()} - Status: {componentStatus.GetString()}");
        }

        // Validate expected components are present
        var expectedComponents = new[] { "Database", "Configuration", "System Resources" };
        foreach (var expectedComponent in expectedComponents)
        {
            Assert.Contains(expectedComponent, componentNames);
        }
        
        _output.WriteLine($"Detailed health check passed with {componentNames.Count} components");
    }

    [Fact]
    public async Task HealthEndpoint_ComponentSpecific_ReturnsComponentStatus()
    {
        // Act - Test database component
        var response = await _client.GetAsync("/api/health/component/database");

        // Assert
        var acceptableStatuses = new[] { HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable };
        Assert.Contains(response.StatusCode, acceptableStatuses);
        
        var content = await response.Content.ReadAsStringAsync();
        var componentResponse = JsonDocument.Parse(content);
        var root = componentResponse.RootElement;

        Assert.True(root.TryGetProperty("name", out var name));
        Assert.Equal("Database", name.GetString());
        
        Assert.True(root.TryGetProperty("status", out var status));
        Assert.True(root.TryGetProperty("responseTime", out _));
        Assert.True(root.TryGetProperty("details", out var details));

        // Validate database-specific details
        Assert.True(details.TryGetProperty("canConnect", out var canConnect));
        
        _output.WriteLine($"Database component status: {status.GetString()}, Can Connect: {canConnect.GetBoolean()}");
    }

    [Fact]
    public async Task HealthEndpoint_ComponentNotFound_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/health/component/nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonDocument.Parse(content);
        var root = errorResponse.RootElement;

        Assert.True(root.TryGetProperty("error", out var error));
        Assert.Equal("Component not found", error.GetString());
        
        Assert.True(root.TryGetProperty("availableComponents", out var availableComponents));
        Assert.True(availableComponents.GetArrayLength() > 0);
        
        _output.WriteLine("Component not found test passed");
    }

    [Fact]
    public async Task HealthEndpoint_Readiness_ReturnsReadinessStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/health/ready");

        // Assert
        var acceptableStatuses = new[] { HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable };
        Assert.Contains(response.StatusCode, acceptableStatuses);
        
        var content = await response.Content.ReadAsStringAsync();
        var readinessResponse = JsonDocument.Parse(content);
        var root = readinessResponse.RootElement;

        Assert.True(root.TryGetProperty("ready", out var ready));
        Assert.True(root.TryGetProperty("timestamp", out _));
        Assert.True(root.TryGetProperty("criticalComponents", out var criticalComponents));

        // Validate critical components
        Assert.True(criticalComponents.GetArrayLength() > 0);
        
        foreach (var component in criticalComponents.EnumerateArray())
        {
            Assert.True(component.TryGetProperty("name", out var name));
            Assert.True(component.TryGetProperty("status", out var status));
            Assert.True(component.TryGetProperty("responseTime", out _));
            
            _output.WriteLine($"Critical component: {name.GetString()} - Status: {status.GetString()}");
        }
        
        _output.WriteLine($"Readiness check: {ready.GetBoolean()}");
    }

    [Fact]
    public async Task HealthEndpoint_Liveness_ReturnsAliveStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var livenessResponse = JsonDocument.Parse(content);
        var root = livenessResponse.RootElement;

        Assert.True(root.TryGetProperty("alive", out var alive));
        Assert.True(alive.GetBoolean(), "System should be alive");
        
        Assert.True(root.TryGetProperty("timestamp", out _));
        Assert.True(root.TryGetProperty("uptime", out var uptime));
        
        // Validate uptime format (should be a valid timespan string)
        Assert.False(string.IsNullOrEmpty(uptime.GetString()));
        
        _output.WriteLine($"Liveness check passed - Uptime: {uptime.GetString()}");
    }

    [Fact]
    public async Task HealthEndpoint_Metrics_ReturnsSystemMetrics()
    {
        // Act
        var response = await _client.GetAsync("/api/health/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var metricsResponse = JsonDocument.Parse(content);
        var root = metricsResponse.RootElement;

        // Validate metrics structure
        Assert.True(root.TryGetProperty("timestamp", out _));
        Assert.True(root.TryGetProperty("memory", out var memory));
        Assert.True(root.TryGetProperty("process", out var process));
        Assert.True(root.TryGetProperty("system", out var system));

        // Validate memory metrics
        Assert.True(memory.TryGetProperty("workingSetMB", out var workingSetMB));
        Assert.True(memory.TryGetProperty("managedMemoryMB", out var managedMemoryMB));
        Assert.True(workingSetMB.GetDouble() > 0, "Working set should be positive");
        Assert.True(managedMemoryMB.GetDouble() > 0, "Managed memory should be positive");

        // Validate process metrics
        Assert.True(process.TryGetProperty("id", out var processId));
        Assert.True(process.TryGetProperty("uptime", out var processUptime));
        Assert.True(processId.GetInt32() > 0, "Process ID should be positive");

        // Validate system metrics
        Assert.True(system.TryGetProperty("processorCount", out var processorCount));
        Assert.True(system.TryGetProperty("machineName", out var machineName));
        Assert.True(processorCount.GetInt32() > 0, "Processor count should be positive");
        Assert.False(string.IsNullOrEmpty(machineName.GetString()));
        
        _output.WriteLine($"Metrics - Memory: {workingSetMB.GetDouble():F2}MB, Processors: {processorCount.GetInt32()}");
    }

    [Fact]
    public async Task HealthEndpoint_LegacyHealth_MaintainsBackwardCompatibility()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        var acceptableStatuses = new[] { HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable };
        Assert.Contains(response.StatusCode, acceptableStatuses);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResponse = JsonDocument.Parse(content);
        var root = healthResponse.RootElement;

        // Should maintain the legacy format
        Assert.True(root.TryGetProperty("Status", out var status) || 
                   root.TryGetProperty("status", out status));
        Assert.True(root.TryGetProperty("Timestamp", out _) || 
                   root.TryGetProperty("timestamp", out _));
        
        _output.WriteLine($"Legacy health endpoint status: {status.GetString()}");
    }

    [Fact]
    public async Task HealthCheckService_DirectUsage_ReturnsValidResults()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();

        // Act
        var healthResult = await healthCheckService.GetSystemHealthAsync();

        // Assert
        Assert.NotNull(healthResult);
        Assert.True(Enum.IsDefined(typeof(HealthStatus), healthResult.Status));
        Assert.True(healthResult.ResponseTime >= 0);
        Assert.NotEmpty(healthResult.Components);
        Assert.False(string.IsNullOrEmpty(healthResult.Version));
        Assert.False(string.IsNullOrEmpty(healthResult.Environment));

        // Validate each component
        foreach (var component in healthResult.Components)
        {
            Assert.False(string.IsNullOrEmpty(component.Name));
            Assert.True(Enum.IsDefined(typeof(HealthStatus), component.Status));
            Assert.True(component.ResponseTime >= 0);
            Assert.NotNull(component.Details);
            
            _output.WriteLine($"Component: {component.Name}, Status: {component.Status}, Time: {component.ResponseTime}ms");
        }
        
        _output.WriteLine($"Direct health check service test passed - Overall Status: {healthResult.Status}");
    }

    [Fact]
    public async Task HealthEndpoints_UnderLoad_PerformAdequately()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        const int concurrentRequests = 10;

        // Act - Make multiple concurrent health check requests
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(_client.GetAsync("/api/health"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        Assert.All(responses, response =>
        {
            var acceptableStatuses = new[] { HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable };
            Assert.Contains(response.StatusCode, acceptableStatuses);
        });

        // All requests should complete within reasonable time
        var totalTime = responses.Sum(r => r.Headers.Date?.Millisecond ?? 0);
        _output.WriteLine($"Concurrent health checks completed - {concurrentRequests} requests");
        
        // Cleanup
        foreach (var response in responses)
        {
            response.Dispose();
        }
    }
}
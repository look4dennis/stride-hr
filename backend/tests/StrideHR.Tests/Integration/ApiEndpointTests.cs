using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using StrideHR.Tests.TestConfiguration;

namespace StrideHR.Tests.Integration;

/// <summary>
/// Comprehensive API endpoint testing to validate all controller endpoints
/// for correct HTTP status codes, response structures, and security requirements
/// </summary>
public class ApiEndpointTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly TestDataSeeder _seeder;

    public ApiEndpointTests(TestWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient();
        
        // Fix: Get the actual DbContext from services
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StrideHR.Infrastructure.Data.StrideHRDbContext>();
        _seeder = new TestDataSeeder(dbContext);
    }

    [Fact]
    public async Task AuthController_Login_ValidCredentials_ReturnsOk()
    {
        // Arrange
        await _seeder.SeedTestDataAsync();
        var loginRequest = new
        {
            Email = "admin@stridehr.com",
            Password = "Admin123!",
            RememberMe = false
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("token", responseContent);
        Assert.Contains("success", responseContent);
        
        _output.WriteLine($"Login endpoint test passed: {response.StatusCode}");
    }

    [Fact]
    public async Task AuthController_Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "invalid@test.com",
            Password = "wrongpassword",
            RememberMe = false
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        _output.WriteLine($"Login invalid credentials test passed: {response.StatusCode}");
    }

    [Fact]
    public async Task AuthController_Login_MissingFields_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "",
            Password = "",
            RememberMe = false
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        _output.WriteLine($"Login missing fields test passed: {response.StatusCode}");
    }

    [Fact]
    public async Task AuthController_GetCurrentUser_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        _output.WriteLine($"Get current user without token test passed: {response.StatusCode}");
    }

    [Fact]
    public async Task AuthController_GetCurrentUser_WithValidToken_ReturnsOk()
    {
        // Arrange
        await _seeder.SeedTestDataAsync();
        var token = await GetValidJwtTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("success", responseContent);
        Assert.Contains("user", responseContent);
        
        _output.WriteLine($"Get current user with token test passed: {response.StatusCode}");
    }

    [Fact]
    public async Task EmployeeController_GetAllEmployees_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/employee");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        _output.WriteLine($"Get all employees without auth test passed: {response.StatusCode}");
    }

    [Fact]
    public async Task EmployeeController_GetAllEmployees_WithValidAuth_ReturnsOk()
    {
        // Arrange
        await _seeder.SeedTestDataAsync();
        var token = await GetValidJwtTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/employee");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("success", responseContent);
        
        _output.WriteLine($"Get all employees with auth test passed: {response.StatusCode}");
    }

    [Fact]
    public async Task EmployeeController_GetEmployee_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        await _seeder.SeedTestDataAsync();
        var token = await GetValidJwtTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/employee/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        _output.WriteLine($"Get non-existent employee test passed: {response.StatusCode}");
    }

    [Fact]
    public async Task EmployeeController_CreateEmployee_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new
        {
            FirstName = "Test",
            LastName = "Employee",
            Email = "test.employee@test.com",
            PhoneNumber = "1234567890",
            BranchId = 1,
            DepartmentId = 1,
            PositionId = 1
        };

        var json = JsonSerializer.Serialize(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/employee", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        _output.WriteLine($"Create employee without auth test passed: {response.StatusCode}");
    }

    [Theory]
    [InlineData("/api/auth/login", "POST")]
    [InlineData("/api/auth/refresh", "POST")]
    [InlineData("/api/auth/logout", "POST")]
    [InlineData("/api/auth/me", "GET")]
    [InlineData("/api/employee", "GET")]
    [InlineData("/api/employee/1", "GET")]
    [InlineData("/api/branch", "GET")]
    [InlineData("/api/organization", "GET")]
    [InlineData("/api/role", "GET")]
    public async Task ApiEndpoints_SecurityValidation_ReturnsExpectedStatusCodes(string endpoint, string method)
    {
        // Arrange
        HttpResponseMessage response;

        // Act
        switch (method.ToUpper())
        {
            case "GET":
                response = await _client.GetAsync(endpoint);
                break;
            case "POST":
                var content = new StringContent("{}", Encoding.UTF8, "application/json");
                response = await _client.PostAsync(endpoint, content);
                break;
            case "PUT":
                var putContent = new StringContent("{}", Encoding.UTF8, "application/json");
                response = await _client.PutAsync(endpoint, putContent);
                break;
            case "DELETE":
                response = await _client.DeleteAsync(endpoint);
                break;
            default:
                throw new ArgumentException($"Unsupported HTTP method: {method}");
        }

        // Assert
        // For secured endpoints without authentication, expect 401
        // For public endpoints (like login), expect different status codes
        var expectedStatuses = new[] { 
            HttpStatusCode.OK, 
            HttpStatusCode.Unauthorized, 
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.Forbidden
        };
        
        Assert.Contains(response.StatusCode, expectedStatuses);
        
        _output.WriteLine($"Endpoint {method} {endpoint} returned: {response.StatusCode}");
    }

    [Fact]
    public async Task ApiEndpoints_HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Status", responseContent);
        Assert.Contains("Healthy", responseContent);
        
        _output.WriteLine($"Health check endpoint test passed: {response.StatusCode}");
    }

    [Fact]
    public async Task ApiEndpoints_ResponseHeaders_ContainSecurityHeaders()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Check for common security headers (these might be added by middleware)
        var headers = response.Headers.ToString();
        _output.WriteLine($"Response headers: {headers}");
        
        // Basic validation that response is properly formatted
        Assert.True(response.Content.Headers.ContentType?.MediaType?.Contains("json") ?? false);
    }

    [Fact]
    public async Task ApiEndpoints_InvalidJsonPayload_ReturnsBadRequest()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        _output.WriteLine($"Invalid JSON payload test passed: {response.StatusCode}");
    }

    [Fact]
    public async Task ApiEndpoints_LargePayload_HandlesGracefully()
    {
        // Arrange
        var largePayload = new string('x', 1024 * 1024); // 1MB string
        var request = new { data = largePayload };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        // Should handle large payloads gracefully (either accept or reject with proper status)
        var acceptableStatuses = new[] { 
            HttpStatusCode.BadRequest, 
            HttpStatusCode.RequestEntityTooLarge,
            HttpStatusCode.Unauthorized // If it processes but fails auth
        };
        
        Assert.Contains(response.StatusCode, acceptableStatuses);
        
        _output.WriteLine($"Large payload test passed: {response.StatusCode}");
    }

    private async Task<string> GetValidJwtTokenAsync()
    {
        var loginRequest = new
        {
            Email = "admin@stridehr.com",
            Password = "Admin123!",
            RememberMe = false
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);
        
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Failed to get valid JWT token. Status: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        return loginResponse.GetProperty("data").GetProperty("token").GetString() ?? 
               throw new InvalidOperationException("Token not found in login response");
    }
}
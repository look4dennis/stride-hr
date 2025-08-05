using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StrideHR.Infrastructure.Data;
using System.Net.Http;

namespace StrideHR.SecurityTests.Infrastructure;

public class SecurityTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;

    public SecurityTestBase(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove the app's ApplicationDbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add ApplicationDbContext using an in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("SecurityTestDb");
                });

                // Reduce logging noise during tests
                services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
            });
        });

        _client = _factory.CreateClient();
    }

    protected async Task<string> GetValidJwtTokenAsync(string employeeId = "test-employee-id", 
        string organizationId = "test-org-id", string branchId = "test-branch-id", 
        string[] roles = null)
    {
        roles ??= new[] { "Employee" };
        
        var loginRequest = new
        {
            Email = "test@example.com",
            Password = "TestPassword123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            // Extract token from response - this would need to match your actual API response format
            return ExtractTokenFromResponse(content);
        }

        // Fallback: create a test token manually
        return CreateTestJwtToken(employeeId, organizationId, branchId, roles);
    }

    private string ExtractTokenFromResponse(string responseContent)
    {
        // This would need to be implemented based on your actual login response format
        // For now, return a placeholder
        return CreateTestJwtToken("test-employee-id", "test-org-id", "test-branch-id", new[] { "Employee" });
    }

    private string CreateTestJwtToken(string employeeId, string organizationId, string branchId, string[] roles)
    {
        // This would create a test JWT token - implementation depends on your JWT service
        // For security testing, we need actual token generation logic
        return "test-jwt-token";
    }

    protected void AddAuthorizationHeader(string token)
    {
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    protected void RemoveAuthorizationHeader()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }
}
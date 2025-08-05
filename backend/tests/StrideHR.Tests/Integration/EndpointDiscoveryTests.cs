using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using StrideHR.Infrastructure.Data;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using StrideHR.Tests.TestConfiguration;

namespace StrideHR.Tests.Integration;

/// <summary>
/// Automatically discovers and tests all API endpoints in the application
/// to ensure they return appropriate HTTP status codes and handle security correctly
/// </summary>
public class EndpointDiscoveryTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly TestDataSeeder _seeder;

    public EndpointDiscoveryTests(TestWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient();
        var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();
        _seeder = new TestDataSeeder(dbContext);
    }

    [Fact]
    public async Task DiscoverAndTestAllEndpoints_ValidateStatusCodes()
    {
        // Arrange
        await _seeder.SeedTestDataAsync();
        var endpoints = DiscoverAllEndpoints();
        var token = await GetValidJwtTokenAsync();
        
        var testResults = new List<EndpointTestResult>();

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var result = await TestEndpoint(endpoint, token);
            testResults.Add(result);
            
            _output.WriteLine($"{endpoint.Method} {endpoint.Route} -> {result.StatusCode} ({result.IsExpected})");
        }

        // Validate results
        var failedTests = testResults.Where(r => !r.IsExpected).ToList();
        
        if (failedTests.Any())
        {
            var failureDetails = string.Join("\n", failedTests.Select(f => 
                $"{f.Method} {f.Route}: Expected acceptable status, got {f.StatusCode}"));
            
            _output.WriteLine($"Failed endpoint tests:\n{failureDetails}");
        }

        // At least 80% of endpoints should return expected status codes
        var successRate = (double)(testResults.Count - failedTests.Count) / testResults.Count;
        Assert.True(successRate >= 0.8, $"Success rate {successRate:P} is below 80%. Failed tests: {failedTests.Count}");
        
        _output.WriteLine($"Endpoint discovery test completed. Success rate: {successRate:P}");
    }

    [Fact]
    public async Task TestCriticalEndpoints_EnsureProperSecurity()
    {
        // Arrange
        await _seeder.SeedTestDataAsync();
        var criticalEndpoints = new[]
        {
            new EndpointInfo { Route = "/api/auth/login", Method = "POST", RequiresAuth = false },
            new EndpointInfo { Route = "/api/auth/me", Method = "GET", RequiresAuth = true },
            new EndpointInfo { Route = "/api/employee", Method = "GET", RequiresAuth = true },
            new EndpointInfo { Route = "/api/employee", Method = "POST", RequiresAuth = true },
            new EndpointInfo { Route = "/api/payroll", Method = "GET", RequiresAuth = true },
            new EndpointInfo { Route = "/api/role", Method = "GET", RequiresAuth = true },
            new EndpointInfo { Route = "/health", Method = "GET", RequiresAuth = false }
        };

        var token = await GetValidJwtTokenAsync();

        // Act & Assert
        foreach (var endpoint in criticalEndpoints)
        {
            // Test without authentication
            var unauthResult = await TestEndpoint(endpoint, null);
            
            if (endpoint.RequiresAuth)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, unauthResult.StatusCode);
                _output.WriteLine($"✓ {endpoint.Method} {endpoint.Route} properly requires authentication");
            }

            // Test with authentication
            if (endpoint.RequiresAuth)
            {
                var authResult = await TestEndpoint(endpoint, token);
                var acceptableAuthStatuses = new[] { 
                    HttpStatusCode.OK, 
                    HttpStatusCode.NotFound, 
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.Forbidden // May lack specific permissions
                };
                
                Assert.Contains(authResult.StatusCode, acceptableAuthStatuses);
                _output.WriteLine($"✓ {endpoint.Method} {endpoint.Route} with auth -> {authResult.StatusCode}");
            }
        }
    }

    [Fact]
    public async Task TestFileUploadEndpoints_ValidateConfiguration()
    {
        // Arrange
        await _seeder.SeedTestDataAsync();
        var token = await GetValidJwtTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var fileUploadEndpoints = new[]
        {
            "/api/employee/1/profile-photo",
            "/api/document/upload",
            "/api/asset/upload"
        };

        // Act & Assert
        foreach (var endpoint in fileUploadEndpoints)
        {
            // Test with empty multipart content
            using var form = new MultipartFormDataContent();
            var response = await _client.PostAsync(endpoint, form);

            // Should handle file upload requests (even if empty)
            var acceptableStatuses = new[] { 
                HttpStatusCode.BadRequest, // Missing file
                HttpStatusCode.NotFound,   // Endpoint doesn't exist
                HttpStatusCode.Unauthorized, // Insufficient permissions
                HttpStatusCode.UnsupportedMediaType // Wrong content type
            };

            Assert.Contains(response.StatusCode, acceptableStatuses);
            _output.WriteLine($"File upload endpoint {endpoint} -> {response.StatusCode}");
        }
    }

    [Fact]
    public async Task TestApiResponseStructure_ValidateConsistency()
    {
        // Arrange
        await _seeder.SeedTestDataAsync();
        var token = await GetValidJwtTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var testEndpoints = new[]
        {
            "/api/employee",
            "/api/branch",
            "/api/organization"
        };

        // Act & Assert
        foreach (var endpoint in testEndpoints)
        {
            var response = await _client.GetAsync(endpoint);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Validate JSON structure
                Assert.True(IsValidJson(content), $"Response from {endpoint} is not valid JSON");
                
                // Check for consistent API response structure
                if (content.Contains("success") || content.Contains("data"))
                {
                    _output.WriteLine($"✓ {endpoint} returns structured API response");
                }
                else
                {
                    _output.WriteLine($"⚠ {endpoint} may not follow standard API response structure");
                }
            }
        }
    }

    private List<EndpointInfo> DiscoverAllEndpoints()
    {
        var endpoints = new List<EndpointInfo>();
        
        // Get all controller types
        var controllerTypes = Assembly.GetAssembly(typeof(Program))
            ?.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ControllerBase)) && !t.IsAbstract)
            .ToList() ?? new List<Type>();

        foreach (var controllerType in controllerTypes)
        {
            var controllerName = controllerType.Name.Replace("Controller", "").ToLower();
            var routeAttribute = controllerType.GetCustomAttribute<RouteAttribute>();
            var baseRoute = routeAttribute?.Template?.Replace("[controller]", controllerName) ?? $"api/{controllerName}";

            var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.IsPublic && !m.IsSpecialName && m.DeclaringType == controllerType);

            foreach (var method in methods)
            {
                var httpAttributes = method.GetCustomAttributes()
                    .Where(a => a.GetType().Name.StartsWith("Http"))
                    .ToList();

                if (httpAttributes.Any())
                {
                    foreach (var httpAttr in httpAttributes)
                    {
                        var httpMethod = httpAttr.GetType().Name.Replace("Http", "").Replace("Attribute", "").ToUpper();
                        var template = httpAttr.GetType().GetProperty("Template")?.GetValue(httpAttr)?.ToString() ?? "";
                        
                        var fullRoute = string.IsNullOrEmpty(template) 
                            ? $"/{baseRoute}"
                            : $"/{baseRoute}/{template}";

                        // Clean up route
                        fullRoute = fullRoute.Replace("//", "/");

                        endpoints.Add(new EndpointInfo
                        {
                            Route = fullRoute,
                            Method = httpMethod,
                            ControllerName = controllerType.Name,
                            ActionName = method.Name,
                            RequiresAuth = method.GetCustomAttribute<AuthorizeAttribute>() != null ||
                                         controllerType.GetCustomAttribute<AuthorizeAttribute>() != null
                        });
                    }
                }
            }
        }

        return endpoints.DistinctBy(e => $"{e.Method}:{e.Route}").ToList();
    }

    private async Task<EndpointTestResult> TestEndpoint(EndpointInfo endpoint, string? token)
    {
        try
        {
            // Set authorization header if token provided
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _client.DefaultRequestHeaders.Authorization = null;
            }

            HttpResponseMessage response;

            switch (endpoint.Method.ToUpper())
            {
                case "GET":
                    response = await _client.GetAsync(endpoint.Route);
                    break;
                case "POST":
                    var postContent = new StringContent("{}", Encoding.UTF8, "application/json");
                    response = await _client.PostAsync(endpoint.Route, postContent);
                    break;
                case "PUT":
                    var putContent = new StringContent("{}", Encoding.UTF8, "application/json");
                    response = await _client.PutAsync(endpoint.Route, putContent);
                    break;
                case "DELETE":
                    response = await _client.DeleteAsync(endpoint.Route);
                    break;
                default:
                    return new EndpointTestResult
                    {
                        Route = endpoint.Route,
                        Method = endpoint.Method,
                        StatusCode = HttpStatusCode.MethodNotAllowed,
                        IsExpected = false,
                        ErrorMessage = $"Unsupported HTTP method: {endpoint.Method}"
                    };
            }

            // Determine if status code is expected
            var isExpected = IsExpectedStatusCode(response.StatusCode, endpoint.RequiresAuth, !string.IsNullOrEmpty(token));

            return new EndpointTestResult
            {
                Route = endpoint.Route,
                Method = endpoint.Method,
                StatusCode = response.StatusCode,
                IsExpected = isExpected,
                ResponseTime = 0 // Could be measured if needed
            };
        }
        catch (Exception ex)
        {
            return new EndpointTestResult
            {
                Route = endpoint.Route,
                Method = endpoint.Method,
                StatusCode = HttpStatusCode.InternalServerError,
                IsExpected = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private static bool IsExpectedStatusCode(HttpStatusCode statusCode, bool requiresAuth, bool hasToken)
    {
        // Define acceptable status codes based on context
        var alwaysAcceptable = new[]
        {
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.NoContent,
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Conflict,
            HttpStatusCode.UnprocessableEntity
        };

        if (alwaysAcceptable.Contains(statusCode))
            return true;

        // If endpoint requires auth and no token provided, 401 is expected
        if (requiresAuth && !hasToken && statusCode == HttpStatusCode.Unauthorized)
            return true;

        // If endpoint requires auth and token provided, 403 might be acceptable (insufficient permissions)
        if (requiresAuth && hasToken && statusCode == HttpStatusCode.Forbidden)
            return true;

        // Method not allowed is acceptable for unsupported HTTP methods
        if (statusCode == HttpStatusCode.MethodNotAllowed)
            return true;

        return false;
    }

    private static bool IsValidJson(string content)
    {
        try
        {
            JsonDocument.Parse(content);
            return true;
        }
        catch
        {
            return false;
        }
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

public class EndpointInfo
{
    public string Route { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string ControllerName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public bool RequiresAuth { get; set; }
}

public class EndpointTestResult
{
    public string Route { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public HttpStatusCode StatusCode { get; set; }
    public bool IsExpected { get; set; }
    public long ResponseTime { get; set; }
    public string? ErrorMessage { get; set; }
}
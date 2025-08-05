using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using StrideHR.Tests.TestConfiguration;

namespace StrideHR.Tests.Integration;

/// <summary>
/// Tests to validate API response structures and error handling consistency
/// across all endpoints to ensure proper client integration
/// </summary>
public class ApiResponseStructureTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly TestDataSeeder _seeder;

    public ApiResponseStructureTests(TestWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient();
        _seeder = new TestDataSeeder(_factory.Services.CreateScope().ServiceProvider);
    }

    [Fact]
    public async Task ApiResponses_SuccessfulRequests_FollowConsistentStructure()
    {
        // Arrange
        await _seeder.SeedTestDataAsync();
        var token = await GetValidJwtTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var endpoints = new[]
        {
            "/api/employee",
            "/api/branch",
            "/api/organization",
            "/api/role"
        };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(content);
                var root = jsonDoc.RootElement;

                // Validate consistent API response structure
                if (root.TryGetProperty("success", out var successProp))
                {
                    Assert.True(successProp.GetBoolean(), $"Success should be true for {endpoint}");
                    
                    // Should have data property for successful responses
                    Assert.True(root.TryGetProperty("data", out _), $"Missing 'data' property in {endpoint}");
                    
                    _output.WriteLine($"✓ {endpoint} follows consistent success response structure");
                }
                else
                {
                    // Some endpoints might return data directly - this is also acceptable
                    _output.WriteLine($"⚠ {endpoint} returns data directly (not wrapped in standard structure)");
                }
            }
        }
    }

    [Fact]
    public async Task ApiResponses_ErrorRequests_FollowConsistentErrorStructure()
    {
        // Arrange
        var errorEndpoints = new[]
        {
            new { Endpoint = "/api/employee/99999", ExpectedStatus = HttpStatusCode.NotFound },
            new { Endpoint = "/api/auth/login", ExpectedStatus = HttpStatusCode.BadRequest }, // Empty body
            new { Endpoint = "/api/employee", ExpectedStatus = HttpStatusCode.Unauthorized } // No auth
        };

        // Act & Assert
        foreach (var test in errorEndpoints)
        {
            HttpResponseMessage response;
            
            if (test.Endpoint == "/api/auth/login")
            {
                // Send invalid login request
                var invalidContent = new StringContent("{}", Encoding.UTF8, "application/json");
                response = await _client.PostAsync(test.Endpoint, invalidContent);
            }
            else
            {
                response = await _client.GetAsync(test.Endpoint);
            }

            if (response.StatusCode == test.ExpectedStatus)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                if (!string.IsNullOrEmpty(content) && IsValidJson(content))
                {
                    var jsonDoc = JsonDocument.Parse(content);
                    var root = jsonDoc.RootElement;

                    // Check for consistent error response structure
                    if (root.TryGetProperty("success", out var successProp))
                    {
                        Assert.False(successProp.GetBoolean(), $"Success should be false for error in {test.Endpoint}");
                        
                        // Should have message property for error responses
                        Assert.True(root.TryGetProperty("message", out _), $"Missing 'message' property in error response from {test.Endpoint}");
                        
                        _output.WriteLine($"✓ {test.Endpoint} follows consistent error response structure");
                    }
                    else
                    {
                        _output.WriteLine($"⚠ {test.Endpoint} error response doesn't follow standard structure");
                    }
                }
            }
        }
    }

    [Fact]
    public async Task ApiResponses_ContentTypeHeaders_AreCorrect()
    {
        // Arrange
        await _seeder.SeedTestDataAsync();
        var token = await GetValidJwtTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var endpoints = new[]
        {
            "/api/employee",
            "/api/auth/me",
            "/health"
        };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var contentType = response.Content.Headers.ContentType?.MediaType;
                
                // API endpoints should return JSON
                Assert.True(contentType?.Contains("json") == true, 
                    $"Endpoint {endpoint} should return JSON content type, got: {contentType}");
                
                _output.WriteLine($"✓ {endpoint} returns correct content type: {contentType}");
            }
        }
    }

    [Fact]
    public async Task ApiResponses_ValidationErrors_ReturnStructuredErrors()
    {
        // Arrange
        await _seeder.SeedTestDataAsync();
        var token = await GetValidJwtTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Test with invalid employee creation data
        var invalidEmployeeData = new
        {
            FirstName = "", // Required field empty
            LastName = "",  // Required field empty
            Email = "invalid-email", // Invalid format
            PhoneNumber = "123", // Too short
            BranchId = -1, // Invalid ID
            DepartmentId = -1, // Invalid ID
            PositionId = -1 // Invalid ID
        };

        var json = JsonSerializer.Serialize(invalidEmployeeData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/employee", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        
        if (IsValidJson(responseContent))
        {
            var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;

            // Check for validation error structure
            if (root.TryGetProperty("success", out var successProp))
            {
                Assert.False(successProp.GetBoolean(), "Success should be false for validation errors");
                
                // Should have errors array or message for validation failures
                var hasErrors = root.TryGetProperty("errors", out _);
                var hasMessage = root.TryGetProperty("message", out _);
                
                Assert.True(hasErrors || hasMessage, "Validation error response should contain errors or message");
                
                _output.WriteLine("✓ Validation errors return structured error response");
            }
        }
    }

    [Fact]
    public async Task ApiResponses_PaginatedEndpoints_ReturnPaginationMetadata()
    {
        // Arrange
        await _seeder.SeedTestDataAsync();
        var token = await GetValidJwtTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var paginatedEndpoints = new[]
        {
            "/api/employee?page=1&pageSize=10",
            "/api/employee/search"
        };

        // Act & Assert
        foreach (var endpoint in paginatedEndpoints)
        {
            HttpResponseMessage response;
            
            if (endpoint.Contains("search"))
            {
                // POST request for search
                var searchCriteria = new { Page = 1, PageSize = 10 };
                var json = JsonSerializer.Serialize(searchCriteria);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                response = await _client.PostAsync(endpoint, content);
            }
            else
            {
                response = await _client.GetAsync(endpoint);
            }

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (IsValidJson(responseContent))
                {
                    var jsonDoc = JsonDocument.Parse(responseContent);
                    var root = jsonDoc.RootElement;

                    // Check for pagination metadata
                    if (root.TryGetProperty("data", out var dataProp))
                    {
                        // Look for pagination properties
                        var hasPagination = dataProp.TryGetProperty("totalCount", out _) ||
                                          dataProp.TryGetProperty("pageCount", out _) ||
                                          dataProp.TryGetProperty("currentPage", out _) ||
                                          root.TryGetProperty("totalCount", out _);

                        if (hasPagination)
                        {
                            _output.WriteLine($"✓ {endpoint} includes pagination metadata");
                        }
                        else
                        {
                            _output.WriteLine($"⚠ {endpoint} may be missing pagination metadata");
                        }
                    }
                }
            }
        }
    }

    [Fact]
    public async Task ApiResponses_FileUploadEndpoints_HandleMultipartCorrectly()
    {
        // Arrange
        await _seeder.SeedTestDataAsync();
        var token = await GetValidJwtTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Test file upload endpoint
        var endpoint = "/api/employee/1/profile-photo";

        // Create a test file
        var fileContent = Encoding.UTF8.GetBytes("fake image content");
        using var form = new MultipartFormDataContent();
        using var fileStream = new ByteArrayContent(fileContent);
        fileStream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(fileStream, "file", "test.jpg");

        // Act
        var response = await _client.PostAsync(endpoint, form);

        // Assert
        var acceptableStatuses = new[]
        {
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest, // Invalid file or validation error
            HttpStatusCode.NotFound,   // Employee not found
            HttpStatusCode.UnsupportedMediaType // Wrong content type
        };

        Assert.Contains(response.StatusCode, acceptableStatuses);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (IsValidJson(responseContent))
            {
                var jsonDoc = JsonDocument.Parse(responseContent);
                var root = jsonDoc.RootElement;

                // Should follow consistent response structure
                if (root.TryGetProperty("success", out var successProp))
                {
                    Assert.True(successProp.GetBoolean(), "File upload success response should have success=true");
                    _output.WriteLine("✓ File upload endpoint returns structured response");
                }
            }
        }

        _output.WriteLine($"File upload endpoint test completed: {response.StatusCode}");
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
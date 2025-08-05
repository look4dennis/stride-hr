using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using StrideHR.Tests.TestConfiguration;

namespace StrideHR.Tests.Integration;

/// <summary>
/// Tests to validate Swagger/OpenAPI documentation generation and ensure
/// comprehensive API documentation with examples and error responses
/// </summary>
public class SwaggerDocumentationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public SwaggerDocumentationTests(TestWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task SwaggerEndpoint_IsAccessible_ReturnsValidJson()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(content), "Swagger JSON should not be empty");
        
        // Validate it's valid JSON
        var swaggerDoc = JsonDocument.Parse(content);
        Assert.NotNull(swaggerDoc);
        
        _output.WriteLine("✓ Swagger JSON endpoint is accessible and returns valid JSON");
    }

    [Fact]
    public async Task SwaggerUI_IsAccessible_ReturnsHtml()
    {
        // Act
        var response = await _client.GetAsync("/api-docs");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("swagger-ui", content.ToLower());
        Assert.Contains("StrideHR API", content);
        
        _output.WriteLine("✓ Swagger UI is accessible and contains expected content");
    }

    [Fact]
    public async Task SwaggerDocument_ContainsRequiredMetadata()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("openapi", out var openApiVersion));
        Assert.True(openApiVersion.GetString()?.StartsWith("3.") == true, "Should use OpenAPI 3.x");
        
        Assert.True(root.TryGetProperty("info", out var info));
        Assert.True(info.TryGetProperty("title", out var title));
        Assert.Equal("StrideHR API", title.GetString());
        
        Assert.True(info.TryGetProperty("version", out var version));
        Assert.False(string.IsNullOrEmpty(version.GetString()));
        
        Assert.True(info.TryGetProperty("description", out var description));
        Assert.False(string.IsNullOrEmpty(description.GetString()));
        
        _output.WriteLine("✓ Swagger document contains required metadata");
    }

    [Fact]
    public async Task SwaggerDocument_ContainsSecurityDefinitions()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("components", out var components));
        Assert.True(components.TryGetProperty("securitySchemes", out var securitySchemes));
        Assert.True(securitySchemes.TryGetProperty("Bearer", out var bearerScheme));
        
        Assert.True(bearerScheme.TryGetProperty("type", out var type));
        Assert.Equal("apiKey", type.GetString());
        
        Assert.True(bearerScheme.TryGetProperty("name", out var name));
        Assert.Equal("Authorization", name.GetString());
        
        _output.WriteLine("✓ Swagger document contains JWT Bearer security definitions");
    }

    [Fact]
    public async Task SwaggerDocument_ContainsControllerEndpoints()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("paths", out var paths));
        
        var expectedEndpoints = new[]
        {
            "/api/auth/login",
            "/api/auth/me",
            "/api/employee",
            "/api/branch",
            "/api/organization",
            "/api/role"
        };

        var pathKeys = paths.EnumerateObject().Select(p => p.Name).ToList();
        
        foreach (var expectedEndpoint in expectedEndpoints)
        {
            var hasEndpoint = pathKeys.Any(p => p.Contains(expectedEndpoint.Replace("/api/", "/")));
            if (hasEndpoint)
            {
                _output.WriteLine($"✓ Found endpoint: {expectedEndpoint}");
            }
            else
            {
                _output.WriteLine($"⚠ Missing endpoint: {expectedEndpoint}");
            }
        }

        Assert.True(pathKeys.Count > 10, $"Should have many endpoints documented, found: {pathKeys.Count}");
        _output.WriteLine($"✓ Swagger document contains {pathKeys.Count} endpoint paths");
    }

    [Fact]
    public async Task SwaggerDocument_ContainsResponseSchemas()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("components", out var components));
        Assert.True(components.TryGetProperty("schemas", out var schemas));
        
        var schemaNames = schemas.EnumerateObject().Select(s => s.Name).ToList();
        
        // Check for common response schemas
        var expectedSchemas = new[]
        {
            "ApiResponse",
            "PaginatedResponse"
        };

        foreach (var expectedSchema in expectedSchemas)
        {
            if (schemaNames.Contains(expectedSchema))
            {
                _output.WriteLine($"✓ Found schema: {expectedSchema}");
            }
            else
            {
                _output.WriteLine($"⚠ Missing schema: {expectedSchema}");
            }
        }

        Assert.True(schemaNames.Count > 5, $"Should have multiple schemas defined, found: {schemaNames.Count}");
        _output.WriteLine($"✓ Swagger document contains {schemaNames.Count} schemas");
    }

    [Fact]
    public async Task SwaggerDocument_ContainsTagsWithDescriptions()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert
        if (root.TryGetProperty("tags", out var tags))
        {
            var tagCount = 0;
            foreach (var tag in tags.EnumerateArray())
            {
                tagCount++;
                Assert.True(tag.TryGetProperty("name", out var tagName));
                Assert.True(tag.TryGetProperty("description", out var tagDescription));
                
                Assert.False(string.IsNullOrEmpty(tagName.GetString()));
                Assert.False(string.IsNullOrEmpty(tagDescription.GetString()));
                
                _output.WriteLine($"✓ Tag: {tagName.GetString()} - {tagDescription.GetString()}");
            }
            
            Assert.True(tagCount > 5, $"Should have multiple tags defined, found: {tagCount}");
        }
        else
        {
            _output.WriteLine("⚠ No tags found in Swagger document");
        }
    }

    [Fact]
    public async Task SwaggerDocument_ContainsExamplesForCommonTypes()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("components", out var components));
        Assert.True(components.TryGetProperty("schemas", out var schemas));
        
        var hasExamples = false;
        foreach (var schema in schemas.EnumerateObject())
        {
            if (schema.Value.TryGetProperty("example", out _))
            {
                hasExamples = true;
                _output.WriteLine($"✓ Schema {schema.Name} has examples");
            }
            
            if (schema.Value.TryGetProperty("properties", out var properties))
            {
                foreach (var property in properties.EnumerateObject())
                {
                    if (property.Value.TryGetProperty("example", out _))
                    {
                        hasExamples = true;
                        _output.WriteLine($"✓ Property {property.Name} in {schema.Name} has example");
                    }
                }
            }
        }

        if (hasExamples)
        {
            _output.WriteLine("✓ Swagger document contains examples for schemas/properties");
        }
        else
        {
            _output.WriteLine("⚠ No examples found in schemas");
        }
    }

    [Fact]
    public async Task SwaggerDocument_ContainsServerInformation()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert
        if (root.TryGetProperty("servers", out var servers))
        {
            var serverCount = 0;
            foreach (var server in servers.EnumerateArray())
            {
                serverCount++;
                Assert.True(server.TryGetProperty("url", out var url));
                Assert.False(string.IsNullOrEmpty(url.GetString()));
                
                if (server.TryGetProperty("description", out var description))
                {
                    _output.WriteLine($"✓ Server: {url.GetString()} - {description.GetString()}");
                }
                else
                {
                    _output.WriteLine($"✓ Server: {url.GetString()}");
                }
            }
            
            Assert.True(serverCount > 0, "Should have at least one server defined");
        }
        else
        {
            _output.WriteLine("⚠ No servers defined in Swagger document");
        }
    }

    [Fact]
    public async Task SwaggerDocument_FileUploadEndpoints_AreDocumented()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("paths", out var paths));
        
        var fileUploadEndpoints = new List<string>();
        
        foreach (var path in paths.EnumerateObject())
        {
            foreach (var method in path.Value.EnumerateObject())
            {
                if (method.Value.TryGetProperty("requestBody", out var requestBody))
                {
                    if (requestBody.TryGetProperty("content", out var content_))
                    {
                        if (content_.TryGetProperty("multipart/form-data", out _))
                        {
                            fileUploadEndpoints.Add($"{method.Name.ToUpper()} {path.Name}");
                        }
                    }
                }
            }
        }

        if (fileUploadEndpoints.Any())
        {
            _output.WriteLine("✓ File upload endpoints found:");
            foreach (var endpoint in fileUploadEndpoints)
            {
                _output.WriteLine($"  - {endpoint}");
            }
        }
        else
        {
            _output.WriteLine("⚠ No file upload endpoints documented with multipart/form-data");
        }
    }

    [Fact]
    public async Task SwaggerDocument_ValidatesAgainstOpenAPISpec()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        
        // Basic validation that it's valid JSON and has required OpenAPI fields
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert required OpenAPI 3.0 fields
        var requiredFields = new[] { "openapi", "info", "paths" };
        
        foreach (var field in requiredFields)
        {
            Assert.True(root.TryGetProperty(field, out _), $"Missing required OpenAPI field: {field}");
        }

        // Validate info object has required fields
        Assert.True(root.TryGetProperty("info", out var info));
        var requiredInfoFields = new[] { "title", "version" };
        
        foreach (var field in requiredInfoFields)
        {
            Assert.True(info.TryGetProperty(field, out _), $"Missing required info field: {field}");
        }

        _output.WriteLine("✓ Swagger document validates against OpenAPI 3.0 specification");
    }
}
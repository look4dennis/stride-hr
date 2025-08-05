using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using StrideHR.Tests.TestConfiguration;

namespace StrideHR.Tests.Integration;

/// <summary>
/// Comprehensive validation tests for Swagger/OpenAPI documentation
/// to ensure all endpoints are properly documented with examples and error responses
/// </summary>
public class SwaggerValidationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public SwaggerValidationTests(TestWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task SwaggerDocument_AllEndpoints_HaveProperDocumentation()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("paths", out var paths));

        var undocumentedEndpoints = new List<string>();
        var wellDocumentedEndpoints = new List<string>();

        foreach (var path in paths.EnumerateObject())
        {
            foreach (var method in path.Value.EnumerateObject())
            {
                var endpoint = $"{method.Name.ToUpper()} {path.Name}";
                
                var hasSummary = method.Value.TryGetProperty("summary", out var summary) && 
                                !string.IsNullOrEmpty(summary.GetString());
                
                var hasDescription = method.Value.TryGetProperty("description", out var description) && 
                                   !string.IsNullOrEmpty(description.GetString());
                
                var hasResponses = method.Value.TryGetProperty("responses", out var responses) && 
                                 responses.EnumerateObject().Any();

                if (hasSummary && hasResponses)
                {
                    wellDocumentedEndpoints.Add(endpoint);
                }
                else
                {
                    undocumentedEndpoints.Add(endpoint);
                }
            }
        }

        // Report results
        _output.WriteLine($"Well-documented endpoints: {wellDocumentedEndpoints.Count}");
        foreach (var endpoint in wellDocumentedEndpoints.Take(10))
        {
            _output.WriteLine($"  ✓ {endpoint}");
        }

        if (undocumentedEndpoints.Any())
        {
            _output.WriteLine($"Underdocumented endpoints: {undocumentedEndpoints.Count}");
            foreach (var endpoint in undocumentedEndpoints.Take(10))
            {
                _output.WriteLine($"  ⚠ {endpoint}");
            }
        }

        // At least 80% of endpoints should be well-documented
        var totalEndpoints = wellDocumentedEndpoints.Count + undocumentedEndpoints.Count;
        var documentationRate = (double)wellDocumentedEndpoints.Count / totalEndpoints;
        
        Assert.True(documentationRate >= 0.8, 
            $"Documentation rate {documentationRate:P} is below 80%. " +
            $"Well-documented: {wellDocumentedEndpoints.Count}, " +
            $"Underdocumented: {undocumentedEndpoints.Count}");
    }

    [Fact]
    public async Task SwaggerDocument_FileUploadEndpoints_AreProperlyDocumented()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("paths", out var paths));

        var fileUploadEndpoints = new List<string>();
        var properlyDocumentedUploads = new List<string>();

        foreach (var path in paths.EnumerateObject())
        {
            foreach (var method in path.Value.EnumerateObject())
            {
                if (method.Value.TryGetProperty("requestBody", out var requestBody))
                {
                    if (requestBody.TryGetProperty("content", out var content_))
                    {
                        if (content_.TryGetProperty("multipart/form-data", out var multipartContent))
                        {
                            var endpoint = $"{method.Name.ToUpper()} {path.Name}";
                            fileUploadEndpoints.Add(endpoint);

                            // Check if it has proper documentation
                            var hasSummary = method.Value.TryGetProperty("summary", out _);
                            var hasDescription = method.Value.TryGetProperty("description", out _);
                            var hasFileSchema = multipartContent.TryGetProperty("schema", out var schema) &&
                                              schema.TryGetProperty("properties", out var properties) &&
                                              properties.EnumerateObject().Any(p => 
                                                  p.Value.TryGetProperty("format", out var format) && 
                                                  format.GetString() == "binary");

                            if (hasSummary && hasDescription && hasFileSchema)
                            {
                                properlyDocumentedUploads.Add(endpoint);
                            }
                        }
                    }
                }
            }
        }

        _output.WriteLine($"File upload endpoints found: {fileUploadEndpoints.Count}");
        foreach (var endpoint in fileUploadEndpoints)
        {
            var isWellDocumented = properlyDocumentedUploads.Contains(endpoint);
            _output.WriteLine($"  {(isWellDocumented ? "✓" : "⚠")} {endpoint}");
        }

        if (fileUploadEndpoints.Any())
        {
            // All file upload endpoints should be properly documented
            Assert.Equal(fileUploadEndpoints.Count, properlyDocumentedUploads.Count);
        }
    }

    [Fact]
    public async Task SwaggerDocument_ErrorResponses_AreDocumented()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("paths", out var paths));

        var endpointsWithErrorResponses = 0;
        var totalEndpoints = 0;

        foreach (var path in paths.EnumerateObject())
        {
            foreach (var method in path.Value.EnumerateObject())
            {
                totalEndpoints++;
                
                if (method.Value.TryGetProperty("responses", out var responses))
                {
                    var hasErrorResponses = responses.EnumerateObject()
                        .Any(r => r.Name.StartsWith("4") || r.Name.StartsWith("5"));
                    
                    if (hasErrorResponses)
                    {
                        endpointsWithErrorResponses++;
                    }
                }
            }
        }

        var errorDocumentationRate = (double)endpointsWithErrorResponses / totalEndpoints;
        
        _output.WriteLine($"Endpoints with error responses documented: {endpointsWithErrorResponses}/{totalEndpoints} ({errorDocumentationRate:P})");
        
        // At least 70% of endpoints should document error responses
        Assert.True(errorDocumentationRate >= 0.7, 
            $"Error response documentation rate {errorDocumentationRate:P} is below 70%");
    }

    [Fact]
    public async Task SwaggerDocument_AuthenticationRequirements_AreDocumented()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("paths", out var paths));

        var securedEndpoints = 0;
        var totalEndpoints = 0;

        foreach (var path in paths.EnumerateObject())
        {
            foreach (var method in path.Value.EnumerateObject())
            {
                totalEndpoints++;
                
                if (method.Value.TryGetProperty("security", out var security) && 
                    security.EnumerateArray().Any())
                {
                    securedEndpoints++;
                }
            }
        }

        var securityDocumentationRate = (double)securedEndpoints / totalEndpoints;
        
        _output.WriteLine($"Endpoints with security requirements: {securedEndpoints}/{totalEndpoints} ({securityDocumentationRate:P})");
        
        // Most endpoints should require authentication (except login, health check, etc.)
        Assert.True(securityDocumentationRate >= 0.6, 
            $"Security documentation rate {securityDocumentationRate:P} is below 60%");
    }

    [Fact]
    public async Task SwaggerDocument_ParameterExamples_AreProvided()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("paths", out var paths));

        var endpointsWithParameters = 0;
        var endpointsWithParameterExamples = 0;

        foreach (var path in paths.EnumerateObject())
        {
            foreach (var method in path.Value.EnumerateObject())
            {
                if (method.Value.TryGetProperty("parameters", out var parameters) && 
                    parameters.EnumerateArray().Any())
                {
                    endpointsWithParameters++;
                    
                    var hasExamples = parameters.EnumerateArray()
                        .Any(p => p.TryGetProperty("example", out _) || 
                                 (p.TryGetProperty("schema", out var schema) && 
                                  schema.TryGetProperty("example", out _)));
                    
                    if (hasExamples)
                    {
                        endpointsWithParameterExamples++;
                    }
                }
            }
        }

        if (endpointsWithParameters > 0)
        {
            var exampleRate = (double)endpointsWithParameterExamples / endpointsWithParameters;
            
            _output.WriteLine($"Endpoints with parameter examples: {endpointsWithParameterExamples}/{endpointsWithParameters} ({exampleRate:P})");
            
            // At least 50% of endpoints with parameters should have examples
            Assert.True(exampleRate >= 0.5, 
                $"Parameter example rate {exampleRate:P} is below 50%");
        }
        else
        {
            _output.WriteLine("No endpoints with parameters found");
        }
    }

    [Fact]
    public async Task SwaggerDocument_ResponseSchemas_AreWellDefined()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("paths", out var paths));

        var responsesWithSchemas = 0;
        var totalResponses = 0;

        foreach (var path in paths.EnumerateObject())
        {
            foreach (var method in path.Value.EnumerateObject())
            {
                if (method.Value.TryGetProperty("responses", out var responses))
                {
                    foreach (var responseCode in responses.EnumerateObject())
                    {
                        totalResponses++;
                        
                        if (responseCode.Value.TryGetProperty("content", out var content_))
                        {
                            var hasSchema = content_.EnumerateObject()
                                .Any(c => c.Value.TryGetProperty("schema", out _));
                            
                            if (hasSchema)
                            {
                                responsesWithSchemas++;
                            }
                        }
                    }
                }
            }
        }

        if (totalResponses > 0)
        {
            var schemaRate = (double)responsesWithSchemas / totalResponses;
            
            _output.WriteLine($"Responses with schemas: {responsesWithSchemas}/{totalResponses} ({schemaRate:P})");
            
            // At least 60% of responses should have schemas
            Assert.True(schemaRate >= 0.6, 
                $"Response schema rate {schemaRate:P} is below 60%");
        }
    }

    [Fact]
    public async Task SwaggerDocument_ConsistentResponseStructure_IsDocumented()
    {
        // Act
        var response = await _client.GetAsync("/api-docs/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        var root = swaggerDoc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("components", out var components));
        Assert.True(components.TryGetProperty("schemas", out var schemas));

        // Check for standard response schemas
        var hasApiResponse = schemas.TryGetProperty("ApiResponse", out _);
        var hasPaginatedResponse = schemas.TryGetProperty("PaginatedResponse", out _);

        if (hasApiResponse)
        {
            _output.WriteLine("✓ ApiResponse schema is documented");
        }
        else
        {
            _output.WriteLine("⚠ ApiResponse schema is missing");
        }

        if (hasPaginatedResponse)
        {
            _output.WriteLine("✓ PaginatedResponse schema is documented");
        }
        else
        {
            _output.WriteLine("⚠ PaginatedResponse schema is missing");
        }

        // At least one standard response schema should be present
        Assert.True(hasApiResponse || hasPaginatedResponse, 
            "Standard response schemas (ApiResponse or PaginatedResponse) should be documented");
    }
}
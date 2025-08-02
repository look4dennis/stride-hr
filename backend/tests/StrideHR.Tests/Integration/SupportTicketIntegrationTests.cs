using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using StrideHR.API;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.SupportTicket;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using StrideHR.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace StrideHR.Tests.Integration;

public class SupportTicketIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SupportTicketIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StrideHRDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<StrideHRDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}");
                });

                // Replace authorization with a policy that always succeeds for testing
                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .RequireAssertion(_ => true)
                        .Build();
                    
                    // Add specific policies that always succeed for testing
                    options.AddPolicy("Permission:SupportTicket.Create", policy => 
                        policy.RequireAssertion(_ => true));
                    options.AddPolicy("Permission:SupportTicket.Read", policy => 
                        policy.RequireAssertion(_ => true));
                    options.AddPolicy("Permission:SupportTicket.Update", policy => 
                        policy.RequireAssertion(_ => true));
                    options.AddPolicy("Permission:SupportTicket.Delete", policy => 
                        policy.RequireAssertion(_ => true));
                    options.AddPolicy("Permission:SupportTicket.ViewAnalytics", policy => 
                        policy.RequireAssertion(_ => true));
                    options.AddPolicy("Permission:SupportTicket.Assign", policy => 
                        policy.RequireAssertion(_ => true));
                    options.AddPolicy("Permission:SupportTicket.UpdateStatus", policy => 
                        policy.RequireAssertion(_ => true));
                    options.AddPolicy("Permission:SupportTicket.Resolve", policy => 
                        policy.RequireAssertion(_ => true));
                    options.AddPolicy("Permission:SupportTicket.Close", policy => 
                        policy.RequireAssertion(_ => true));
                    options.AddPolicy("Permission:SupportTicket.Reopen", policy => 
                        policy.RequireAssertion(_ => true));
                    options.AddPolicy("Permission:SupportTicket.ViewAssigned", policy => 
                        policy.RequireAssertion(_ => true));
                    options.AddPolicy("Permission:SupportTicket.ViewOverdue", policy => 
                        policy.RequireAssertion(_ => true));
                    options.AddPolicy("Permission:SupportTicket.Comment", policy => 
                        policy.RequireAssertion(_ => true));
                });

                // Add test authentication
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            });
        });
        _client = CreateAuthenticatedClient();
        
        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();
        
        // Ensure database is created
        context.Database.EnsureCreated();
        
        // Add test organization
        if (!context.Organizations.Any())
        {
            var organization = new Organization
            {
                Id = 1,
                Name = "Test Organization",
                Email = "test@test.com",
                Phone = "123-456-7890",
                Address = "Test Address",
                CreatedAt = DateTime.UtcNow
            };
            context.Organizations.Add(organization);
        }

        // Add test branch
        if (!context.Branches.Any())
        {
            var branch = new Branch
            {
                Id = 1,
                OrganizationId = 1,
                Name = "Test Branch",
                Email = "branch@test.com",
                Phone = "123-456-7890",
                Address = "Branch Address",
                City = "Branch City",
                State = "Branch State",
                Country = "Branch Country",
                PostalCode = "12345",
                TimeZone = "UTC",
                Currency = "USD",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Branches.Add(branch);
        }

        // Add test employee
        if (!context.Employees.Any())
        {
            var employee = new Employee
            {
                Id = 1,
                BranchId = 1,
                EmployeeId = "EMP001",
                FirstName = "Test",
                LastName = "Employee",
                Email = "test.employee@test.com",
                Phone = "123-456-7890",
                DateOfBirth = new DateTime(1990, 1, 1),
                Address = "Employee Address",
                JoiningDate = DateTime.UtcNow.AddYears(-1),
                Designation = "Test Employee",
                Department = "IT",
                Status = EmployeeStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
            context.Employees.Add(employee);
        }

        context.SaveChanges();
    }

    private HttpClient CreateAuthenticatedClient()
    {
        return _factory.CreateClient();
    }

    [Fact]
    public async Task CreateSupportTicket_ValidData_ReturnsCreatedTicket()
    {
        // Arrange
        var createDto = new CreateSupportTicketDto
        {
            Title = "Integration Test Ticket",
            Description = "This is a test ticket created during integration testing",
            Category = SupportTicketCategory.Software,
            Priority = SupportTicketPriority.Medium,
            RequiresRemoteAccess = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/supportticket", createDto);

        // Debug: Print response details if not successful
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed with status {response.StatusCode}: {errorContent}");
        }

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<SupportTicketDto>>(content, options);
        
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(createDto.Title, apiResponse.Data.Title);
        Assert.Equal(createDto.Description, apiResponse.Data.Description);
        Assert.Equal(SupportTicketStatus.Open, apiResponse.Data.Status);
    }

    [Fact]
    public async Task GetSupportTicket_ExistingId_ReturnsTicket()
    {
        // Arrange
        // First create a ticket
        var createDto = new CreateSupportTicketDto
        {
            Title = "Test Ticket for Get",
            Description = "Test Description",
            Category = SupportTicketCategory.Hardware,
            Priority = SupportTicketPriority.High,
            RequiresRemoteAccess = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/supportticket", createDto);
        
        // Debug: Check if creation was successful
        if (!createResponse.IsSuccessStatusCode)
        {
            var errorContent = await createResponse.Content.ReadAsStringAsync();
            throw new Exception($"Ticket creation failed with status {createResponse.StatusCode}: {errorContent}");
        }
        
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var createApiResponse = JsonSerializer.Deserialize<ApiResponse<SupportTicketDto>>(createContent, options);
        
        if (createApiResponse?.Data == null)
        {
            throw new Exception($"Failed to deserialize ticket creation response: {createContent}");
        }
        
        var ticketId = createApiResponse.Data.Id;

        // Act
        var response = await _client.GetAsync($"/api/supportticket/{ticketId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<SupportTicketDto>>(content, options);
        
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(ticketId, apiResponse.Data.Id);
        Assert.Equal(createDto.Title, apiResponse.Data.Title);
    }

    [Fact]
    public async Task SearchSupportTickets_WithFilters_ReturnsFilteredResults()
    {
        // Arrange
        var searchCriteria = new SupportTicketSearchCriteria
        {
            Status = SupportTicketStatus.Open,
            Category = SupportTicketCategory.Software,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/supportticket/search", searchCriteria);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, options);
        
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
    }

    [Fact]
    public async Task AddCommentToTicket_ValidData_ReturnsComment()
    {
        // Arrange
        // First create a ticket
        var createDto = new CreateSupportTicketDto
        {
            Title = "Test Ticket for Comment",
            Description = "Test Description",
            Category = SupportTicketCategory.Network,
            Priority = SupportTicketPriority.Low,
            RequiresRemoteAccess = false
        };

        var createResponse = await _client.PostAsJsonAsync("/api/supportticket", createDto);
        
        // Debug: Check if creation was successful
        if (!createResponse.IsSuccessStatusCode)
        {
            var errorContent = await createResponse.Content.ReadAsStringAsync();
            throw new Exception($"Ticket creation failed with status {createResponse.StatusCode}: {errorContent}");
        }
        
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var createApiResponse = JsonSerializer.Deserialize<ApiResponse<SupportTicketDto>>(createContent, options);
        
        if (createApiResponse?.Data == null)
        {
            throw new Exception($"Failed to deserialize ticket creation response: {createContent}");
        }
        
        var ticketId = createApiResponse.Data.Id;

        var commentDto = new CreateSupportTicketCommentDto
        {
            Comment = "This is a test comment",
            IsInternal = false
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/supportticket/{ticketId}/comments", commentDto);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<SupportTicketCommentDto>>(content, options);
        
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(commentDto.Comment, apiResponse.Data.Comment);
        Assert.Equal(commentDto.IsInternal, apiResponse.Data.IsInternal);
    }

    [Fact]
    public async Task GetSupportTicketAnalytics_ReturnsAnalyticsData()
    {
        // Act
        var response = await _client.GetAsync("/api/supportticket/analytics");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<SupportTicketAnalyticsDto>>(content, options);
        
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.True(apiResponse.Data.TotalTickets >= 0);
    }
}

public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim("EmployeeId", "1"),
            new Claim(ClaimTypes.Name, "Test Employee"),
            new Claim(ClaimTypes.Email, "test.employee@test.com")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// Helper class for API response deserialization
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}
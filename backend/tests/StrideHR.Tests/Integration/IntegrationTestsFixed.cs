// This file contains the corrected integration tests with shared utilities

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using StrideHR.API;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.Employee;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using StrideHR.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FluentAssertions;
using StrideHR.Tests.TestConfiguration;
using Microsoft.AspNetCore.Authentication;

namespace StrideHR.Tests.Integration;

/// <summary>
/// Fixed integration tests using shared test utilities
/// This demonstrates the corrected approach for all integration tests
/// </summary>
public class FixedEmployeeIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public FixedEmployeeIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StrideHRDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<StrideHRDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}");
                });

                ConfigureTestAuthorization(services);

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            });
        });
        _client = _factory.CreateClient();
        
        SeedTestData();
    }

    private void ConfigureTestAuthorization(IServiceCollection services)
    {
        services.RemoveAll<IAuthorizationService>();
        services.RemoveAll<IAuthorizationPolicyProvider>();
        services.RemoveAll<IAuthorizationHandlerProvider>();

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build();
            
            var policies = new[]
            {
                "Permission:Employee.Create",
                "Permission:Employee.Read", 
                "Permission:Employee.Update",
                "Permission:Employee.Delete",
                "Permission:Employee.ViewAll",
                "Permission:Employee.ViewProfile",
                "Permission:Employee.UpdateProfile"
            };

            foreach (var policy in policies)
            {
                options.AddPolicy(policy, policyBuilder => 
                    policyBuilder.RequireAssertion(_ => true));
            }
        });
    }

    private void SeedTestData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();
        
        context.Database.EnsureCreated();
        
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

        context.SaveChanges();
    }

    [Fact]
    public async Task CreateEmployee_ValidData_ReturnsCreatedEmployee()
    {
        // Arrange
        var createDto = new CreateEmployeeDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "123 Test Street",
            JoiningDate = DateTime.UtcNow,
            Designation = "Software Developer",
            Department = "IT",
            BranchId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/employee", createDto);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<TestApiResponse<EmployeeDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.FirstName.Should().Be(createDto.FirstName);
        apiResponse.Data.LastName.Should().Be(createDto.LastName);
        apiResponse.Data.Email.Should().Be(createDto.Email);
        apiResponse.Data.Status.Should().Be(EmployeeStatus.Active);
    }

    [Fact]
    public async Task GetEmployee_ExistingId_ReturnsEmployee()
    {
        // Arrange - First create an employee
        var createDto = new CreateEmployeeDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1985, 5, 15),
            Address = "456 Test Avenue",
            JoiningDate = DateTime.UtcNow,
            Designation = "Project Manager",
            Department = "IT",
            BranchId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/employee", createDto);
        createResponse.IsSuccessStatusCode.Should().BeTrue();
        
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var createApiResponse = JsonSerializer.Deserialize<TestApiResponse<EmployeeDto>>(createContent, options);
        var employeeId = createApiResponse!.Data!.Id;

        // Act
        var response = await _client.GetAsync($"/api/employee/{employeeId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<TestApiResponse<EmployeeDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(employeeId);
        apiResponse.Data.FirstName.Should().Be(createDto.FirstName);
        apiResponse.Data.LastName.Should().Be(createDto.LastName);
    }
}
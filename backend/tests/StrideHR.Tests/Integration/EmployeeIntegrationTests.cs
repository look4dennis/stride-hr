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

public class EmployeeIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EmployeeIntegrationTests(WebApplicationFactory<Program> factory)
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

                // Configure test authorization
                ConfigureTestAuthorization(services);

                // Add test authentication
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            });
        });
        _client = _factory.CreateClient();
        
        // Seed test data
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
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(content, options);
        
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
        var createApiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(createContent, options);
        var employeeId = createApiResponse!.Data!.Id;

        // Act
        var response = await _client.GetAsync($"/api/employee/{employeeId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(employeeId);
        apiResponse.Data.FirstName.Should().Be(createDto.FirstName);
        apiResponse.Data.LastName.Should().Be(createDto.LastName);
    }

    [Fact]
    public async Task UpdateEmployee_ValidData_ReturnsUpdatedEmployee()
    {
        // Arrange - First create an employee
        var createDto = new CreateEmployeeDto
        {
            FirstName = "Bob",
            LastName = "Johnson",
            Email = "bob.johnson@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1988, 3, 20),
            Address = "789 Test Boulevard",
            JoiningDate = DateTime.UtcNow,
            Designation = "Developer",
            Department = "IT",
            BranchId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/employee", createDto);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var createApiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(createContent, options);
        var employeeId = createApiResponse!.Data!.Id;

        var updateDto = new UpdateEmployeeDto
        {
            FirstName = "Robert",
            LastName = "Johnson",
            Email = "robert.johnson@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1988, 3, 20),
            Address = "789 Test Boulevard",
            Designation = "Senior Developer",
            Department = "IT"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/employee/{employeeId}", updateDto);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.FirstName.Should().Be(updateDto.FirstName);
        apiResponse.Data.Designation.Should().Be(updateDto.Designation);
    }

    [Fact]
    public async Task SearchEmployees_WithFilters_ReturnsFilteredResults()
    {
        // Arrange - Create multiple employees
        var employees = new[]
        {
            new CreateEmployeeDto
            {
                FirstName = "Alice",
                LastName = "Wilson",
                Email = "alice.wilson@test.com",
                Phone = "123-456-7890",
                DateOfBirth = new DateTime(1992, 7, 10),
                Address = "111 Test Street",
                JoiningDate = DateTime.UtcNow,
                Designation = "Developer",
                Department = "IT",
                BranchId = 1
            },
            new CreateEmployeeDto
            {
                FirstName = "Charlie",
                LastName = "Brown",
                Email = "charlie.brown@test.com",
                Phone = "123-456-7890",
                DateOfBirth = new DateTime(1987, 12, 5),
                Address = "222 Test Avenue",
                JoiningDate = DateTime.UtcNow,
                Designation = "Manager",
                Department = "HR",
                BranchId = 1
            }
        };

        foreach (var emp in employees)
        {
            await _client.PostAsJsonAsync("/api/employee", emp);
        }

        var searchCriteria = new EmployeeSearchCriteria
        {
            Department = "IT",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/employee/search", searchCriteria);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task DeactivateEmployee_ExistingEmployee_ReturnsSuccess()
    {
        // Arrange - First create an employee
        var createDto = new CreateEmployeeDto
        {
            FirstName = "David",
            LastName = "Miller",
            Email = "david.miller@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1990, 9, 25),
            Address = "333 Test Road",
            JoiningDate = DateTime.UtcNow,
            Designation = "Analyst",
            Department = "Finance",
            BranchId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/employee", createDto);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var createApiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(createContent, options);
        var employeeId = createApiResponse!.Data!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/employee/{employeeId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        // Verify employee is deactivated
        var getResponse = await _client.GetAsync($"/api/employee/{employeeId}");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getApiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(getContent, options);
        
        getApiResponse!.Data!.Status.Should().Be(EmployeeStatus.Inactive);
    }

    [Fact]
    public async Task GetEmployeesByBranch_ValidBranchId_ReturnsEmployees()
    {
        // Arrange - Create employees in the branch
        var createDto = new CreateEmployeeDto
        {
            FirstName = "Emma",
            LastName = "Davis",
            Email = "emma.davis@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1991, 4, 18),
            Address = "444 Test Lane",
            JoiningDate = DateTime.UtcNow,
            Designation = "Designer",
            Department = "Marketing",
            BranchId = 1
        };

        await _client.PostAsJsonAsync("/api/employee", createDto);

        // Act
        var response = await _client.GetAsync("/api/employee/branch/1");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<EmployeeDto>>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Should().NotBeEmpty();
        apiResponse.Data.Should().Contain(e => e.FirstName == "Emma" && e.LastName == "Davis");
    }
}


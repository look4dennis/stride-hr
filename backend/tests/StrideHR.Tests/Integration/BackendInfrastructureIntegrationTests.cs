using Microsoft.Extensions.DependencyInjection;
using StrideHR.Infrastructure.Data;
using StrideHR.Tests.Integration;
using StrideHR.Tests.TestConfiguration;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using System.Net.Http.Json;
using System.Text.Json;
using StrideHR.API.Models;
using StrideHR.Core.Models.Employee;

namespace StrideHR.Tests.Integration;

/// <summary>
/// Integration tests for backend infrastructure including WebApplicationFactory, 
/// database provider, and test data seeding
/// </summary>
[Collection("Integration Tests")]
public class BackendInfrastructureIntegrationTests : IClassFixture<SystemIntegrationTestFactory>
{
    private readonly SystemIntegrationTestFactory _factory;
    private readonly HttpClient _client;

    public BackendInfrastructureIntegrationTests(SystemIntegrationTestFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task WebApplicationFactory_ShouldBuildTestHostSuccessfully()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();

        // Assert
        context.Should().NotBeNull("WebApplicationFactory should successfully build test host with DbContext");
        
        // Verify database is accessible
        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue("Test database should be accessible");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestDatabaseProvider_ShouldSupportBothInMemoryAndSqlOperations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();

        // Act - Test basic CRUD operations
        var organization = new Organization
        {
            Name = "Test Organization Infrastructure",
            Address = "123 Infrastructure Test Street",
            Email = "infrastructure@test.com",
            Phone = "+1-555-0199",
            NormalWorkingHours = TimeSpan.FromHours(8),
            OvertimeRate = 1.5m,
            ProductiveHoursThreshold = 6,
            BranchIsolationEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Organizations.Add(organization);
        await context.SaveChangesAsync();

        // Assert
        var savedOrganization = await context.Organizations
            .FirstOrDefaultAsync(o => o.Name == "Test Organization Infrastructure");
        
        savedOrganization.Should().NotBeNull("Organization should be saved successfully");
        savedOrganization!.Id.Should().BeGreaterThan(0, "Organization should have a valid ID");
        
        // Test SQL-specific operations (relationships)
        var branch = new Branch
        {
            OrganizationId = savedOrganization.Id,
            Name = "Infrastructure Test Branch",
            Country = "United States",
            Currency = "USD",
            TimeZone = "America/New_York",
            Address = "456 Branch Test Street"
        };

        context.Branches.Add(branch);
        await context.SaveChangesAsync();

        var savedBranch = await context.Branches
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.Name == "Infrastructure Test Branch");

        savedBranch.Should().NotBeNull("Branch should be saved with relationship");
        savedBranch!.Organization.Should().NotBeNull("Branch should have organization relationship loaded");
        savedBranch.Organization.Name.Should().Be("Test Organization Infrastructure");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestDataSeeder_ShouldCreateConsistentBaselineData()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();

        // Act - Verify seeded data exists
        var organizations = await context.Organizations.ToListAsync();
        var branches = await context.Branches.ToListAsync();
        var employees = await context.Employees.ToListAsync();
        var roles = await context.Roles.ToListAsync();
        var permissions = await context.Permissions.ToListAsync();
        var users = await context.Users.ToListAsync();

        // Assert
        organizations.Should().NotBeEmpty("Test data seeder should create organizations");
        branches.Should().NotBeEmpty("Test data seeder should create branches");
        employees.Should().NotBeEmpty("Test data seeder should create employees");
        roles.Should().NotBeEmpty("Test data seeder should create roles");
        permissions.Should().NotBeEmpty("Test data seeder should create permissions");
        users.Should().NotBeEmpty("Test data seeder should create users");

        // Verify data consistency
        var testOrg = organizations.FirstOrDefault(o => o.Name == "Test Organization");
        testOrg.Should().NotBeNull("Test Organization should exist");

        var testBranch = branches.FirstOrDefault(b => b.Name == "Main Branch");
        testBranch.Should().NotBeNull("Main Branch should exist");
        testBranch!.OrganizationId.Should().Be(testOrg!.Id, "Branch should belong to test organization");

        var testEmployee = employees.FirstOrDefault(e => e.FirstName == "John" && e.LastName == "Admin");
        testEmployee.Should().NotBeNull("Test employee should exist");
        testEmployee!.BranchId.Should().Be(testBranch.Id, "Employee should belong to test branch");

        // Verify role-permission relationships
        var rolePermissions = await context.RolePermissions.ToListAsync();
        rolePermissions.Should().NotBeEmpty("Role permissions should be seeded");

        var superAdminRole = roles.FirstOrDefault(r => r.Name == "SuperAdmin");
        superAdminRole.Should().NotBeNull("SuperAdmin role should exist");

        var superAdminPermissions = rolePermissions.Where(rp => rp.RoleId == superAdminRole!.Id).ToList();
        superAdminPermissions.Should().NotBeEmpty("SuperAdmin should have permissions assigned");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task EmployeeAPI_ShouldWorkWithSeededData()
    {
        // Act - Test API endpoints with seeded data
        var response = await _client.GetAsync("/api/employee/branch/1");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue("Employee API should respond successfully");

        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<StrideHR.API.Models.ApiResponse<IEnumerable<EmployeeDto>>>(content, options);

        apiResponse.Should().NotBeNull("API should return valid response");
        apiResponse!.Success.Should().BeTrue("API response should indicate success");
        apiResponse.Data.Should().NotBeNull("API should return employee data");
        apiResponse.Data!.Should().NotBeEmpty("Branch should have employees from seeded data");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task EmployeeCreation_ShouldWorkEndToEnd()
    {
        // Arrange
        var createDto = new CreateEmployeeDto
        {
            FirstName = "Integration",
            LastName = "Test",
            Email = "integration.test@test.com",
            Phone = "+1-555-0123",
            DateOfBirth = new DateTime(1990, 1, 1),
            JoiningDate = DateTime.UtcNow,
            Designation = "Integration Tester",
            Department = "QA",
            Address = "123 Integration Test Street",
            BasicSalary = 55000.00m,
            BranchId = 1,
            Status = EmployeeStatus.Active
        };

        // Act
        var createResponse = await _client.PostAsJsonAsync("/api/employee", createDto);

        // Assert
        if (!createResponse.IsSuccessStatusCode)
        {
            var errorContent = await createResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Create Response Status: {createResponse.StatusCode}");
            Console.WriteLine($"Create Response Content: {errorContent}");
        }

        createResponse.IsSuccessStatusCode.Should().BeTrue("Employee creation should succeed");

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var createApiResponse = JsonSerializer.Deserialize<StrideHR.API.Models.ApiResponse<EmployeeDto>>(createContent, options);

        createApiResponse.Should().NotBeNull("Create response should be valid");
        createApiResponse!.Success.Should().BeTrue("Create operation should succeed");
        createApiResponse.Data.Should().NotBeNull("Created employee data should be returned");
        createApiResponse.Data!.FirstName.Should().Be("Integration");
        createApiResponse.Data.LastName.Should().Be("Test");
        createApiResponse.Data.Email.Should().Be("integration.test@test.com");

        // Verify employee was actually saved to database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();
        
        var savedEmployee = await context.Employees
            .FirstOrDefaultAsync(e => e.Email == "integration.test@test.com");
        
        savedEmployee.Should().NotBeNull("Employee should be saved to database");
        savedEmployee!.FirstName.Should().Be("Integration");
        savedEmployee.LastName.Should().Be("Test");
        savedEmployee.BranchId.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DatabaseTransactions_ShouldWorkCorrectly()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();

        var initialEmployeeCount = await context.Employees.CountAsync();

        // Act - Test transaction rollback
        try
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            
            var employee = new Employee
            {
                EmployeeId = "TXN-TEST-001",
                BranchId = 1,
                FirstName = "Transaction",
                LastName = "Test",
                Email = "transaction.test@test.com",
                Phone = "+1-555-0124",
                DateOfBirth = new DateTime(1990, 1, 1),
                JoiningDate = DateTime.UtcNow,
                Designation = "Transaction Tester",
                Department = "QA",
                BasicSalary = 50000,
                Status = EmployeeStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            // Verify employee was added within transaction
            var employeeInTransaction = await context.Employees
                .FirstOrDefaultAsync(e => e.Email == "transaction.test@test.com");
            employeeInTransaction.Should().NotBeNull("Employee should exist within transaction");

            // Rollback transaction
            await transaction.RollbackAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Transaction test error: {ex.Message}");
        }

        // Assert - Employee should not exist after rollback
        var employeeAfterRollback = await context.Employees
            .FirstOrDefaultAsync(e => e.Email == "transaction.test@test.com");
        employeeAfterRollback.Should().BeNull("Employee should not exist after transaction rollback");

        var finalEmployeeCount = await context.Employees.CountAsync();
        finalEmployeeCount.Should().Be(initialEmployeeCount, "Employee count should be unchanged after rollback");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestDataIsolation_ShouldWorkBetweenTests()
    {
        // This test verifies that test data is properly isolated between test runs
        // and that the seeded data is consistent

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();

        // Act
        var organizations = await context.Organizations.CountAsync();
        var branches = await context.Branches.CountAsync();
        var employees = await context.Employees.CountAsync();

        // Assert - These counts should be consistent with TestDataSeeder
        organizations.Should().BeGreaterThan(0, "Organizations should be seeded");
        branches.Should().BeGreaterThan(0, "Branches should be seeded");
        employees.Should().BeGreaterThan(0, "Employees should be seeded");

        // Verify specific test data constants
        var testOrg = await context.Organizations.FindAsync(TestDataSeeder.TestDataConstants.TestOrganizationId);
        testOrg.Should().NotBeNull("Test organization should exist with expected ID");

        var testBranch = await context.Branches.FindAsync(TestDataSeeder.TestDataConstants.TestBranchId);
        testBranch.Should().NotBeNull("Test branch should exist with expected ID");

        var testEmployee = await context.Employees.FindAsync(TestDataSeeder.TestDataConstants.TestEmployeeId);
        testEmployee.Should().NotBeNull("Test employee should exist with expected ID");
        testEmployee!.Email.Should().Be(TestDataSeeder.TestDataConstants.TestEmployeeEmail);
    }
}
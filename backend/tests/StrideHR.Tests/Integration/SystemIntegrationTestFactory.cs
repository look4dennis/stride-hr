using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StrideHR.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using StrideHR.Core.Entities;

namespace StrideHR.Tests.Integration
{
    public class SystemIntegrationTestFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Use test configuration
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:",
                    ["JwtSettings:SecretKey"] = "test-super-secret-jwt-key-for-integration-testing-that-is-long-enough",
                    ["JwtSettings:Issuer"] = "StrideHR-Test",
                    ["JwtSettings:Audience"] = "StrideHR-Test-Users",
                    ["JwtSettings:ExpirationHours"] = "24",
                    ["EncryptionSettings:MasterKey"] = "test-encryption-key-for-integration-testing-that-is-long-enough",
                    ["EncryptionSettings:Salt"] = "test-salt-value",
                    ["EncryptionSettings:EnableEncryption"] = "false"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<StrideHRDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<StrideHRDbContext>(options =>
                {
                    options.UseInMemoryDatabase("StrideHR_Test");
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                // Ensure database is created
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();
                
                try
                {
                    context.Database.EnsureCreated();
                    
                    // Seed test data
                    SeedTestData(context);
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<SystemIntegrationTestFactory>>();
                    logger.LogError(ex, "An error occurred while setting up the test database");
                }
            });

            builder.UseEnvironment("Testing");
        }

        private static void SeedTestData(StrideHRDbContext context)
        {
            // Create test organization
            var organization = new Organization
            {
                Name = "Test Organization",
                Address = "123 Test St",
                Email = "test@test.com",
                Phone = "1234567890",
                NormalWorkingHours = TimeSpan.FromHours(8),
                OvertimeRate = 1.5m,
                ProductiveHoursThreshold = 6,
                BranchIsolationEnabled = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Organizations.Add(organization);
            context.SaveChanges();

            // Create test branch
            var branch = new Branch
            {
                OrganizationId = organization.Id,
                Name = "Test Branch",
                Country = "Test Country",
                Currency = "USD",
                TimeZone = "UTC",
                Address = "456 Test Ave"
            };

            context.Branches.Add(branch);
            context.SaveChanges();

            // Create test roles
            var adminRole = new Role
            {
                Name = "Admin",
                Description = "Administrator role",
                HierarchyLevel = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var employeeRole = new Role
            {
                Name = "Employee",
                Description = "Employee role",
                HierarchyLevel = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Roles.AddRange(adminRole, employeeRole);
            context.SaveChanges();

            // Create test employee
            var employee = new Employee
            {
                EmployeeId = "TEST-001",
                BranchId = branch.Id,
                FirstName = "Test",
                LastName = "Employee",
                Email = "test.employee@test.com",
                Phone = "1234567890",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                JoiningDate = DateTime.UtcNow.AddMonths(-6),
                Designation = "Test Engineer",
                Department = "Testing",
                BasicSalary = 50000,
                Status = StrideHR.Core.Enums.EmployeeStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            context.Employees.Add(employee);
            context.SaveChanges();
        }
    }
}
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using StrideHR.API;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.Employee;
using StrideHR.Core.Models.Attendance;
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
using Microsoft.Extensions.DependencyInjection.Extensions;
using StrideHR.Infrastructure.DTOs;
using FluentAssertions;
using System.Diagnostics;
using StrideHR.Tests.TestConfiguration;

namespace StrideHR.Tests.Integration;

/// <summary>
/// Performance and load tests to ensure the system can handle expected load
/// and meets performance requirements
/// </summary>
public class PerformanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PerformanceTests(WebApplicationFactory<Program> factory)
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
                "Permission:Employee.Create", "Permission:Employee.Read", "Permission:Employee.ViewAll",
                "Permission:Attendance.CheckIn", "Permission:Attendance.ViewAll",
                "Permission:Payroll.ViewAll", "Permission:Project.ViewAll"
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

        // Seed multiple employees for performance testing
        if (!context.Employees.Any())
        {
            var employees = new List<Employee>();
            for (int i = 1; i <= 100; i++)
            {
                employees.Add(new Employee
                {
                    BranchId = 1,
                    EmployeeId = $"EMP{i:000}",
                    FirstName = $"Employee{i}",
                    LastName = "Test",
                    Email = $"employee{i}@test.com",
                    Phone = "123-456-7890",
                    DateOfBirth = new DateTime(1990, 1, 1).AddDays(i),
                    Address = $"Address {i}",
                    JoiningDate = DateTime.UtcNow.AddDays(-i),
                    Designation = "Test Employee",
                    Department = "IT",
                    BasicSalary = 50000 + (i * 100),
                    Status = EmployeeStatus.Active,
                    CreatedAt = DateTime.UtcNow
                });
            }
            context.Employees.AddRange(employees);
        }

        context.SaveChanges();
    }

    [Fact]
    public async Task EmployeeCreation_BulkOperations_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        var employeesToCreate = 50;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Create multiple employees concurrently
        for (int i = 1; i <= employeesToCreate; i++)
        {
            var createDto = new CreateEmployeeDto
            {
                FirstName = $"BulkEmployee{i}",
                LastName = "Performance",
                Email = $"bulk.employee{i}@test.com",
                Phone = "123-456-7890",
                DateOfBirth = new DateTime(1990, 1, 1),
                Address = $"Bulk Address {i}",
                JoiningDate = DateTime.UtcNow,
                Designation = "Bulk Test Employee",
                Department = "IT",
                BranchId = 1
            };

            tasks.Add(_client.PostAsJsonAsync("/api/employee", createDto));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.IsSuccessStatusCode.Should().BeTrue());
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, 
            $"Creating {employeesToCreate} employees should complete within 10 seconds");

        var averageTimePerEmployee = stopwatch.ElapsedMilliseconds / employeesToCreate;
        averageTimePerEmployee.Should().BeLessThan(200, 
            "Average time per employee creation should be less than 200ms");
    }

    [Fact]
    public async Task EmployeeSearch_LargeDataset_ShouldReturnResultsQuickly()
    {
        // Arrange
        var searchCriteria = new EmployeeSearchCriteria
        {
            Department = "IT",
            PageNumber = 1,
            PageSize = 20
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/api/employee/search", searchCriteria);
        stopwatch.Stop();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, 
            "Employee search should complete within 1 second");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AttendanceCheckIn_ConcurrentUsers_ShouldHandleLoadEfficiently()
    {
        // Arrange
        var concurrentUsers = 20;
        var tasks = new List<Task<HttpResponseMessage>>();
        var stopwatch = Stopwatch.StartNew();

        // Act - Simulate concurrent check-ins
        for (int i = 1; i <= concurrentUsers; i++)
        {
            var checkInDto = new CheckInDto
            {
                Location = $"Office-{i}",
                Notes = $"Concurrent check-in {i}"
            };

            tasks.Add(_client.PostAsJsonAsync("/api/attendance/checkin", checkInDto));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.IsSuccessStatusCode.Should().BeTrue());
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, 
            $"Concurrent check-ins for {concurrentUsers} users should complete within 5 seconds");

        var averageTimePerCheckIn = stopwatch.ElapsedMilliseconds / concurrentUsers;
        averageTimePerCheckIn.Should().BeLessThan(250, 
            "Average time per check-in should be less than 250ms");
    }

    [Fact]
    public async Task DatabaseQuery_ComplexJoins_ShouldExecuteEfficiently()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();
        
        var stopwatch = Stopwatch.StartNew();

        // Act - Execute complex query with multiple joins
        var result = await context.Employees
            .Include(e => e.Branch)
            .ThenInclude(b => b.Organization)
            .Include(e => e.AttendanceRecords)
            .Include(e => e.PayrollRecords)
            .Where(e => e.Status == EmployeeStatus.Active)
            .Where(e => e.Branch.IsActive)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Take(50)
            .ToListAsync();

        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, 
            "Complex database query should complete within 2 seconds");
    }

    [Fact]
    public async Task ApiEndpoint_HighFrequencyRequests_ShouldMaintainPerformance()
    {
        // Arrange
        var requestCount = 100;
        var tasks = new List<Task<HttpResponseMessage>>();
        var stopwatch = Stopwatch.StartNew();

        // Act - Make high frequency requests to employee list endpoint
        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(_client.GetAsync("/api/employee/branch/1"));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.IsSuccessStatusCode.Should().BeTrue());
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000, 
            $"{requestCount} requests should complete within 15 seconds");

        var averageTimePerRequest = stopwatch.ElapsedMilliseconds / requestCount;
        averageTimePerRequest.Should().BeLessThan(150, 
            "Average time per request should be less than 150ms");
    }

    [Fact]
    public async Task MemoryUsage_ExtendedOperations_ShouldNotExceedLimits()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        var operationCount = 200;

        // Act - Perform extended operations
        for (int i = 0; i < operationCount; i++)
        {
            var response = await _client.GetAsync("/api/employee/branch/1");
            response.IsSuccessStatusCode.Should().BeTrue();
            
            // Force garbage collection every 50 operations
            if (i % 50 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;

        // Assert
        memoryIncrease.Should().BeLessThan(50 * 1024 * 1024, 
            "Memory increase should be less than 50MB after extended operations");
    }

    [Fact]
    public async Task ResponseTime_UnderLoad_ShouldMeetSLA()
    {
        // Arrange
        var responseTimes = new List<long>();
        var requestCount = 50;

        // Act - Measure response times under load
        for (int i = 0; i < requestCount; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await _client.GetAsync($"/api/employee/{i + 1}");
            stopwatch.Stop();

            if (response.IsSuccessStatusCode)
            {
                responseTimes.Add(stopwatch.ElapsedMilliseconds);
            }
        }

        // Assert
        responseTimes.Should().NotBeEmpty();
        
        var averageResponseTime = responseTimes.Average();
        var maxResponseTime = responseTimes.Max();
        var p95ResponseTime = responseTimes.OrderBy(x => x).Skip((int)(responseTimes.Count * 0.95)).First();

        averageResponseTime.Should().BeLessThan(500, "Average response time should be less than 500ms");
        maxResponseTime.Should().BeLessThan(2000, "Maximum response time should be less than 2 seconds");
        p95ResponseTime.Should().BeLessThan(1000, "95th percentile response time should be less than 1 second");
    }

    [Fact]
    public async Task ConcurrentDataModification_HighContention_ShouldHandleGracefully()
    {
        // Arrange
        var concurrentOperations = 10;
        var tasks = new List<Task>();
        var successCount = 0;
        var errorCount = 0;

        // Act - Simulate concurrent modifications to the same data
        for (int i = 0; i < concurrentOperations; i++)
        {
            var taskIndex = i;
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var updateDto = new UpdateEmployeeDto
                    {
                        FirstName = $"Updated{taskIndex}",
                        LastName = "ConcurrentTest",
                        Email = $"updated{taskIndex}@test.com",
                        Phone = "123-456-7890",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        Address = $"Updated Address {taskIndex}",
                        Designation = $"Updated Designation {taskIndex}",
                        Department = "IT"
                    };

                    var response = await _client.PutAsJsonAsync("/api/employee/1", updateDto);
                    if (response.IsSuccessStatusCode)
                    {
                        Interlocked.Increment(ref successCount);
                    }
                    else
                    {
                        Interlocked.Increment(ref errorCount);
                    }
                }
                catch
                {
                    Interlocked.Increment(ref errorCount);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        (successCount + errorCount).Should().Be(concurrentOperations);
        successCount.Should().BeGreaterThan(0, "At least some concurrent operations should succeed");
        
        // System should handle contention gracefully without crashing
        var successRate = (double)successCount / concurrentOperations;
        successRate.Should().BeGreaterThan(0.5, "Success rate should be greater than 50% under high contention");
    }

    [Fact]
    public async Task LargePayloadHandling_ShouldProcessEfficiently()
    {
        // Arrange
        var largeEmployeeList = new List<CreateEmployeeDto>();
        for (int i = 1; i <= 100; i++)
        {
            largeEmployeeList.Add(new CreateEmployeeDto
            {
                FirstName = $"LargePayload{i}",
                LastName = "Test",
                Email = $"largepayload{i}@test.com",
                Phone = "123-456-7890",
                DateOfBirth = new DateTime(1990, 1, 1),
                Address = $"Large Payload Address {i} with lots of additional text to make the payload larger and test system performance under load",
                JoiningDate = DateTime.UtcNow,
                Designation = $"Large Payload Test Employee with extended designation text {i}",
                Department = "IT",
                BranchId = 1
            });
        }

        var stopwatch = Stopwatch.StartNew();

        // Act - Process large payload
        var tasks = largeEmployeeList.Select(emp => 
            _client.PostAsJsonAsync("/api/employee", emp)).ToArray();
        
        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.IsSuccessStatusCode.Should().BeTrue());
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000, 
            "Processing large payload should complete within 30 seconds");

        var throughput = largeEmployeeList.Count / (stopwatch.ElapsedMilliseconds / 1000.0);
        throughput.Should().BeGreaterThan(5, "System should process at least 5 records per second");
    }

    [Fact]
    public async Task SystemStability_ExtendedLoad_ShouldMaintainConsistency()
    {
        // Arrange
        var duration = TimeSpan.FromMinutes(1); // Run for 1 minute
        var startTime = DateTime.UtcNow;
        var operationCount = 0;
        var errorCount = 0;

        // Act - Run continuous load for specified duration
        while (DateTime.UtcNow - startTime < duration)
        {
            try
            {
                var response = await _client.GetAsync("/api/employee/branch/1");
                if (response.IsSuccessStatusCode)
                {
                    Interlocked.Increment(ref operationCount);
                }
                else
                {
                    Interlocked.Increment(ref errorCount);
                }
            }
            catch
            {
                Interlocked.Increment(ref errorCount);
            }

            // Small delay to prevent overwhelming the system
            await Task.Delay(10);
        }

        // Assert
        operationCount.Should().BeGreaterThan(100, "Should complete at least 100 operations in 1 minute");
        
        var errorRate = (double)errorCount / (operationCount + errorCount);
        errorRate.Should().BeLessThan(0.05, "Error rate should be less than 5% under extended load");
    }
}
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using StrideHR.API;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.Attendance;
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
using StrideHR.Infrastructure.DTOs;

namespace StrideHR.Tests.Integration;

public class AttendanceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AttendanceIntegrationTests(WebApplicationFactory<Program> factory)
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
                "Permission:Attendance.CheckIn",
                "Permission:Attendance.CheckOut",
                "Permission:Attendance.StartBreak",
                "Permission:Attendance.EndBreak",
                "Permission:Attendance.ViewOwn",
                "Permission:Attendance.ViewAll",
                "Permission:Attendance.Correct",
                "Permission:Attendance.GenerateReports"
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

    [Fact]
    public async Task CheckIn_ValidData_ReturnsAttendanceRecord()
    {
        // Arrange
        var checkInDto = new CheckInDto
        {
            Location = "Office",
            Notes = "Regular check-in"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/attendance/checkin", checkInDto);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<TestApiResponse<AttendanceRecordDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.CheckInTime.Should().NotBe(default(DateTime));
        apiResponse.Data.Location.Should().Be(checkInDto.Location);
        apiResponse.Data.Status.Should().Be(AttendanceStatus.Present.ToString());
    }

    [Fact]
    public async Task CheckOut_AfterCheckIn_ReturnsUpdatedRecord()
    {
        // Arrange - First check in
        var checkInDto = new CheckInDto
        {
            Location = "Office",
            Notes = "Check-in for checkout test"
        };

        var checkInResponse = await _client.PostAsJsonAsync("/api/attendance/checkin", checkInDto);
        checkInResponse.IsSuccessStatusCode.Should().BeTrue();

        var checkOutDto = new CheckOutDto
        {
            Notes = "End of day checkout"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/attendance/checkout", checkOutDto);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<AttendanceRecordDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.CheckOutTime.Should().NotBeNull();
        apiResponse.Data.TotalWorkingHours.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task StartBreak_ValidBreakType_ReturnsBreakRecord()
    {
        // Arrange - First check in
        var checkInDto = new CheckInDto
        {
            Location = "Office",
            Notes = "Check-in for break test"
        };

        await _client.PostAsJsonAsync("/api/attendance/checkin", checkInDto);

        var startBreakDto = new StartBreakDto
        {
            BreakType = BreakType.Lunch.ToString(),
            Notes = "Lunch break"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/attendance/break/start", startBreakDto);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<BreakRecordDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Type.Should().Be(BreakType.Lunch.ToString());
        apiResponse.Data.StartTime.Should().NotBe(default(DateTime));
        apiResponse.Data.EndTime.Should().BeNull();
    }

    [Fact]
    public async Task EndBreak_AfterStartBreak_ReturnsCompletedBreakRecord()
    {
        // Arrange - Check in and start break
        var checkInDto = new CheckInDto
        {
            Location = "Office",
            Notes = "Check-in for end break test"
        };

        await _client.PostAsJsonAsync("/api/attendance/checkin", checkInDto);

        var startBreakDto = new StartBreakDto
        {
            BreakType = BreakType.Tea.ToString(),
            Notes = "Tea break"
        };

        await _client.PostAsJsonAsync("/api/attendance/break/start", startBreakDto);

        var endBreakDto = new EndBreakDto
        {
            Notes = "Break completed"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/attendance/break/end", endBreakDto);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<BreakRecordDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.EndTime.Should().NotBeNull();
        apiResponse.Data.Duration.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTodayAttendance_ValidBranch_ReturnsAttendanceRecords()
    {
        // Arrange - Create some attendance records
        var checkInDto = new CheckInDto
        {
            Location = "Office",
            Notes = "Today's attendance test"
        };

        await _client.PostAsJsonAsync("/api/attendance/checkin", checkInDto);

        // Act
        var response = await _client.GetAsync("/api/attendance/today/1");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<AttendanceRecordDto>>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAttendanceReport_ValidCriteria_ReturnsReport()
    {
        // Arrange
        var reportCriteria = new AttendanceReportCriteria
        {
            BranchId = 1,
            StartDate = DateTime.Today.AddDays(-7),
            EndDate = DateTime.Today,
            EmployeeIds = new List<int> { 1 }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/attendance/report", reportCriteria);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<AttendanceReportDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEmployeeAttendance_ValidEmployeeAndDateRange_ReturnsAttendanceHistory()
    {
        // Arrange - Create attendance record
        var checkInDto = new CheckInDto
        {
            Location = "Office",
            Notes = "Employee attendance history test"
        };

        await _client.PostAsJsonAsync("/api/attendance/checkin", checkInDto);

        var startDate = DateTime.Today.AddDays(-1);
        var endDate = DateTime.Today.AddDays(1);

        // Act
        var response = await _client.GetAsync($"/api/attendance/employee/1?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<AttendanceRecordDto>>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task CorrectAttendance_ValidCorrection_ReturnsUpdatedRecord()
    {
        // Arrange - Create attendance record first
        var checkInDto = new CheckInDto
        {
            Location = "Office",
            Notes = "Record to be corrected"
        };

        var checkInResponse = await _client.PostAsJsonAsync("/api/attendance/checkin", checkInDto);
        var checkInContent = await checkInResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var checkInApiResponse = JsonSerializer.Deserialize<ApiResponse<AttendanceRecordDto>>(checkInContent, options);
        var recordId = checkInApiResponse!.Data!.Id;

        var correctionDto = new AttendanceCorrectionDto
        {
            CheckInTime = DateTime.Today.AddHours(9),
            CheckOutTime = DateTime.Today.AddHours(17),
            Reason = "Time correction due to system error",
            Notes = "Corrected by HR"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/attendance/{recordId}/correct", correctionDto);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<AttendanceRecordDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.CheckInTime.Should().Be(correctionDto.CheckInTime);
        apiResponse.Data.CheckOutTime.Should().Be(correctionDto.CheckOutTime);
    }
}


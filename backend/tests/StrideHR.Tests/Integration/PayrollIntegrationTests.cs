using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using StrideHR.API;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.Payroll;
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
using FluentAssertions;
using StrideHR.Infrastructure.DTOs;
using StrideHR.Tests.TestConfiguration;

namespace StrideHR.Tests.Integration;

public class PayrollIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PayrollIntegrationTests(WebApplicationFactory<Program> factory)
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
                "Permission:Payroll.Calculate",
                "Permission:Payroll.Process",
                "Permission:Payroll.Approve",
                "Permission:Payroll.Release",
                "Permission:Payroll.ViewAll",
                "Permission:Payroll.ViewOwn",
                "Permission:Payroll.GeneratePayslip",
                "Permission:Payroll.CreateFormula",
                "Permission:Payroll.ManageFormulas"
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
                BasicSalary = 50000,
                Status = EmployeeStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
            context.Employees.Add(employee);
        }

        context.SaveChanges();
    }

    [Fact]
    public async Task CalculatePayroll_ValidEmployee_ReturnsPayrollRecord()
    {
        // Arrange
        var calculateDto = new CalculatePayrollDto
        {
            EmployeeId = 1,
            Period = PayrollPeriod.Monthly,
            Month = DateTime.Today.Month,
            Year = DateTime.Today.Year
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/payroll/calculate", calculateDto);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PayrollRecordDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.EmployeeId.Should().Be(1);
        apiResponse.Data.Status.Should().Be(PayrollStatus.Calculated);
        apiResponse.Data.GrossSalary.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ProcessBranchPayroll_ValidBranch_ReturnsPayrollRecords()
    {
        // Arrange
        var processBranchDto = new ProcessBranchPayrollDto
        {
            BranchId = 1,
            Period = PayrollPeriod.Monthly,
            Month = DateTime.Today.Month,
            Year = DateTime.Today.Year
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/payroll/process-branch", processBranchDto);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<PayrollRecordDto>>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Should().NotBeEmpty();
        apiResponse.Data.Should().Contain(p => p.EmployeeId == 1);
    }

    [Fact]
    public async Task CreatePayrollFormula_ValidFormula_ReturnsCreatedFormula()
    {
        // Arrange
        var createFormulaDto = new StrideHR.Infrastructure.DTOs.CreatePayrollFormulaDto
        {
            Name = "Basic Overtime Formula",
            Description = "Calculate overtime pay at 1.5x rate",
            FormulaExpression = "BasicSalary * (OvertimeHours / 160) * 1.5",
            Category = PayrollFormulaCategory.Allowance,
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/payroll/formula", createFormulaDto);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PayrollFormulaDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Name.Should().Be(createFormulaDto.Name);
        apiResponse.Data.FormulaExpression.Should().Be(createFormulaDto.FormulaExpression);
        apiResponse.Data.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GeneratePayslip_ValidPayrollRecord_ReturnsPayslip()
    {
        // Arrange - First calculate payroll
        var calculateDto = new CalculatePayrollDto
        {
            EmployeeId = 1,
            Period = PayrollPeriod.Monthly,
            Month = DateTime.Today.Month,
            Year = DateTime.Today.Year,
            PayrollPeriod = new PayrollPeriodDto
            {
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today,
                Month = DateTime.Today.Month,
                Year = DateTime.Today.Year
            }
        };

        var calculateResponse = await _client.PostAsJsonAsync("/api/payroll/calculate", calculateDto);
        var calculateContent = await calculateResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var calculateApiResponse = JsonSerializer.Deserialize<ApiResponse<PayrollRecordDto>>(calculateContent, options);
        var payrollRecordId = calculateApiResponse!.Data!.Id;

        // Act
        var response = await _client.GetAsync($"/api/payroll/{payrollRecordId}/payslip");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PayslipDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.PayrollRecordId.Should().Be(payrollRecordId);
        apiResponse.Data.EmployeeName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ApprovePayroll_ValidPayrollRecord_ReturnsApprovedRecord()
    {
        // Arrange - First calculate payroll
        var calculateDto = new CalculatePayrollDto
        {
            EmployeeId = 1,
            Period = PayrollPeriod.Monthly,
            Month = DateTime.Today.Month,
            Year = DateTime.Today.Year,
            PayrollPeriod = new PayrollPeriodDto
            {
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today,
                Month = DateTime.Today.Month,
                Year = DateTime.Today.Year
            }
        };

        var calculateResponse = await _client.PostAsJsonAsync("/api/payroll/calculate", calculateDto);
        var calculateContent = await calculateResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var calculateApiResponse = JsonSerializer.Deserialize<ApiResponse<PayrollRecordDto>>(calculateContent, options);
        var payrollRecordId = calculateApiResponse!.Data!.Id;

        var approveDto = new ApprovePayrollDto
        {
            ApprovalNotes = "Approved by HR Manager",
            ApprovalLevel = PayrollApprovalLevel.HRManager
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/payroll/{payrollRecordId}/approve", approveDto);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PayrollRecordDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Status.Should().Be(PayrollStatus.HRApproved);
    }

    [Fact]
    public async Task GetPayrollHistory_ValidEmployee_ReturnsPayrollHistory()
    {
        // Arrange - Create some payroll records first
        var calculateDto = new CalculatePayrollDto
        {
            EmployeeId = 1,
            Period = PayrollPeriod.Monthly,
            Month = DateTime.Today.Month,
            Year = DateTime.Today.Year,
            PayrollPeriod = new PayrollPeriodDto
            {
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today,
                Month = DateTime.Today.Month,
                Year = DateTime.Today.Year
            }
        };

        await _client.PostAsJsonAsync("/api/payroll/calculate", calculateDto);

        // Act
        var response = await _client.GetAsync("/api/payroll/employee/1/history?pageNumber=1&pageSize=10");

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
    public async Task GetPayrollReport_ValidCriteria_ReturnsReport()
    {
        // Arrange
        var reportCriteria = new PayrollReportCriteria
        {
            BranchId = 1,
            StartDate = DateTime.Today.AddDays(-30),
            EndDate = DateTime.Today,
            IncludeDeductions = true,
            IncludeAllowances = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/payroll/report", reportCriteria);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PayrollReportDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPayrollFormulas_ReturnsActiveFormulas()
    {
        // Arrange - Create a formula first
        var createFormulaDto = new StrideHR.Infrastructure.DTOs.CreatePayrollFormulaDto
        {
            Name = "Test Formula",
            Description = "Test formula for integration test",
            FormulaExpression = "BasicSalary * 0.1",
            Category = PayrollFormulaCategory.Allowance,
            IsActive = true
        };

        await _client.PostAsJsonAsync("/api/payroll/formula", createFormulaDto);

        // Act
        var response = await _client.GetAsync("/api/payroll/formulas");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<PayrollFormulaDto>>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Should().Contain(f => f.Name == "Test Formula");
    }

    [Fact]
    public async Task ReleasePayroll_ApprovedPayroll_ReturnsReleasedRecord()
    {
        // Arrange - Calculate, approve, then release payroll
        var calculateDto = new CalculatePayrollDto
        {
            EmployeeId = 1,
            Period = PayrollPeriod.Monthly,
            Month = DateTime.Today.Month,
            Year = DateTime.Today.Year,
            PayrollPeriod = new PayrollPeriodDto
            {
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today,
                Month = DateTime.Today.Month,
                Year = DateTime.Today.Year
            }
        };

        var calculateResponse = await _client.PostAsJsonAsync("/api/payroll/calculate", calculateDto);
        var calculateContent = await calculateResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var calculateApiResponse = JsonSerializer.Deserialize<ApiResponse<PayrollRecordDto>>(calculateContent, options);
        var payrollRecordId = calculateApiResponse!.Data!.Id;

        // Approve first
        var approveDto = new ApprovePayrollDto
        {
            ApprovalNotes = "Approved for release",
            ApprovalLevel = PayrollApprovalLevel.FinanceManager
        };

        await _client.PostAsJsonAsync($"/api/payroll/{payrollRecordId}/approve", approveDto);

        var releaseDto = new ReleasePayrollDto
        {
            ReleaseNotes = "Payroll released to employees",
            NotifyEmployees = true
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/payroll/{payrollRecordId}/release", releaseDto);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PayrollRecordDto>>(content, options);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Status.Should().Be(PayrollStatus.Released);
    }
}

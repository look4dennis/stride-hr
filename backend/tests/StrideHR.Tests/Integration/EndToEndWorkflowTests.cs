using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using StrideHR.API;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.Employee;
using StrideHR.Core.Models.Attendance;
using StrideHR.Core.Models.Payroll;
using StrideHR.Core.Models.Leave;
using StrideHR.Core.Models.Project;
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
using StrideHR.Infrastructure.DTOs;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FluentAssertions;
using StrideHR.Tests.TestConfiguration;

namespace StrideHR.Tests.Integration;

/// <summary>
/// End-to-end workflow tests that simulate complete business processes
/// from start to finish, testing the integration of multiple modules
/// </summary>
public class EndToEndWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EndToEndWorkflowTests(WebApplicationFactory<Program> factory)
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
            
            // Add all required policies for end-to-end workflows
            var policies = new[]
            {
                "Permission:Employee.Create", "Permission:Employee.Read", "Permission:Employee.Update",
                "Permission:Attendance.CheckIn", "Permission:Attendance.CheckOut", "Permission:Attendance.ViewAll",
                "Permission:Payroll.Calculate", "Permission:Payroll.Process", "Permission:Payroll.Approve",
                "Permission:Leave.Request", "Permission:Leave.Approve", "Permission:Leave.ViewAll",
                "Permission:Project.Create", "Permission:Project.Assign", "Permission:Project.ViewAll",
                "Permission:DSR.Submit", "Permission:DSR.Review", "Permission:DSR.ViewAll"
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
    public async Task CompleteEmployeeOnboardingWorkflow_FromCreationToFirstPayroll_ShouldSucceed()
    {
        // Step 1: Create new employee
        var createEmployeeDto = new CreateEmployeeDto
        {
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice.johnson@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1990, 5, 15),
            Address = "123 Main Street",
            JoiningDate = DateTime.UtcNow,
            Designation = "Software Developer",
            Department = "IT",
            BasicSalary = 60000,
            BranchId = 1
        };

        var createEmployeeResponse = await _client.PostAsJsonAsync("/api/employee", createEmployeeDto);
        createEmployeeResponse.IsSuccessStatusCode.Should().BeTrue();
        
        var employeeContent = await createEmployeeResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var employeeApiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(employeeContent, options);
        var employeeId = employeeApiResponse!.Data!.Id;

        // Step 2: Employee checks in for first day
        var checkInDto = new CheckInDto
        {
            Location = "Office - Main Building",
            Notes = "First day at work"
        };

        var checkInResponse = await _client.PostAsJsonAsync("/api/attendance/checkin", checkInDto);
        checkInResponse.IsSuccessStatusCode.Should().BeTrue();

        // Step 3: Employee takes lunch break
        var startBreakDto = new StartBreakDto
        {
            BreakType = BreakType.Lunch.ToString(),
            Notes = "Lunch break"
        };

        var startBreakResponse = await _client.PostAsJsonAsync("/api/attendance/break/start", startBreakDto);
        startBreakResponse.IsSuccessStatusCode.Should().BeTrue();

        // Step 4: Employee ends lunch break
        var endBreakDto = new EndBreakDto
        {
            Notes = "Back from lunch"
        };

        var endBreakResponse = await _client.PostAsJsonAsync("/api/attendance/break/end", endBreakDto);
        endBreakResponse.IsSuccessStatusCode.Should().BeTrue();

        // Step 5: Employee checks out
        var checkOutDto = new CheckOutDto
        {
            Notes = "End of first day"
        };

        var checkOutResponse = await _client.PostAsJsonAsync("/api/attendance/checkout", checkOutDto);
        checkOutResponse.IsSuccessStatusCode.Should().BeTrue();

        // Step 6: Calculate payroll for the employee
        var calculatePayrollDto = new CalculatePayrollDto
        {
            EmployeeId = employeeId,
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

        var payrollResponse = await _client.PostAsJsonAsync("/api/payroll/calculate", calculatePayrollDto);
        payrollResponse.IsSuccessStatusCode.Should().BeTrue();

        var payrollContent = await payrollResponse.Content.ReadAsStringAsync();
        var payrollApiResponse = JsonSerializer.Deserialize<ApiResponse<PayrollRecordDto>>(payrollContent, options);
        
        // Verify the complete workflow
        payrollApiResponse!.Data!.EmployeeId.Should().Be(employeeId);
        payrollApiResponse.Data.GrossSalary.Should().BeGreaterThan(0);
        payrollApiResponse.Data.Status.Should().Be(PayrollStatus.Calculated);
    }

    [Fact]
    public async Task CompleteLeaveRequestWorkflow_FromRequestToApproval_ShouldSucceed()
    {
        // Step 1: Create employee first
        var createEmployeeDto = new CreateEmployeeDto
        {
            FirstName = "Bob",
            LastName = "Smith",
            Email = "bob.smith@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1985, 8, 20),
            Address = "456 Oak Avenue",
            JoiningDate = DateTime.UtcNow.AddYears(-1), // Employee with 1 year tenure
            Designation = "Project Manager",
            Department = "IT",
            BasicSalary = 75000,
            BranchId = 1
        };

        var createEmployeeResponse = await _client.PostAsJsonAsync("/api/employee", createEmployeeDto);
        var employeeContent = await createEmployeeResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var employeeApiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(employeeContent, options);
        var employeeId = employeeApiResponse!.Data!.Id;

        // Step 2: Employee submits leave request
        var leaveRequestDto = new StrideHR.Infrastructure.DTOs.CreateLeaveRequestDto
        {
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.Today.AddDays(7),
            EndDate = DateTime.Today.AddDays(10),
            Reason = "Family vacation",
            Notes = "Pre-planned family trip"
        };

        var leaveRequestResponse = await _client.PostAsJsonAsync("/api/leave/request", leaveRequestDto);
        leaveRequestResponse.IsSuccessStatusCode.Should().BeTrue();

        var leaveContent = await leaveRequestResponse.Content.ReadAsStringAsync();
        var leaveApiResponse = JsonSerializer.Deserialize<ApiResponse<LeaveRequestDto>>(leaveContent, options);
        var leaveRequestId = leaveApiResponse!.Data!.Id;

        // Step 3: Manager approves the leave request
        var approveLeaveDto = new ApproveLeaveRequestDto
        {
            ApprovalStatus = LeaveApprovalStatus.Approved,
            ApprovalNotes = "Approved by manager",
            ApprovalDate = DateTime.UtcNow
        };

        var approveResponse = await _client.PostAsJsonAsync($"/api/leave/{leaveRequestId}/approve", approveLeaveDto);
        approveResponse.IsSuccessStatusCode.Should().BeTrue();

        // Step 4: Verify leave balance is updated
        var balanceResponse = await _client.GetAsync($"/api/leave/balance/{employeeId}");
        balanceResponse.IsSuccessStatusCode.Should().BeTrue();

        var balanceContent = await balanceResponse.Content.ReadAsStringAsync();
        var balanceApiResponse = JsonSerializer.Deserialize<ApiResponse<LeaveBalanceDto>>(balanceContent, options);
        
        // Verify the workflow completed successfully
        balanceApiResponse!.Data.Should().NotBeNull();
        balanceApiResponse.Data!.EmployeeId.Should().Be(employeeId);
    }

    [Fact]
    public async Task CompleteProjectWorkflow_FromCreationToCompletion_ShouldSucceed()
    {
        // Step 1: Create employees for the project
        var projectManagerDto = new CreateEmployeeDto
        {
            FirstName = "Carol",
            LastName = "Davis",
            Email = "carol.davis@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1982, 3, 10),
            Address = "789 Pine Street",
            JoiningDate = DateTime.UtcNow.AddYears(-2),
            Designation = "Senior Project Manager",
            Department = "IT",
            BasicSalary = 85000,
            BranchId = 1
        };

        var developerDto = new CreateEmployeeDto
        {
            FirstName = "David",
            LastName = "Wilson",
            Email = "david.wilson@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1988, 11, 25),
            Address = "321 Elm Street",
            JoiningDate = DateTime.UtcNow.AddMonths(-6),
            Designation = "Senior Developer",
            Department = "IT",
            BasicSalary = 70000,
            BranchId = 1
        };

        var pmResponse = await _client.PostAsJsonAsync("/api/employee", projectManagerDto);
        var devResponse = await _client.PostAsJsonAsync("/api/employee", developerDto);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var pmContent = await pmResponse.Content.ReadAsStringAsync();
        var devContent = await devResponse.Content.ReadAsStringAsync();
        
        var pmApiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(pmContent, options);
        var devApiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(devContent, options);
        
        var pmId = pmApiResponse!.Data!.Id;
        var devId = devApiResponse!.Data!.Id;

        // Step 2: Create project
        var createProjectDto = new CreateProjectDto
        {
            Name = "E-commerce Platform",
            Description = "Build a modern e-commerce platform",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(90),
            EstimatedHours = 720,
            Budget = 100000,
            Priority = ProjectPriority.High
        };

        var projectResponse = await _client.PostAsJsonAsync("/api/project", createProjectDto);
        projectResponse.IsSuccessStatusCode.Should().BeTrue();

        var projectContent = await projectResponse.Content.ReadAsStringAsync();
        var projectApiResponse = JsonSerializer.Deserialize<ApiResponse<ProjectDto>>(projectContent, options);
        var projectId = projectApiResponse!.Data!.Id;

        // Step 3: Assign team members to project
        var assignTeamDto = new AssignTeamMembersDto
        {
            ProjectId = projectId,
            EmployeeIds = new List<int> { pmId, devId }
        };

        var assignResponse = await _client.PostAsJsonAsync($"/api/project/{projectId}/assign-team", assignTeamDto);
        assignResponse.IsSuccessStatusCode.Should().BeTrue();

        // Step 4: Create tasks for the project
        var createTaskDto = new CreateTaskDto
        {
            ProjectId = projectId,
            Title = "Setup Database Schema",
            Description = "Design and implement database schema",
            EstimatedHours = 40,
            Priority = TaskPriority.High,
            DueDate = DateTime.Today.AddDays(14)
        };

        var taskResponse = await _client.PostAsJsonAsync("/api/project/task", createTaskDto);
        taskResponse.IsSuccessStatusCode.Should().BeTrue();

        var taskContent = await taskResponse.Content.ReadAsStringAsync();
        var taskApiResponse = JsonSerializer.Deserialize<ApiResponse<TaskDto>>(taskContent, options);
        var taskId = taskApiResponse!.Data!.Id;

        // Step 5: Assign task to developer
        var assignTaskDto = new AssignTaskDto
        {
            TaskId = taskId,
            AssignedToId = devId
        };

        var assignTaskResponse = await _client.PostAsJsonAsync($"/api/project/task/{taskId}/assign", assignTaskDto);
        assignTaskResponse.IsSuccessStatusCode.Should().BeTrue();

        // Step 6: Developer submits DSR for the task
        var dsrDto = new CreateDSRDto
        {
            Date = DateTime.Today,
            ProjectId = projectId,
            TaskId = taskId,
            HoursWorked = 8,
            Description = "Worked on database schema design and initial implementation"
        };

        var dsrResponse = await _client.PostAsJsonAsync("/api/dsr", dsrDto);
        dsrResponse.IsSuccessStatusCode.Should().BeTrue();

        // Step 7: Get project progress
        var progressResponse = await _client.GetAsync($"/api/project/{projectId}/progress");
        progressResponse.IsSuccessStatusCode.Should().BeTrue();

        var progressContent = await progressResponse.Content.ReadAsStringAsync();
        var progressApiResponse = JsonSerializer.Deserialize<ApiResponse<ProjectProgressDto>>(progressContent, options);
        
        // Verify the complete workflow
        progressApiResponse!.Data.Should().NotBeNull();
        progressApiResponse.Data!.ProjectId.Should().Be(projectId);
        progressApiResponse.Data.ActualHoursWorked.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CompleteAttendanceToPayrollWorkflow_FullMonthCycle_ShouldSucceed()
    {
        // Step 1: Create employee
        var createEmployeeDto = new CreateEmployeeDto
        {
            FirstName = "Emma",
            LastName = "Brown",
            Email = "emma.brown@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1992, 7, 8),
            Address = "654 Maple Drive",
            JoiningDate = DateTime.UtcNow.AddMonths(-3),
            Designation = "Business Analyst",
            Department = "Operations",
            BasicSalary = 55000,
            BranchId = 1
        };

        var createEmployeeResponse = await _client.PostAsJsonAsync("/api/employee", createEmployeeDto);
        var employeeContent = await createEmployeeResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var employeeApiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(employeeContent, options);
        var employeeId = employeeApiResponse!.Data!.Id;

        // Step 2: Simulate daily attendance for a week
        for (int day = 1; day <= 5; day++)
        {
            // Check in
            var checkInDto = new CheckInDto
            {
                Location = "Office",
                Notes = $"Day {day} check-in"
            };
            await _client.PostAsJsonAsync("/api/attendance/checkin", checkInDto);

            // Take lunch break
            var startBreakDto = new StartBreakDto
            {
                BreakType = BreakType.Lunch.ToString(),
                Notes = "Lunch break"
            };
            await _client.PostAsJsonAsync("/api/attendance/break/start", startBreakDto);

            // End lunch break
            var endBreakDto = new EndBreakDto
            {
                Notes = "Back from lunch"
            };
            await _client.PostAsJsonAsync("/api/attendance/break/end", endBreakDto);

            // Check out
            var checkOutDto = new CheckOutDto
            {
                Notes = $"Day {day} check-out"
            };
            await _client.PostAsJsonAsync("/api/attendance/checkout", checkOutDto);
        }

        // Step 3: Generate attendance report
        var reportCriteria = new AttendanceReportCriteria
        {
            BranchId = 1,
            StartDate = DateTime.Today.AddDays(-7),
            EndDate = DateTime.Today,
            EmployeeIds = new List<int> { employeeId }
        };

        var reportResponse = await _client.PostAsJsonAsync("/api/attendance/report", reportCriteria);
        reportResponse.IsSuccessStatusCode.Should().BeTrue();

        // Step 4: Calculate payroll based on attendance
        var calculatePayrollDto = new CalculatePayrollDto
        {
            EmployeeId = employeeId,
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

        var payrollResponse = await _client.PostAsJsonAsync("/api/payroll/calculate", calculatePayrollDto);
        payrollResponse.IsSuccessStatusCode.Should().BeTrue();

        var payrollContent = await payrollResponse.Content.ReadAsStringAsync();
        var payrollApiResponse = JsonSerializer.Deserialize<ApiResponse<PayrollRecordDto>>(payrollContent, options);
        var payrollRecordId = payrollApiResponse!.Data!.Id;

        // Step 5: Approve payroll
        var approveDto = new ApprovePayrollDto
        {
            ApprovalNotes = "Approved based on attendance records",
            ApprovalLevel = PayrollApprovalLevel.HRManager
        };

        var approveResponse = await _client.PostAsJsonAsync($"/api/payroll/{payrollRecordId}/approve", approveDto);
        approveResponse.IsSuccessStatusCode.Should().BeTrue();

        // Step 6: Generate payslip
        var payslipResponse = await _client.GetAsync($"/api/payroll/{payrollRecordId}/payslip");
        payslipResponse.IsSuccessStatusCode.Should().BeTrue();

        var payslipContent = await payslipResponse.Content.ReadAsStringAsync();
        var payslipApiResponse = JsonSerializer.Deserialize<ApiResponse<PayslipDto>>(payslipContent, options);
        
        // Verify the complete workflow
        payslipApiResponse!.Data.Should().NotBeNull();
        payslipApiResponse.Data!.EmployeeName.Should().Contain("Emma Brown");
        payslipApiResponse.Data.NetSalary.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CompletePerformanceReviewWorkflow_FromGoalSettingToCompletion_ShouldSucceed()
    {
        // Step 1: Create employee and manager
        var managerDto = new CreateEmployeeDto
        {
            FirstName = "Frank",
            LastName = "Miller",
            Email = "frank.miller@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1978, 12, 15),
            Address = "987 Cedar Lane",
            JoiningDate = DateTime.UtcNow.AddYears(-5),
            Designation = "Engineering Manager",
            Department = "IT",
            BasicSalary = 95000,
            BranchId = 1
        };

        var employeeDto = new CreateEmployeeDto
        {
            FirstName = "Grace",
            LastName = "Taylor",
            Email = "grace.taylor@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1990, 4, 22),
            Address = "147 Birch Road",
            JoiningDate = DateTime.UtcNow.AddYears(-1),
            Designation = "Software Engineer",
            Department = "IT",
            BasicSalary = 65000,
            BranchId = 1
        };

        var managerResponse = await _client.PostAsJsonAsync("/api/employee", managerDto);
        var employeeResponse = await _client.PostAsJsonAsync("/api/employee", employeeDto);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var managerContent = await managerResponse.Content.ReadAsStringAsync();
        var employeeContent = await employeeResponse.Content.ReadAsStringAsync();
        
        var managerApiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(managerContent, options);
        var employeeApiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(employeeContent, options);
        
        var managerId = managerApiResponse!.Data!.Id;
        var employeeId = employeeApiResponse!.Data!.Id;

        // Step 2: Set performance goals
        var setGoalsDto = new SetPerformanceGoalsDto
        {
            EmployeeId = employeeId,
            ReviewPeriod = new PerformanceReviewPeriodDto
            {
                StartDate = DateTime.Today.AddDays(-90),
                EndDate = DateTime.Today.AddDays(90)
            },
            Goals = new List<PerformanceGoalDto>
            {
                new PerformanceGoalDto
                {
                    Title = "Complete Project Deliverables",
                    Description = "Deliver all assigned project tasks on time",
                    Weight = 40,
                    TargetValue = 100
                },
                new PerformanceGoalDto
                {
                    Title = "Code Quality Improvement",
                    Description = "Maintain code review score above 85%",
                    Weight = 30,
                    TargetValue = 85
                }
            }
        };

        var goalsResponse = await _client.PostAsJsonAsync("/api/performance/goals", setGoalsDto);
        goalsResponse.IsSuccessStatusCode.Should().BeTrue();

        // Step 3: Conduct performance review
        var reviewDto = new CreatePerformanceReviewDto
        {
            EmployeeId = employeeId,
            ReviewerId = managerId,
            ReviewPeriod = setGoalsDto.ReviewPeriod,
            OverallRating = PerformanceRating.MeetsExpectations,
            Comments = "Good performance with room for improvement",
            GoalAchievements = new List<GoalAchievementDto>
            {
                new GoalAchievementDto
                {
                    GoalTitle = "Complete Project Deliverables",
                    AchievedValue = 95,
                    Comments = "Delivered most tasks on time"
                }
            }
        };

        var reviewResponse = await _client.PostAsJsonAsync("/api/performance/review", reviewDto);
        reviewResponse.IsSuccessStatusCode.Should().BeTrue();

        var reviewContent = await reviewResponse.Content.ReadAsStringAsync();
        var reviewApiResponse = JsonSerializer.Deserialize<ApiResponse<PerformanceReviewDto>>(reviewContent, options);
        
        // Verify the workflow completed successfully
        reviewApiResponse!.Data.Should().NotBeNull();
        reviewApiResponse.Data!.EmployeeId.Should().Be(employeeId);
        reviewApiResponse.Data.OverallRating.Should().Be(PerformanceRating.MeetsExpectations);
    }
}

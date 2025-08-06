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
/// User Acceptance Testing - Complete end-to-end business workflow tests
/// that validate the entire employee lifecycle from onboarding to exit,
/// multi-currency payroll processing, and comprehensive business logic validation
/// </summary>
public class UserAcceptanceWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UserAcceptanceWorkflowTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase($"UAT_TestDatabase_{Guid.NewGuid()}");
                });

                ConfigureTestAuthorization(services);

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            });
        });
        _client = _factory.CreateClient();
        
        SeedMultiBranchTestData();
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
            
            // Add all required policies for comprehensive UAT
            var policies = new[]
            {
                "Permission:Employee.Create", "Permission:Employee.Read", "Permission:Employee.Update", "Permission:Employee.Delete",
                "Permission:Attendance.CheckIn", "Permission:Attendance.CheckOut", "Permission:Attendance.ViewAll", "Permission:Attendance.Manage",
                "Permission:Payroll.Calculate", "Permission:Payroll.Process", "Permission:Payroll.Approve", "Permission:Payroll.Release",
                "Permission:Leave.Request", "Permission:Leave.Approve", "Permission:Leave.ViewAll", "Permission:Leave.Manage",
                "Permission:Project.Create", "Permission:Project.Assign", "Permission:Project.ViewAll", "Permission:Project.Manage",
                "Permission:DSR.Submit", "Permission:DSR.Review", "Permission:DSR.ViewAll", "Permission:DSR.Approve",
                "Permission:Performance.Review", "Permission:Performance.SetGoals", "Permission:Performance.ViewAll",
                "Permission:Branch.Manage", "Permission:Organization.Manage", "Permission:Reports.Generate"
            };

            foreach (var policy in policies)
            {
                options.AddPolicy(policy, policyBuilder => 
                    policyBuilder.RequireAssertion(_ => true));
            }
        });
    }

    private void SeedMultiBranchTestData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();
        
        context.Database.EnsureCreated();
        
        if (!context.Organizations.Any())
        {
            var organization = new Organization
            {
                Id = 1,
                Name = "Global Tech Solutions",
                Email = "admin@globaltech.com",
                Phone = "+1-555-0100",
                Address = "123 Corporate Blvd, Suite 100",
                CreatedAt = DateTime.UtcNow
            };
            context.Organizations.Add(organization);
        }

        if (!context.Branches.Any())
        {
            var branches = new[]
            {
                new Branch
                {
                    Id = 1,
                    OrganizationId = 1,
                    Name = "US Headquarters",
                    Email = "us@globaltech.com",
                    Phone = "+1-555-0101",
                    Address = "123 Corporate Blvd",
                    City = "New York",
                    State = "NY",
                    Country = "United States",
                    PostalCode = "10001",
                    TimeZone = "America/New_York",
                    Currency = "USD",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Branch
                {
                    Id = 2,
                    OrganizationId = 1,
                    Name = "European Office",
                    Email = "eu@globaltech.com",
                    Phone = "+44-20-7946-0958",
                    Address = "456 Tech Street",
                    City = "London",
                    State = "England",
                    Country = "United Kingdom",
                    PostalCode = "SW1A 1AA",
                    TimeZone = "Europe/London",
                    Currency = "GBP",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Branch
                {
                    Id = 3,
                    OrganizationId = 1,
                    Name = "Asia Pacific Hub",
                    Email = "apac@globaltech.com",
                    Phone = "+65-6123-4567",
                    Address = "789 Innovation Drive",
                    City = "Singapore",
                    State = "Singapore",
                    Country = "Singapore",
                    PostalCode = "018956",
                    TimeZone = "Asia/Singapore",
                    Currency = "SGD",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.Branches.AddRange(branches);
        }

        context.SaveChanges();
    }

    [Fact]
    public async Task CompleteEmployeeLifecycle_FromOnboardingToExit_ShouldSucceedAcrossAllModules()
    {
        // === PHASE 1: EMPLOYEE ONBOARDING ===
        
        // Step 1: Create new employee in US branch
        var newEmployeeDto = new CreateEmployeeDto
        {
            FirstName = "Sarah",
            LastName = "Johnson",
            Email = "sarah.johnson@globaltech.com",
            Phone = "+1-555-0123",
            DateOfBirth = new DateTime(1988, 6, 15),
            Address = "456 Residential Ave, Apt 3B",
            JoiningDate = DateTime.UtcNow,
            Designation = "Senior Software Engineer",
            Department = "Engineering",
            BasicSalary = 85000,
            BranchId = 1 // US Headquarters
        };

        var createResponse = await _client.PostAsJsonAsync("/api/employee", newEmployeeDto);
        createResponse.IsSuccessStatusCode.Should().BeTrue();
        
        var employeeContent = await createResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var employeeApiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(employeeContent, options);
        var employeeId = employeeApiResponse!.Data!.Id;

        // Step 2: Set initial performance goals
        var performanceGoalsDto = new SetPerformanceGoalsDto
        {
            EmployeeId = employeeId,
            ReviewPeriod = new PerformanceReviewPeriodDto
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(90)
            },
            Goals = new List<PerformanceGoalDto>
            {
                new PerformanceGoalDto
                {
                    Title = "Complete Onboarding Training",
                    Description = "Finish all required training modules within 30 days",
                    Weight = 25,
                    TargetValue = 100
                },
                new PerformanceGoalDto
                {
                    Title = "First Project Delivery",
                    Description = "Successfully deliver first assigned project",
                    Weight = 50,
                    TargetValue = 100
                },
                new PerformanceGoalDto
                {
                    Title = "Team Integration",
                    Description = "Integrate well with team and establish working relationships",
                    Weight = 25,
                    TargetValue = 85
                }
            }
        };

        var goalsResponse = await _client.PostAsJsonAsync("/api/performance/goals", performanceGoalsDto);
        goalsResponse.IsSuccessStatusCode.Should().BeTrue();

        // === PHASE 2: DAILY WORK ACTIVITIES ===
        
        // Step 3: Simulate 30 days of work with attendance, projects, and DSRs
        var projectId = await CreateAndAssignProject(employeeId);
        var taskId = await CreateAndAssignTask(projectId, employeeId);
        
        for (int day = 1; day <= 30; day++)
        {
            // Daily check-in
            var checkInDto = new CheckInDto
            {
                Location = "US Headquarters - Floor 5",
                Notes = $"Day {day} - Ready to work"
            };
            await _client.PostAsJsonAsync("/api/attendance/checkin", checkInDto);

            // Submit DSR
            var dsrDto = new CreateDSRDto
            {
                Date = DateTime.Today.AddDays(-30 + day),
                ProjectId = projectId,
                TaskId = taskId,
                HoursWorked = 8,
                Description = $"Day {day}: Worked on project implementation, code reviews, and team meetings"
            };
            await _client.PostAsJsonAsync("/api/dsr", dsrDto);

            // Take breaks (simulate realistic work pattern)
            if (day % 5 != 0) // Skip breaks on every 5th day (simulate busy days)
            {
                var startBreakDto = new StartBreakDto
                {
                    BreakType = BreakType.Lunch.ToString(),
                    Notes = "Lunch break"
                };
                await _client.PostAsJsonAsync("/api/attendance/break/start", startBreakDto);

                var endBreakDto = new EndBreakDto
                {
                    Notes = "Back from lunch"
                };
                await _client.PostAsJsonAsync("/api/attendance/break/end", endBreakDto);
            }

            // Daily check-out
            var checkOutDto = new CheckOutDto
            {
                Notes = $"Day {day} - Work completed"
            };
            await _client.PostAsJsonAsync("/api/attendance/checkout", checkOutDto);
        }

        // === PHASE 3: LEAVE MANAGEMENT ===
        
        // Step 4: Request and approve vacation leave
        var leaveRequestDto = new StrideHR.Infrastructure.DTOs.CreateLeaveRequestDto
        {
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.Today.AddDays(35),
            EndDate = DateTime.Today.AddDays(39), // 5 days vacation
            Reason = "Annual vacation with family",
            Notes = "Pre-planned vacation, all work will be completed before leave"
        };

        var leaveResponse = await _client.PostAsJsonAsync("/api/leave/request", leaveRequestDto);
        leaveResponse.IsSuccessStatusCode.Should().BeTrue();

        var leaveContent = await leaveResponse.Content.ReadAsStringAsync();
        var leaveApiResponse = JsonSerializer.Deserialize<ApiResponse<LeaveRequestDto>>(leaveContent, options);
        var leaveRequestId = leaveApiResponse!.Data!.Id;

        // Approve the leave request
        var approveLeaveDto = new ApproveLeaveRequestDto
        {
            ApprovalStatus = LeaveApprovalStatus.Approved,
            ApprovalNotes = "Approved - good work performance and proper advance notice",
            ApprovalDate = DateTime.UtcNow
        };

        var approveLeaveResponse = await _client.PostAsJsonAsync($"/api/leave/{leaveRequestId}/approve", approveLeaveDto);
        approveLeaveResponse.IsSuccessStatusCode.Should().BeTrue();

        // === PHASE 4: PAYROLL PROCESSING ===
        
        // Step 5: Calculate monthly payroll
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

        // Approve payroll
        var approvePayrollDto = new ApprovePayrollDto
        {
            ApprovalNotes = "Payroll approved - attendance and performance satisfactory",
            ApprovalLevel = PayrollApprovalLevel.HRManager
        };

        var approvePayrollResponse = await _client.PostAsJsonAsync($"/api/payroll/{payrollRecordId}/approve", approvePayrollDto);
        approvePayrollResponse.IsSuccessStatusCode.Should().BeTrue();

        // Generate payslip
        var payslipResponse = await _client.GetAsync($"/api/payroll/{payrollRecordId}/payslip");
        payslipResponse.IsSuccessStatusCode.Should().BeTrue();

        // === PHASE 5: PERFORMANCE REVIEW ===
        
        // Step 6: Conduct 90-day performance review
        var performanceReviewDto = new CreatePerformanceReviewDto
        {
            EmployeeId = employeeId,
            ReviewerId = 1, // Assuming manager ID
            ReviewPeriod = performanceGoalsDto.ReviewPeriod,
            OverallRating = PerformanceRating.ExceedsExpectations,
            Comments = "Excellent performance during probation period. Quick learner, great team player, and delivers quality work.",
            GoalAchievements = new List<GoalAchievementDto>
            {
                new GoalAchievementDto
                {
                    GoalTitle = "Complete Onboarding Training",
                    AchievedValue = 100,
                    Comments = "Completed all training modules ahead of schedule"
                },
                new GoalAchievementDto
                {
                    GoalTitle = "First Project Delivery",
                    AchievedValue = 95,
                    Comments = "Delivered project successfully with minor revisions"
                },
                new GoalAchievementDto
                {
                    GoalTitle = "Team Integration",
                    AchievedValue = 90,
                    Comments = "Excellent team collaboration and communication"
                }
            }
        };

        var reviewResponse = await _client.PostAsJsonAsync("/api/performance/review", performanceReviewDto);
        reviewResponse.IsSuccessStatusCode.Should().BeTrue();

        // === PHASE 6: CAREER PROGRESSION ===
        
        // Step 7: Update employee details (promotion)
        var updateEmployeeDto = new UpdateEmployeeDto
        {
            Id = employeeId,
            FirstName = newEmployeeDto.FirstName,
            LastName = newEmployeeDto.LastName,
            Email = newEmployeeDto.Email,
            Phone = newEmployeeDto.Phone,
            DateOfBirth = newEmployeeDto.DateOfBirth,
            Address = newEmployeeDto.Address,
            Designation = "Lead Software Engineer", // Promotion
            Department = newEmployeeDto.Department,
            BasicSalary = 95000, // Salary increase
            BranchId = newEmployeeDto.BranchId
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/employee/{employeeId}", updateEmployeeDto);
        updateResponse.IsSuccessStatusCode.Should().BeTrue();

        // === VERIFICATION ===
        
        // Verify complete lifecycle data integrity
        var finalEmployeeResponse = await _client.GetAsync($"/api/employee/{employeeId}");
        finalEmployeeResponse.IsSuccessStatusCode.Should().BeTrue();

        var finalEmployeeContent = await finalEmployeeResponse.Content.ReadAsStringAsync();
        var finalEmployeeApiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(finalEmployeeContent, options);
        
        // Assertions
        finalEmployeeApiResponse!.Data!.Designation.Should().Be("Lead Software Engineer");
        finalEmployeeApiResponse.Data.BasicSalary.Should().Be(95000);
        
        // Verify attendance records exist
        var attendanceResponse = await _client.GetAsync($"/api/attendance/employee/{employeeId}/summary?startDate={DateTime.Today.AddDays(-35)}&endDate={DateTime.Today}");
        attendanceResponse.IsSuccessStatusCode.Should().BeTrue();

        // Verify payroll history
        var payrollHistoryResponse = await _client.GetAsync($"/api/payroll/employee/{employeeId}/history?pageNumber=1&pageSize=10");
        payrollHistoryResponse.IsSuccessStatusCode.Should().BeTrue();

        // Verify leave balance
        var leaveBalanceResponse = await _client.GetAsync($"/api/leave/balance/{employeeId}");
        leaveBalanceResponse.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task MultiCurrencyPayrollProcessing_AcrossThreeBranches_ShouldCalculateCorrectly()
    {
        // Create employees in different branches with different currencies
        var employees = new[]
        {
            new { Branch = 1, Currency = "USD", Salary = 80000, Name = "John", LastName = "Smith" },
            new { Branch = 2, Currency = "GBP", Salary = 55000, Name = "Emma", LastName = "Wilson" },
            new { Branch = 3, Currency = "SGD", Salary = 90000, Name = "Li", LastName = "Chen" }
        };

        var employeeIds = new List<int>();

        foreach (var emp in employees)
        {
            var createEmployeeDto = new CreateEmployeeDto
            {
                FirstName = emp.Name,
                LastName = emp.LastName,
                Email = $"{emp.Name.ToLower()}.{emp.LastName.ToLower()}@globaltech.com",
                Phone = "+1-555-0199",
                DateOfBirth = new DateTime(1985, 1, 1),
                Address = "Test Address",
                JoiningDate = DateTime.UtcNow.AddMonths(-6),
                Designation = "Software Engineer",
                Department = "Engineering",
                BasicSalary = emp.Salary,
                BranchId = emp.Branch
            };

            var response = await _client.PostAsJsonAsync("/api/employee", createEmployeeDto);
            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(content, options);
            employeeIds.Add(apiResponse!.Data!.Id);
        }

        // Process payroll for each branch
        var payrollResults = new List<PayrollRecordDto>();

        for (int i = 0; i < employees.Length; i++)
        {
            var calculateDto = new CalculatePayrollDto
            {
                EmployeeId = employeeIds[i],
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

            var payrollResponse = await _client.PostAsJsonAsync("/api/payroll/calculate", calculateDto);
            payrollResponse.IsSuccessStatusCode.Should().BeTrue();

            var payrollContent = await payrollResponse.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var payrollApiResponse = JsonSerializer.Deserialize<ApiResponse<PayrollRecordDto>>(payrollContent, options);
            payrollResults.Add(payrollApiResponse!.Data!);
        }

        // Verify multi-currency calculations
        payrollResults.Should().HaveCount(3);
        
        // USD employee
        var usdPayroll = payrollResults.First(p => p.EmployeeId == employeeIds[0]);
        usdPayroll.Currency.Should().Be("USD");
        usdPayroll.GrossSalary.Should().BeGreaterThan(0);

        // GBP employee
        var gbpPayroll = payrollResults.First(p => p.EmployeeId == employeeIds[1]);
        gbpPayroll.Currency.Should().Be("GBP");
        gbpPayroll.GrossSalary.Should().BeGreaterThan(0);

        // SGD employee
        var sgdPayroll = payrollResults.First(p => p.EmployeeId == employeeIds[2]);
        sgdPayroll.Currency.Should().Be("SGD");
        sgdPayroll.GrossSalary.Should().BeGreaterThan(0);

        // Generate consolidated payroll report across all branches
        var reportCriteria = new PayrollReportCriteria
        {
            StartDate = DateTime.Today.AddDays(-30),
            EndDate = DateTime.Today,
            IncludeAllBranches = true,
            IncludeDeductions = true,
            IncludeAllowances = true
        };

        var reportResponse = await _client.PostAsJsonAsync("/api/payroll/report", reportCriteria);
        reportResponse.IsSuccessStatusCode.Should().BeTrue();

        var reportContent = await reportResponse.Content.ReadAsStringAsync();
        var options2 = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var reportApiResponse = JsonSerializer.Deserialize<ApiResponse<PayrollReportDto>>(reportContent, options2);
        
        reportApiResponse!.Data.Should().NotBeNull();
        reportApiResponse.Data!.TotalEmployees.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task CompleteAttendanceAndLeaveWorkflow_WithComplexScenarios_ShouldHandleAllCases()
    {
        // Create employee
        var createEmployeeDto = new CreateEmployeeDto
        {
            FirstName = "Michael",
            LastName = "Rodriguez",
            Email = "michael.rodriguez@globaltech.com",
            Phone = "+1-555-0155",
            DateOfBirth = new DateTime(1987, 9, 12),
            Address = "789 Professional Blvd",
            JoiningDate = DateTime.UtcNow.AddYears(-2),
            Designation = "Senior Project Manager",
            Department = "Operations",
            BasicSalary = 90000,
            BranchId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/employee", createEmployeeDto);
        var employeeContent = await createResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var employeeApiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(employeeContent, options);
        var employeeId = employeeApiResponse!.Data!.Id;

        // Scenario 1: Regular work days with overtime
        for (int day = 1; day <= 10; day++)
        {
            // Check in early
            var checkInDto = new CheckInDto
            {
                Location = "US Headquarters",
                Notes = $"Day {day} - Early start for project deadline"
            };
            await _client.PostAsJsonAsync("/api/attendance/checkin", checkInDto);

            // Multiple breaks throughout the day
            var breaks = new[] { BreakType.Coffee, BreakType.Lunch, BreakType.Coffee };
            foreach (var breakType in breaks)
            {
                var startBreakDto = new StartBreakDto
                {
                    BreakType = breakType.ToString(),
                    Notes = $"{breakType} break"
                };
                await _client.PostAsJsonAsync("/api/attendance/break/start", startBreakDto);

                var endBreakDto = new EndBreakDto
                {
                    Notes = $"Back from {breakType} break"
                };
                await _client.PostAsJsonAsync("/api/attendance/break/end", endBreakDto);
            }

            // Late checkout (overtime)
            var checkOutDto = new CheckOutDto
            {
                Notes = $"Day {day} - Overtime for project completion"
            };
            await _client.PostAsJsonAsync("/api/attendance/checkout", checkOutDto);
        }

        // Scenario 2: Sick leave request and approval
        var sickLeaveDto = new StrideHR.Infrastructure.DTOs.CreateLeaveRequestDto
        {
            LeaveType = LeaveType.Sick,
            StartDate = DateTime.Today.AddDays(15),
            EndDate = DateTime.Today.AddDays(17), // 3 days
            Reason = "Medical procedure and recovery",
            Notes = "Doctor's appointment and recovery time needed"
        };

        var sickLeaveResponse = await _client.PostAsJsonAsync("/api/leave/request", sickLeaveDto);
        var sickLeaveContent = await sickLeaveResponse.Content.ReadAsStringAsync();
        var sickLeaveApiResponse = JsonSerializer.Deserialize<ApiResponse<LeaveRequestDto>>(sickLeaveContent, options);
        var sickLeaveId = sickLeaveApiResponse!.Data!.Id;

        var approveSickLeaveDto = new ApproveLeaveRequestDto
        {
            ApprovalStatus = LeaveApprovalStatus.Approved,
            ApprovalNotes = "Approved - medical documentation provided",
            ApprovalDate = DateTime.UtcNow
        };

        await _client.PostAsJsonAsync($"/api/leave/{sickLeaveId}/approve", approveSickLeaveDto);

        // Scenario 3: Emergency leave request
        var emergencyLeaveDto = new StrideHR.Infrastructure.DTOs.CreateLeaveRequestDto
        {
            LeaveType = LeaveType.Emergency,
            StartDate = DateTime.Today.AddDays(20),
            EndDate = DateTime.Today.AddDays(21), // 2 days
            Reason = "Family emergency",
            Notes = "Urgent family matter requiring immediate attention"
        };

        var emergencyLeaveResponse = await _client.PostAsJsonAsync("/api/leave/request", emergencyLeaveDto);
        var emergencyLeaveContent = await emergencyLeaveResponse.Content.ReadAsStringAsync();
        var emergencyLeaveApiResponse = JsonSerializer.Deserialize<ApiResponse<LeaveRequestDto>>(emergencyLeaveContent, options);
        var emergencyLeaveId = emergencyLeaveApiResponse!.Data!.Id;

        var approveEmergencyLeaveDto = new ApproveLeaveRequestDto
        {
            ApprovalStatus = LeaveApprovalStatus.Approved,
            ApprovalNotes = "Approved - emergency situation",
            ApprovalDate = DateTime.UtcNow
        };

        await _client.PostAsJsonAsync($"/api/leave/{emergencyLeaveId}/approve", approveEmergencyLeaveDto);

        // Scenario 4: Work from home days
        for (int day = 25; day <= 27; day++)
        {
            var wfhCheckInDto = new CheckInDto
            {
                Location = "Home Office",
                Notes = $"Work from home day {day - 24}"
            };
            await _client.PostAsJsonAsync("/api/attendance/checkin", wfhCheckInDto);

            var wfhCheckOutDto = new CheckOutDto
            {
                Notes = $"WFH day {day - 24} completed"
            };
            await _client.PostAsJsonAsync("/api/attendance/checkout", wfhCheckOutDto);
        }

        // Generate comprehensive attendance report
        var attendanceReportCriteria = new AttendanceReportCriteria
        {
            BranchId = 1,
            StartDate = DateTime.Today.AddDays(-30),
            EndDate = DateTime.Today.AddDays(30),
            EmployeeIds = new List<int> { employeeId },
            IncludeBreaks = true,
            IncludeOvertime = true
        };

        var attendanceReportResponse = await _client.PostAsJsonAsync("/api/attendance/report", attendanceReportCriteria);
        attendanceReportResponse.IsSuccessStatusCode.Should().BeTrue();

        // Verify leave balance calculations
        var leaveBalanceResponse = await _client.GetAsync($"/api/leave/balance/{employeeId}");
        leaveBalanceResponse.IsSuccessStatusCode.Should().BeTrue();

        var leaveBalanceContent = await leaveBalanceResponse.Content.ReadAsStringAsync();
        var leaveBalanceApiResponse = JsonSerializer.Deserialize<ApiResponse<LeaveBalanceDto>>(leaveBalanceContent, options);
        
        leaveBalanceApiResponse!.Data.Should().NotBeNull();
        leaveBalanceApiResponse.Data!.EmployeeId.Should().Be(employeeId);
        
        // Verify that sick and emergency leaves were deducted from balance
        leaveBalanceApiResponse.Data.SickLeaveUsed.Should().BeGreaterThan(0);
        leaveBalanceApiResponse.Data.EmergencyLeaveUsed.Should().BeGreaterThan(0);
    }

    private async Task<int> CreateAndAssignProject(int employeeId)
    {
        var createProjectDto = new CreateProjectDto
        {
            Name = "Customer Portal Enhancement",
            Description = "Enhance the customer portal with new features and improved UX",
            StartDate = DateTime.Today.AddDays(-30),
            EndDate = DateTime.Today.AddDays(60),
            EstimatedHours = 480,
            Budget = 75000,
            Priority = ProjectPriority.High
        };

        var projectResponse = await _client.PostAsJsonAsync("/api/project", createProjectDto);
        var projectContent = await projectResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var projectApiResponse = JsonSerializer.Deserialize<ApiResponse<ProjectDto>>(projectContent, options);
        var projectId = projectApiResponse!.Data!.Id;

        // Assign employee to project
        var assignTeamDto = new AssignTeamMembersDto
        {
            ProjectId = projectId,
            EmployeeIds = new List<int> { employeeId }
        };

        await _client.PostAsJsonAsync($"/api/project/{projectId}/assign-team", assignTeamDto);

        return projectId;
    }

    private async Task<int> CreateAndAssignTask(int projectId, int employeeId)
    {
        var createTaskDto = new CreateTaskDto
        {
            ProjectId = projectId,
            Title = "Implement User Authentication Enhancement",
            Description = "Enhance user authentication with multi-factor authentication",
            EstimatedHours = 80,
            Priority = TaskPriority.High,
            DueDate = DateTime.Today.AddDays(30)
        };

        var taskResponse = await _client.PostAsJsonAsync("/api/project/task", createTaskDto);
        var taskContent = await taskResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var taskApiResponse = JsonSerializer.Deserialize<ApiResponse<TaskDto>>(taskContent, options);
        var taskId = taskApiResponse!.Data!.Id;

        // Assign task to employee
        var assignTaskDto = new AssignTaskDto
        {
            TaskId = taskId,
            AssignedToId = employeeId
        };

        await _client.PostAsJsonAsync($"/api/project/task/{taskId}/assign", assignTaskDto);

        return taskId;
    }
}
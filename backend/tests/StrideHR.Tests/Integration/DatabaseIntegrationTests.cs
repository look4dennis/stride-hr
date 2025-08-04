using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Infrastructure.Data;
using StrideHR.Tests.TestConfiguration;
using Xunit;
using FluentAssertions;

namespace StrideHR.Tests.Integration;

/// <summary>
/// Database integration tests that verify Entity Framework configurations,
/// relationships, constraints, and data integrity
/// </summary>
public class DatabaseIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task DatabaseSchema_WhenCreated_ShouldHaveAllRequiredTables()
    {
        // Act - Database is created in base constructor
        var tableNames = await DbContext.Database.SqlQueryRaw<string>(
            "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'")
            .ToListAsync();

        // Assert - Verify core tables exist
        var expectedTables = new[]
        {
            "Organizations", "Branches", "Employees", "AttendanceRecords", "BreakRecords",
            "PayrollRecords", "LeaveRequests", "Projects", "Tasks", "DSRs",
            "Roles", "Permissions", "RolePermissions", "EmployeeRoles"
        };

        foreach (var expectedTable in expectedTables)
        {
            tableNames.Should().Contain(t => t.Equals(expectedTable, StringComparison.OrdinalIgnoreCase),
                $"Table {expectedTable} should exist in the database");
        }
    }

    [Fact]
    public async Task OrganizationBranchRelationship_WhenCreated_ShouldMaintainReferentialIntegrity()
    {
        // Arrange
        var organization = new Organization
        {
            Name = "Test Organization",
            Email = "test@test.com",
            Phone = "123-456-7890",
            Address = "Test Address",
            CreatedAt = DateTime.UtcNow
        };

        DbContext.Organizations.Add(organization);
        await DbContext.SaveChangesAsync();

        var branch = new Branch
        {
            OrganizationId = organization.Id,
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

        // Act
        DbContext.Branches.Add(branch);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedBranch = await DbContext.Branches
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.Id == branch.Id);

        savedBranch.Should().NotBeNull();
        savedBranch!.Organization.Should().NotBeNull();
        savedBranch.Organization.Name.Should().Be("Test Organization");
    }

    [Fact]
    public async Task EmployeeAttendanceRelationship_WhenCreated_ShouldMaintainCorrectAssociations()
    {
        // Arrange
        await SeedBasicDataAsync();

        var employee = new Employee
        {
            BranchId = 1,
            EmployeeId = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "Employee Address",
            JoiningDate = DateTime.UtcNow,
            Designation = "Developer",
            Department = "IT",
            Status = EmployeeStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        DbContext.Employees.Add(employee);
        await DbContext.SaveChangesAsync();

        var attendanceRecord = new AttendanceRecord
        {
            EmployeeId = employee.Id,
            Date = DateTime.Today,
            CheckInTime = DateTime.Now.AddHours(-8),
            CheckOutTime = DateTime.Now,
            TotalWorkingHours = TimeSpan.FromHours(8),
            Status = AttendanceStatus.Present,
            Location = "Office"
        };

        // Act
        DbContext.AttendanceRecords.Add(attendanceRecord);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedRecord = await DbContext.AttendanceRecords
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == attendanceRecord.Id);

        savedRecord.Should().NotBeNull();
        savedRecord!.Employee.Should().NotBeNull();
        savedRecord.Employee.FirstName.Should().Be("John");
        savedRecord.Employee.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task AttendanceBreakRecordsCascade_WhenAttendanceDeleted_ShouldDeleteBreakRecords()
    {
        // Arrange
        await SeedBasicDataAsync();

        var employee = new Employee
        {
            BranchId = 1,
            EmployeeId = "EMP002",
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1985, 5, 15),
            Address = "Employee Address",
            JoiningDate = DateTime.UtcNow,
            Designation = "Manager",
            Department = "HR",
            Status = EmployeeStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        DbContext.Employees.Add(employee);
        await DbContext.SaveChangesAsync();

        var attendanceRecord = new AttendanceRecord
        {
            EmployeeId = employee.Id,
            Date = DateTime.Today,
            CheckInTime = DateTime.Now.AddHours(-8),
            Status = AttendanceStatus.Present,
            Location = "Office"
        };

        DbContext.AttendanceRecords.Add(attendanceRecord);
        await DbContext.SaveChangesAsync();

        var breakRecord = new BreakRecord
        {
            AttendanceRecordId = attendanceRecord.Id,
            Type = BreakType.Lunch,
            StartTime = DateTime.Now.AddHours(-4),
            EndTime = DateTime.Now.AddHours(-3),
            Duration = TimeSpan.FromHours(1)
        };

        DbContext.BreakRecords.Add(breakRecord);
        await DbContext.SaveChangesAsync();

        var breakRecordId = breakRecord.Id;

        // Act - Delete attendance record
        DbContext.AttendanceRecords.Remove(attendanceRecord);
        await DbContext.SaveChangesAsync();

        // Assert - Break record should be deleted due to cascade
        var deletedBreakRecord = await DbContext.BreakRecords
            .FirstOrDefaultAsync(b => b.Id == breakRecordId);

        deletedBreakRecord.Should().BeNull();
    }

    [Fact]
    public async Task PayrollRecordCalculation_WhenSaved_ShouldMaintainDataIntegrity()
    {
        // Arrange
        await SeedBasicDataAsync();

        var employee = new Employee
        {
            BranchId = 1,
            EmployeeId = "EMP003",
            FirstName = "Bob",
            LastName = "Johnson",
            Email = "bob.johnson@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1988, 3, 20),
            Address = "Employee Address",
            JoiningDate = DateTime.UtcNow,
            Designation = "Analyst",
            Department = "Finance",
            BasicSalary = 60000,
            Status = EmployeeStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        DbContext.Employees.Add(employee);
        await DbContext.SaveChangesAsync();

        var payrollRecord = new PayrollRecord
        {
            EmployeeId = employee.Id,
            PayrollPeriod = PayrollPeriod.Monthly,
            BasicSalary = 60000,
            GrossSalary = 65000,
            TotalDeductions = 15000,
            NetSalary = 50000,
            Currency = "USD",
            Status = PayrollStatus.Calculated,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        DbContext.PayrollRecords.Add(payrollRecord);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedRecord = await DbContext.PayrollRecords
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == payrollRecord.Id);

        savedRecord.Should().NotBeNull();
        savedRecord!.Employee.Should().NotBeNull();
        savedRecord.NetSalary.Should().Be(50000);
        savedRecord.Currency.Should().Be("USD");
        
        // Verify calculation integrity
        (savedRecord.GrossSalary - savedRecord.TotalDeductions).Should().Be(savedRecord.NetSalary);
    }

    [Fact]
    public async Task ProjectTaskRelationship_WhenCreated_ShouldMaintainHierarchy()
    {
        // Arrange
        await SeedBasicDataAsync();

        var employee = new Employee
        {
            BranchId = 1,
            EmployeeId = "EMP004",
            FirstName = "Alice",
            LastName = "Wilson",
            Email = "alice.wilson@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1992, 7, 10),
            Address = "Employee Address",
            JoiningDate = DateTime.UtcNow,
            Designation = "Project Manager",
            Department = "IT",
            Status = EmployeeStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        DbContext.Employees.Add(employee);
        await DbContext.SaveChangesAsync();

        var project = new Project
        {
            Name = "Test Project",
            Description = "Test project description",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30),
            EstimatedHours = 240,
            Budget = 50000,
            Status = ProjectStatus.Active,
            Priority = ProjectPriority.Medium,
            CreatedByEmployeeId = employee.Id,
            CreatedAt = DateTime.UtcNow
        };

        DbContext.Projects.Add(project);
        await DbContext.SaveChangesAsync();

        var task = new ProjectTask
        {
            ProjectId = project.Id,
            Title = "Test Task",
            Description = "Test task description",
            EstimatedHours = 40,
            Status = ProjectTaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            DueDate = DateTime.Today.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        DbContext.ProjectTasks.Add(task);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedTask = await DbContext.ProjectTasks
            .Include(t => t.Project)
            .ThenInclude(p => p.CreatedByEmployee)
            .FirstOrDefaultAsync(t => t.Id == task.Id);

        savedTask.Should().NotBeNull();
        savedTask!.Project.Should().NotBeNull();
        savedTask.Project.Name.Should().Be("Test Project");
        savedTask.Project.CreatedByEmployee.Should().NotBeNull();
        savedTask.Project.CreatedByEmployee.FirstName.Should().Be("Alice");
    }

    [Fact]
    public async Task RolePermissionRelationship_WhenConfigured_ShouldSupportManyToMany()
    {
        // Arrange
        var role = new Role
        {
            Name = "HR Manager",
            Description = "Human Resources Manager Role",
            HierarchyLevel = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var permissions = new[]
        {
            new Permission
            {
                Name = "Employee.Create",
                Module = "Employee",
                Action = "Create",
                Resource = "Employee"
            },
            new Permission
            {
                Name = "Employee.Read",
                Module = "Employee",
                Action = "Read",
                Resource = "Employee"
            },
            new Permission
            {
                Name = "Payroll.Process",
                Module = "Payroll",
                Action = "Process",
                Resource = "Payroll"
            }
        };

        DbContext.Roles.Add(role);
        DbContext.Permissions.AddRange(permissions);
        await DbContext.SaveChangesAsync();

        var rolePermissions = permissions.Select(p => new RolePermission
        {
            RoleId = role.Id,
            PermissionId = p.Id
        }).ToArray();

        // Act
        DbContext.RolePermissions.AddRange(rolePermissions);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedRole = await DbContext.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == role.Id);

        savedRole.Should().NotBeNull();
        savedRole!.RolePermissions.Should().HaveCount(3);
        savedRole.RolePermissions.Should().Contain(rp => rp.Permission.Name == "Employee.Create");
        savedRole.RolePermissions.Should().Contain(rp => rp.Permission.Name == "Employee.Read");
        savedRole.RolePermissions.Should().Contain(rp => rp.Permission.Name == "Payroll.Process");
    }

    [Fact]
    public async Task LeaveRequestApprovalWorkflow_WhenSaved_ShouldMaintainAuditTrail()
    {
        // Arrange
        await SeedBasicDataAsync();

        var employee = new Employee
        {
            BranchId = 1,
            EmployeeId = "EMP005",
            FirstName = "Charlie",
            LastName = "Brown",
            Email = "charlie.brown@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1987, 12, 5),
            Address = "Employee Address",
            JoiningDate = DateTime.UtcNow.AddYears(-1),
            Designation = "Developer",
            Department = "IT",
            Status = EmployeeStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var manager = new Employee
        {
            BranchId = 1,
            EmployeeId = "MGR001",
            FirstName = "Diana",
            LastName = "Prince",
            Email = "diana.prince@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1980, 6, 15),
            Address = "Manager Address",
            JoiningDate = DateTime.UtcNow.AddYears(-3),
            Designation = "Team Lead",
            Department = "IT",
            Status = EmployeeStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        DbContext.Employees.AddRange(employee, manager);
        await DbContext.SaveChangesAsync();

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.Today.AddDays(7),
            EndDate = DateTime.Today.AddDays(10),
            TotalDays = 4,
            Reason = "Family vacation",
            Status = LeaveStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        DbContext.LeaveRequests.Add(leaveRequest);
        await DbContext.SaveChangesAsync();

        // Act - Approve the leave request
        leaveRequest.Status = LeaveStatus.Approved;
        leaveRequest.ApprovedBy = manager.Id;
        leaveRequest.ApprovedAt = DateTime.UtcNow;
        leaveRequest.ApprovalNotes = "Approved by team lead";

        await DbContext.SaveChangesAsync();

        // Assert
        var savedRequest = await DbContext.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.ApprovedByEmployee)
            .FirstOrDefaultAsync(lr => lr.Id == leaveRequest.Id);

        savedRequest.Should().NotBeNull();
        savedRequest!.Status.Should().Be(LeaveStatus.Approved);
        savedRequest.ApprovedByEmployee.Should().NotBeNull();
        savedRequest.ApprovedByEmployee.FirstName.Should().Be("Diana");
        savedRequest.ApprovedAt.Should().NotBeNull();
        savedRequest.ApprovalNotes.Should().Be("Approved by team lead");
    }

    [Fact]
    public async Task ConcurrentDataModification_WhenHandled_ShouldMaintainConsistency()
    {
        // Arrange
        await SeedBasicDataAsync();

        var employee = new Employee
        {
            BranchId = 1,
            EmployeeId = "EMP006",
            FirstName = "Eve",
            LastName = "Adams",
            Email = "eve.adams@test.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1991, 9, 30),
            Address = "Employee Address",
            JoiningDate = DateTime.UtcNow,
            Designation = "Tester",
            Department = "QA",
            BasicSalary = 45000,
            Status = EmployeeStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        DbContext.Employees.Add(employee);
        await DbContext.SaveChangesAsync();

        // Act - Simulate concurrent modifications
        using var scope1 = ServiceProvider.CreateScope();
        using var scope2 = ServiceProvider.CreateScope();
        
        var context1 = scope1.ServiceProvider.GetRequiredService<StrideHRDbContext>();
        var context2 = scope2.ServiceProvider.GetRequiredService<StrideHRDbContext>();

        var employee1 = await context1.Employees.FindAsync(employee.Id);
        var employee2 = await context2.Employees.FindAsync(employee.Id);

        employee1!.BasicSalary = 50000;
        employee2!.Designation = "Senior Tester";

        await context1.SaveChangesAsync();
        
        // This should handle concurrency appropriately
        await context2.SaveChangesAsync();

        // Assert - Verify final state
        var finalEmployee = await DbContext.Employees.FindAsync(employee.Id);
        finalEmployee.Should().NotBeNull();
        
        // At least one of the changes should be persisted
        (finalEmployee!.BasicSalary == 50000 || finalEmployee.Designation == "Senior Tester")
            .Should().BeTrue("At least one concurrent change should be persisted");
    }

    private async Task SeedBasicDataAsync()
    {
        if (!await DbContext.Organizations.AnyAsync())
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
            DbContext.Organizations.Add(organization);
        }

        if (!await DbContext.Branches.AnyAsync())
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
            DbContext.Branches.Add(branch);
        }

        await DbContext.SaveChangesAsync();
    }
}
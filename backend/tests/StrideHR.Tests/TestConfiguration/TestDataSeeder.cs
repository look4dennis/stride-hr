using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrideHR.Tests.TestConfiguration;

public class TestDataSeeder
{
    private readonly StrideHRDbContext _context;
    private readonly ILogger<TestDataSeeder>? _logger;

    public TestDataSeeder(StrideHRDbContext context, ILogger<TestDataSeeder>? logger = null)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAllAsync()
    {
        try
        {
            _logger?.LogInformation("Starting test data seeding...");

            // Clear existing data first
            await ClearExistingDataAsync();

            // Seed data in dependency order
            await SeedOrganizationsAsync();
            await SeedBranchesAsync();
            await SeedRolesAndPermissionsAsync();
            await SeedEmployeesAsync();
            await SeedUsersAsync();
            await SeedUserRolesAsync();

            _logger?.LogInformation("Test data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to seed test data");
            throw;
        }
    }

    // Alias for SeedAllAsync to maintain compatibility with existing tests
    public Task SeedTestDataAsync() => SeedAllAsync();

    public async Task ClearAllAsync()
    {
        try
        {
            _logger?.LogInformation("Clearing all test data...");
            await ClearExistingDataAsync();
            _logger?.LogInformation("Test data cleared successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to clear test data");
            throw;
        }
    }

    private async Task ClearExistingDataAsync()
    {
        // Clear in reverse dependency order
        _context.EmployeeRoles.RemoveRange(_context.EmployeeRoles);
        _context.Users.RemoveRange(_context.Users);
        _context.Employees.RemoveRange(_context.Employees);
        _context.RolePermissions.RemoveRange(_context.RolePermissions);
        _context.Permissions.RemoveRange(_context.Permissions);
        _context.Roles.RemoveRange(_context.Roles);
        _context.Branches.RemoveRange(_context.Branches);
        _context.Organizations.RemoveRange(_context.Organizations);

        await _context.SaveChangesAsync();
    }

    private async Task SeedOrganizationsAsync()
    {
        if (_context.Organizations.Any())
        {
            _logger?.LogDebug("Organizations already exist, skipping seeding");
            return;
        }

        var organizations = new List<Organization>
        {
            new Organization
            {
                Id = 1,
                Name = "Test Organization",
                Address = "123 Test Street, Test City",
                Email = "admin@testorg.com",
                Phone = "+1-555-0100",
                NormalWorkingHours = TimeSpan.FromHours(8),
                OvertimeRate = 1.5m,
                ProductiveHoursThreshold = 6,
                BranchIsolationEnabled = true,
                CreatedAt = DateTime.UtcNow
            },
            new Organization
            {
                Id = 2,
                Name = "Secondary Test Organization",
                Address = "456 Secondary Street, Test City",
                Email = "admin@secondaryorg.com",
                Phone = "+1-555-0200",
                NormalWorkingHours = TimeSpan.FromHours(8),
                OvertimeRate = 1.75m,
                ProductiveHoursThreshold = 7,
                BranchIsolationEnabled = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Organizations.AddRange(organizations);
        await _context.SaveChangesAsync();

        _logger?.LogDebug("Seeded {Count} organizations", organizations.Count);
    }

    private async Task SeedBranchesAsync()
    {
        if (_context.Branches.Any())
        {
            _logger?.LogDebug("Branches already exist, skipping seeding");
            return;
        }

        var branches = new List<Branch>
        {
            new Branch
            {
                Id = 1,
                OrganizationId = 1,
                Name = "Main Branch",
                Country = "United States",
                Currency = "USD",
                TimeZone = "America/New_York",
                Address = "123 Main Street, New York, NY 10001"
            },
            new Branch
            {
                Id = 2,
                OrganizationId = 1,
                Name = "West Coast Branch",
                Country = "United States",
                Currency = "USD",
                TimeZone = "America/Los_Angeles",
                Address = "456 West Avenue, Los Angeles, CA 90210"
            },
            new Branch
            {
                Id = 3,
                OrganizationId = 2,
                Name = "International Branch",
                Country = "United Kingdom",
                Currency = "GBP",
                TimeZone = "Europe/London",
                Address = "789 London Road, London, UK"
            }
        };

        _context.Branches.AddRange(branches);
        await _context.SaveChangesAsync();

        _logger?.LogDebug("Seeded {Count} branches", branches.Count);
    }

    private async Task SeedRolesAndPermissionsAsync()
    {
        await SeedRolesAsync();
        await SeedPermissionsAsync();
        await SeedRolePermissionsAsync();
    }

    private async Task SeedRolesAsync()
    {
        if (_context.Roles.Any())
        {
            _logger?.LogDebug("Roles already exist, skipping seeding");
            return;
        }

        var roles = new List<Role>
        {
            new Role
            {
                Id = 1,
                Name = "SuperAdmin",
                Description = "Super Administrator with full system access",
                HierarchyLevel = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = 2,
                Name = "Admin",
                Description = "Administrator with organization-level access",
                HierarchyLevel = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = 3,
                Name = "HRManager",
                Description = "HR Manager with HR operations access",
                HierarchyLevel = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = 4,
                Name = "Manager",
                Description = "Department Manager with team management access",
                HierarchyLevel = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = 5,
                Name = "Employee",
                Description = "Regular Employee with basic access",
                HierarchyLevel = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Roles.AddRange(roles);
        await _context.SaveChangesAsync();

        _logger?.LogDebug("Seeded {Count} roles", roles.Count);
    }

    private async Task SeedPermissionsAsync()
    {
        if (_context.Permissions.Any())
        {
            _logger?.LogDebug("Permissions already exist, skipping seeding");
            return;
        }

        var permissions = new List<Permission>
        {
            // Employee permissions
            new Permission { Id = 1, Name = "Employee.View", Module = "Employee", Action = "View", Resource = "Employee", Description = "View employee information" },
            new Permission { Id = 2, Name = "Employee.Create", Module = "Employee", Action = "Create", Resource = "Employee", Description = "Create new employees" },
            new Permission { Id = 3, Name = "Employee.Update", Module = "Employee", Action = "Update", Resource = "Employee", Description = "Update employee information" },
            new Permission { Id = 4, Name = "Employee.Delete", Module = "Employee", Action = "Delete", Resource = "Employee", Description = "Delete employees" },

            // Role permissions
            new Permission { Id = 5, Name = "Role.View", Module = "Role", Action = "View", Resource = "Role", Description = "View roles" },
            new Permission { Id = 6, Name = "Role.Create", Module = "Role", Action = "Create", Resource = "Role", Description = "Create new roles" },
            new Permission { Id = 7, Name = "Role.Update", Module = "Role", Action = "Update", Resource = "Role", Description = "Update roles" },
            new Permission { Id = 8, Name = "Role.Delete", Module = "Role", Action = "Delete", Resource = "Role", Description = "Delete roles" },

            // Payroll permissions
            new Permission { Id = 9, Name = "Payroll.View", Module = "Payroll", Action = "View", Resource = "Payroll", Description = "View payroll information" },
            new Permission { Id = 10, Name = "Payroll.Process", Module = "Payroll", Action = "Process", Resource = "Payroll", Description = "Process payroll" },
            new Permission { Id = 11, Name = "Payroll.Approve", Module = "Payroll", Action = "Approve", Resource = "Payroll", Description = "Approve payroll" },

            // Report permissions
            new Permission { Id = 12, Name = "Report.View", Module = "Report", Action = "View", Resource = "Report", Description = "View reports" },
            new Permission { Id = 13, Name = "Report.Create", Module = "Report", Action = "Create", Resource = "Report", Description = "Create reports" }
        };

        _context.Permissions.AddRange(permissions);
        await _context.SaveChangesAsync();

        _logger?.LogDebug("Seeded {Count} permissions", permissions.Count);
    }

    private async Task SeedRolePermissionsAsync()
    {
        if (_context.RolePermissions.Any())
        {
            _logger?.LogDebug("Role permissions already exist, skipping seeding");
            return;
        }

        var rolePermissions = new List<RolePermission>();

        // SuperAdmin gets all permissions
        for (int permissionId = 1; permissionId <= 13; permissionId++)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = 1, // SuperAdmin
                PermissionId = permissionId,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Admin gets most permissions except some sensitive ones
        var adminPermissions = new[] { 1, 2, 3, 5, 6, 7, 9, 10, 12, 13 };
        foreach (var permissionId in adminPermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = 2, // Admin
                PermissionId = permissionId,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // HRManager gets HR-related permissions
        var hrPermissions = new[] { 1, 2, 3, 9, 10, 12 };
        foreach (var permissionId in hrPermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = 3, // HRManager
                PermissionId = permissionId,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Manager gets basic management permissions
        var managerPermissions = new[] { 1, 3, 12 };
        foreach (var permissionId in managerPermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = 4, // Manager
                PermissionId = permissionId,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Employee gets basic view permissions
        var employeePermissions = new[] { 1, 12 };
        foreach (var permissionId in employeePermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = 5, // Employee
                PermissionId = permissionId,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.RolePermissions.AddRange(rolePermissions);
        await _context.SaveChangesAsync();

        _logger?.LogDebug("Seeded {Count} role permissions", rolePermissions.Count);
    }

    private async Task SeedEmployeesAsync()
    {
        if (_context.Employees.Any())
        {
            _logger?.LogDebug("Employees already exist, skipping seeding");
            return;
        }

        var employees = new List<Employee>
        {
            new Employee
            {
                Id = 1,
                EmployeeId = "EMP-001",
                BranchId = 1,
                FirstName = "John",
                LastName = "Admin",
                Email = "john.admin@testorg.com",
                Phone = "+1-555-0101",
                DateOfBirth = new DateTime(1985, 5, 15),
                JoiningDate = new DateTime(2020, 1, 15),
                Designation = "System Administrator",
                Department = "IT",
                BasicSalary = 75000,
                Status = EmployeeStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new Employee
            {
                Id = 2,
                EmployeeId = "EMP-002",
                BranchId = 1,
                FirstName = "Jane",
                LastName = "Manager",
                Email = "jane.manager@testorg.com",
                Phone = "+1-555-0102",
                DateOfBirth = new DateTime(1988, 8, 22),
                JoiningDate = new DateTime(2021, 3, 10),
                Designation = "HR Manager",
                Department = "Human Resources",
                BasicSalary = 65000,
                Status = EmployeeStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new Employee
            {
                Id = 3,
                EmployeeId = "EMP-003",
                BranchId = 2,
                FirstName = "Bob",
                LastName = "Employee",
                Email = "bob.employee@testorg.com",
                Phone = "+1-555-0103",
                DateOfBirth = new DateTime(1992, 12, 5),
                JoiningDate = new DateTime(2022, 6, 1),
                Designation = "Software Developer",
                Department = "Engineering",
                BasicSalary = 55000,
                Status = EmployeeStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new Employee
            {
                Id = 4,
                EmployeeId = "EMP-004",
                BranchId = 3,
                FirstName = "Alice",
                LastName = "International",
                Email = "alice.international@secondaryorg.com",
                Phone = "+44-20-7946-0958",
                DateOfBirth = new DateTime(1990, 3, 18),
                JoiningDate = new DateTime(2021, 9, 15),
                Designation = "Business Analyst",
                Department = "Operations",
                BasicSalary = 45000,
                Status = EmployeeStatus.Active,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Employees.AddRange(employees);
        await _context.SaveChangesAsync();

        _logger?.LogDebug("Seeded {Count} employees", employees.Count);
    }

    private async Task SeedUsersAsync()
    {
        if (_context.Users.Any())
        {
            _logger?.LogDebug("Users already exist, skipping seeding");
            return;
        }

        var users = new List<User>
        {
            new User
            {
                Id = 1,
                Username = "admin",
                Email = "john.admin@testorg.com",
                PasswordHash = "hashed_password_admin", // In real tests, use proper hashing
                EmployeeId = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow.AddDays(-1)
            },
            new User
            {
                Id = 2,
                Username = "hrmanager",
                Email = "jane.manager@testorg.com",
                PasswordHash = "hashed_password_hrmanager",
                EmployeeId = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow.AddDays(-2)
            },
            new User
            {
                Id = 3,
                Username = "employee",
                Email = "bob.employee@testorg.com",
                PasswordHash = "hashed_password_employee",
                EmployeeId = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow.AddDays(-3)
            },
            new User
            {
                Id = 4,
                Username = "international",
                Email = "alice.international@secondaryorg.com",
                PasswordHash = "hashed_password_international",
                EmployeeId = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        _logger?.LogDebug("Seeded {Count} users", users.Count);
    }

    private async Task SeedUserRolesAsync()
    {
        if (_context.EmployeeRoles.Any())
        {
            _logger?.LogDebug("Employee roles already exist, skipping seeding");
            return;
        }

        var employeeRoles = new List<EmployeeRole>
        {
            new EmployeeRole
            {
                EmployeeId = 1,
                RoleId = 1, // SuperAdmin
                AssignedDate = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new EmployeeRole
            {
                EmployeeId = 2,
                RoleId = 3, // HRManager
                AssignedDate = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new EmployeeRole
            {
                EmployeeId = 3,
                RoleId = 5, // Employee
                AssignedDate = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new EmployeeRole
            {
                EmployeeId = 4,
                RoleId = 4, // Manager
                AssignedDate = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.EmployeeRoles.AddRange(employeeRoles);
        await _context.SaveChangesAsync();

        _logger?.LogDebug("Seeded {Count} employee roles", employeeRoles.Count);
    }

    public static class TestDataConstants
    {
        public const int TestOrganizationId = 1;
        public const int TestBranchId = 1;
        public const int TestEmployeeId = 1;
        public const int TestUserId = 1;
        public const int AdminRoleId = 1;
        public const int HRManagerRoleId = 3;
        public const int EmployeeRoleId = 5;

        public const string TestEmployeeEmail = "john.admin@testorg.com";
        public const string TestUsername = "admin";
    }
}
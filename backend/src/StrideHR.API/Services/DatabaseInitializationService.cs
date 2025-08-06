using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace StrideHR.API.Services;

public class DatabaseInitializationService
{
    private readonly StrideHRDbContext _context;
    private readonly ILogger<DatabaseInitializationService> _logger;

    public DatabaseInitializationService(StrideHRDbContext context, ILogger<DatabaseInitializationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> InitializeDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Starting database initialization...");

            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();
            _logger.LogInformation("Database schema created successfully");

            // Check if super admin already exists
            var existingSuperAdmin = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == "Superadmin");

            if (existingSuperAdmin == null)
            {
                await CreateSuperAdminUserAsync();
                _logger.LogInformation("Super admin user created successfully");
            }
            else
            {
                _logger.LogInformation("Super admin user already exists");
            }

            // Create default organization if not exists
            await CreateDefaultOrganizationAsync();

            // Create default branch if not exists
            await CreateDefaultBranchAsync();

            // Create default roles and permissions
            await CreateDefaultRolesAndPermissionsAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database initialization completed successfully");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database initialization");
            return false;
        }
    }

    private async Task CreateSuperAdminUserAsync()
    {
        var hashedPassword = HashPassword("adminsuper2025$");

        var superAdminUser = new User
        {
            Username = "Superadmin",
            Email = "superadmin@stridehr.com",
            PasswordHash = hashedPassword,
            IsActive = true,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(superAdminUser);
        await _context.SaveChangesAsync();

        // Create corresponding employee record
        var superAdminEmployee = new Employee
        {
            EmployeeId = "EMP001",
            FirstName = "Super",
            LastName = "Admin",
            Email = "superadmin@stridehr.com",
            Phone = "+1234567890",
            DateOfBirth = new DateTime(1990, 1, 1),
            JoiningDate = DateTime.UtcNow,
            Designation = "System Administrator",
            Department = "IT",
            BasicSalary = 0,
            Status = Core.Enums.EmployeeStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BranchId = 1, // Will be set after branch creation
            OrganizationId = 1 // Will be set after organization creation
        };

        _context.Employees.Add(superAdminEmployee);
        await _context.SaveChangesAsync();

        // Link user to employee
        superAdminUser.EmployeeId = superAdminEmployee.Id;
        _context.Users.Update(superAdminUser);
    }

    private async Task CreateDefaultOrganizationAsync()
    {
        var existingOrg = await _context.Organizations.FirstOrDefaultAsync();
        if (existingOrg == null)
        {
            var organization = new Organization
            {
                Name = "StrideHR Organization",
                Address = "123 Business Street, City, State 12345",
                Email = "info@stridehr.com",
                Phone = "+1234567890",
                Website = "https://stridehr.com",
                NormalWorkingHours = "8",
                OvertimeRate = 1.5m,
                ProductiveHoursThreshold = 6,
                BranchIsolationEnabled = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Organizations.Add(organization);
        }
    }

    private async Task CreateDefaultBranchAsync()
    {
        var existingBranch = await _context.Branches.FirstOrDefaultAsync();
        if (existingBranch == null)
        {
            var branch = new Branch
            {
                Name = "Main Branch",
                Address = "123 Business Street, City, State 12345",
                Phone = "+1234567890",
                Email = "main@stridehr.com",
                IsActive = true,
                OrganizationId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Branches.Add(branch);
        }
    }

    private async Task CreateDefaultRolesAndPermissionsAsync()
    {
        // Create Super Admin role if not exists
        var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        if (superAdminRole == null)
        {
            superAdminRole = new Role
            {
                Name = "SuperAdmin",
                Description = "System Administrator with full access",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Roles.Add(superAdminRole);
            await _context.SaveChangesAsync();

            // Assign super admin role to super admin user
            var superAdminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "Superadmin");
            var superAdminEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == "superadmin@stridehr.com");
            
            if (superAdminUser != null && superAdminEmployee != null)
            {
                var employeeRole = new EmployeeRole
                {
                    EmployeeId = superAdminEmployee.Id,
                    RoleId = superAdminRole.Id,
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.EmployeeRoles.Add(employeeRole);
            }
        }

        // Create other default roles
        var defaultRoles = new[]
        {
            new { Name = "HRManager", Description = "HR Manager with HR operations access" },
            new { Name = "Manager", Description = "Department Manager with team management access" },
            new { Name = "Employee", Description = "Regular employee with basic access" }
        };

        foreach (var roleData in defaultRoles)
        {
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleData.Name);
            if (existingRole == null)
            {
                var role = new Role
                {
                    Name = roleData.Name,
                    Description = roleData.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Roles.Add(role);
            }
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "StrideHR_Salt"));
        return Convert.ToBase64String(hashedBytes);
    }

    public async Task<bool> TestDatabaseConnectionAsync()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            _logger.LogInformation("Database connection test successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed");
            return false;
        }
    }
}
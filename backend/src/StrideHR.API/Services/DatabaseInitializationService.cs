using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace StrideHR.API.Services;

public class DatabaseInitializationService
{
    private readonly StrideHRDbContext _context;
    private readonly ILogger<DatabaseInitializationService> _logger;
    private readonly IPasswordService _passwordService;

    public DatabaseInitializationService(
        StrideHRDbContext context, 
        ILogger<DatabaseInitializationService> logger,
        IPasswordService passwordService)
    {
        _context = context;
        _logger = logger;
        _passwordService = passwordService;
    }

    public async Task<bool> InitializeDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Starting database initialization...");

            // Test database connection with retry logic
            var canConnect = false;
            var retryCount = 0;
            var maxRetries = 5;
            
            while (!canConnect && retryCount < maxRetries)
            {
                try
                {
                    canConnect = await _context.Database.CanConnectAsync();
                    if (canConnect)
                    {
                        _logger.LogInformation("Database connection established successfully on attempt {AttemptNumber}", retryCount + 1);
                        break;
                    }
                    else
                    {
                        retryCount++;
                        _logger.LogWarning("Database connection attempt {RetryCount} failed, retrying in {DelaySeconds} seconds...", retryCount, 2);
                        await Task.Delay(2000); // Wait 2 seconds before retry
                    }
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogWarning(ex, "Database connection attempt {RetryCount} failed with exception: {Message}", retryCount, ex.Message);
                    if (retryCount < maxRetries)
                    {
                        _logger.LogInformation("Retrying database connection in {DelaySeconds} seconds...", 2);
                        await Task.Delay(2000); // Wait 2 seconds before retry
                    }
                }
            }

            if (!canConnect)
            {
                _logger.LogError("Cannot connect to database after {MaxRetries} attempts. Please check:", maxRetries);
                _logger.LogError("1. MySQL service is running");
                _logger.LogError("2. Database 'StrideHR_Dev' exists");
                _logger.LogError("3. Connection string is correct: Server=localhost;Database=StrideHR_Dev;User=root;Password=***;Port=3306;");
                _logger.LogError("4. MySQL is accepting connections on port 3306");
                return false;
            }

            // Ensure database is created
            try
            {
                _logger.LogInformation("Creating database schema if it doesn't exist...");
                await _context.Database.EnsureCreatedAsync();
                _logger.LogInformation("Database schema created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create database schema: {Message}", ex.Message);
                _logger.LogError("Full exception details: {FullException}", ex.ToString());
                return false;
            }

            // Create default organization if not exists
            await CreateDefaultOrganizationAsync();
            await _context.SaveChangesAsync();

            // Create default branch if not exists
            await CreateDefaultBranchAsync();
            await _context.SaveChangesAsync();

            // Create default roles first
            await CreateDefaultRolesAndPermissionsAsync();
            await _context.SaveChangesAsync();

            // Create super admin user and handle any conflicts gracefully
            try
            {
                await CreateSuperAdminUserAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error creating super admin user, but continuing initialization: {Message}", ex.Message);
                
                // Check if super admin exists anyway
                var existingSuperAdmin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == "Superadmin" || 
                                             u.Email == "superadmin@stridehr.com" || 
                                             u.Email == "Superadmin@stridehr");
                
                if (existingSuperAdmin != null)
                {
                    _logger.LogInformation("Super admin user exists despite creation error - initialization can continue");
                }
                else
                {
                    _logger.LogError("Super admin user creation failed and no existing user found");
                    return false;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database initialization completed successfully");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database initialization: {Message}", ex.Message);
            return false;
        }
    }

    private async Task CreateSuperAdminUserAsync()
    {
        // Check if user already exists by username or email (checking both formats)
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == "Superadmin" || 
                                     u.Email == "superadmin@stridehr.com" || 
                                     u.Email == "Superadmin@stridehr");

        if (existingUser != null)
        {
            _logger.LogInformation("Super admin user already exists (Username: {Username}, Email: {Email}), updating if needed", 
                existingUser.Username, existingUser.Email);
            
            // Update the email and password to match the expected format for testing
            bool needsUpdate = false;
            if (existingUser.Email != "Superadmin@stridehr")
            {
                existingUser.Email = "Superadmin@stridehr";
                needsUpdate = true;
                _logger.LogInformation("Updated super admin email to match expected format: Superadmin@stridehr");
            }
            
            // Always update the password hash to ensure it uses the proper PBKDF2 format
            var expectedPasswordHash = _passwordService.HashPassword("adminsuper2025$");
            if (existingUser.PasswordHash != expectedPasswordHash)
            {
                existingUser.PasswordHash = expectedPasswordHash;
                needsUpdate = true;
                _logger.LogInformation("Updated super admin password hash to use proper PBKDF2 format");
            }
            
            if (needsUpdate)
            {
                existingUser.UpdatedAt = DateTime.UtcNow;
                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Super admin user updated successfully");
            }
            return;
        }

        // Check if employee already exists
        var existingEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == "EMP001" || 
                                     e.Email == "superadmin@stridehr.com" || 
                                     e.Email == "Superadmin@stridehr");

        Employee superAdminEmployee;
        if (existingEmployee == null)
        {
            // Ensure we have a branch to assign to
            var branch = await _context.Branches.FirstOrDefaultAsync();
            if (branch == null)
            {
                _logger.LogError("No branch found to assign super admin employee to");
                return;
            }

            // Create the employee record with the expected email format
            superAdminEmployee = new Employee
            {
                EmployeeId = "EMP001",
                FirstName = "Super",
                LastName = "Admin",
                Email = "Superadmin@stridehr",
                Phone = "+1234567890",
                DateOfBirth = new DateTime(1990, 1, 1),
                JoiningDate = DateTime.UtcNow,
                Designation = "System Administrator",
                Department = "IT",
                BasicSalary = 0,
                Status = Core.Enums.EmployeeStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                BranchId = branch.Id
            };

            _context.Employees.Add(superAdminEmployee);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Super admin employee created successfully with email: Superadmin@stridehr");
        }
        else
        {
            superAdminEmployee = existingEmployee;
            _logger.LogInformation("Super admin employee already exists (ID: {EmployeeId}, Email: {Email}), using existing record", 
                existingEmployee.EmployeeId, existingEmployee.Email);
            
            // Update the email to match the expected format for testing
            if (existingEmployee.Email != "Superadmin@stridehr")
            {
                existingEmployee.Email = "Superadmin@stridehr";
                existingEmployee.UpdatedAt = DateTime.UtcNow;
                _context.Employees.Update(existingEmployee);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated super admin employee email to match expected format: Superadmin@stridehr");
            }
        }

        // Check if there's already a user linked to this employee
        var existingUserForEmployee = await _context.Users
            .FirstOrDefaultAsync(u => u.EmployeeId == superAdminEmployee.Id);

        if (existingUserForEmployee != null)
        {
            _logger.LogInformation("User already exists for super admin employee (EmployeeId: {EmployeeId}), updating if needed", 
                superAdminEmployee.Id);
            
            // Update the email and password to match the expected format for testing
            bool needsUpdate = false;
            if (existingUserForEmployee.Email != "Superadmin@stridehr")
            {
                existingUserForEmployee.Email = "Superadmin@stridehr";
                needsUpdate = true;
                _logger.LogInformation("Updated existing super admin user email to match expected format: Superadmin@stridehr");
            }
            
            // Always update the password hash to ensure it uses the proper PBKDF2 format
            var expectedPasswordHash = _passwordService.HashPassword("adminsuper2025$");
            if (existingUserForEmployee.PasswordHash != expectedPasswordHash)
            {
                existingUserForEmployee.PasswordHash = expectedPasswordHash;
                needsUpdate = true;
                _logger.LogInformation("Updated existing super admin user password hash to use proper PBKDF2 format");
            }
            
            if (needsUpdate)
            {
                existingUserForEmployee.UpdatedAt = DateTime.UtcNow;
                _context.Users.Update(existingUserForEmployee);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Existing super admin user updated successfully");
            }
            return;
        }

        // Now create the user record with reference to employee using proper password hashing
        var hashedPassword = _passwordService.HashPassword("adminsuper2025$");

        var superAdminUser = new User
        {
            Username = "Superadmin",
            Email = "Superadmin@stridehr",
            PasswordHash = hashedPassword,
            IsActive = true,
            IsEmailVerified = true,
            EmployeeId = superAdminEmployee.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(superAdminUser);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Super admin user created successfully with email: Superadmin@stridehr and proper password hash");
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
                NormalWorkingHours = TimeSpan.FromHours(8),
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
            var superAdminEmployee = await _context.Employees.FirstOrDefaultAsync(e => 
                e.Email == "superadmin@stridehr.com" || e.Email == "Superadmin@stridehr");
            
            if (superAdminUser != null && superAdminEmployee != null)
            {
                var employeeRole = new EmployeeRole
                {
                    EmployeeId = superAdminEmployee.Id,
                    RoleId = superAdminRole.Id,
                    AssignedDate = DateTime.UtcNow,
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



    public async Task<bool> TestDatabaseConnectionAsync()
    {
        try
        {
            _logger.LogInformation("Testing database connection...");
            var canConnect = await _context.Database.CanConnectAsync();
            if (canConnect)
            {
                _logger.LogInformation("Database connection test successful");
                
                // Also test if we can execute a simple query
                try
                {
                    var result = await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                    _logger.LogInformation("Database query test successful");
                }
                catch (Exception queryEx)
                {
                    _logger.LogWarning(queryEx, "Database connection works but query test failed: {Message}", queryEx.Message);
                }
                
                return true;
            }
            else
            {
                _logger.LogError("Database connection test failed - CanConnectAsync returned false");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed with exception: {Message}", ex.Message);
            _logger.LogError("Connection string being used: Server=localhost;Database=StrideHR_Dev;User=root;Password=***;Port=3306;");
            return false;
        }
    }

    public async Task<DatabaseStatus> GetDatabaseStatusAsync()
    {
        try
        {
            var status = new DatabaseStatus();
            
            // Check if database exists and is accessible
            status.CanConnect = await _context.Database.CanConnectAsync();
            
            if (status.CanConnect)
            {
                // Count existing records
                status.OrganizationCount = await _context.Organizations.CountAsync();
                status.BranchCount = await _context.Branches.CountAsync();
                status.UserCount = await _context.Users.CountAsync();
                status.EmployeeCount = await _context.Employees.CountAsync();
                status.RoleCount = await _context.Roles.CountAsync();
                
                // Check for super admin
                status.HasSuperAdmin = await _context.Users
                    .AnyAsync(u => u.Username == "Superadmin" || 
                                  u.Email == "superadmin@stridehr.com" || 
                                  u.Email == "Superadmin@stridehr");
                
                // Check for super admin employee
                status.HasSuperAdminEmployee = await _context.Employees
                    .AnyAsync(e => e.EmployeeId == "EMP001" || 
                                  e.Email == "superadmin@stridehr.com" || 
                                  e.Email == "Superadmin@stridehr");
            }
            
            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database status");
            return new DatabaseStatus { CanConnect = false, ErrorMessage = ex.Message };
        }
    }

    public class DatabaseStatus
    {
        public bool CanConnect { get; set; }
        public int OrganizationCount { get; set; }
        public int BranchCount { get; set; }
        public int UserCount { get; set; }
        public int EmployeeCount { get; set; }
        public int RoleCount { get; set; }
        public bool HasSuperAdmin { get; set; }
        public bool HasSuperAdminEmployee { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
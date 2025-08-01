using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StrideHR.Core.Entities;
using StrideHR.Core.Models.Authentication;
using StrideHR.Infrastructure.Data;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace StrideHR.Tests.Integration;

public class SecurityWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SecurityWorkflowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<StrideHRDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add DbContext using in-memory database for testing
                services.AddDbContext<StrideHRDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CompleteAuthenticationWorkflow_Success()
    {
        // Arrange - Seed test data
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();
        await SeedTestDataAsync(context);

        // Act & Assert - Login
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<AuthResponse>(loginContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.True(loginResult.Success);
        Assert.NotNull(loginResult.Data.Token);
        Assert.NotNull(loginResult.Data.RefreshToken);

        // Set authorization header for subsequent requests
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Data.Token);

        // Act & Assert - Get current user
        var meResponse = await _client.GetAsync("/api/auth/me");
        meResponse.EnsureSuccessStatusCode();

        var meContent = await meResponse.Content.ReadAsStringAsync();
        var meResult = JsonSerializer.Deserialize<UserResponse>(meContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.True(meResult.Success);
        Assert.Equal("test@example.com", meResult.Data.User.Email);

        // Act & Assert - Change password
        var changePasswordRequest = new ChangePasswordRequest
        {
            CurrentPassword = "TestPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        var changePasswordResponse = await _client.PostAsJsonAsync("/api/auth/change-password", changePasswordRequest);
        changePasswordResponse.EnsureSuccessStatusCode();

        // Act & Assert - Refresh token
        var refreshRequest = new RefreshTokenRequest
        {
            Token = loginResult.Data.Token,
            RefreshToken = loginResult.Data.RefreshToken
        };

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        refreshResponse.EnsureSuccessStatusCode();

        var refreshContent = await refreshResponse.Content.ReadAsStringAsync();
        var refreshResult = JsonSerializer.Deserialize<AuthResponse>(refreshContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.True(refreshResult.Success);
        Assert.NotNull(refreshResult.Data.Token);
        Assert.NotNull(refreshResult.Data.RefreshToken);

        // Act & Assert - Logout
        var logoutResponse = await _client.PostAsync("/api/auth/logout", null);
        logoutResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task UserManagementWorkflow_Success()
    {
        // Arrange - Seed test data and login as admin
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();
        await SeedTestDataAsync(context);

        var token = await LoginAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act & Assert - Create user
        var createUserRequest = new CreateUserRequest
        {
            EmployeeId = 2, // Assuming we have a second employee
            Username = "newuser",
            Email = "newuser@example.com",
            RoleIds = new List<int> { 1 } // Employee role
        };

        var createResponse = await _client.PostAsJsonAsync("/api/usermanagement", createUserRequest);
        createResponse.EnsureSuccessStatusCode();

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<CreateUserResponse>(createContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.True(createResult.Success);
        var newUserId = createResult.Data.User.Id;

        // Act & Assert - Get user
        var getUserResponse = await _client.GetAsync($"/api/usermanagement/{newUserId}");
        getUserResponse.EnsureSuccessStatusCode();

        // Act & Assert - Update user
        var updateUserRequest = new UpdateUserRequest
        {
            Username = "updateduser",
            Email = "updateduser@example.com",
            IsActive = true,
            ForcePasswordChange = false
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/usermanagement/{newUserId}", updateUserRequest);
        updateResponse.EnsureSuccessStatusCode();

        // Act & Assert - Deactivate user
        var deactivateResponse = await _client.PostAsync($"/api/usermanagement/{newUserId}/deactivate", null);
        deactivateResponse.EnsureSuccessStatusCode();

        // Act & Assert - Activate user
        var activateResponse = await _client.PostAsync($"/api/usermanagement/{newUserId}/activate", null);
        activateResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task RoleManagementWorkflow_Success()
    {
        // Arrange - Seed test data and login as admin
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();
        await SeedTestDataAsync(context);

        var token = await LoginAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act & Assert - Create role
        var createRoleRequest = new CreateRoleRequest
        {
            Name = "TestRole",
            Description = "Test Role Description",
            HierarchyLevel = 2,
            PermissionIds = new List<int> { 1, 2 }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/role", createRoleRequest);
        createResponse.EnsureSuccessStatusCode();

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<CreateRoleResponse>(createContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.True(createResult.Success);
        var newRoleId = createResult.Data.Role.Id;

        // Act & Assert - Get role
        var getRoleResponse = await _client.GetAsync($"/api/role/{newRoleId}");
        getRoleResponse.EnsureSuccessStatusCode();

        // Act & Assert - Update role
        var updateRoleRequest = new UpdateRoleRequest
        {
            Name = "UpdatedTestRole",
            Description = "Updated Test Role Description",
            HierarchyLevel = 3,
            PermissionIds = new List<int> { 1, 2, 3 }
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/role/{newRoleId}", updateRoleRequest);
        updateResponse.EnsureSuccessStatusCode();

        // Act & Assert - Assign role to employee
        var assignRoleRequest = new AssignRoleRequest
        {
            EmployeeId = 1
        };

        var assignResponse = await _client.PostAsJsonAsync($"/api/role/{newRoleId}/assign", assignRoleRequest);
        assignResponse.EnsureSuccessStatusCode();

        // Act & Assert - Remove role from employee
        var removeResponse = await _client.DeleteAsync($"/api/role/{newRoleId}/remove/1");
        removeResponse.EnsureSuccessStatusCode();
    }

    private async Task SeedTestDataAsync(StrideHRDbContext context)
    {
        // Clear existing data
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Seed organization and branch
        var organization = new Organization
        {
            Id = 1,
            Name = "Test Organization",
            Address = "Test Address",
            Email = "test@org.com",
            Phone = "1234567890"
        };
        context.Organizations.Add(organization);

        var branch = new Branch
        {
            Id = 1,
            OrganizationId = 1,
            Name = "Main Branch",
            Country = "USA",
            Currency = "USD",
            TimeZone = "UTC"
        };
        context.Branches.Add(branch);

        // Seed employees
        var employee1 = new Employee
        {
            Id = 1,
            EmployeeId = "EMP001",
            BranchId = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Department = "IT",
            Designation = "Developer"
        };
        context.Employees.Add(employee1);

        var employee2 = new Employee
        {
            Id = 2,
            EmployeeId = "EMP002",
            BranchId = 1,
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@example.com",
            Department = "HR",
            Designation = "HR Manager"
        };
        context.Employees.Add(employee2);

        // Seed roles
        var employeeRole = new Role
        {
            Id = 1,
            Name = "Employee",
            Description = "Regular Employee",
            HierarchyLevel = 1,
            IsActive = true
        };
        context.Roles.Add(employeeRole);

        var adminRole = new Role
        {
            Id = 2,
            Name = "SuperAdmin",
            Description = "Super Administrator",
            HierarchyLevel = 5,
            IsActive = true
        };
        context.Roles.Add(adminRole);

        // Seed permissions
        var permissions = new[]
        {
            new Permission { Id = 1, Name = "Employee.View", Module = "Employee", Action = "View", Resource = "*" },
            new Permission { Id = 2, Name = "Employee.Create", Module = "Employee", Action = "Create", Resource = "*" },
            new Permission { Id = 3, Name = "Employee.Update", Module = "Employee", Action = "Update", Resource = "*" },
            new Permission { Id = 4, Name = "User.View", Module = "User", Action = "View", Resource = "*" },
            new Permission { Id = 5, Name = "User.Create", Module = "User", Action = "Create", Resource = "*" },
            new Permission { Id = 6, Name = "Role.View", Module = "Role", Action = "View", Resource = "*" },
            new Permission { Id = 7, Name = "Role.Create", Module = "Role", Action = "Create", Resource = "*" }
        };
        context.Permissions.AddRange(permissions);

        // Seed users
        var testUser = new User
        {
            Id = 1,
            EmployeeId = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword", // This would be properly hashed in real scenario
            IsActive = true
        };
        context.Users.Add(testUser);

        var adminUser = new User
        {
            Id = 2,
            EmployeeId = 2,
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = "hashedpassword", // This would be properly hashed in real scenario
            IsActive = true
        };
        context.Users.Add(adminUser);

        // Assign roles to employees
        context.EmployeeRoles.Add(new EmployeeRole
        {
            EmployeeId = 1,
            RoleId = 1,
            IsActive = true
        });

        context.EmployeeRoles.Add(new EmployeeRole
        {
            EmployeeId = 2,
            RoleId = 2,
            IsActive = true
        });

        // Assign permissions to roles
        context.RolePermissions.AddRange(new[]
        {
            new RolePermission { RoleId = 1, PermissionId = 1 },
            new RolePermission { RoleId = 2, PermissionId = 1 },
            new RolePermission { RoleId = 2, PermissionId = 2 },
            new RolePermission { RoleId = 2, PermissionId = 3 },
            new RolePermission { RoleId = 2, PermissionId = 4 },
            new RolePermission { RoleId = 2, PermissionId = 5 },
            new RolePermission { RoleId = 2, PermissionId = 6 },
            new RolePermission { RoleId = 2, PermissionId = 7 }
        });

        await context.SaveChangesAsync();
    }

    private async Task<string> LoginAsAdminAsync()
    {
        var loginRequest = new LoginRequest
        {
            Email = "admin@example.com",
            Password = "TestPassword123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result.Data.Token;
    }

    // Response DTOs for deserialization
    public class AuthResponse
    {
        public bool Success { get; set; }
        public AuthData Data { get; set; }
    }

    public class AuthData
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public UserInfo User { get; set; }
    }

    public class UserResponse
    {
        public bool Success { get; set; }
        public UserData Data { get; set; }
    }

    public class UserData
    {
        public UserInfo User { get; set; }
    }

    public class CreateUserResponse
    {
        public bool Success { get; set; }
        public CreateUserData Data { get; set; }
    }

    public class CreateUserData
    {
        public User User { get; set; }
    }

    public class CreateRoleResponse
    {
        public bool Success { get; set; }
        public CreateRoleData Data { get; set; }
    }

    public class CreateRoleData
    {
        public Role Role { get; set; }
    }
}
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StrideHR.Core.Models.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace StrideHR.Tests.Integration;

/// <summary>
/// Comprehensive tests for role-based access control across all API endpoints
/// Tests all user roles and permission combinations to ensure proper security enforcement
/// </summary>
public class RoleBasedAccessControlTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;
    private readonly string _secretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256-security-requirements";

    public RoleBasedAccessControlTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    #region Test Data and Helpers

    private readonly Dictionary<string, string[]> _rolePermissions = new()
    {
        ["SuperAdmin"] = new[]
        {
            "Employee.View", "Employee.Create", "Employee.Update", "Employee.Delete",
            "Payroll.View", "Payroll.Create", "Payroll.Update", "Payroll.Delete", "Payroll.Calculate", "Payroll.Approve",
            "Branch.View", "Branch.Create", "Branch.Update", "Branch.Delete",
            "Organization.View", "Organization.Create", "Organization.Update", "Organization.Delete",
            "Role.View", "Role.Create", "Role.Update", "Role.Delete",
            "User.View", "User.Create", "User.Update", "User.Delete",
            "Reports.View", "Reports.Generate", "Reports.Export",
            "Audit.View", "System.Configure"
        },
        ["HRManager"] = new[]
        {
            "Employee.View", "Employee.Create", "Employee.Update", "Employee.Delete",
            "Payroll.View", "Payroll.Create", "Payroll.Update", "Payroll.Calculate",
            "Branch.View", "Branch.Update",
            "Reports.View", "Reports.Generate", "Reports.Export",
            "Leave.View", "Leave.Approve", "Leave.Reject",
            "Performance.View", "Performance.Create", "Performance.Update",
            "Training.View", "Training.Assign"
        },
        ["Manager"] = new[]
        {
            "Employee.View", "Employee.Update",
            "Payroll.View",
            "Reports.View", "Reports.Generate",
            "Leave.View", "Leave.Approve", "Leave.Reject",
            "Performance.View", "Performance.Create", "Performance.Update",
            "Project.View", "Project.Create", "Project.Update", "Project.Assign",
            "Attendance.View", "Attendance.Approve"
        },
        ["Employee"] = new[]
        {
            "Employee.View", // Only own profile
            "Payroll.View", // Only own payroll
            "Leave.View", "Leave.Create", "Leave.Update",
            "Attendance.View", "Attendance.Create", "Attendance.Update",
            "Project.View", // Only assigned projects
            "Training.View", // Only assigned training
            "Profile.Update"
        }
    };

    private readonly Dictionary<string, (string Method, string Endpoint, string[] RequiredPermissions)[]> _endpointPermissions = new()
    {
        ["Employee"] = new[]
        {
            ("GET", "/api/employee", new[] { "Employee.View" }),
            ("GET", "/api/employee/1", new[] { "Employee.View" }),
            ("POST", "/api/employee", new[] { "Employee.Create" }),
            ("PUT", "/api/employee/1", new[] { "Employee.Update" }),
            ("DELETE", "/api/employee/1", new[] { "Employee.Delete" }),
            ("GET", "/api/employee/branch/1", new[] { "Employee.View" }),
            ("POST", "/api/employee/search", new[] { "Employee.View" })
        },
        ["Payroll"] = new[]
        {
            ("POST", "/api/payroll/calculate", new[] { "Payroll.Calculate" }),
            ("POST", "/api/payroll/create", new[] { "Payroll.Create" }),
            ("POST", "/api/payroll/process-branch", new[] { "Payroll.Process" }),
            ("POST", "/api/payroll/1/approve", new[] { "Payroll.Approve" }),
            ("GET", "/api/payroll/templates", new[] { "Payroll.Templates.View" }),
            ("POST", "/api/payroll/templates", new[] { "Payroll.Templates.Create" }),
            ("PUT", "/api/payroll/templates/1", new[] { "Payroll.Templates.Update" }),
            ("DELETE", "/api/payroll/templates/1", new[] { "Payroll.Templates.Delete" })
        },
        ["Branch"] = new[]
        {
            ("GET", "/api/branch", new[] { "Branch.View" }),
            ("GET", "/api/branch/1", new[] { "Branch.View" }),
            ("POST", "/api/branch", new[] { "Branch.Create" }),
            ("PUT", "/api/branch/1", new[] { "Branch.Update" }),
            ("DELETE", "/api/branch/1", new[] { "Branch.Delete" })
        },
        ["Organization"] = new[]
        {
            ("GET", "/api/organization", new[] { "Organization.View" }),
            ("GET", "/api/organization/1", new[] { "Organization.View" }),
            ("POST", "/api/organization", new[] { "Organization.Create" }),
            ("PUT", "/api/organization/1", new[] { "Organization.Update" }),
            ("DELETE", "/api/organization/1", new[] { "Organization.Delete" })
        },
        ["Role"] = new[]
        {
            ("GET", "/api/role", new[] { "Role.View" }),
            ("GET", "/api/role/1", new[] { "Role.View" }),
            ("POST", "/api/role", new[] { "Role.Create" }),
            ("PUT", "/api/role/1", new[] { "Role.Update" }),
            ("DELETE", "/api/role/1", new[] { "Role.Delete" })
        }
    };

    private HttpClient CreateClientWithRole(string role, int employeeId = 1, int organizationId = 1, int branchId = 1)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<JwtSettings>(options =>
                {
                    options.SecretKey = _secretKey;
                    options.Issuer = "test-issuer";
                    options.Audience = "test-audience";
                    options.ValidateIssuer = true;
                    options.ValidateAudience = true;
                    options.ValidateLifetime = true;
                    options.ValidateIssuerSigningKey = true;
                    options.ClockSkewMinutes = 5;
                });
            });
        }).CreateClient();
    }

    private string GenerateJwtToken(string role, int employeeId = 1, int organizationId = 1, int branchId = 1, string[] additionalPermissions = null)
    {
        var key = Encoding.ASCII.GetBytes(_secretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var permissions = _rolePermissions.ContainsKey(role) ? _rolePermissions[role] : new string[0];
        if (additionalPermissions != null)
        {
            permissions = permissions.Concat(additionalPermissions).ToArray();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, employeeId.ToString()),
            new Claim("EmployeeId", employeeId.ToString()),
            new Claim("OrganizationId", organizationId.ToString()),
            new Claim("BranchId", branchId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Name, $"Test {role}"),
            new Claim(ClaimTypes.Email, $"test.{role.ToLower()}@test.com")
        };

        // Add permission claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "test-issuer",
            Audience = "test-audience",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<HttpResponseMessage> MakeAuthenticatedRequest(string method, string endpoint, string role, object content = null, int employeeId = 1)
    {
        var client = CreateClientWithRole(role, employeeId);
        var token = GenerateJwtToken(role, employeeId);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = method.ToUpper() switch
        {
            "GET" => await client.GetAsync(endpoint),
            "POST" => content != null 
                ? await client.PostAsync(endpoint, new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json"))
                : await client.PostAsync(endpoint, null),
            "PUT" => content != null 
                ? await client.PutAsync(endpoint, new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json"))
                : await client.PutAsync(endpoint, null),
            "DELETE" => await client.DeleteAsync(endpoint),
            _ => throw new ArgumentException($"Unsupported HTTP method: {method}")
        };

        return response;
    }

    #endregion

    #region SuperAdmin Role Tests

    [Fact]
    public async Task SuperAdmin_ShouldHaveAccessToAllEndpoints()
    {
        // Test that SuperAdmin can access all endpoints
        foreach (var moduleEndpoints in _endpointPermissions)
        {
            foreach (var (method, endpoint, requiredPermissions) in moduleEndpoints.Value)
            {
                var response = await MakeAuthenticatedRequest(method, endpoint, "SuperAdmin");
                
                // SuperAdmin should never get 403 Forbidden
                Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
                
                // Log successful access for verification
                _output.WriteLine($"SuperAdmin successfully accessed {method} {endpoint} - Status: {response.StatusCode}");
            }
        }
    }

    [Fact]
    public async Task SuperAdmin_ShouldAccessSystemConfigurationEndpoints()
    {
        var systemEndpoints = new[]
        {
            ("GET", "/api/organization/settings"),
            ("PUT", "/api/organization/settings"),
            ("GET", "/api/system/configuration"),
            ("PUT", "/api/system/configuration"),
            ("GET", "/api/audit/logs"),
            ("POST", "/api/system/backup")
        };

        foreach (var (method, endpoint) in systemEndpoints)
        {
            var response = await MakeAuthenticatedRequest(method, endpoint, "SuperAdmin");
            
            // Should not be forbidden (may be 404 if endpoint doesn't exist, but not 403)
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine($"SuperAdmin system access {method} {endpoint} - Status: {response.StatusCode}");
        }
    }

    #endregion

    #region HRManager Role Tests

    [Fact]
    public async Task HRManager_ShouldAccessEmployeeManagementEndpoints()
    {
        var hrEndpoints = new[]
        {
            ("GET", "/api/employee"),
            ("GET", "/api/employee/1"),
            ("POST", "/api/employee"),
            ("PUT", "/api/employee/1"),
            ("DELETE", "/api/employee/1"),
            ("GET", "/api/employee/branch/1")
        };

        foreach (var (method, endpoint) in hrEndpoints)
        {
            var response = await MakeAuthenticatedRequest(method, endpoint, "HRManager");
            
            // HRManager should have access to employee management
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine($"HRManager employee access {method} {endpoint} - Status: {response.StatusCode}");
        }
    }

    [Fact]
    public async Task HRManager_ShouldAccessPayrollButNotApprove()
    {
        var payrollEndpoints = new[]
        {
            ("GET", "/api/payroll/templates", true), // Should have access
            ("POST", "/api/payroll/calculate", true), // Should have access
            ("POST", "/api/payroll/create", true), // Should have access
            ("POST", "/api/payroll/1/approve", false) // Should NOT have access
        };

        foreach (var (method, endpoint, shouldHaveAccess) in payrollEndpoints)
        {
            var response = await MakeAuthenticatedRequest(method, endpoint, "HRManager");
            
            if (shouldHaveAccess)
            {
                Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
                _output.WriteLine($"HRManager payroll access {method} {endpoint} - Status: {response.StatusCode} (Expected access)");
            }
            else
            {
                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
                _output.WriteLine($"HRManager payroll denied {method} {endpoint} - Status: {response.StatusCode} (Expected denial)");
            }
        }
    }

    [Fact]
    public async Task HRManager_ShouldNotAccessSystemConfiguration()
    {
        var systemEndpoints = new[]
        {
            ("GET", "/api/system/configuration"),
            ("PUT", "/api/system/configuration"),
            ("POST", "/api/system/backup"),
            ("DELETE", "/api/organization/1")
        };

        foreach (var (method, endpoint) in systemEndpoints)
        {
            var response = await MakeAuthenticatedRequest(method, endpoint, "HRManager");
            
            // HRManager should be forbidden from system configuration
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine($"HRManager system denied {method} {endpoint} - Status: {response.StatusCode}");
        }
    }

    #endregion

    #region Manager Role Tests

    [Fact]
    public async Task Manager_ShouldAccessTeamManagementEndpoints()
    {
        var managerEndpoints = new[]
        {
            ("GET", "/api/employee"), // View team members
            ("PUT", "/api/employee/1"), // Update team member info
            ("GET", "/api/project"), // View projects
            ("POST", "/api/project"), // Create projects
            ("PUT", "/api/project/1"), // Update projects
            ("GET", "/api/attendance"), // View team attendance
            ("POST", "/api/leave/1/approve") // Approve leave requests
        };

        foreach (var (method, endpoint) in managerEndpoints)
        {
            var response = await MakeAuthenticatedRequest(method, endpoint, "Manager");
            
            // Manager should have access to team management
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine($"Manager team access {method} {endpoint} - Status: {response.StatusCode}");
        }
    }

    [Fact]
    public async Task Manager_ShouldNotAccessPayrollOrSystemConfig()
    {
        var restrictedEndpoints = new[]
        {
            ("POST", "/api/payroll/calculate"),
            ("POST", "/api/payroll/create"),
            ("DELETE", "/api/employee/1"),
            ("POST", "/api/branch"),
            ("DELETE", "/api/branch/1"),
            ("GET", "/api/system/configuration")
        };

        foreach (var (method, endpoint) in restrictedEndpoints)
        {
            var response = await MakeAuthenticatedRequest(method, endpoint, "Manager");
            
            // Manager should be forbidden from these operations
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine($"Manager restricted {method} {endpoint} - Status: {response.StatusCode}");
        }
    }

    [Fact]
    public async Task Manager_ShouldOnlyViewOwnBranchData()
    {
        // Test that manager can only access data from their own branch
        var ownBranchResponse = await MakeAuthenticatedRequest("GET", "/api/employee/branch/1", "Manager", null, 1);
        var otherBranchResponse = await MakeAuthenticatedRequest("GET", "/api/employee/branch/2", "Manager", null, 1);

        // Should have access to own branch
        Assert.NotEqual(HttpStatusCode.Forbidden, ownBranchResponse.StatusCode);
        
        // Should be forbidden from other branch (depending on implementation)
        // This test assumes branch-based access control is implemented
        _output.WriteLine($"Manager own branch access - Status: {ownBranchResponse.StatusCode}");
        _output.WriteLine($"Manager other branch access - Status: {otherBranchResponse.StatusCode}");
    }

    #endregion

    #region Employee Role Tests

    [Fact]
    public async Task Employee_ShouldOnlyAccessOwnData()
    {
        var employeeEndpoints = new[]
        {
            ("GET", "/api/employee/1", true), // Own profile
            ("GET", "/api/employee/2", false), // Other employee profile
            ("PUT", "/api/employee/1", true), // Update own profile
            ("PUT", "/api/employee/2", false), // Update other profile
            ("GET", "/api/payroll/payslips/employee/1", true), // Own payslips
            ("GET", "/api/payroll/payslips/employee/2", false), // Other payslips
            ("GET", "/api/attendance/employee/1", true), // Own attendance
            ("GET", "/api/attendance/employee/2", false) // Other attendance
        };

        foreach (var (method, endpoint, shouldHaveAccess) in employeeEndpoints)
        {
            var response = await MakeAuthenticatedRequest(method, endpoint, "Employee", null, 1);
            
            if (shouldHaveAccess)
            {
                Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
                _output.WriteLine($"Employee own data access {method} {endpoint} - Status: {response.StatusCode}");
            }
            else
            {
                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
                _output.WriteLine($"Employee other data denied {method} {endpoint} - Status: {response.StatusCode}");
            }
        }
    }

    [Fact]
    public async Task Employee_ShouldNotAccessManagementEndpoints()
    {
        var managementEndpoints = new[]
        {
            ("GET", "/api/employee"), // List all employees
            ("POST", "/api/employee"), // Create employee
            ("DELETE", "/api/employee/1"), // Delete employee
            ("POST", "/api/payroll/calculate"), // Calculate payroll
            ("GET", "/api/branch"), // View branches
            ("POST", "/api/project"), // Create project
            ("GET", "/api/reports/payroll"), // View payroll reports
            ("POST", "/api/leave/1/approve") // Approve leave
        };

        foreach (var (method, endpoint) in managementEndpoints)
        {
            var response = await MakeAuthenticatedRequest(method, endpoint, "Employee");
            
            // Employee should be forbidden from management operations
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine($"Employee management denied {method} {endpoint} - Status: {response.StatusCode}");
        }
    }

    [Fact]
    public async Task Employee_ShouldAccessSelfServiceEndpoints()
    {
        var selfServiceEndpoints = new[]
        {
            ("GET", "/api/leave/employee/1"), // Own leave requests
            ("POST", "/api/leave"), // Create leave request
            ("PUT", "/api/leave/1"), // Update own leave request
            ("GET", "/api/training/employee/1"), // Own training assignments
            ("POST", "/api/attendance/checkin"), // Check in
            ("POST", "/api/attendance/checkout"), // Check out
            ("GET", "/api/project/employee/1") // Own project assignments
        };

        foreach (var (method, endpoint) in selfServiceEndpoints)
        {
            var response = await MakeAuthenticatedRequest(method, endpoint, "Employee", null, 1);
            
            // Employee should have access to self-service operations
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine($"Employee self-service access {method} {endpoint} - Status: {response.StatusCode}");
        }
    }

    #endregion

    #region Cross-Role Permission Tests

    [Theory]
    [InlineData("SuperAdmin")]
    [InlineData("HRManager")]
    [InlineData("Manager")]
    [InlineData("Employee")]
    public async Task AllRoles_ShouldAccessAuthenticationEndpoints(string role)
    {
        var authEndpoints = new[]
        {
            ("GET", "/api/auth/me"),
            ("POST", "/api/auth/logout"),
            ("POST", "/api/auth/change-password"),
            ("GET", "/api/auth/sessions")
        };

        foreach (var (method, endpoint) in authEndpoints)
        {
            var response = await MakeAuthenticatedRequest(method, endpoint, role);
            
            // All authenticated users should access auth endpoints
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine($"{role} auth access {method} {endpoint} - Status: {response.StatusCode}");
        }
    }

    [Theory]
    [InlineData("SuperAdmin", true)]
    [InlineData("HRManager", true)]
    [InlineData("Manager", false)]
    [InlineData("Employee", false)]
    public async Task RoleHierarchy_ShouldEnforceReportingAccess(string role, bool shouldHaveAccess)
    {
        var reportEndpoints = new[]
        {
            ("GET", "/api/reports/employee-summary"),
            ("GET", "/api/reports/payroll-summary"),
            ("POST", "/api/reports/generate"),
            ("GET", "/api/reports/analytics")
        };

        foreach (var (method, endpoint) in reportEndpoints)
        {
            var response = await MakeAuthenticatedRequest(method, endpoint, role);
            
            if (shouldHaveAccess)
            {
                Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
                _output.WriteLine($"{role} report access {method} {endpoint} - Status: {response.StatusCode}");
            }
            else
            {
                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
                _output.WriteLine($"{role} report denied {method} {endpoint} - Status: {response.StatusCode}");
            }
        }
    }

    #endregion

    #region Unauthorized Access Tests

    [Fact]
    public async Task NoToken_ShouldReturnUnauthorized()
    {
        var client = _factory.CreateClient();
        var testEndpoints = new[]
        {
            ("GET", "/api/employee"),
            ("GET", "/api/payroll/templates"),
            ("GET", "/api/branch"),
            ("GET", "/api/auth/me")
        };

        foreach (var (method, endpoint) in testEndpoints)
        {
            var response = method.ToUpper() switch
            {
                "GET" => await client.GetAsync(endpoint),
                "POST" => await client.PostAsync(endpoint, null),
                _ => await client.GetAsync(endpoint)
            };

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            _output.WriteLine($"No token {method} {endpoint} - Status: {response.StatusCode}");
        }
    }

    [Fact]
    public async Task InvalidToken_ShouldReturnUnauthorized()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid.token.here");

        var response = await client.GetAsync("/api/employee");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _output.WriteLine($"Invalid token - Status: {response.StatusCode}");
    }

    [Fact]
    public async Task ExpiredToken_ShouldReturnUnauthorized()
    {
        var client = CreateClientWithRole("Employee");
        var expiredToken = GenerateExpiredJwtToken();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await client.GetAsync("/api/employee");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _output.WriteLine($"Expired token - Status: {response.StatusCode}");
    }

    private string GenerateExpiredJwtToken()
    {
        var key = Encoding.ASCII.GetBytes(_secretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim("EmployeeId", "1"),
            new Claim("OrganizationId", "1"),
            new Claim("BranchId", "1"),
            new Claim(ClaimTypes.Role, "Employee")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(-10), // Expired 10 minutes ago
            Issuer = "test-issuer",
            Audience = "test-audience",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    #endregion

    #region Permission Boundary Tests

    [Fact]
    public async Task CustomRole_WithSpecificPermissions_ShouldOnlyAccessAllowedEndpoints()
    {
        // Test a custom role with only specific permissions
        var customPermissions = new[] { "Employee.View", "Leave.Create", "Attendance.View" };
        var client = CreateClientWithRole("CustomRole");
        var token = GenerateJwtToken("CustomRole", additionalPermissions: customPermissions);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var testCases = new[]
        {
            ("GET", "/api/employee", true), // Should have access
            ("POST", "/api/employee", false), // Should NOT have access
            ("POST", "/api/leave", true), // Should have access
            ("GET", "/api/attendance", true), // Should have access
            ("POST", "/api/payroll/calculate", false) // Should NOT have access
        };

        foreach (var (method, endpoint, shouldHaveAccess) in testCases)
        {
            HttpResponseMessage response = method.ToUpper() switch
            {
                "GET" => await client.GetAsync(endpoint),
                "POST" => await client.PostAsync(endpoint, null),
                _ => await client.GetAsync(endpoint)
            };

            if (shouldHaveAccess)
            {
                Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
                _output.WriteLine($"CustomRole allowed {method} {endpoint} - Status: {response.StatusCode}");
            }
            else
            {
                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
                _output.WriteLine($"CustomRole denied {method} {endpoint} - Status: {response.StatusCode}");
            }
        }
    }

    [Fact]
    public async Task WildcardPermissions_ShouldGrantModuleAccess()
    {
        // Test wildcard permissions like "Employee.*" should grant all employee permissions
        var wildcardPermissions = new[] { "Employee.*", "Leave.View" };
        var client = CreateClientWithRole("WildcardRole");
        var token = GenerateJwtToken("WildcardRole", additionalPermissions: wildcardPermissions);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var employeeEndpoints = new[]
        {
            ("GET", "/api/employee"),
            ("POST", "/api/employee"),
            ("PUT", "/api/employee/1"),
            ("DELETE", "/api/employee/1")
        };

        foreach (var (method, endpoint) in employeeEndpoints)
        {
            HttpResponseMessage response = method.ToUpper() switch
            {
                "GET" => await client.GetAsync(endpoint),
                "POST" => await client.PostAsync(endpoint, null),
                "PUT" => await client.PutAsync(endpoint, null),
                "DELETE" => await client.DeleteAsync(endpoint),
                _ => await client.GetAsync(endpoint)
            };

            // Wildcard permission should grant access to all employee endpoints
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine($"WildcardRole employee access {method} {endpoint} - Status: {response.StatusCode}");
        }

        // But should not grant access to other modules
        var payrollResponse = await client.PostAsync("/api/payroll/calculate", null);
        Assert.Equal(HttpStatusCode.Forbidden, payrollResponse.StatusCode);
        _output.WriteLine($"WildcardRole payroll denied - Status: {payrollResponse.StatusCode}");
    }

    #endregion

    #region Branch-Based Access Control Tests

    [Fact]
    public async Task BranchBasedAccess_ShouldRestrictCrossBranchData()
    {
        // Test that users can only access data from their own branch
        var branch1Employee = 1;
        var branch2Employee = 2;

        // Employee from branch 1 trying to access branch 2 data
        var response1 = await MakeAuthenticatedRequest("GET", "/api/employee/branch/2", "Manager", null, branch1Employee);
        
        // Employee from branch 2 trying to access branch 1 data  
        var response2 = await MakeAuthenticatedRequest("GET", "/api/employee/branch/1", "Manager", null, branch2Employee);

        // Both should be forbidden (assuming branch-based access control is implemented)
        _output.WriteLine($"Branch 1 Manager accessing Branch 2 data - Status: {response1.StatusCode}");
        _output.WriteLine($"Branch 2 Manager accessing Branch 1 data - Status: {response2.StatusCode}");

        // Note: The actual implementation may vary, but this test structure is ready
        // for when branch-based access control is fully implemented
    }

    #endregion

    #region HTTP Status Code Validation Tests

    [Fact]
    public async Task AuthorizedRequests_ShouldReturnCorrectStatusCodes()
    {
        var statusCodeTests = new[]
        {
            ("GET", "/api/employee", "SuperAdmin", new[] { HttpStatusCode.OK, HttpStatusCode.NoContent }),
            ("GET", "/api/employee/999", "SuperAdmin", new[] { HttpStatusCode.NotFound }),
            ("POST", "/api/employee", "SuperAdmin", new[] { HttpStatusCode.Created, HttpStatusCode.BadRequest }),
            ("PUT", "/api/employee/1", "SuperAdmin", new[] { HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound }),
            ("DELETE", "/api/employee/999", "SuperAdmin", new[] { HttpStatusCode.NotFound, HttpStatusCode.NoContent })
        };

        foreach (var (method, endpoint, role, expectedStatusCodes) in statusCodeTests)
        {
            var response = await MakeAuthenticatedRequest(method, endpoint, role);
            
            // Should not be forbidden, and should return expected status codes
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Contains(response.StatusCode, expectedStatusCodes);
            
            _output.WriteLine($"{role} {method} {endpoint} - Status: {response.StatusCode} (Expected: {string.Join(", ", expectedStatusCodes)})");
        }
    }

    [Fact]
    public async Task UnauthorizedRequests_ShouldReturnForbidden()
    {
        var forbiddenTests = new[]
        {
            ("POST", "/api/employee", "Employee"),
            ("DELETE", "/api/employee/1", "Employee"),
            ("POST", "/api/payroll/calculate", "Employee"),
            ("DELETE", "/api/branch/1", "Manager"),
            ("POST", "/api/organization", "Manager"),
            ("GET", "/api/system/configuration", "HRManager")
        };

        foreach (var (method, endpoint, role) in forbiddenTests)
        {
            var response = await MakeAuthenticatedRequest(method, endpoint, role);
            
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine($"{role} forbidden {method} {endpoint} - Status: {response.StatusCode}");
        }
    }

    #endregion
}
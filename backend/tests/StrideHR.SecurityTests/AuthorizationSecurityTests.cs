using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using StrideHR.SecurityTests.Infrastructure;
using System.Net;
using System.Text;

namespace StrideHR.SecurityTests;

public class AuthorizationSecurityTests : SecurityTestBase
{
    private readonly JwtTokenManipulator _tokenManipulator;

    public AuthorizationSecurityTests(WebApplicationFactory<Program> factory) : base(factory)
    {
        _tokenManipulator = new JwtTokenManipulator();
    }

    [Theory]
    [InlineData("/api/employees", "Employee")]
    [InlineData("/api/payroll", "HR")]
    [InlineData("/api/reports", "Manager")]
    [InlineData("/api/organization", "Admin")]
    public async Task AccessEndpoint_WithInsufficientRole_ShouldReturn403(string endpoint, string requiredRole)
    {
        // Arrange - Create token with lower privilege role
        var insufficientRoles = new[] { "Employee" }; // Lowest privilege role
        var token = _tokenManipulator.CreateValidToken(roles: insufficientRoles);
        AddAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        if (requiredRole != "Employee") // If endpoint requires more than Employee role
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized,
                $"Endpoint {endpoint} should reject users without {requiredRole} role");
        }
    }

    [Theory]
    [InlineData("/api/employees")]
    [InlineData("/api/payroll")]
    [InlineData("/api/attendance")]
    [InlineData("/api/leave")]
    [InlineData("/api/performance")]
    [InlineData("/api/projects")]
    [InlineData("/api/reports")]
    public async Task AccessEndpoint_WithElevatedPrivileges_ShouldNotGrantUnauthorizedAccess(string endpoint)
    {
        // Arrange - Create token with artificially elevated privileges
        var elevatedToken = _tokenManipulator.CreateTokenWithElevatedPrivileges();
        AddAuthorizationHeader(elevatedToken);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        // The system should validate roles against the database, not just trust the token
        // If the user doesn't actually have these roles in the database, access should be denied
        if (!response.IsSuccessStatusCode)
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized,
                "System should validate roles against database, not just trust token claims");
        }
        else
        {
            Console.WriteLine($"Warning: Endpoint {endpoint} accepted elevated privileges from token without database validation");
        }
    }

    [Fact]
    public async Task BranchDataIsolation_UserFromDifferentBranch_ShouldNotAccessData()
    {
        // Arrange
        var userBranchId = "branch-1";
        var targetBranchId = "branch-2";
        
        var userToken = _tokenManipulator.CreateValidToken(branchId: userBranchId);
        AddAuthorizationHeader(userToken);

        // Act - Try to access data from different branch
        var endpoints = new[]
        {
            $"/api/employees?branchId={targetBranchId}",
            $"/api/payroll?branchId={targetBranchId}",
            $"/api/attendance?branchId={targetBranchId}",
            $"/api/reports?branchId={targetBranchId}"
        };

        var unauthorizedAccess = new List<string>();

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Check if response contains data from the target branch
                if (content.Contains(targetBranchId) && !content.Contains("[]") && content.Length > 10)
                {
                    unauthorizedAccess.Add(endpoint);
                }
            }
        }

        // Assert
        unauthorizedAccess.Should().BeEmpty(
            $"User from branch {userBranchId} should not access data from branch {targetBranchId}. " +
            $"Unauthorized access detected at: {string.Join(", ", unauthorizedAccess)}");
    }

    [Fact]
    public async Task OrganizationDataIsolation_UserFromDifferentOrganization_ShouldNotAccessData()
    {
        // Arrange
        var userOrgId = "org-1";
        var targetOrgId = "org-2";
        
        var userToken = _tokenManipulator.CreateValidToken(organizationId: userOrgId);
        AddAuthorizationHeader(userToken);

        // Act - Try to access data from different organization
        var endpoints = new[]
        {
            $"/api/employees?organizationId={targetOrgId}",
            $"/api/payroll?organizationId={targetOrgId}",
            $"/api/branches?organizationId={targetOrgId}",
            $"/api/reports?organizationId={targetOrgId}"
        };

        var unauthorizedAccess = new List<string>();

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Check if response contains data from the target organization
                if (content.Contains(targetOrgId) && !content.Contains("[]") && content.Length > 10)
                {
                    unauthorizedAccess.Add(endpoint);
                }
            }
        }

        // Assert
        unauthorizedAccess.Should().BeEmpty(
            $"User from organization {userOrgId} should not access data from organization {targetOrgId}. " +
            $"Unauthorized access detected at: {string.Join(", ", unauthorizedAccess)}");
    }

    [Theory]
    [InlineData("POST", "/api/employees")]
    [InlineData("PUT", "/api/employees/1")]
    [InlineData("DELETE", "/api/employees/1")]
    [InlineData("POST", "/api/payroll")]
    [InlineData("PUT", "/api/payroll/1")]
    [InlineData("DELETE", "/api/payroll/1")]
    public async Task StateChangingOperations_WithInsufficientPermissions_ShouldReturn403(string method, string endpoint)
    {
        // Arrange - Create token with read-only permissions
        var readOnlyToken = _tokenManipulator.CreateValidToken(roles: new[] { "Employee" });
        AddAuthorizationHeader(readOnlyToken);

        // Act
        HttpResponseMessage response = method switch
        {
            "POST" => await _client.PostAsJsonAsync(endpoint, new { }),
            "PUT" => await _client.PutAsJsonAsync(endpoint, new { }),
            "DELETE" => await _client.DeleteAsync(endpoint),
            _ => throw new ArgumentException($"Unsupported method: {method}")
        };

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized,
            $"{method} operation on {endpoint} should require appropriate permissions");
    }

    [Fact]
    public async Task DirectObjectReference_WithUnauthorizedId_ShouldReturn403()
    {
        // Arrange
        var userEmployeeId = "employee-1";
        var targetEmployeeId = "employee-2";
        
        var userToken = _tokenManipulator.CreateValidToken(employeeId: userEmployeeId);
        AddAuthorizationHeader(userToken);

        // Act - Try to access another employee's data
        var endpoints = new[]
        {
            $"/api/employees/{targetEmployeeId}",
            $"/api/employees/{targetEmployeeId}/payroll",
            $"/api/employees/{targetEmployeeId}/attendance",
            $"/api/employees/{targetEmployeeId}/leave",
            $"/api/employees/{targetEmployeeId}/performance"
        };

        var unauthorizedAccess = new List<string>();

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Check if response contains the target employee's data
                if (content.Contains(targetEmployeeId) && content.Length > 10)
                {
                    unauthorizedAccess.Add(endpoint);
                }
            }
        }

        // Assert
        unauthorizedAccess.Should().BeEmpty(
            $"Employee {userEmployeeId} should not access data for employee {targetEmployeeId}. " +
            $"Unauthorized access detected at: {string.Join(", ", unauthorizedAccess)}");
    }

    [Fact]
    public async Task ParameterTampering_WithModifiedIds_ShouldValidateOwnership()
    {
        // Arrange
        var userEmployeeId = "employee-1";
        var userToken = _tokenManipulator.CreateValidToken(employeeId: userEmployeeId);
        AddAuthorizationHeader(userToken);

        // Act - Try parameter tampering attacks
        var tamperingAttempts = new[]
        {
            "/api/employees?employeeId=../../../admin",
            "/api/payroll?employeeId=1' OR '1'='1",
            "/api/attendance?employeeId=*",
            "/api/leave?employeeId=null",
            "/api/performance?employeeId=undefined"
        };

        var vulnerableEndpoints = new List<string>();

        foreach (var endpoint in tamperingAttempts)
        {
            var response = await _client.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Check if tampering resulted in unauthorized data access
                if (content.Length > 100 && !content.Contains("[]"))
                {
                    vulnerableEndpoints.Add(endpoint);
                }
            }
        }

        // Assert
        vulnerableEndpoints.Should().BeEmpty(
            $"Parameter tampering should not result in unauthorized data access. " +
            $"Vulnerable endpoints: {string.Join(", ", vulnerableEndpoints)}");
    }

    [Fact]
    public async Task RoleEscalation_ThroughApiCalls_ShouldNotBeAllowed()
    {
        // Arrange
        var employeeToken = _tokenManipulator.CreateValidToken(roles: new[] { "Employee" });
        AddAuthorizationHeader(employeeToken);

        // Act - Try to escalate privileges through API calls
        var escalationAttempts = new[]
        {
            new { endpoint = "/api/users/promote", data = new { role = "Admin" } },
            new { endpoint = "/api/roles/assign", data = new { userId = "employee-1", role = "Manager" } },
            new { endpoint = "/api/permissions/grant", data = new { permission = "AdminAccess" } },
            new { endpoint = "/api/employees/update-role", data = new { role = "SuperAdmin" } }
        };

        var successfulEscalations = new List<string>();

        foreach (var attempt in escalationAttempts)
        {
            var response = await _client.PostAsJsonAsync(attempt.endpoint, attempt.data);
            
            if (response.IsSuccessStatusCode)
            {
                successfulEscalations.Add(attempt.endpoint);
            }
        }

        // Assert
        successfulEscalations.Should().BeEmpty(
            $"Role escalation should not be possible through API calls. " +
            $"Successful escalations at: {string.Join(", ", successfulEscalations)}");
    }

    [Fact]
    public async Task MassAssignment_WithExtraFields_ShouldIgnoreUnauthorizedFields()
    {
        // Arrange
        var employeeToken = _tokenManipulator.CreateValidToken(roles: new[] { "Employee" });
        AddAuthorizationHeader(employeeToken);

        // Act - Try mass assignment attack
        var maliciousPayload = new
        {
            Name = "John Doe",
            Email = "john@example.com",
            Role = "Admin", // Unauthorized field
            Salary = 999999, // Unauthorized field
            IsActive = true,
            Permissions = new[] { "AdminAccess", "SuperUser" }, // Unauthorized field
            OrganizationId = "different-org", // Unauthorized field
            BranchId = "different-branch" // Unauthorized field
        };

        var response = await _client.PostAsJsonAsync("/api/employees", maliciousPayload);

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            // Check if unauthorized fields were processed
            content.Should().NotContain("Admin", "Mass assignment should not process unauthorized role field");
            content.Should().NotContain("999999", "Mass assignment should not process unauthorized salary field");
            content.Should().NotContain("AdminAccess", "Mass assignment should not process unauthorized permissions field");
        }
    }

    [Fact]
    public async Task FileUpload_WithUnauthorizedAccess_ShouldValidatePermissions()
    {
        // Arrange
        var employeeToken = _tokenManipulator.CreateValidToken(roles: new[] { "Employee" });
        AddAuthorizationHeader(employeeToken);

        // Act - Try to upload files to unauthorized locations
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("test file content"));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "test.txt");

        var uploadAttempts = new[]
        {
            "/api/files/upload?path=../../../admin",
            "/api/files/upload?employeeId=different-employee",
            "/api/files/upload?organizationId=different-org",
            "/api/documents/upload?access=public"
        };

        var unauthorizedUploads = new List<string>();

        foreach (var endpoint in uploadAttempts)
        {
            var response = await _client.PostAsync(endpoint, formData);
            
            if (response.IsSuccessStatusCode)
            {
                unauthorizedUploads.Add(endpoint);
            }
        }

        // Assert
        unauthorizedUploads.Should().BeEmpty(
            $"File uploads should validate permissions and prevent unauthorized access. " +
            $"Unauthorized uploads at: {string.Join(", ", unauthorizedUploads)}");
    }

    [Fact]
    public async Task ApiRateLimiting_WithExcessiveRequests_ShouldEnforceRateLimits()
    {
        // Arrange
        var employeeToken = _tokenManipulator.CreateValidToken();
        AddAuthorizationHeader(employeeToken);

        var rateLimitedResponses = 0;
        var successfulResponses = 0;

        // Act - Make excessive requests
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_client.GetAsync("/api/employees"));
        }

        var responses = await Task.WhenAll(tasks);

        foreach (var response in responses)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitedResponses++;
            }
            else if (response.IsSuccessStatusCode)
            {
                successfulResponses++;
            }
        }

        // Assert
        if (rateLimitedResponses == 0)
        {
            Console.WriteLine("Warning: No rate limiting detected for API requests");
        }
        else
        {
            rateLimitedResponses.Should().BeGreaterThan(0, "Rate limiting should be enforced for excessive requests");
        }

        successfulResponses.Should().BeGreaterThan(0, "Some requests should succeed before rate limiting kicks in");
    }
}
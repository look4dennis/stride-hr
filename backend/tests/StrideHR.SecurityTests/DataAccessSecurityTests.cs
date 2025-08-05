using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using StrideHR.SecurityTests.Infrastructure;
using System.Net;
using System.Text.Json;

namespace StrideHR.SecurityTests;

public class DataAccessSecurityTests : SecurityTestBase
{
    private readonly JwtTokenManipulator _tokenManipulator;

    public DataAccessSecurityTests(WebApplicationFactory<Program> factory) : base(factory)
    {
        _tokenManipulator = new JwtTokenManipulator();
    }

    [Fact]
    public async Task MultiTenancy_BranchIsolation_ShouldEnforceDataSeparation()
    {
        // Arrange
        var branch1Token = _tokenManipulator.CreateValidToken(branchId: "branch-1");
        var branch2Token = _tokenManipulator.CreateValidToken(branchId: "branch-2");

        var testEndpoints = new[]
        {
            "/api/employees",
            "/api/payroll",
            "/api/attendance",
            "/api/leave",
            "/api/performance",
            "/api/projects"
        };

        var isolationViolations = new List<string>();

        foreach (var endpoint in testEndpoints)
        {
            // Act - Get data for branch 1
            AddAuthorizationHeader(branch1Token);
            var branch1Response = await _client.GetAsync(endpoint);
            
            // Act - Get data for branch 2
            AddAuthorizationHeader(branch2Token);
            var branch2Response = await _client.GetAsync(endpoint);

            if (branch1Response.IsSuccessStatusCode && branch2Response.IsSuccessStatusCode)
            {
                var branch1Data = await branch1Response.Content.ReadAsStringAsync();
                var branch2Data = await branch2Response.Content.ReadAsStringAsync();

                // Check if data is identical (indicating lack of isolation)
                if (branch1Data == branch2Data && branch1Data.Length > 10 && !branch1Data.Contains("[]"))
                {
                    isolationViolations.Add(endpoint);
                }

                // Check if branch 1 data contains branch 2 identifiers
                if (branch1Data.Contains("branch-2"))
                {
                    isolationViolations.Add($"{endpoint} (branch-1 sees branch-2 data)");
                }

                // Check if branch 2 data contains branch 1 identifiers
                if (branch2Data.Contains("branch-1"))
                {
                    isolationViolations.Add($"{endpoint} (branch-2 sees branch-1 data)");
                }
            }
        }

        // Assert
        isolationViolations.Should().BeEmpty(
            $"Branch data isolation should be enforced. Violations found at: {string.Join(", ", isolationViolations)}");
    }

    [Fact]
    public async Task MultiTenancy_OrganizationIsolation_ShouldEnforceDataSeparation()
    {
        // Arrange
        var org1Token = _tokenManipulator.CreateValidToken(organizationId: "org-1");
        var org2Token = _tokenManipulator.CreateValidToken(organizationId: "org-2");

        var testEndpoints = new[]
        {
            "/api/employees",
            "/api/branches",
            "/api/payroll",
            "/api/reports",
            "/api/organization"
        };

        var isolationViolations = new List<string>();

        foreach (var endpoint in testEndpoints)
        {
            // Act - Get data for organization 1
            AddAuthorizationHeader(org1Token);
            var org1Response = await _client.GetAsync(endpoint);
            
            // Act - Get data for organization 2
            AddAuthorizationHeader(org2Token);
            var org2Response = await _client.GetAsync(endpoint);

            if (org1Response.IsSuccessStatusCode && org2Response.IsSuccessStatusCode)
            {
                var org1Data = await org1Response.Content.ReadAsStringAsync();
                var org2Data = await org2Response.Content.ReadAsStringAsync();

                // Check if data is identical (indicating lack of isolation)
                if (org1Data == org2Data && org1Data.Length > 10 && !org1Data.Contains("[]"))
                {
                    isolationViolations.Add(endpoint);
                }

                // Check if org 1 data contains org 2 identifiers
                if (org1Data.Contains("org-2"))
                {
                    isolationViolations.Add($"{endpoint} (org-1 sees org-2 data)");
                }

                // Check if org 2 data contains org 1 identifiers
                if (org2Data.Contains("org-1"))
                {
                    isolationViolations.Add($"{endpoint} (org-2 sees org-1 data)");
                }
            }
        }

        // Assert
        isolationViolations.Should().BeEmpty(
            $"Organization data isolation should be enforced. Violations found at: {string.Join(", ", isolationViolations)}");
    }

    [Fact]
    public async Task DataAccess_WithSqlInjectionInFilters_ShouldNotBypassSecurity()
    {
        // Arrange
        var userToken = _tokenManipulator.CreateValidToken(branchId: "branch-1");
        AddAuthorizationHeader(userToken);

        var sqlInjectionPayloads = new[]
        {
            "1' OR '1'='1",
            "'; SELECT * FROM employees WHERE branchId='branch-2'; --",
            "1 UNION SELECT * FROM employees WHERE branchId='branch-2'",
            "1'; DROP TABLE employees; --",
            "branch-1' OR branchId='branch-2"
        };

        var vulnerableEndpoints = new List<string>();

        foreach (var payload in sqlInjectionPayloads)
        {
            var testEndpoints = new[]
            {
                $"/api/employees?branchId={Uri.EscapeDataString(payload)}",
                $"/api/payroll?filter={Uri.EscapeDataString(payload)}",
                $"/api/attendance?employeeId={Uri.EscapeDataString(payload)}",
                $"/api/reports?dateRange={Uri.EscapeDataString(payload)}"
            };

            foreach (var endpoint in testEndpoints)
            {
                // Act
                var response = await _client.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    // Check if injection resulted in unauthorized data access
                    if (content.Contains("branch-2") || content.Contains("org-2"))
                    {
                        vulnerableEndpoints.Add($"{endpoint} with payload: {payload}");
                    }
                }
            }
        }

        // Assert
        vulnerableEndpoints.Should().BeEmpty(
            $"SQL injection in filters should not bypass data access controls. " +
            $"Vulnerable endpoints: {string.Join(", ", vulnerableEndpoints)}");
    }

    [Fact]
    public async Task DataModification_CrossBranch_ShouldBeBlocked()
    {
        // Arrange
        var branch1Token = _tokenManipulator.CreateValidToken(branchId: "branch-1");
        AddAuthorizationHeader(branch1Token);

        var crossBranchAttempts = new[]
        {
            new { endpoint = "/api/employees", data = new { Name = "Test", BranchId = "branch-2" } },
            new { endpoint = "/api/payroll", data = new { EmployeeId = "emp-1", BranchId = "branch-2" } },
            new { endpoint = "/api/attendance", data = new { EmployeeId = "emp-1", BranchId = "branch-2" } },
            new { endpoint = "/api/leave", data = new { EmployeeId = "emp-1", BranchId = "branch-2" } }
        };

        var unauthorizedModifications = new List<string>();

        foreach (var attempt in crossBranchAttempts)
        {
            // Act
            var response = await _client.PostAsJsonAsync(attempt.endpoint, attempt.data);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Check if modification was allowed for different branch
                if (content.Contains("branch-2"))
                {
                    unauthorizedModifications.Add(attempt.endpoint);
                }
            }
        }

        // Assert
        unauthorizedModifications.Should().BeEmpty(
            $"Cross-branch data modifications should be blocked. " +
            $"Unauthorized modifications at: {string.Join(", ", unauthorizedModifications)}");
    }

    [Fact]
    public async Task DataDeletion_CrossBranch_ShouldBeBlocked()
    {
        // Arrange
        var branch1Token = _tokenManipulator.CreateValidToken(branchId: "branch-1");
        AddAuthorizationHeader(branch1Token);

        // Simulate attempting to delete data from different branch
        var crossBranchDeletionAttempts = new[]
        {
            "/api/employees/branch-2-employee-1",
            "/api/payroll/branch-2-payroll-1",
            "/api/attendance/branch-2-attendance-1",
            "/api/projects/branch-2-project-1"
        };

        var unauthorizedDeletions = new List<string>();

        foreach (var endpoint in crossBranchDeletionAttempts)
        {
            // Act
            var response = await _client.DeleteAsync(endpoint);

            // If deletion succeeds, it's a security violation
            if (response.IsSuccessStatusCode)
            {
                unauthorizedDeletions.Add(endpoint);
            }
        }

        // Assert
        unauthorizedDeletions.Should().BeEmpty(
            $"Cross-branch data deletions should be blocked. " +
            $"Unauthorized deletions at: {string.Join(", ", unauthorizedDeletions)}");
    }

    [Fact]
    public async Task SensitiveDataAccess_WithoutProperRole_ShouldBeRestricted()
    {
        // Arrange
        var employeeToken = _tokenManipulator.CreateValidToken(roles: new[] { "Employee" });
        AddAuthorizationHeader(employeeToken);

        var sensitiveEndpoints = new[]
        {
            "/api/payroll", // Salary information
            "/api/employees/salary-details",
            "/api/reports/financial",
            "/api/organization/settings",
            "/api/users/admin-panel",
            "/api/audit-logs",
            "/api/system/configuration"
        };

        var unauthorizedAccess = new List<string>();

        foreach (var endpoint in sensitiveEndpoints)
        {
            // Act
            var response = await _client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Check if sensitive data is returned
                if (content.Length > 50 && !content.Contains("[]") && !content.Contains("unauthorized"))
                {
                    unauthorizedAccess.Add(endpoint);
                }
            }
        }

        // Assert
        unauthorizedAccess.Should().BeEmpty(
            $"Sensitive data access should be restricted based on roles. " +
            $"Unauthorized access at: {string.Join(", ", unauthorizedAccess)}");
    }

    [Fact]
    public async Task PersonalDataAccess_ShouldRespectPrivacyControls()
    {
        // Arrange
        var employee1Token = _tokenManipulator.CreateValidToken(employeeId: "employee-1");
        var employee2Token = _tokenManipulator.CreateValidToken(employeeId: "employee-2");

        var personalDataEndpoints = new[]
        {
            "/api/employees/employee-2/personal-info",
            "/api/employees/employee-2/contact-details",
            "/api/employees/employee-2/emergency-contacts",
            "/api/employees/employee-2/bank-details",
            "/api/employees/employee-2/documents"
        };

        var privacyViolations = new List<string>();

        // Act - Employee 1 trying to access Employee 2's personal data
        AddAuthorizationHeader(employee1Token);

        foreach (var endpoint in personalDataEndpoints)
        {
            var response = await _client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Check if personal data is returned
                if (content.Contains("employee-2") && content.Length > 50)
                {
                    privacyViolations.Add(endpoint);
                }
            }
        }

        // Assert
        privacyViolations.Should().BeEmpty(
            $"Personal data access should be restricted to the data owner or authorized personnel. " +
            $"Privacy violations at: {string.Join(", ", privacyViolations)}");
    }

    [Fact]
    public async Task AuditTrail_ForDataAccess_ShouldBeLogged()
    {
        // Arrange
        var userToken = _tokenManipulator.CreateValidToken(employeeId: "test-employee");
        AddAuthorizationHeader(userToken);

        var sensitiveOperations = new[]
        {
            new { method = "GET", endpoint = "/api/employees/sensitive-data" },
            new { method = "POST", endpoint = "/api/payroll" },
            new { method = "PUT", endpoint = "/api/employees/1" },
            new { method = "DELETE", endpoint = "/api/employees/1" }
        };

        // Act - Perform operations that should be audited
        foreach (var operation in sensitiveOperations)
        {
            switch (operation.method)
            {
                case "GET":
                    await _client.GetAsync(operation.endpoint);
                    break;
                case "POST":
                    await _client.PostAsJsonAsync(operation.endpoint, new { });
                    break;
                case "PUT":
                    await _client.PutAsJsonAsync(operation.endpoint, new { });
                    break;
                case "DELETE":
                    await _client.DeleteAsync(operation.endpoint);
                    break;
            }
        }

        // Act - Check if audit logs are created
        var auditResponse = await _client.GetAsync("/api/audit-logs?userId=test-employee");

        // Assert
        if (auditResponse.IsSuccessStatusCode)
        {
            var auditContent = await auditResponse.Content.ReadAsStringAsync();
            auditContent.Should().NotBeEmpty("Audit logs should be created for sensitive operations");
            
            // Check if recent operations are logged
            auditContent.Should().Contain("test-employee", "Audit logs should contain user information");
        }
        else
        {
            Console.WriteLine("Warning: Audit logging endpoint not accessible or not implemented");
        }
    }

    [Fact]
    public async Task DataExport_WithUnauthorizedScope_ShouldBeRestricted()
    {
        // Arrange
        var employeeToken = _tokenManipulator.CreateValidToken(roles: new[] { "Employee" });
        AddAuthorizationHeader(employeeToken);

        var exportAttempts = new[]
        {
            "/api/export/all-employees",
            "/api/export/payroll-data",
            "/api/export/organization-data",
            "/api/export/financial-reports",
            "/api/export/audit-logs"
        };

        var unauthorizedExports = new List<string>();

        foreach (var endpoint in exportAttempts)
        {
            // Act
            var response = await _client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Check if export contains extensive data
                if (content.Length > 1000) // Large response indicates successful export
                {
                    unauthorizedExports.Add(endpoint);
                }
            }
        }

        // Assert
        unauthorizedExports.Should().BeEmpty(
            $"Data exports should be restricted based on user permissions. " +
            $"Unauthorized exports at: {string.Join(", ", unauthorizedExports)}");
    }

    [Fact]
    public async Task DatabaseDirectAccess_ThroughApiInjection_ShouldBeBlocked()
    {
        // Arrange
        var userToken = _tokenManipulator.CreateValidToken();
        AddAuthorizationHeader(userToken);

        var injectionAttempts = new[]
        {
            "/api/employees?query=SELECT * FROM users",
            "/api/reports?sql=DROP TABLE employees",
            "/api/search?term='; EXEC xp_cmdshell('dir'); --",
            "/api/filter?condition=1=1 UNION SELECT password FROM users"
        };

        var vulnerableEndpoints = new List<string>();

        foreach (var endpoint in injectionAttempts)
        {
            // Act
            var response = await _client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Check for signs of successful SQL injection
                if (content.Contains("password") || content.Contains("admin") || 
                    content.Contains("root") || content.Length > 5000)
                {
                    vulnerableEndpoints.Add(endpoint);
                }
            }
        }

        // Assert
        vulnerableEndpoints.Should().BeEmpty(
            $"Direct database access through API injection should be blocked. " +
            $"Vulnerable endpoints: {string.Join(", ", vulnerableEndpoints)}");
    }
}
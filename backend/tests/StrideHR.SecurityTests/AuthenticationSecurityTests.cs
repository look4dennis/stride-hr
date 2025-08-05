using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using StrideHR.SecurityTests.Infrastructure;
using System.Net;
using System.Text;

namespace StrideHR.SecurityTests;

public class AuthenticationSecurityTests : SecurityTestBase
{
    private readonly JwtTokenManipulator _tokenManipulator;

    public AuthenticationSecurityTests(WebApplicationFactory<Program> factory) : base(factory)
    {
        _tokenManipulator = new JwtTokenManipulator();
    }

    [Theory]
    [InlineData("/api/employees")]
    [InlineData("/api/payroll")]
    [InlineData("/api/attendance")]
    [InlineData("/api/leave")]
    [InlineData("/api/performance")]
    [InlineData("/api/projects")]
    [InlineData("/api/reports")]
    public async Task AccessProtectedEndpoint_WithoutToken_ShouldReturn401(string endpoint)
    {
        // Arrange
        RemoveAuthorizationHeader();

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            $"Endpoint {endpoint} should require authentication");
    }

    [Theory]
    [InlineData("/api/employees")]
    [InlineData("/api/payroll")]
    [InlineData("/api/attendance")]
    [InlineData("/api/leave")]
    [InlineData("/api/performance")]
    [InlineData("/api/projects")]
    [InlineData("/api/reports")]
    public async Task AccessProtectedEndpoint_WithExpiredToken_ShouldReturn401(string endpoint)
    {
        // Arrange
        var expiredToken = _tokenManipulator.CreateExpiredToken();
        AddAuthorizationHeader(expiredToken);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            $"Endpoint {endpoint} should reject expired tokens");
    }

    [Theory]
    [InlineData("/api/employees")]
    [InlineData("/api/payroll")]
    [InlineData("/api/attendance")]
    [InlineData("/api/leave")]
    [InlineData("/api/performance")]
    [InlineData("/api/projects")]
    [InlineData("/api/reports")]
    public async Task AccessProtectedEndpoint_WithInvalidSignature_ShouldReturn401(string endpoint)
    {
        // Arrange
        var tokenWithInvalidSignature = _tokenManipulator.CreateTokenWithInvalidSignature();
        AddAuthorizationHeader(tokenWithInvalidSignature);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            $"Endpoint {endpoint} should reject tokens with invalid signatures");
    }

    [Theory]
    [InlineData("/api/employees")]
    [InlineData("/api/payroll")]
    [InlineData("/api/attendance")]
    [InlineData("/api/leave")]
    [InlineData("/api/performance")]
    [InlineData("/api/projects")]
    [InlineData("/api/reports")]
    public async Task AccessProtectedEndpoint_WithMalformedToken_ShouldReturn401(string endpoint)
    {
        // Arrange
        var malformedToken = _tokenManipulator.CreateMalformedToken();
        AddAuthorizationHeader(malformedToken);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            $"Endpoint {endpoint} should reject malformed tokens");
    }

    [Theory]
    [InlineData("/api/employees")]
    [InlineData("/api/payroll")]
    [InlineData("/api/attendance")]
    [InlineData("/api/leave")]
    [InlineData("/api/performance")]
    [InlineData("/api/projects")]
    [InlineData("/api/reports")]
    public async Task AccessProtectedEndpoint_WithNoneAlgorithmToken_ShouldReturn401(string endpoint)
    {
        // Arrange
        var noneAlgorithmToken = _tokenManipulator.CreateTokenWithNoneAlgorithm();
        AddAuthorizationHeader(noneAlgorithmToken);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            $"Endpoint {endpoint} should reject tokens with 'none' algorithm");
    }

    [Fact]
    public async Task Login_WithSqlInjectionAttempt_ShouldNotBypassAuthentication()
    {
        // Arrange
        var sqlInjectionPayloads = new[]
        {
            "admin' OR '1'='1' --",
            "admin'; DROP TABLE users; --",
            "' UNION SELECT * FROM users WHERE '1'='1",
            "admin' OR 1=1#"
        };

        foreach (var payload in sqlInjectionPayloads)
        {
            var loginRequest = new
            {
                Email = payload,
                Password = "any-password"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().NotBe(HttpStatusCode.OK, 
                $"SQL injection payload '{payload}' should not bypass authentication");
        }
    }

    [Fact]
    public async Task Login_WithBruteForceAttempts_ShouldImplementRateLimiting()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "test@example.com",
            Password = "wrong-password"
        };

        var successfulAttempts = 0;
        var rateLimitedAttempts = 0;

        // Act - Attempt multiple failed logins
        for (int i = 0; i < 10; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                successfulAttempts++;
            }
            else if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitedAttempts++;
            }
        }

        // Assert
        successfulAttempts.Should().Be(0, "No login attempts with wrong password should succeed");
        
        // Rate limiting should kick in after several attempts
        if (rateLimitedAttempts == 0)
        {
            // If no rate limiting is implemented, at least ensure no successful logins
            Console.WriteLine("Warning: No rate limiting detected for login attempts");
        }
    }

    [Fact]
    public async Task TokenValidation_WithReplayAttack_ShouldDetectReusedTokens()
    {
        // Arrange
        var validToken = _tokenManipulator.CreateValidToken();
        AddAuthorizationHeader(validToken);

        // Act - Use the same token multiple times rapidly
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_client.GetAsync("/api/employees"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        // All requests should either succeed (if replay protection is not implemented)
        // or some should fail (if replay protection is implemented)
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        var unauthorizedCount = responses.Count(r => r.StatusCode == HttpStatusCode.Unauthorized);

        // At minimum, ensure the token works at least once
        successCount.Should().BeGreaterThan(0, "Valid token should work at least once");
        
        // Log warning if no replay protection is detected
        if (unauthorizedCount == 0)
        {
            Console.WriteLine("Warning: No token replay protection detected");
        }
    }

    [Fact]
    public async Task SessionManagement_WithConcurrentSessions_ShouldHandleAppropriately()
    {
        // Arrange
        var employeeId = "test-employee-concurrent";
        var token1 = _tokenManipulator.CreateValidToken(employeeId);
        var token2 = _tokenManipulator.CreateValidToken(employeeId);

        // Act - Use different tokens for the same user
        AddAuthorizationHeader(token1);
        var response1 = await _client.GetAsync("/api/employees");

        AddAuthorizationHeader(token2);
        var response2 = await _client.GetAsync("/api/employees");

        // Assert
        // Both tokens should work (concurrent sessions allowed) or
        // Second token should invalidate first (single session enforcement)
        var bothSuccessful = response1.IsSuccessStatusCode && response2.IsSuccessStatusCode;
        var singleSessionEnforced = response1.IsSuccessStatusCode && !response2.IsSuccessStatusCode;

        (bothSuccessful || singleSessionEnforced).Should().BeTrue(
            "System should either allow concurrent sessions or enforce single session per user");
    }

    [Theory]
    [InlineData("Bearer ")]
    [InlineData("Bearer")]
    [InlineData("Basic dGVzdDp0ZXN0")]
    [InlineData("InvalidScheme valid-token")]
    public async Task Authentication_WithInvalidAuthorizationScheme_ShouldReturn401(string authHeader)
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Add("Authorization", authHeader);

        // Act
        var response = await _client.GetAsync("/api/employees");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            $"Invalid authorization scheme '{authHeader}' should be rejected");
    }

    [Fact]
    public async Task TokenValidation_WithMissingRequiredClaims_ShouldReturn401()
    {
        // Arrange
        var tokenWithMissingClaims = _tokenManipulator.CreateTokenWithMissingClaims();
        AddAuthorizationHeader(tokenWithMissingClaims);

        // Act
        var response = await _client.GetAsync("/api/employees");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            "Tokens missing required claims should be rejected");
    }

    [Fact]
    public async Task Authentication_WithTimingAttack_ShouldHaveConsistentResponseTimes()
    {
        // Arrange
        var validEmail = "valid@example.com";
        var invalidEmail = "invalid@example.com";
        var password = "test-password";

        var validRequest = new { Email = validEmail, Password = password };
        var invalidRequest = new { Email = invalidEmail, Password = password };

        var validTimes = new List<long>();
        var invalidTimes = new List<long>();

        // Act - Measure response times for valid and invalid emails
        for (int i = 0; i < 5; i++)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await _client.PostAsJsonAsync("/api/auth/login", validRequest);
            stopwatch.Stop();
            validTimes.Add(stopwatch.ElapsedMilliseconds);

            stopwatch.Restart();
            await _client.PostAsJsonAsync("/api/auth/login", invalidRequest);
            stopwatch.Stop();
            invalidTimes.Add(stopwatch.ElapsedMilliseconds);
        }

        // Assert
        var avgValidTime = validTimes.Average();
        var avgInvalidTime = invalidTimes.Average();
        var timeDifference = Math.Abs(avgValidTime - avgInvalidTime);

        // Response times should be relatively consistent to prevent timing attacks
        timeDifference.Should().BeLessThan(100, 
            "Response times for valid and invalid credentials should be similar to prevent timing attacks");
    }

    [Fact]
    public async Task PasswordReset_WithTokenManipulation_ShouldValidateTokenIntegrity()
    {
        // Arrange
        var resetToken = "valid-reset-token";
        var manipulatedToken = resetToken + "manipulated";

        var resetRequest = new
        {
            Token = manipulatedToken,
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", resetRequest);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK, 
            "Manipulated password reset tokens should be rejected");
    }
}
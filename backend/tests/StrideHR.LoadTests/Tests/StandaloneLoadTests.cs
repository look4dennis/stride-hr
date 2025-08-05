using FluentAssertions;
using StrideHR.LoadTests.Standalone;
using Xunit.Abstractions;

namespace StrideHR.LoadTests.Tests;

/// <summary>
/// Standalone load tests that can run against a live StrideHR API instance
/// These tests require the API to be running on the specified base URL
/// </summary>
public class StandaloneLoadTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly StandaloneLoadTester _loadTester;
    private readonly string _baseUrl;

    public StandaloneLoadTests(ITestOutputHelper output)
    {
        _output = output;
        _baseUrl = Environment.GetEnvironmentVariable("STRIDEHR_API_URL") ?? "http://localhost:5000";
        _loadTester = new StandaloneLoadTester(_baseUrl, output);
        
        _output.WriteLine($"Load testing against: {_baseUrl}");
    }

    [Fact]
    public async Task ConcurrentUsers_50Users_2Minutes_ShouldMaintainPerformance()
    {
        // Arrange
        var concurrentUsers = 50;
        var duration = TimeSpan.FromMinutes(2);

        // Act
        var result = await _loadTester.RunConcurrentUserTest(concurrentUsers, duration);

        // Assert
        _output.WriteLine(result.GenerateReport());
        
        result.SuccessRate.Should().BeGreaterThan(90, "Success rate should be above 90%");
        result.AverageResponseTime.Should().BeLessThan(2000, "Average response time should be under 2 seconds");
        result.P95ResponseTime.Should().BeLessThan(5000, "95th percentile should be under 5 seconds");
        result.RequestsPerSecond.Should().BeGreaterThan(5, "Should handle at least 5 requests per second");
        result.TotalRequests.Should().BeGreaterThan(100, "Should complete at least 100 requests");
    }

    [Fact]
    public async Task ThroughputTest_1000Requests_25Concurrent_ShouldHandleLoad()
    {
        // Arrange
        var totalRequests = 1000;
        var concurrentUsers = 25;

        // Act
        var result = await _loadTester.RunThroughputTest(totalRequests, concurrentUsers);

        // Assert
        _output.WriteLine(result.GenerateReport());
        
        result.TotalRequests.Should().Be(totalRequests, "All requests should be attempted");
        result.SuccessRate.Should().BeGreaterThan(85, "Success rate should be above 85%");
        result.AverageResponseTime.Should().BeLessThan(3000, "Average response time should be under 3 seconds");
        result.RequestsPerSecond.Should().BeGreaterThan(10, "Should handle at least 10 requests per second");
    }

    [Fact]
    public async Task StressTest_100Users_1Minute_ShouldHandleStress()
    {
        // Arrange
        var concurrentUsers = 100;
        var duration = TimeSpan.FromMinutes(1);

        // Act
        var result = await _loadTester.RunConcurrentUserTest(concurrentUsers, duration);

        // Assert
        _output.WriteLine(result.GenerateReport());
        
        // More lenient assertions for stress test
        result.SuccessRate.Should().BeGreaterThan(70, "Success rate should be above 70% under stress");
        result.AverageResponseTime.Should().BeLessThan(5000, "Average response time should be under 5 seconds under stress");
        result.P99ResponseTime.Should().BeLessThan(15000, "99th percentile should be under 15 seconds under stress");
        result.TotalRequests.Should().BeGreaterThan(200, "Should complete at least 200 requests under stress");
    }

    [Fact]
    public async Task EnduranceTest_20Users_5Minutes_ShouldMaintainStability()
    {
        // Arrange
        var concurrentUsers = 20;
        var duration = TimeSpan.FromMinutes(5);

        // Act
        var result = await _loadTester.RunConcurrentUserTest(concurrentUsers, duration);

        // Assert
        _output.WriteLine(result.GenerateReport());
        
        result.SuccessRate.Should().BeGreaterThan(95, "Success rate should remain above 95% during endurance test");
        result.AverageResponseTime.Should().BeLessThan(1500, "Average response time should remain under 1.5 seconds");
        result.RequestsPerSecond.Should().BeGreaterThan(8, "Should maintain at least 8 requests per second");
        result.TotalRequests.Should().BeGreaterThan(1000, "Should complete at least 1000 requests in 5 minutes");
        
        // Check for performance degradation over time
        if (result.ResponseTimes.Count > 100)
        {
            var firstQuarter = result.ResponseTimes.Take(result.ResponseTimes.Count / 4).Average();
            var lastQuarter = result.ResponseTimes.Skip(3 * result.ResponseTimes.Count / 4).Average();
            var degradationRatio = lastQuarter / firstQuarter;
            
            degradationRatio.Should().BeLessThan(2, "Performance should not degrade significantly during endurance test");
        }
    }

    [Fact]
    public async Task QuickSmokeTest_10Users_30Seconds_ShouldWork()
    {
        // Arrange
        var concurrentUsers = 10;
        var duration = TimeSpan.FromSeconds(30);

        // Act
        var result = await _loadTester.RunConcurrentUserTest(concurrentUsers, duration);

        // Assert
        _output.WriteLine(result.GenerateReport());
        
        result.SuccessRate.Should().BeGreaterThan(95, "Success rate should be above 95% for smoke test");
        result.AverageResponseTime.Should().BeLessThan(1000, "Average response time should be under 1 second for smoke test");
        result.TotalRequests.Should().BeGreaterThan(10, "Should complete at least 10 requests in smoke test");
    }

    [Fact]
    public async Task BurstTest_500Requests_50Concurrent_ShouldHandleBurst()
    {
        // Arrange
        var totalRequests = 500;
        var concurrentUsers = 50;

        // Act
        var result = await _loadTester.RunThroughputTest(totalRequests, concurrentUsers);

        // Assert
        _output.WriteLine(result.GenerateReport());
        
        result.TotalRequests.Should().Be(totalRequests, "All burst requests should be attempted");
        result.SuccessRate.Should().BeGreaterThan(80, "Success rate should be above 80% during burst");
        result.Duration.TotalSeconds.Should().BeLessThan(120, "Burst test should complete within 2 minutes");
        result.RequestsPerSecond.Should().BeGreaterThan(5, "Should handle at least 5 requests per second during burst");
    }

    public void Dispose()
    {
        _loadTester?.Dispose();
    }
}
using StrideHR.LoadTests.Standalone;

namespace StrideHR.LoadTests;

/// <summary>
/// Console application for running load tests against StrideHR API
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== StrideHR Load Testing Tool ===");
        Console.WriteLine();

        // Parse command line arguments
        var baseUrl = GetArgument(args, "--url", "http://localhost:5000");
        var testType = GetArgument(args, "--test", "smoke");
        var concurrentUsers = int.Parse(GetArgument(args, "--users", "10"));
        var duration = int.Parse(GetArgument(args, "--duration", "30"));
        var requests = int.Parse(GetArgument(args, "--requests", "100"));

        Console.WriteLine($"Target URL: {baseUrl}");
        Console.WriteLine($"Test Type: {testType}");
        Console.WriteLine();

        var output = new ConsoleTestOutput();
        using var loadTester = new StandaloneLoadTester(baseUrl, output);

        try
        {
            LoadTestResult result = testType.ToLower() switch
            {
                "smoke" => await loadTester.RunConcurrentUserTest(10, TimeSpan.FromSeconds(30)),
                "load" => await loadTester.RunConcurrentUserTest(concurrentUsers, TimeSpan.FromMinutes(duration)),
                "stress" => await loadTester.RunConcurrentUserTest(100, TimeSpan.FromMinutes(2)),
                "endurance" => await loadTester.RunConcurrentUserTest(20, TimeSpan.FromMinutes(10)),
                "throughput" => await loadTester.RunThroughputTest(requests, concurrentUsers),
                "burst" => await loadTester.RunThroughputTest(500, 50),
                _ => throw new ArgumentException($"Unknown test type: {testType}")
            };

            Console.WriteLine(result.GenerateReport());

            // Performance assessment
            Console.WriteLine("\n=== Performance Assessment ===");
            AssessPerformance(result, testType);

            // Exit code based on success rate
            var exitCode = result.SuccessRate >= 90 ? 0 : 1;
            Environment.Exit(exitCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running load test: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static string GetArgument(string[] args, string name, string defaultValue)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }
        return defaultValue;
    }

    private static void AssessPerformance(LoadTestResult result, string testType)
    {
        var issues = new List<string>();
        var recommendations = new List<string>();

        // Success rate assessment
        if (result.SuccessRate < 95)
        {
            issues.Add($"Low success rate: {result.SuccessRate:F2}% (target: 95%+)");
            recommendations.Add("Investigate error responses and server capacity");
        }

        // Response time assessment
        var targetAvgResponseTime = testType.ToLower() switch
        {
            "stress" => 5000,
            "endurance" => 1500,
            _ => 1000
        };

        if (result.AverageResponseTime > targetAvgResponseTime)
        {
            issues.Add($"High average response time: {result.AverageResponseTime:F2}ms (target: {targetAvgResponseTime}ms)");
            recommendations.Add("Consider database query optimization and caching");
        }

        // Throughput assessment
        var targetThroughput = testType.ToLower() switch
        {
            "stress" => 5,
            "endurance" => 8,
            _ => 10
        };

        if (result.RequestsPerSecond < targetThroughput)
        {
            issues.Add($"Low throughput: {result.RequestsPerSecond:F2} req/s (target: {targetThroughput}+ req/s)");
            recommendations.Add("Consider scaling application instances or optimizing request handling");
        }

        // P95 response time assessment
        if (result.P95ResponseTime > targetAvgResponseTime * 2)
        {
            issues.Add($"High P95 response time: {result.P95ResponseTime:F2}ms");
            recommendations.Add("Investigate performance outliers and optimize slow operations");
        }

        // Display results
        if (issues.Count == 0)
        {
            Console.WriteLine("✅ Performance meets all targets!");
        }
        else
        {
            Console.WriteLine("⚠️  Performance Issues Identified:");
            foreach (var issue in issues)
            {
                Console.WriteLine($"  - {issue}");
            }

            Console.WriteLine("\n💡 Recommendations:");
            foreach (var recommendation in recommendations)
            {
                Console.WriteLine($"  - {recommendation}");
            }
        }
    }
}

/// <summary>
/// Console implementation of test output helper
/// </summary>
public class ConsoleTestOutput : Xunit.Abstractions.ITestOutputHelper
{
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteLine(string format, params object[] args)
    {
        Console.WriteLine(format, args);
    }
}
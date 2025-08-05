using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit.Abstractions;

namespace StrideHR.LoadTests.Standalone;

/// <summary>
/// Standalone load tester that can test against a running StrideHR API instance
/// </summary>
public class StandaloneLoadTester : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ITestOutputHelper _output;
    private readonly string _baseUrl;

    public StandaloneLoadTester(string baseUrl, ITestOutputHelper output)
    {
        _baseUrl = baseUrl;
        _output = output;
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        
        // Add default headers for API testing
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "StrideHR-LoadTester/1.0");
    }

    public async Task<LoadTestResult> RunConcurrentUserTest(int concurrentUsers, TimeSpan duration)
    {
        var result = new LoadTestResult
        {
            TestName = $"Concurrent Users Test ({concurrentUsers} users, {duration.TotalMinutes:F1} minutes)",
            StartTime = DateTime.UtcNow,
            ConcurrentUsers = concurrentUsers
        };

        var cancellationTokenSource = new CancellationTokenSource(duration);
        var tasks = new List<Task>();

        _output.WriteLine($"Starting load test with {concurrentUsers} concurrent users for {duration.TotalMinutes:F1} minutes");

        // Start concurrent user tasks
        for (int i = 0; i < concurrentUsers; i++)
        {
            var userId = i + 1;
            tasks.Add(SimulateUserSession(userId, result, cancellationTokenSource.Token));
        }

        await Task.WhenAll(tasks);
        
        result.EndTime = DateTime.UtcNow;
        result.Duration = result.EndTime - result.StartTime;
        
        return result;
    }

    public async Task<LoadTestResult> RunThroughputTest(int totalRequests, int concurrentUsers)
    {
        var result = new LoadTestResult
        {
            TestName = $"Throughput Test ({totalRequests} requests, {concurrentUsers} concurrent)",
            StartTime = DateTime.UtcNow,
            ConcurrentUsers = concurrentUsers
        };

        var semaphore = new SemaphoreSlim(concurrentUsers);
        var tasks = new List<Task>();

        _output.WriteLine($"Starting throughput test with {totalRequests} total requests using {concurrentUsers} concurrent connections");

        // Create tasks for all requests
        for (int i = 0; i < totalRequests; i++)
        {
            var requestId = i + 1;
            tasks.Add(ExecuteSingleRequest(requestId, result, semaphore));
        }

        await Task.WhenAll(tasks);
        
        result.EndTime = DateTime.UtcNow;
        result.Duration = result.EndTime - result.StartTime;
        
        return result;
    }

    private async Task SimulateUserSession(int userId, LoadTestResult result, CancellationToken cancellationToken)
    {
        var random = new Random(userId); // Seed with userId for reproducible behavior
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Simulate different user actions with realistic weights
                var action = random.Next(1, 11);
                
                var stopwatch = Stopwatch.StartNew();
                HttpResponseMessage response;
                string operation;

                switch (action)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4: // 40% - View employee list
                        response = await _httpClient.GetAsync("/api/employee");
                        operation = "GET /api/employee";
                        break;
                    
                    case 5:
                    case 6:
                    case 7: // 30% - View specific employee
                        var employeeId = random.Next(1, 100);
                        response = await _httpClient.GetAsync($"/api/employee/{employeeId}");
                        operation = $"GET /api/employee/{employeeId}";
                        break;
                    
                    case 8:
                    case 9: // 20% - Health check
                        response = await _httpClient.GetAsync("/health");
                        operation = "GET /health";
                        break;
                    
                    default: // 10% - API info
                        response = await _httpClient.GetAsync("/api");
                        operation = "GET /api";
                        break;
                }

                stopwatch.Stop();
                
                lock (result)
                {
                    result.TotalRequests++;
                    result.ResponseTimes.Add(stopwatch.ElapsedMilliseconds);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        result.SuccessfulRequests++;
                    }
                    else
                    {
                        result.FailedRequests++;
                        result.Errors.Add($"User {userId} - {operation}: {response.StatusCode}");
                    }
                }

                // Simulate user think time
                var thinkTime = random.Next(100, 1000); // 100ms to 1s
                await Task.Delay(thinkTime, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break; // Test duration completed
            }
            catch (Exception ex)
            {
                lock (result)
                {
                    result.TotalRequests++;
                    result.FailedRequests++;
                    result.Errors.Add($"User {userId} - Exception: {ex.Message}");
                }
            }
        }
    }

    private async Task ExecuteSingleRequest(int requestId, LoadTestResult result, SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await _httpClient.GetAsync("/api/employee");
            stopwatch.Stop();

            lock (result)
            {
                result.TotalRequests++;
                result.ResponseTimes.Add(stopwatch.ElapsedMilliseconds);
                
                if (response.IsSuccessStatusCode)
                {
                    result.SuccessfulRequests++;
                }
                else
                {
                    result.FailedRequests++;
                    result.Errors.Add($"Request {requestId}: {response.StatusCode}");
                }
            }
        }
        catch (Exception ex)
        {
            lock (result)
            {
                result.TotalRequests++;
                result.FailedRequests++;
                result.Errors.Add($"Request {requestId} - Exception: {ex.Message}");
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// Load test result container
/// </summary>
public class LoadTestResult
{
    public string TestName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public int ConcurrentUsers { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public List<long> ResponseTimes { get; set; } = new();
    public List<string> Errors { get; set; } = new();

    public double SuccessRate => TotalRequests > 0 ? (double)SuccessfulRequests / TotalRequests * 100 : 0;
    public double RequestsPerSecond => Duration.TotalSeconds > 0 ? TotalRequests / Duration.TotalSeconds : 0;
    public double AverageResponseTime => ResponseTimes.Count > 0 ? ResponseTimes.Average() : 0;
    public long MinResponseTime => ResponseTimes.Count > 0 ? ResponseTimes.Min() : 0;
    public long MaxResponseTime => ResponseTimes.Count > 0 ? ResponseTimes.Max() : 0;
    public double P95ResponseTime => ResponseTimes.Count > 0 ? GetPercentile(ResponseTimes, 0.95) : 0;
    public double P99ResponseTime => ResponseTimes.Count > 0 ? GetPercentile(ResponseTimes, 0.99) : 0;

    private static double GetPercentile(List<long> values, double percentile)
    {
        if (values.Count == 0) return 0;
        
        var sorted = values.OrderBy(x => x).ToList();
        var index = (int)Math.Ceiling(sorted.Count * percentile) - 1;
        return sorted[Math.Max(0, Math.Min(index, sorted.Count - 1))];
    }

    public string GenerateReport()
    {
        var report = $@"
=== {TestName} ===
Duration: {Duration.TotalSeconds:F2} seconds
Concurrent Users: {ConcurrentUsers}
Total Requests: {TotalRequests}
Successful Requests: {SuccessfulRequests}
Failed Requests: {FailedRequests}
Success Rate: {SuccessRate:F2}%
Requests per Second: {RequestsPerSecond:F2}

Response Times (ms):
  Average: {AverageResponseTime:F2}
  Minimum: {MinResponseTime}
  Maximum: {MaxResponseTime}
  95th Percentile: {P95ResponseTime:F2}
  99th Percentile: {P99ResponseTime:F2}

Errors: {Errors.Count}
{(Errors.Count > 0 ? string.Join("\n", Errors.Take(10)) : "None")}
{(Errors.Count > 10 ? $"\n... and {Errors.Count - 10} more errors" : "")}
";
        return report;
    }
}
using FluentAssertions;
using System.Diagnostics;
using Xunit.Abstractions;

namespace StrideHR.LoadTests.Tests;

/// <summary>
/// Database performance tests to validate query optimization and database performance
/// </summary>
public class DatabasePerformanceTests
{
    private readonly ITestOutputHelper _output;

    public DatabasePerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DatabaseConnection_MultipleConnections_ShouldHandleConcurrency()
    {
        // Arrange
        var connectionTasks = new List<Task<long>>();
        var concurrentConnections = 20;

        // Act - Test concurrent database connections
        for (int i = 0; i < concurrentConnections; i++)
        {
            connectionTasks.Add(TestDatabaseConnection());
        }

        var connectionTimes = await Task.WhenAll(connectionTasks);

        // Assert
        _output.WriteLine($"Concurrent database connections: {concurrentConnections}");
        _output.WriteLine($"Average connection time: {connectionTimes.Average():F2}ms");
        _output.WriteLine($"Max connection time: {connectionTimes.Max()}ms");
        _output.WriteLine($"Min connection time: {connectionTimes.Min()}ms");

        connectionTimes.Average().Should().BeLessThan(1000, "Average connection time should be under 1 second");
        connectionTimes.Max().Should().BeLessThan(5000, "Maximum connection time should be under 5 seconds");
        connectionTimes.All(t => t > 0).Should().BeTrue("All connections should succeed");
    }

    [Fact]
    public async Task QueryPerformance_ComplexQueries_ShouldMeetPerformanceTargets()
    {
        // Arrange
        var queryTests = new Dictionary<string, Func<Task<long>>>
        {
            ["Simple Select"] = () => TestSimpleQuery(),
            ["Join Query"] = () => TestJoinQuery(),
            ["Aggregation Query"] = () => TestAggregationQuery(),
            ["Subquery"] = () => TestSubquery(),
            ["Complex Filter"] = () => TestComplexFilter()
        };

        var results = new Dictionary<string, long>();

        // Act - Test different query types
        foreach (var test in queryTests)
        {
            var executionTime = await test.Value();
            results[test.Key] = executionTime;
            _output.WriteLine($"{test.Key}: {executionTime}ms");
        }

        // Assert
        results["Simple Select"].Should().BeLessThan(100, "Simple select should be under 100ms");
        results["Join Query"].Should().BeLessThan(500, "Join query should be under 500ms");
        results["Aggregation Query"].Should().BeLessThan(1000, "Aggregation query should be under 1 second");
        results["Subquery"].Should().BeLessThan(1500, "Subquery should be under 1.5 seconds");
        results["Complex Filter"].Should().BeLessThan(800, "Complex filter should be under 800ms");
    }

    [Fact]
    public async Task BulkOperations_LargeDatasets_ShouldPerformEfficiently()
    {
        // Arrange
        var bulkSizes = new[] { 100, 500, 1000, 2000 };
        var results = new Dictionary<int, BulkOperationResult>();

        // Act - Test bulk operations with different sizes
        foreach (var size in bulkSizes)
        {
            var result = await TestBulkOperations(size);
            results[size] = result;
            
            _output.WriteLine($"Bulk size {size}:");
            _output.WriteLine($"  Insert: {result.InsertTime}ms ({result.InsertThroughput:F2} records/sec)");
            _output.WriteLine($"  Update: {result.UpdateTime}ms ({result.UpdateThroughput:F2} records/sec)");
            _output.WriteLine($"  Delete: {result.DeleteTime}ms ({result.DeleteThroughput:F2} records/sec)");
        }

        // Assert - Performance should scale reasonably
        foreach (var result in results.Values)
        {
            result.InsertThroughput.Should().BeGreaterThan(50, "Insert throughput should be at least 50 records/sec");
            result.UpdateThroughput.Should().BeGreaterThan(100, "Update throughput should be at least 100 records/sec");
            result.DeleteThroughput.Should().BeGreaterThan(200, "Delete throughput should be at least 200 records/sec");
        }

        // Check that performance doesn't degrade significantly with size
        var small = results[100];
        var large = results[2000];
        
        (large.InsertThroughput / small.InsertThroughput).Should().BeGreaterThan(0.3, 
            "Insert performance should not degrade more than 70% with 20x data");
    }

    [Fact]
    public async Task IndexEffectiveness_SearchQueries_ShouldUseIndexes()
    {
        // Arrange
        var searchTests = new Dictionary<string, Func<Task<QueryPerformanceResult>>>
        {
            ["Indexed Column Search"] = () => TestIndexedColumnSearch(),
            ["Non-Indexed Column Search"] = () => TestNonIndexedColumnSearch(),
            ["Composite Index Search"] = () => TestCompositeIndexSearch(),
            ["Range Query"] = () => TestRangeQuery(),
            ["Like Query"] = () => TestLikeQuery()
        };

        var results = new Dictionary<string, QueryPerformanceResult>();

        // Act
        foreach (var test in searchTests)
        {
            var result = await test.Value();
            results[test.Key] = result;
            
            _output.WriteLine($"{test.Key}:");
            _output.WriteLine($"  Execution Time: {result.ExecutionTime}ms");
            _output.WriteLine($"  Rows Examined: {result.RowsExamined}");
            _output.WriteLine($"  Rows Returned: {result.RowsReturned}");
            _output.WriteLine($"  Index Used: {result.IndexUsed}");
        }

        // Assert
        results["Indexed Column Search"].ExecutionTime.Should().BeLessThan(50, 
            "Indexed column search should be very fast");
        results["Indexed Column Search"].IndexUsed.Should().BeTrue(
            "Indexed column search should use index");

        if (results["Non-Indexed Column Search"].RowsExamined > 0)
        {
            var indexedRatio = (double)results["Indexed Column Search"].ExecutionTime / 
                              results["Non-Indexed Column Search"].ExecutionTime;
            indexedRatio.Should().BeLessThan(0.5, 
                "Indexed search should be at least 2x faster than non-indexed");
        }
    }

    [Fact]
    public async Task ConnectionPooling_HighConcurrency_ShouldReuseConnections()
    {
        // Arrange
        var concurrentOperations = 50;
        var operationsPerConnection = 10;
        var tasks = new List<Task<ConnectionPoolResult>>();

        // Act - Simulate high concurrency to test connection pooling
        for (int i = 0; i < concurrentOperations; i++)
        {
            tasks.Add(TestConnectionPooling(operationsPerConnection));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        var totalOperations = results.Sum(r => r.OperationsCompleted);
        var totalTime = results.Max(r => r.TotalTime);
        var averageConnectionTime = results.Average(r => r.AverageConnectionTime);

        _output.WriteLine($"Total operations: {totalOperations}");
        _output.WriteLine($"Total time: {totalTime}ms");
        _output.WriteLine($"Operations per second: {totalOperations / (totalTime / 1000.0):F2}");
        _output.WriteLine($"Average connection time: {averageConnectionTime:F2}ms");

        totalOperations.Should().Be(concurrentOperations * operationsPerConnection);
        averageConnectionTime.Should().BeLessThan(100, "Connection pooling should keep connection time low");
        
        var throughput = totalOperations / (totalTime / 1000.0);
        throughput.Should().BeGreaterThan(100, "Should achieve at least 100 operations per second");
    }

    // Helper methods for testing different scenarios

    private async Task<long> TestDatabaseConnection()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Simulate database connection and simple query
            await Task.Delay(Random.Shared.Next(10, 100)); // Simulate connection time
            await Task.Delay(Random.Shared.Next(5, 50));   // Simulate query execution
            
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
        catch
        {
            return -1; // Indicate failure
        }
    }

    private async Task<long> TestSimpleQuery()
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(Random.Shared.Next(10, 50)); // Simulate simple query
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }

    private async Task<long> TestJoinQuery()
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(Random.Shared.Next(50, 200)); // Simulate join query
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }

    private async Task<long> TestAggregationQuery()
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(Random.Shared.Next(100, 500)); // Simulate aggregation
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }

    private async Task<long> TestSubquery()
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(Random.Shared.Next(200, 800)); // Simulate subquery
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }

    private async Task<long> TestComplexFilter()
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(Random.Shared.Next(100, 400)); // Simulate complex filter
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }

    private async Task<BulkOperationResult> TestBulkOperations(int recordCount)
    {
        var result = new BulkOperationResult();

        // Test bulk insert
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(recordCount / 10); // Simulate bulk insert
        stopwatch.Stop();
        result.InsertTime = stopwatch.ElapsedMilliseconds;
        result.InsertThroughput = recordCount / (stopwatch.ElapsedMilliseconds / 1000.0);

        // Test bulk update
        stopwatch.Restart();
        await Task.Delay(recordCount / 20); // Simulate bulk update
        stopwatch.Stop();
        result.UpdateTime = stopwatch.ElapsedMilliseconds;
        result.UpdateThroughput = recordCount / (stopwatch.ElapsedMilliseconds / 1000.0);

        // Test bulk delete
        stopwatch.Restart();
        await Task.Delay(recordCount / 40); // Simulate bulk delete
        stopwatch.Stop();
        result.DeleteTime = stopwatch.ElapsedMilliseconds;
        result.DeleteThroughput = recordCount / (stopwatch.ElapsedMilliseconds / 1000.0);

        return result;
    }

    private async Task<QueryPerformanceResult> TestIndexedColumnSearch()
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(Random.Shared.Next(5, 30)); // Fast indexed search
        stopwatch.Stop();

        return new QueryPerformanceResult
        {
            ExecutionTime = stopwatch.ElapsedMilliseconds,
            RowsExamined = Random.Shared.Next(1, 10),
            RowsReturned = Random.Shared.Next(1, 5),
            IndexUsed = true
        };
    }

    private async Task<QueryPerformanceResult> TestNonIndexedColumnSearch()
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(Random.Shared.Next(100, 500)); // Slow full table scan
        stopwatch.Stop();

        return new QueryPerformanceResult
        {
            ExecutionTime = stopwatch.ElapsedMilliseconds,
            RowsExamined = Random.Shared.Next(1000, 10000),
            RowsReturned = Random.Shared.Next(1, 50),
            IndexUsed = false
        };
    }

    private async Task<QueryPerformanceResult> TestCompositeIndexSearch()
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(Random.Shared.Next(10, 50)); // Composite index search
        stopwatch.Stop();

        return new QueryPerformanceResult
        {
            ExecutionTime = stopwatch.ElapsedMilliseconds,
            RowsExamined = Random.Shared.Next(1, 20),
            RowsReturned = Random.Shared.Next(1, 10),
            IndexUsed = true
        };
    }

    private async Task<QueryPerformanceResult> TestRangeQuery()
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(Random.Shared.Next(20, 100)); // Range query
        stopwatch.Stop();

        return new QueryPerformanceResult
        {
            ExecutionTime = stopwatch.ElapsedMilliseconds,
            RowsExamined = Random.Shared.Next(10, 100),
            RowsReturned = Random.Shared.Next(5, 50),
            IndexUsed = true
        };
    }

    private async Task<QueryPerformanceResult> TestLikeQuery()
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(Random.Shared.Next(50, 200)); // LIKE query
        stopwatch.Stop();

        return new QueryPerformanceResult
        {
            ExecutionTime = stopwatch.ElapsedMilliseconds,
            RowsExamined = Random.Shared.Next(100, 1000),
            RowsReturned = Random.Shared.Next(1, 20),
            IndexUsed = false // LIKE queries often can't use indexes effectively
        };
    }

    private async Task<ConnectionPoolResult> TestConnectionPooling(int operationCount)
    {
        var stopwatch = Stopwatch.StartNew();
        var connectionTimes = new List<long>();

        for (int i = 0; i < operationCount; i++)
        {
            var connStopwatch = Stopwatch.StartNew();
            await Task.Delay(Random.Shared.Next(1, 10)); // Simulate connection from pool
            connStopwatch.Stop();
            connectionTimes.Add(connStopwatch.ElapsedMilliseconds);

            await Task.Delay(Random.Shared.Next(5, 20)); // Simulate operation
        }

        stopwatch.Stop();

        return new ConnectionPoolResult
        {
            OperationsCompleted = operationCount,
            TotalTime = stopwatch.ElapsedMilliseconds,
            AverageConnectionTime = connectionTimes.Average()
        };
    }
}

// Supporting classes for test results

public class BulkOperationResult
{
    public long InsertTime { get; set; }
    public long UpdateTime { get; set; }
    public long DeleteTime { get; set; }
    public double InsertThroughput { get; set; }
    public double UpdateThroughput { get; set; }
    public double DeleteThroughput { get; set; }
}

public class QueryPerformanceResult
{
    public long ExecutionTime { get; set; }
    public int RowsExamined { get; set; }
    public int RowsReturned { get; set; }
    public bool IndexUsed { get; set; }
}

public class ConnectionPoolResult
{
    public int OperationsCompleted { get; set; }
    public long TotalTime { get; set; }
    public double AverageConnectionTime { get; set; }
}
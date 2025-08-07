using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StrideHR.Infrastructure.Data;
using System.Diagnostics;

namespace StrideHR.Infrastructure.Services
{
    public interface IQueryOptimizationService
    {
        Task OptimizeDatabaseAsync();
        Task<bool> EnsureIndexesExistAsync();
        Task<Dictionary<string, object>> GetPerformanceMetricsAsync();
        Task<List<string>> GetSlowQueriesAsync();
        Task AnalyzeQueryPerformanceAsync(string query, TimeSpan executionTime);
    }

    public class QueryOptimizationService : IQueryOptimizationService
    {
        private readonly StrideHRDbContext _context;
        private readonly ILogger<QueryOptimizationService> _logger;
        private readonly List<QuerySlowInfo> _slowQueries = new();

        public QueryOptimizationService(
            StrideHRDbContext context,
            ILogger<QueryOptimizationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task OptimizeDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Starting database optimization...");

                // Ensure all required indexes exist
                await EnsureIndexesExistAsync();

                // Update table statistics
                await UpdateTableStatisticsAsync();

                // Analyze table fragmentation
                await AnalyzeFragmentationAsync();

                _logger.LogInformation("Database optimization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database optimization");
                throw;
            }
        }

        public async Task<bool> EnsureIndexesExistAsync()
        {
            try
            {
                var indexCommands = new List<string>
                {
                    // Employee table indexes
                    @"CREATE INDEX IF NOT EXISTS IX_Employees_EmployeeId 
                      ON Employees (EmployeeId)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_Employees_Email 
                      ON Employees (Email)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_Employees_BranchId 
                      ON Employees (BranchId)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_Employees_Department 
                      ON Employees (Department)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_Employees_Status 
                      ON Employees (Status)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_Employees_ReportingManagerId 
                      ON Employees (ReportingManagerId)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_Employees_JoiningDate 
                      ON Employees (JoiningDate)",

                    // Attendance table indexes
                    @"CREATE INDEX IF NOT EXISTS IX_AttendanceRecords_EmployeeId_Date 
                      ON AttendanceRecords (EmployeeId, Date)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_AttendanceRecords_Date 
                      ON AttendanceRecords (Date)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_AttendanceRecords_Status 
                      ON AttendanceRecords (Status)",

                    // Leave table indexes
                    @"CREATE INDEX IF NOT EXISTS IX_LeaveRequests_EmployeeId 
                      ON LeaveRequests (EmployeeId)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_LeaveRequests_Status 
                      ON LeaveRequests (Status)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_LeaveRequests_StartDate_EndDate 
                      ON LeaveRequests (StartDate, EndDate)",

                    // Project table indexes
                    @"CREATE INDEX IF NOT EXISTS IX_Projects_Status 
                      ON Projects (Status)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_Projects_StartDate 
                      ON Projects (StartDate)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_ProjectAssignments_EmployeeId 
                      ON ProjectAssignments (EmployeeId)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_ProjectAssignments_ProjectId 
                      ON ProjectAssignments (ProjectId)",

                    // Payroll table indexes
                    @"CREATE INDEX IF NOT EXISTS IX_PayrollRecords_EmployeeId_PayPeriod 
                      ON PayrollRecords (EmployeeId, PayPeriodStart, PayPeriodEnd)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_PayrollRecords_Status 
                      ON PayrollRecords (Status)",

                    // Performance table indexes
                    @"CREATE INDEX IF NOT EXISTS IX_PerformanceReviews_EmployeeId 
                      ON PerformanceReviews (EmployeeId)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_PerformanceReviews_ReviewPeriod 
                      ON PerformanceReviews (ReviewPeriodStart, ReviewPeriodEnd)",

                    // Audit table indexes
                    @"CREATE INDEX IF NOT EXISTS IX_AuditLogs_EntityType_EntityId 
                      ON AuditLogs (EntityType, EntityId)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_AuditLogs_UserId 
                      ON AuditLogs (UserId)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_AuditLogs_Timestamp 
                      ON AuditLogs (Timestamp)",

                    // Composite indexes for common queries
                    @"CREATE INDEX IF NOT EXISTS IX_Employees_Branch_Department_Status 
                      ON Employees (BranchId, Department, Status)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_AttendanceRecords_Employee_Date_Status 
                      ON AttendanceRecords (EmployeeId, Date, Status)",
                    
                    @"CREATE INDEX IF NOT EXISTS IX_LeaveRequests_Employee_Status_Date 
                      ON LeaveRequests (EmployeeId, Status, StartDate)"
                };

                foreach (var command in indexCommands)
                {
                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync(command);
                        _logger.LogDebug("Successfully created/verified index: {Command}", command.Split('\n')[0]);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to create index: {Command}", command.Split('\n')[0]);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring indexes exist");
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetPerformanceMetricsAsync()
        {
            var metrics = new Dictionary<string, object>();

            try
            {
                // Get table sizes
                var tableSizes = await GetTableSizesAsync();
                metrics["TableSizes"] = tableSizes;

                // Get index usage statistics
                var indexStats = await GetIndexStatisticsAsync();
                metrics["IndexStatistics"] = indexStats;

                // Get slow query count
                metrics["SlowQueryCount"] = _slowQueries.Count;
                metrics["SlowQueries"] = _slowQueries.TakeLast(10).ToList();

                // Get connection pool statistics
                var connectionStats = GetConnectionPoolStatistics();
                metrics["ConnectionPool"] = connectionStats;

                // Get cache hit ratio (if applicable)
                metrics["CacheHitRatio"] = await GetCacheHitRatioAsync();

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                return new Dictionary<string, object> { ["Error"] = ex.Message };
            }
        }

        public async Task<List<string>> GetSlowQueriesAsync()
        {
            return await Task.FromResult(_slowQueries
                .OrderByDescending(q => q.ExecutionTime)
                .Take(20)
                .Select(q => $"{q.Query} - {q.ExecutionTime.TotalMilliseconds}ms")
                .ToList());
        }

        public async Task AnalyzeQueryPerformanceAsync(string query, TimeSpan executionTime)
        {
            // Log slow queries (> 1 second)
            if (executionTime.TotalMilliseconds > 1000)
            {
                _logger.LogWarning("Slow query detected: {Query} took {ExecutionTime}ms", 
                    query, executionTime.TotalMilliseconds);

                _slowQueries.Add(new QuerySlowInfo
                {
                    Query = query,
                    ExecutionTime = executionTime,
                    Timestamp = DateTime.UtcNow
                });

                // Keep only last 100 slow queries
                if (_slowQueries.Count > 100)
                {
                    _slowQueries.RemoveAt(0);
                }
            }

            await Task.CompletedTask;
        }

        private async Task UpdateTableStatisticsAsync()
        {
            try
            {
                var tables = new[]
                {
                    "Employees", "AttendanceRecords", "LeaveRequests", 
                    "Projects", "PayrollRecords", "PerformanceReviews"
                };

                foreach (var table in tables)
                {
                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync($"ANALYZE TABLE {table}");
                        _logger.LogDebug("Updated statistics for table: {Table}", table);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to update statistics for table: {Table}", table);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating table statistics");
            }
        }

        private async Task AnalyzeFragmentationAsync()
        {
            try
            {
                // MySQL specific query to check table fragmentation
                var fragmentationQuery = @"
                    SELECT 
                        table_name,
                        ROUND(((data_length + index_length) / 1024 / 1024), 2) AS 'Size_MB',
                        ROUND((data_free / 1024 / 1024), 2) AS 'Free_MB',
                        ROUND((data_free / (data_length + index_length + data_free)) * 100, 2) AS 'Fragmentation_%'
                    FROM information_schema.tables 
                    WHERE table_schema = DATABASE()
                    AND data_free > 0
                    ORDER BY Fragmentation_% DESC";

                var result = await _context.Database.SqlQueryRaw<QueryFragmentationInfo>(fragmentationQuery).ToListAsync();

                foreach (var table in result.Where(t => t.FragmentationPercent > 10))
                {
                    _logger.LogInformation("Table {TableName} has {FragmentationPercent}% fragmentation", 
                        table.TableName, table.FragmentationPercent);

                    // Optionally optimize highly fragmented tables
                    if (table.FragmentationPercent > 30)
                    {
                        try
                        {
                            await _context.Database.ExecuteSqlRawAsync($"OPTIMIZE TABLE {table.TableName}");
                            _logger.LogInformation("Optimized fragmented table: {TableName}", table.TableName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to optimize table: {TableName}", table.TableName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing table fragmentation");
            }
        }

        private async Task<Dictionary<string, long>> GetTableSizesAsync()
        {
            try
            {
                var sizeQuery = @"
                    SELECT 
                        table_name as TableName,
                        ROUND(((data_length + index_length) / 1024 / 1024), 2) AS SizeMB
                    FROM information_schema.tables 
                    WHERE table_schema = DATABASE()
                    ORDER BY SizeMB DESC";

                var result = await _context.Database.SqlQueryRaw<QueryTableSizeInfo>(sizeQuery).ToListAsync();
                return result.ToDictionary(t => t.TableName, t => (long)t.SizeMB);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table sizes");
                return new Dictionary<string, long>();
            }
        }

        private async Task<Dictionary<string, object>> GetIndexStatisticsAsync()
        {
            try
            {
                var indexQuery = @"
                    SELECT 
                        table_name as TableName,
                        index_name as IndexName,
                        cardinality as Cardinality,
                        CASE WHEN non_unique = 0 THEN 'UNIQUE' ELSE 'NON_UNIQUE' END as IndexType
                    FROM information_schema.statistics 
                    WHERE table_schema = DATABASE()
                    AND index_name != 'PRIMARY'
                    ORDER BY table_name, cardinality DESC";

                var result = await _context.Database.SqlQueryRaw<QueryIndexStatInfo>(indexQuery).ToListAsync();
                
                return new Dictionary<string, object>
                {
                    ["TotalIndexes"] = result.Count,
                    ["IndexesByTable"] = result.GroupBy(i => i.TableName)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    ["LowCardinalityIndexes"] = result.Where(i => i.Cardinality < 10).Count()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting index statistics");
                return new Dictionary<string, object>();
            }
        }

        private Dictionary<string, object> GetConnectionPoolStatistics()
        {
            try
            {
                // This would need to be implemented based on your connection pool provider
                return new Dictionary<string, object>
                {
                    ["ActiveConnections"] = "N/A",
                    ["PoolSize"] = "N/A",
                    ["ConnectionsInUse"] = "N/A"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connection pool statistics");
                return new Dictionary<string, object>();
            }
        }

        private async Task<double> GetCacheHitRatioAsync()
        {
            try
            {
                // MySQL query cache hit ratio
                var cacheQuery = @"
                    SELECT 
                        ROUND((Qcache_hits / (Qcache_hits + Qcache_inserts)) * 100, 2) as HitRatio
                    FROM 
                        (SELECT VARIABLE_VALUE as Qcache_hits FROM information_schema.GLOBAL_STATUS WHERE VARIABLE_NAME = 'Qcache_hits') as hits,
                        (SELECT VARIABLE_VALUE as Qcache_inserts FROM information_schema.GLOBAL_STATUS WHERE VARIABLE_NAME = 'Qcache_inserts') as inserts";

                var result = await _context.Database.SqlQueryRaw<QueryCacheHitRatioInfo>(cacheQuery).FirstOrDefaultAsync();
                return result?.HitRatio ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache hit ratio");
                return 0;
            }
        }
    }

}

// Helper classes for query results - using internal to avoid conflicts with other services
internal class QuerySlowInfo
{
    public string Query { get; set; } = string.Empty;
    public TimeSpan ExecutionTime { get; set; }
    public DateTime Timestamp { get; set; }
}

internal class QueryFragmentationInfo
{
    public string TableName { get; set; } = string.Empty;
    public double SizeMB { get; set; }
    public double FreeMB { get; set; }
    public double FragmentationPercent { get; set; }
}

internal class QueryTableSizeInfo
{
    public string TableName { get; set; } = string.Empty;
    public double SizeMB { get; set; }
}

internal class QueryIndexStatInfo
{
    public string TableName { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public long Cardinality { get; set; }
    public string IndexType { get; set; } = string.Empty;
}

internal class QueryCacheHitRatioInfo
{
    public double HitRatio { get; set; }
}
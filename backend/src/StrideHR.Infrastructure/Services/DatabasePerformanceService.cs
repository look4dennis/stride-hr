using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StrideHR.Infrastructure.Data;
using System.Diagnostics;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// Service for monitoring and optimizing database performance
/// </summary>
public class DatabasePerformanceService
{
    private readonly StrideHRDbContext _context;
    private readonly ILogger<DatabasePerformanceService> _logger;

    public DatabasePerformanceService(StrideHRDbContext context, ILogger<DatabasePerformanceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Analyze slow queries and provide optimization recommendations
    /// </summary>
    public async Task<DatabasePerformanceReport> AnalyzePerformanceAsync()
    {
        var report = new DatabasePerformanceReport
        {
            AnalysisDate = DateTime.UtcNow
        };

        try
        {
            // Test common query patterns
            await AnalyzeEmployeeQueries(report);
            await AnalyzeAttendanceQueries(report);
            await AnalyzePayrollQueries(report);
            await AnalyzeProjectQueries(report);

            // Check database statistics
            await CheckDatabaseStatistics(report);

            _logger.LogInformation("Database performance analysis completed. Found {SlowQueryCount} slow queries", 
                report.SlowQueries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database performance analysis");
            report.Errors.Add($"Analysis error: {ex.Message}");
        }

        return report;
    }

    private async Task AnalyzeEmployeeQueries(DatabasePerformanceReport report)
    {
        // Test employee search by department
        var stopwatch = Stopwatch.StartNew();
        var employeeCount = await _context.Employees
            .Where(e => e.Department == "IT" && e.Status == Core.Enums.EmployeeStatus.Active)
            .CountAsync();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > 1000) // Slow if > 1 second
        {
            report.SlowQueries.Add(new SlowQueryInfo
            {
                QueryType = "Employee Search by Department",
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Recommendation = "Consider adding composite index on (Department, Status)"
            });
        }

        // Test employee hierarchy query
        stopwatch.Restart();
        var managersCount = await _context.Employees
            .Where(e => e.Subordinates.Any())
            .CountAsync();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > 1500)
        {
            report.SlowQueries.Add(new SlowQueryInfo
            {
                QueryType = "Employee Hierarchy Query",
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Recommendation = "Consider denormalizing manager-subordinate relationships or using CTE"
            });
        }
    }

    private async Task AnalyzeAttendanceQueries(DatabasePerformanceReport report)
    {
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        // Test attendance range query
        var stopwatch = Stopwatch.StartNew();
        var attendanceCount = await _context.AttendanceRecords
            .Where(a => a.Date >= startDate && a.Date <= endDate)
            .CountAsync();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > 2000)
        {
            report.SlowQueries.Add(new SlowQueryInfo
            {
                QueryType = "Attendance Date Range Query",
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Recommendation = "Ensure index on Date column and consider partitioning by date"
            });
        }

        // Test employee attendance summary
        stopwatch.Restart();
        var summaryCount = await _context.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a => a.Date >= startDate)
            .GroupBy(a => a.EmployeeId)
            .CountAsync();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > 3000)
        {
            report.SlowQueries.Add(new SlowQueryInfo
            {
                QueryType = "Attendance Summary with Employee Join",
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Recommendation = "Consider using projection instead of Include, or create materialized view"
            });
        }
    }

    private async Task AnalyzePayrollQueries(DatabasePerformanceReport report)
    {
        var currentYear = DateTime.UtcNow.Year;
        var currentMonth = DateTime.UtcNow.Month;

        // Test payroll period query
        var stopwatch = Stopwatch.StartNew();
        var payrollCount = await _context.PayrollRecords
            .Where(p => p.PayrollYear == currentYear && p.PayrollMonth == currentMonth)
            .CountAsync();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > 1000)
        {
            report.SlowQueries.Add(new SlowQueryInfo
            {
                QueryType = "Payroll Period Query",
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Recommendation = "Ensure composite index on (PayrollYear, PayrollMonth)"
            });
        }

        // Test payroll with employee details
        stopwatch.Restart();
        var payrollWithEmployees = await _context.PayrollRecords
            .Include(p => p.Employee)
            .Where(p => p.PayrollYear == currentYear)
            .Take(100)
            .ToListAsync();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > 2000)
        {
            report.SlowQueries.Add(new SlowQueryInfo
            {
                QueryType = "Payroll with Employee Details",
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Recommendation = "Use projection or split into separate queries to avoid N+1 problem"
            });
        }
    }

    private async Task AnalyzeProjectQueries(DatabasePerformanceReport report)
    {
        // Test active projects with tasks
        var stopwatch = Stopwatch.StartNew();
        var activeProjects = await _context.Projects
            .Include(p => p.Tasks)
            .Where(p => p.Status == Core.Enums.ProjectStatus.Active)
            .ToListAsync();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > 2500)
        {
            report.SlowQueries.Add(new SlowQueryInfo
            {
                QueryType = "Active Projects with Tasks",
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Recommendation = "Consider lazy loading or separate queries for tasks"
            });
        }

        // Test project assignments
        stopwatch.Restart();
        var assignmentCount = await _context.ProjectAssignments
            .Include(pa => pa.Employee)
            .Include(pa => pa.Project)
            .Where(pa => pa.UnassignedDate == null)
            .CountAsync();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > 1500)
        {
            report.SlowQueries.Add(new SlowQueryInfo
            {
                QueryType = "Project Assignments with Includes",
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Recommendation = "Use AsNoTracking() for read-only queries and consider projection"
            });
        }
    }

    private async Task CheckDatabaseStatistics(DatabasePerformanceReport report)
    {
        try
        {
            // Check table sizes (MySQL specific)
            var tableSizes = await _context.Database.SqlQueryRaw<TableSizeInfo>(@"
                SELECT 
                    table_name as TableName,
                    ROUND(((data_length + index_length) / 1024 / 1024), 2) as SizeMB,
                    table_rows as RowCount
                FROM information_schema.tables 
                WHERE table_schema = DATABASE()
                ORDER BY (data_length + index_length) DESC
                LIMIT 20").ToListAsync();

            report.TableSizes = tableSizes;

            // Identify large tables that might need optimization
            var largeTables = tableSizes.Where(t => t.SizeMB > 100).ToList();
            foreach (var table in largeTables)
            {
                report.Recommendations.Add($"Table {table.TableName} is {table.SizeMB}MB with {table.RowCount} rows. Consider archiving old data or partitioning.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve database statistics");
            report.Errors.Add($"Statistics error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get query execution plan for analysis
    /// </summary>
    public async Task<string> GetQueryExecutionPlanAsync(string query)
    {
        try
        {
            // MySQL specific - get execution plan
            var plan = await _context.Database.SqlQueryRaw<string>($"EXPLAIN FORMAT=JSON {query}").FirstOrDefaultAsync();
            return plan ?? "No execution plan available";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query execution plan");
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Optimize database by updating statistics and rebuilding indexes
    /// </summary>
    public async Task OptimizeDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Starting database optimization");

            // MySQL specific optimization commands
            await _context.Database.ExecuteSqlRawAsync("ANALYZE TABLE employees, attendance_records, payroll_records");
            
            _logger.LogInformation("Database optimization completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database optimization");
            throw;
        }
    }
}

/// <summary>
/// Database performance analysis report
/// </summary>
public class DatabasePerformanceReport
{
    public DateTime AnalysisDate { get; set; }
    public List<SlowQueryInfo> SlowQueries { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<TableSizeInfo> TableSizes { get; set; } = new();

    public string GenerateReport()
    {
        var report = $@"
=== Database Performance Report ===
Analysis Date: {AnalysisDate:yyyy-MM-dd HH:mm:ss}

Slow Queries Found: {SlowQueries.Count}
{string.Join("\n", SlowQueries.Select(q => $"- {q.QueryType}: {q.ExecutionTimeMs}ms - {q.Recommendation}"))}

Top 10 Largest Tables:
{string.Join("\n", TableSizes.Take(10).Select(t => $"- {t.TableName}: {t.SizeMB}MB ({t.RowCount:N0} rows)"))}

Recommendations:
{string.Join("\n", Recommendations.Select(r => $"- {r}"))}

{(Errors.Count > 0 ? $"Errors:\n{string.Join("\n", Errors.Select(e => $"- {e}"))}" : "")}
";
        return report;
    }
}

/// <summary>
/// Information about slow queries
/// </summary>
public class SlowQueryInfo
{
    public string QueryType { get; set; } = string.Empty;
    public long ExecutionTimeMs { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// Database table size information
/// </summary>
public class TableSizeInfo
{
    public string TableName { get; set; } = string.Empty;
    public decimal SizeMB { get; set; }
    public long RowCount { get; set; }
}
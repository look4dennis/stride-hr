using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models;
using StrideHR.Infrastructure.Data;
using System.Data;

namespace StrideHR.Infrastructure.Services;

public class ReportBuilderService : IReportBuilderService
{
    private readonly IReportRepository _reportRepository;
    private readonly IReportExecutionRepository _executionRepository;
    private readonly StrideHRDbContext _context;
    private readonly ILogger<ReportBuilderService> _logger;

    public ReportBuilderService(
        IReportRepository reportRepository,
        IReportExecutionRepository executionRepository,
        StrideHRDbContext context,
        ILogger<ReportBuilderService> logger)
    {
        _reportRepository = reportRepository;
        _executionRepository = executionRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<Report> CreateReportAsync(string name, string description, ReportType type, 
        ReportBuilderConfiguration configuration, int userId, int? branchId = null)
    {
        var report = new Report
        {
            Name = name,
            Description = description,
            Type = type,
            DataSource = configuration.DataSource,
            Configuration = JsonConvert.SerializeObject(configuration),
            Filters = JsonConvert.SerializeObject(configuration.Filters),
            Columns = JsonConvert.SerializeObject(configuration.Columns),
            ChartConfiguration = configuration.ChartConfiguration != null 
                ? JsonConvert.SerializeObject(configuration.ChartConfiguration) 
                : string.Empty,
            Status = ReportStatus.Active,
            CreatedBy = userId,
            BranchId = branchId,
            CreatedAt = DateTime.UtcNow
        };

        await _reportRepository.AddAsync(report);
        await _reportRepository.SaveChangesAsync();

        _logger.LogInformation("Report created: {ReportId} by user {UserId}", report.Id, userId);
        return report;
    }

    public async Task<Report> UpdateReportAsync(int reportId, string name, string description, 
        ReportBuilderConfiguration configuration, int userId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
            throw new ArgumentException("Report not found");

        if (report.CreatedBy != userId)
            throw new UnauthorizedAccessException("User does not have permission to update this report");

        report.Name = name;
        report.Description = description;
        report.DataSource = configuration.DataSource;
        report.Configuration = JsonConvert.SerializeObject(configuration);
        report.Filters = JsonConvert.SerializeObject(configuration.Filters);
        report.Columns = JsonConvert.SerializeObject(configuration.Columns);
        report.ChartConfiguration = configuration.ChartConfiguration != null 
            ? JsonConvert.SerializeObject(configuration.ChartConfiguration) 
            : string.Empty;
        report.UpdatedAt = DateTime.UtcNow;

        await _reportRepository.UpdateAsync(report);
        await _reportRepository.SaveChangesAsync();

        _logger.LogInformation("Report updated: {ReportId} by user {UserId}", reportId, userId);
        return report;
    }

    public async Task<bool> DeleteReportAsync(int reportId, int userId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
            return false;

        if (report.CreatedBy != userId)
            throw new UnauthorizedAccessException("User does not have permission to delete this report");

        await _reportRepository.DeleteAsync(report);
        await _reportRepository.SaveChangesAsync();

        _logger.LogInformation("Report deleted: {ReportId} by user {UserId}", reportId, userId);
        return true;
    }

    public async Task<Report?> GetReportAsync(int reportId, int userId)
    {
        var hasPermission = await _reportRepository.HasPermissionAsync(reportId, userId, ReportPermission.View);
        if (!hasPermission)
            return null;

        return await _reportRepository.GetReportWithExecutionsAsync(reportId);
    }

    public async Task<IEnumerable<Report>> GetUserReportsAsync(int userId, int? branchId = null)
    {
        return await _reportRepository.GetReportsByUserAsync(userId, branchId);
    }

    public async Task<IEnumerable<Report>> GetPublicReportsAsync(int? branchId = null)
    {
        return await _reportRepository.GetPublicReportsAsync(branchId);
    }

    public async Task<IEnumerable<Report>> GetSharedReportsAsync(int userId)
    {
        return await _reportRepository.GetSharedReportsAsync(userId);
    }

    public async Task<bool> ShareReportAsync(int reportId, int sharedWithUserId, ReportPermission permission, 
        int sharedByUserId, DateTime? expiresAt = null)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null || report.CreatedBy != sharedByUserId)
            return false;

        var existingShare = await _context.ReportShares
            .FirstOrDefaultAsync(rs => rs.ReportId == reportId && rs.SharedWith == sharedWithUserId);

        if (existingShare != null)
        {
            existingShare.Permission = permission;
            existingShare.ExpiresAt = expiresAt;
            existingShare.IsActive = true;
            existingShare.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var reportShare = new ReportShare
            {
                ReportId = reportId,
                SharedWith = sharedWithUserId,
                SharedBy = sharedByUserId,
                Permission = permission,
                SharedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.ReportShares.AddAsync(reportShare);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeReportShareAsync(int reportId, int userId, int revokedByUserId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null || report.CreatedBy != revokedByUserId)
            return false;

        var share = await _context.ReportShares
            .FirstOrDefaultAsync(rs => rs.ReportId == reportId && rs.SharedWith == userId);

        if (share != null)
        {
            share.IsActive = false;
            share.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<ReportExecutionResult> ExecuteReportAsync(int reportId, int userId, 
        Dictionary<string, object>? parameters = null)
    {
        var hasPermission = await _reportRepository.HasPermissionAsync(reportId, userId, ReportPermission.Execute);
        if (!hasPermission)
            throw new UnauthorizedAccessException("User does not have permission to execute this report");

        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
            throw new ArgumentException("Report not found");

        var execution = new ReportExecution
        {
            ReportId = reportId,
            ExecutedBy = userId,
            ExecutedAt = DateTime.UtcNow,
            Status = ReportExecutionStatus.Running,
            Parameters = parameters != null ? JsonConvert.SerializeObject(parameters) : null,
            CreatedAt = DateTime.UtcNow
        };

        await _executionRepository.AddAsync(execution);
        await _executionRepository.SaveChangesAsync();

        var startTime = DateTime.UtcNow;
        try
        {
            var configuration = JsonConvert.DeserializeObject<ReportBuilderConfiguration>(report.Configuration);
            var result = await ExecuteReportQueryAsync(configuration!, parameters);

            execution.Status = ReportExecutionStatus.Completed;
            execution.ResultData = JsonConvert.SerializeObject(result.Data);
            execution.RecordCount = result.TotalRecords;
            execution.ExecutionTime = DateTime.UtcNow - startTime;

            await _executionRepository.UpdateAsync(execution);
            await _executionRepository.SaveChangesAsync();

            _logger.LogInformation("Report executed successfully: {ReportId} by user {UserId}", reportId, userId);
            return result;
        }
        catch (Exception ex)
        {
            execution.Status = ReportExecutionStatus.Failed;
            execution.ErrorMessage = ex.Message;
            execution.ExecutionTime = DateTime.UtcNow - startTime;

            await _executionRepository.UpdateAsync(execution);
            await _executionRepository.SaveChangesAsync();

            _logger.LogError(ex, "Report execution failed: {ReportId} by user {UserId}", reportId, userId);
            throw;
        }
    }

    public async Task<ReportExecutionResult> PreviewReportAsync(ReportBuilderConfiguration configuration, 
        int userId, int limit = 100)
    {
        try
        {
            var originalPagination = configuration.Pagination;
            configuration.Pagination = new ReportPagination { PageSize = limit, EnablePaging = true };

            var result = await ExecuteReportQueryAsync(configuration);
            
            configuration.Pagination = originalPagination;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report preview failed for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<ReportDataSource>> GetAvailableDataSourcesAsync(int userId)
    {
        // This would typically be configured based on user permissions and available data sources
        var dataSources = new List<ReportDataSource>
        {
            new ReportDataSource
            {
                Name = "employees",
                DisplayName = "Employees",
                Description = "Employee information and details",
                Query = "SELECT * FROM Employees",
                Columns = new List<ReportDataSourceColumn>
                {
                    new() { Name = "Id", DisplayName = "Employee ID", DataType = "int" },
                    new() { Name = "FirstName", DisplayName = "First Name", DataType = "string" },
                    new() { Name = "LastName", DisplayName = "Last Name", DataType = "string" },
                    new() { Name = "Email", DisplayName = "Email", DataType = "string" },
                    new() { Name = "Department", DisplayName = "Department", DataType = "string" },
                    new() { Name = "JoiningDate", DisplayName = "Joining Date", DataType = "datetime" }
                }
            },
            new ReportDataSource
            {
                Name = "attendance",
                DisplayName = "Attendance Records",
                Description = "Employee attendance and time tracking",
                Query = "SELECT * FROM AttendanceRecords",
                Columns = new List<ReportDataSourceColumn>
                {
                    new() { Name = "Id", DisplayName = "Record ID", DataType = "int" },
                    new() { Name = "EmployeeId", DisplayName = "Employee ID", DataType = "int" },
                    new() { Name = "Date", DisplayName = "Date", DataType = "date" },
                    new() { Name = "CheckInTime", DisplayName = "Check In", DataType = "datetime" },
                    new() { Name = "CheckOutTime", DisplayName = "Check Out", DataType = "datetime" },
                    new() { Name = "TotalWorkingHours", DisplayName = "Working Hours", DataType = "timespan" }
                }
            },
            new ReportDataSource
            {
                Name = "payroll",
                DisplayName = "Payroll Records",
                Description = "Employee payroll and salary information",
                Query = "SELECT * FROM PayrollRecords",
                Columns = new List<ReportDataSourceColumn>
                {
                    new() { Name = "Id", DisplayName = "Payroll ID", DataType = "int" },
                    new() { Name = "EmployeeId", DisplayName = "Employee ID", DataType = "int" },
                    new() { Name = "PayPeriodStart", DisplayName = "Pay Period Start", DataType = "date" },
                    new() { Name = "PayPeriodEnd", DisplayName = "Pay Period End", DataType = "date" },
                    new() { Name = "GrossSalary", DisplayName = "Gross Salary", DataType = "decimal" },
                    new() { Name = "NetSalary", DisplayName = "Net Salary", DataType = "decimal" }
                }
            }
        };

        return await Task.FromResult(dataSources);
    }

    public async Task<ReportDataSource> GetDataSourceSchemaAsync(string dataSourceName, int userId)
    {
        var dataSources = await GetAvailableDataSourcesAsync(userId);
        var dataSource = dataSources.FirstOrDefault(ds => ds.Name == dataSourceName);
        
        if (dataSource == null)
            throw new ArgumentException($"Data source '{dataSourceName}' not found");

        return dataSource;
    }

    private async Task<ReportExecutionResult> ExecuteReportQueryAsync(ReportBuilderConfiguration configuration, 
        Dictionary<string, object>? parameters = null)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            // This is a simplified implementation. In a real scenario, you would:
            // 1. Build dynamic SQL queries based on configuration
            // 2. Apply filters, sorting, grouping
            // 3. Execute against the actual database
            // 4. Handle different data sources
            
            var query = BuildDynamicQuery(configuration, parameters);
            var data = await ExecuteDynamicQueryAsync(query);
            
            var result = new ReportExecutionResult
            {
                Success = true,
                Data = data,
                TotalRecords = data.Count,
                ExecutionTime = DateTime.UtcNow - startTime,
                Metadata = new Dictionary<string, object>
                {
                    ["DataSource"] = configuration.DataSource,
                    ["FilterCount"] = configuration.Filters.Count,
                    ["ColumnCount"] = configuration.Columns.Count
                }
            };

            return result;
        }
        catch (Exception ex)
        {
            return new ReportExecutionResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionTime = DateTime.UtcNow - startTime
            };
        }
    }

    private string BuildDynamicQuery(ReportBuilderConfiguration configuration, Dictionary<string, object>? parameters)
    {
        // Simplified query building - in production, use a proper query builder
        var selectColumns = string.Join(", ", configuration.Columns.Where(c => c.IsVisible).Select(c => c.Name));
        var query = $"SELECT {selectColumns} FROM {configuration.DataSource}";
        
        if (configuration.Filters.Any())
        {
            var whereClause = string.Join(" AND ", configuration.Filters.Select(f => $"{f.Column} {f.Operator} '{f.Value}'"));
            query += $" WHERE {whereClause}";
        }
        
        if (configuration.Sortings.Any())
        {
            var orderBy = string.Join(", ", configuration.Sortings.Select(s => $"{s.Column} {s.Direction}"));
            query += $" ORDER BY {orderBy}";
        }

        return query;
    }

    private async Task<List<Dictionary<string, object>>> ExecuteDynamicQueryAsync(string query)
    {
        // This is a placeholder implementation
        // In production, you would execute the actual SQL query against the database
        var sampleData = new List<Dictionary<string, object>>();
        
        // Generate some sample data for demonstration
        for (int i = 1; i <= 10; i++)
        {
            sampleData.Add(new Dictionary<string, object>
            {
                ["Id"] = i,
                ["Name"] = $"Sample Record {i}",
                ["Value"] = i * 100,
                ["Date"] = DateTime.Now.AddDays(-i)
            });
        }

        return await Task.FromResult(sampleData);
    }
}
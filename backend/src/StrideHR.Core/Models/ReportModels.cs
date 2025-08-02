using StrideHR.Core.Enums;

namespace StrideHR.Core.Models;

public class ReportBuilderConfiguration
{
    public string DataSource { get; set; } = string.Empty;
    public List<ReportColumn> Columns { get; set; } = new();
    public List<ReportFilter> Filters { get; set; } = new();
    public List<ReportGrouping> Groupings { get; set; } = new();
    public List<ReportSorting> Sortings { get; set; } = new();
    public ReportChartConfiguration? ChartConfiguration { get; set; }
    public ReportPagination? Pagination { get; set; }
}

public class ReportColumn
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
    public int Order { get; set; }
    public string? Format { get; set; }
    public string? AggregateFunction { get; set; }
    public int? Width { get; set; }
    public string? Alignment { get; set; }
}

public class ReportFilter
{
    public string Column { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public object? Value { get; set; }
    public string? LogicalOperator { get; set; } = "AND";
    public int Order { get; set; }
}

public class ReportGrouping
{
    public string Column { get; set; } = string.Empty;
    public string? AggregateFunction { get; set; }
    public int Order { get; set; }
}

public class ReportSorting
{
    public string Column { get; set; } = string.Empty;
    public string Direction { get; set; } = "ASC";
    public int Order { get; set; }
}

public class ReportChartConfiguration
{
    public ChartType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string XAxisColumn { get; set; } = string.Empty;
    public string YAxisColumn { get; set; } = string.Empty;
    public string? SeriesColumn { get; set; }
    public Dictionary<string, object> Options { get; set; } = new();
    public List<string> Colors { get; set; } = new();
}

public class ReportPagination
{
    public int PageSize { get; set; } = 50;
    public bool EnablePaging { get; set; } = true;
}

public class ReportExecutionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<Dictionary<string, object>> Data { get; set; } = new();
    public int TotalRecords { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ReportExportRequest
{
    public int ReportId { get; set; }
    public ReportExportFormat Format { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public string? FileName { get; set; }
    public bool IncludeCharts { get; set; } = true;
}

public class ReportScheduleRequest
{
    public int ReportId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();
    public ReportExportFormat ExportFormat { get; set; }
    public string? EmailSubject { get; set; }
    public string? EmailBody { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

public class ReportDataSource
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public List<ReportDataSourceColumn> Columns { get; set; } = new();
}

public class ReportDataSourceColumn
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsFilterable { get; set; } = true;
    public bool IsSortable { get; set; } = true;
    public bool IsGroupable { get; set; } = true;
    public List<string>? PossibleValues { get; set; }
}
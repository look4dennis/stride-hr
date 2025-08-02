using Microsoft.Extensions.Logging;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models;
using System.Text;
using System.Text.Json;

namespace StrideHR.Infrastructure.Services;

public class ReportDataVisualizationService : IReportDataVisualizationService
{
    private readonly ILogger<ReportDataVisualizationService> _logger;

    public ReportDataVisualizationService(ILogger<ReportDataVisualizationService> logger)
    {
        _logger = logger;
    }

    public async Task<Dictionary<string, object>> GenerateChartDataAsync(ReportExecutionResult data, 
        ReportChartConfiguration chartConfig)
    {
        var chartData = new Dictionary<string, object>();

        try
        {
            switch (chartConfig.Type)
            {
                case ChartType.Bar:
                case ChartType.Line:
                    chartData = await GenerateXYChartDataAsync(data, chartConfig);
                    break;
                case ChartType.Pie:
                case ChartType.Doughnut:
                    chartData = await GeneratePieChartDataAsync(data, chartConfig);
                    break;
                case ChartType.Area:
                    chartData = await GenerateAreaChartDataAsync(data, chartConfig);
                    break;
                case ChartType.Scatter:
                    chartData = await GenerateScatterChartDataAsync(data, chartConfig);
                    break;
                default:
                    chartData = await GenerateXYChartDataAsync(data, chartConfig);
                    break;
            }

            chartData["type"] = chartConfig.Type.ToString().ToLower();
            chartData["title"] = chartConfig.Title;
            chartData["options"] = chartConfig.Options;

            return chartData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate chart data for type: {ChartType}", chartConfig.Type);
            throw;
        }
    }

    public async Task<byte[]> GenerateChartImageAsync(Dictionary<string, object> chartData, 
        ReportChartConfiguration chartConfig, int width = 800, int height = 600)
    {
        // This would typically use a charting library like Chart.js with a headless browser
        // or a server-side charting library. For now, we'll return a placeholder.
        
        var placeholder = $"Chart Image Placeholder - {chartConfig.Type} - {width}x{height}";
        return await Task.FromResult(Encoding.UTF8.GetBytes(placeholder));
    }

    public async Task<string> GenerateChartHtmlAsync(Dictionary<string, object> chartData, 
        ReportChartConfiguration chartConfig)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("  <title>Chart</title>");
        html.AppendLine("  <script src=\"https://cdn.jsdelivr.net/npm/chart.js\"></script>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("  <div style=\"width: 800px; height: 600px; margin: 0 auto;\">");
        html.AppendLine("    <canvas id=\"chart\"></canvas>");
        html.AppendLine("  </div>");
        html.AppendLine("  <script>");
        html.AppendLine("    const ctx = document.getElementById('chart').getContext('2d');");
        html.AppendLine("    const chart = new Chart(ctx, {");
        html.AppendLine($"      type: '{chartConfig.Type.ToString().ToLower()}',");
        html.AppendLine($"      data: {JsonSerializer.Serialize(chartData)},");
        html.AppendLine("      options: {");
        html.AppendLine("        responsive: true,");
        html.AppendLine("        maintainAspectRatio: false,");
        html.AppendLine($"        plugins: {{ title: {{ display: true, text: '{chartConfig.Title}' }} }}");
        html.AppendLine("      }");
        html.AppendLine("    });");
        html.AppendLine("  </script>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return await Task.FromResult(html.ToString());
    }

    public async Task<List<ChartType>> GetSupportedChartTypesAsync()
    {
        return await Task.FromResult(new List<ChartType>
        {
            ChartType.Bar,
            ChartType.Line,
            ChartType.Pie,
            ChartType.Doughnut,
            ChartType.Area,
            ChartType.Scatter,
            ChartType.Bubble,
            ChartType.Radar,
            ChartType.PolarArea
        });
    }

    public async Task<Dictionary<string, object>> GetChartOptionsAsync(ChartType chartType)
    {
        var options = new Dictionary<string, object>();

        switch (chartType)
        {
            case ChartType.Bar:
                options = new Dictionary<string, object>
                {
                    ["indexAxis"] = "x",
                    ["responsive"] = true,
                    ["plugins"] = new Dictionary<string, object>
                    {
                        ["legend"] = new { position = "top" },
                        ["title"] = new { display = true, text = "Bar Chart" }
                    }
                };
                break;
            case ChartType.Line:
                options = new Dictionary<string, object>
                {
                    ["responsive"] = true,
                    ["interaction"] = new { mode = "index", intersect = false },
                    ["plugins"] = new Dictionary<string, object>
                    {
                        ["legend"] = new { position = "top" },
                        ["title"] = new { display = true, text = "Line Chart" }
                    }
                };
                break;
            case ChartType.Pie:
            case ChartType.Doughnut:
                options = new Dictionary<string, object>
                {
                    ["responsive"] = true,
                    ["plugins"] = new Dictionary<string, object>
                    {
                        ["legend"] = new { position = "right" },
                        ["title"] = new { display = true, text = $"{chartType} Chart" }
                    }
                };
                break;
        }

        return await Task.FromResult(options);
    }

    public async Task<bool> ValidateChartConfigurationAsync(ReportChartConfiguration chartConfig, 
        List<ReportColumn> columns)
    {
        try
        {
            // Check if required columns exist
            if (string.IsNullOrEmpty(chartConfig.XAxisColumn) || 
                !columns.Any(c => c.Name == chartConfig.XAxisColumn))
                return false;

            if (string.IsNullOrEmpty(chartConfig.YAxisColumn) || 
                !columns.Any(c => c.Name == chartConfig.YAxisColumn))
                return false;

            // Check if series column exists (if specified)
            if (!string.IsNullOrEmpty(chartConfig.SeriesColumn) && 
                !columns.Any(c => c.Name == chartConfig.SeriesColumn))
                return false;

            // Validate chart type specific requirements
            switch (chartConfig.Type)
            {
                case ChartType.Scatter:
                case ChartType.Bubble:
                    // Both X and Y should be numeric for scatter/bubble charts
                    var xColumn = columns.First(c => c.Name == chartConfig.XAxisColumn);
                    var yColumn = columns.First(c => c.Name == chartConfig.YAxisColumn);
                    if (!IsNumericDataType(xColumn.DataType) || !IsNumericDataType(yColumn.DataType))
                        return false;
                    break;
            }

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate chart configuration");
            return await Task.FromResult(false);
        }
    }

    public async Task<ReportChartConfiguration> SuggestChartConfigurationAsync(List<ReportColumn> columns, 
        List<Dictionary<string, object>> sampleData)
    {
        var suggestion = new ReportChartConfiguration();

        try
        {
            var numericColumns = columns.Where(c => IsNumericDataType(c.DataType)).ToList();
            var categoricalColumns = columns.Where(c => !IsNumericDataType(c.DataType) && c.DataType != "datetime").ToList();
            var dateColumns = columns.Where(c => c.DataType == "datetime" || c.DataType == "date").ToList();

            // Default suggestions based on available column types
            if (dateColumns.Any() && numericColumns.Any())
            {
                // Time series data - suggest line chart
                suggestion.Type = ChartType.Line;
                suggestion.XAxisColumn = dateColumns.First().Name;
                suggestion.YAxisColumn = numericColumns.First().Name;
                suggestion.Title = $"{numericColumns.First().DisplayName} over Time";
            }
            else if (categoricalColumns.Any() && numericColumns.Any())
            {
                // Categorical data - suggest bar chart
                suggestion.Type = ChartType.Bar;
                suggestion.XAxisColumn = categoricalColumns.First().Name;
                suggestion.YAxisColumn = numericColumns.First().Name;
                suggestion.Title = $"{numericColumns.First().DisplayName} by {categoricalColumns.First().DisplayName}";
            }
            else if (numericColumns.Count >= 2)
            {
                // Multiple numeric columns - suggest scatter plot
                suggestion.Type = ChartType.Scatter;
                suggestion.XAxisColumn = numericColumns[0].Name;
                suggestion.YAxisColumn = numericColumns[1].Name;
                suggestion.Title = $"{numericColumns[1].DisplayName} vs {numericColumns[0].DisplayName}";
            }
            else
            {
                // Default to bar chart
                suggestion.Type = ChartType.Bar;
                suggestion.XAxisColumn = columns.First().Name;
                suggestion.YAxisColumn = columns.Skip(1).FirstOrDefault()?.Name ?? columns.First().Name;
                suggestion.Title = "Data Visualization";
            }

            suggestion.Options = await GetChartOptionsAsync(suggestion.Type);
            suggestion.Colors = new List<string> 
            { 
                "#3b82f6", "#ef4444", "#10b981", "#f59e0b", "#8b5cf6", "#06b6d4", "#84cc16", "#f97316" 
            };

            return suggestion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to suggest chart configuration");
            
            // Return a basic configuration as fallback
            return new ReportChartConfiguration
            {
                Type = ChartType.Bar,
                XAxisColumn = columns.FirstOrDefault()?.Name ?? "x",
                YAxisColumn = columns.Skip(1).FirstOrDefault()?.Name ?? "y",
                Title = "Chart",
                Options = await GetChartOptionsAsync(ChartType.Bar)
            };
        }
    }

    private async Task<Dictionary<string, object>> GenerateXYChartDataAsync(ReportExecutionResult data, 
        ReportChartConfiguration chartConfig)
    {
        var labels = new List<string>();
        var datasets = new List<Dictionary<string, object>>();

        if (!data.Data.Any())
            return new Dictionary<string, object> { ["labels"] = labels, ["datasets"] = datasets };

        // Extract labels (X-axis values)
        labels = data.Data.Select(row => 
            row.ContainsKey(chartConfig.XAxisColumn) ? row[chartConfig.XAxisColumn]?.ToString() ?? "" : "")
            .Distinct()
            .ToList();

        // Group data by series if specified
        if (!string.IsNullOrEmpty(chartConfig.SeriesColumn))
        {
            var seriesGroups = data.Data.GroupBy(row => 
                row.ContainsKey(chartConfig.SeriesColumn) ? row[chartConfig.SeriesColumn]?.ToString() ?? "Unknown" : "Unknown");

            var colorIndex = 0;
            foreach (var group in seriesGroups)
            {
                var seriesData = labels.Select(label =>
                {
                    var matchingRow = group.FirstOrDefault(row => 
                        row.ContainsKey(chartConfig.XAxisColumn) && 
                        row[chartConfig.XAxisColumn]?.ToString() == label);
                    
                    return matchingRow?.ContainsKey(chartConfig.YAxisColumn) == true 
                        ? Convert.ToDouble(matchingRow[chartConfig.YAxisColumn] ?? 0) 
                        : 0.0;
                }).ToList();

                datasets.Add(new Dictionary<string, object>
                {
                    ["label"] = group.Key,
                    ["data"] = seriesData,
                    ["backgroundColor"] = chartConfig.Colors.ElementAtOrDefault(colorIndex % chartConfig.Colors.Count) ?? "#3b82f6",
                    ["borderColor"] = chartConfig.Colors.ElementAtOrDefault(colorIndex % chartConfig.Colors.Count) ?? "#3b82f6"
                });
                colorIndex++;
            }
        }
        else
        {
            // Single series
            var seriesData = data.Data.Select(row =>
                row.ContainsKey(chartConfig.YAxisColumn) ? Convert.ToDouble(row[chartConfig.YAxisColumn] ?? 0) : 0.0)
                .ToList();

            datasets.Add(new Dictionary<string, object>
            {
                ["label"] = chartConfig.YAxisColumn,
                ["data"] = seriesData,
                ["backgroundColor"] = chartConfig.Colors.FirstOrDefault() ?? "#3b82f6",
                ["borderColor"] = chartConfig.Colors.FirstOrDefault() ?? "#3b82f6"
            });
        }

        return await Task.FromResult(new Dictionary<string, object>
        {
            ["labels"] = labels,
            ["datasets"] = datasets
        });
    }

    private async Task<Dictionary<string, object>> GeneratePieChartDataAsync(ReportExecutionResult data, 
        ReportChartConfiguration chartConfig)
    {
        var labels = new List<string>();
        var values = new List<double>();
        var colors = new List<string>();

        if (data.Data.Any())
        {
            var groupedData = data.Data.GroupBy(row => 
                row.ContainsKey(chartConfig.XAxisColumn) ? row[chartConfig.XAxisColumn]?.ToString() ?? "Unknown" : "Unknown");

            var colorIndex = 0;
            foreach (var group in groupedData)
            {
                labels.Add(group.Key);
                
                var sum = group.Sum(row => 
                    row.ContainsKey(chartConfig.YAxisColumn) ? Convert.ToDouble(row[chartConfig.YAxisColumn] ?? 0) : 0.0);
                values.Add(sum);
                
                colors.Add(chartConfig.Colors.ElementAtOrDefault(colorIndex % chartConfig.Colors.Count) ?? $"#{'0' + (colorIndex * 40).ToString("X")}{'0' + (colorIndex * 60).ToString("X")}{'0' + (colorIndex * 80).ToString("X")}");
                colorIndex++;
            }
        }

        var datasets = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                ["data"] = values,
                ["backgroundColor"] = colors,
                ["borderColor"] = colors.Select(c => c).ToList(),
                ["borderWidth"] = 1
            }
        };

        return await Task.FromResult(new Dictionary<string, object>
        {
            ["labels"] = labels,
            ["datasets"] = datasets
        });
    }

    private async Task<Dictionary<string, object>> GenerateAreaChartDataAsync(ReportExecutionResult data, 
        ReportChartConfiguration chartConfig)
    {
        var chartData = await GenerateXYChartDataAsync(data, chartConfig);
        
        // Modify datasets for area chart
        if (chartData.ContainsKey("datasets") && chartData["datasets"] is List<Dictionary<string, object>> datasets)
        {
            foreach (var dataset in datasets)
            {
                dataset["fill"] = true;
                if (dataset.ContainsKey("backgroundColor"))
                {
                    var color = dataset["backgroundColor"]?.ToString() ?? "#3b82f6";
                    dataset["backgroundColor"] = color + "33"; // Add transparency
                }
            }
        }

        return chartData;
    }

    private async Task<Dictionary<string, object>> GenerateScatterChartDataAsync(ReportExecutionResult data, 
        ReportChartConfiguration chartConfig)
    {
        var datasets = new List<Dictionary<string, object>>();

        if (data.Data.Any())
        {
            var scatterData = data.Data.Select(row => new Dictionary<string, object>
            {
                ["x"] = row.ContainsKey(chartConfig.XAxisColumn) ? Convert.ToDouble(row[chartConfig.XAxisColumn] ?? 0) : 0.0,
                ["y"] = row.ContainsKey(chartConfig.YAxisColumn) ? Convert.ToDouble(row[chartConfig.YAxisColumn] ?? 0) : 0.0
            }).ToList();

            datasets.Add(new Dictionary<string, object>
            {
                ["label"] = $"{chartConfig.YAxisColumn} vs {chartConfig.XAxisColumn}",
                ["data"] = scatterData,
                ["backgroundColor"] = chartConfig.Colors.FirstOrDefault() ?? "#3b82f6",
                ["borderColor"] = chartConfig.Colors.FirstOrDefault() ?? "#3b82f6"
            });
        }

        return await Task.FromResult(new Dictionary<string, object>
        {
            ["datasets"] = datasets
        });
    }

    private bool IsNumericDataType(string dataType)
    {
        return dataType.ToLower() switch
        {
            "int" or "integer" or "long" or "decimal" or "double" or "float" or "number" => true,
            _ => false
        };
    }
}
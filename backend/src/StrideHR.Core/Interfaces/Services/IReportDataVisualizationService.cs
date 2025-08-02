using StrideHR.Core.Enums;
using StrideHR.Core.Models;

namespace StrideHR.Core.Interfaces.Services;

public interface IReportDataVisualizationService
{
    Task<Dictionary<string, object>> GenerateChartDataAsync(ReportExecutionResult data, ReportChartConfiguration chartConfig);
    Task<byte[]> GenerateChartImageAsync(Dictionary<string, object> chartData, ReportChartConfiguration chartConfig, int width = 800, int height = 600);
    Task<string> GenerateChartHtmlAsync(Dictionary<string, object> chartData, ReportChartConfiguration chartConfig);
    Task<List<ChartType>> GetSupportedChartTypesAsync();
    Task<Dictionary<string, object>> GetChartOptionsAsync(ChartType chartType);
    Task<bool> ValidateChartConfigurationAsync(ReportChartConfiguration chartConfig, List<ReportColumn> columns);
    Task<ReportChartConfiguration> SuggestChartConfigurationAsync(List<ReportColumn> columns, List<Dictionary<string, object>> sampleData);
}
using StrideHR.Core.Enums;
using StrideHR.Core.Models;

namespace StrideHR.Core.Interfaces.Services;

public interface IReportExportService
{
    Task<byte[]> ExportReportAsync(int reportId, ReportExportFormat format, int userId, Dictionary<string, object>? parameters = null);
    Task<byte[]> ExportReportDataAsync(ReportExecutionResult data, ReportExportFormat format, string reportName, ReportBuilderConfiguration? configuration = null);
    Task<string> GetExportMimeTypeAsync(ReportExportFormat format);
    Task<string> GetExportFileExtensionAsync(ReportExportFormat format);
    Task<bool> SaveExportAsync(byte[] data, string filePath, ReportExportFormat format);
}
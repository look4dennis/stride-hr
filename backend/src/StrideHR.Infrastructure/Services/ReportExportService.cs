using Microsoft.Extensions.Logging;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models;
using System.Text;
using System.Text.Json;
using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace StrideHR.Infrastructure.Services;

public class ReportExportService : IReportExportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IReportBuilderService _reportBuilderService;
    private readonly ILogger<ReportExportService> _logger;

    public ReportExportService(
        IReportRepository reportRepository,
        IReportBuilderService reportBuilderService,
        ILogger<ReportExportService> logger)
    {
        _reportRepository = reportRepository;
        _reportBuilderService = reportBuilderService;
        _logger = logger;
    }

    public async Task<byte[]> ExportReportAsync(int reportId, ReportExportFormat format, int userId, 
        Dictionary<string, object>? parameters = null)
    {
        var hasPermission = await _reportRepository.HasPermissionAsync(reportId, userId, ReportPermission.Execute);
        if (!hasPermission)
            throw new UnauthorizedAccessException("User does not have permission to export this report");

        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
            throw new ArgumentException("Report not found");

        var executionResult = await _reportBuilderService.ExecuteReportAsync(reportId, userId, parameters);
        var configuration = JsonSerializer.Deserialize<ReportBuilderConfiguration>(report.Configuration);

        return await ExportReportDataAsync(executionResult, format, report.Name, configuration);
    }

    public async Task<byte[]> ExportReportDataAsync(ReportExecutionResult data, ReportExportFormat format, 
        string reportName, ReportBuilderConfiguration? configuration = null)
    {
        return format switch
        {
            ReportExportFormat.PDF => await ExportToPdfAsync(data, reportName, configuration),
            ReportExportFormat.Excel => await ExportToExcelAsync(data, reportName, configuration),
            ReportExportFormat.CSV => await ExportToCsvAsync(data, reportName, configuration),
            ReportExportFormat.JSON => await ExportToJsonAsync(data, reportName),
            ReportExportFormat.XML => await ExportToXmlAsync(data, reportName),
            ReportExportFormat.HTML => await ExportToHtmlAsync(data, reportName, configuration),
            _ => throw new ArgumentException($"Unsupported export format: {format}")
        };
    }

    public async Task<string> GetExportMimeTypeAsync(ReportExportFormat format)
    {
        return await Task.FromResult(format switch
        {
            ReportExportFormat.PDF => "application/pdf",
            ReportExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ReportExportFormat.CSV => "text/csv",
            ReportExportFormat.JSON => "application/json",
            ReportExportFormat.XML => "application/xml",
            ReportExportFormat.HTML => "text/html",
            ReportExportFormat.Word => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        });
    }

    public async Task<string> GetExportFileExtensionAsync(ReportExportFormat format)
    {
        return await Task.FromResult(format switch
        {
            ReportExportFormat.PDF => ".pdf",
            ReportExportFormat.Excel => ".xlsx",
            ReportExportFormat.CSV => ".csv",
            ReportExportFormat.JSON => ".json",
            ReportExportFormat.XML => ".xml",
            ReportExportFormat.HTML => ".html",
            ReportExportFormat.Word => ".docx",
            _ => ".bin"
        });
    }

    public async Task<bool> SaveExportAsync(byte[] data, string filePath, ReportExportFormat format)
    {
        try
        {
            await File.WriteAllBytesAsync(filePath, data);
            _logger.LogInformation("Report exported to file: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save export to file: {FilePath}", filePath);
            return false;
        }
    }

    private async Task<byte[]> ExportToPdfAsync(ReportExecutionResult data, string reportName, 
        ReportBuilderConfiguration? configuration)
    {
        using var memoryStream = new MemoryStream();
        var document = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30);
        var writer = PdfWriter.GetInstance(document, memoryStream);

        document.Open();

        // Add title
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
        var title = new Paragraph(reportName, titleFont)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 20
        };
        document.Add(title);

        // Add generation info
        var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
        var info = new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", infoFont)
        {
            Alignment = Element.ALIGN_RIGHT,
            SpacingAfter = 20
        };
        document.Add(info);

        if (data.Data.Any())
        {
            // Create table
            var columns = data.Data.First().Keys.ToList();
            var table = new PdfPTable(columns.Count)
            {
                WidthPercentage = 100
            };

            // Add headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            foreach (var column in columns)
            {
                var cell = new PdfPCell(new Phrase(column, headerFont))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5
                };
                table.AddCell(cell);
            }

            // Add data rows
            var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            foreach (var row in data.Data)
            {
                foreach (var column in columns)
                {
                    var value = row.ContainsKey(column) ? row[column]?.ToString() ?? "" : "";
                    var cell = new PdfPCell(new Phrase(value, dataFont))
                    {
                        Padding = 3
                    };
                    table.AddCell(cell);
                }
            }

            document.Add(table);
        }

        document.Close();
        return await Task.FromResult(memoryStream.ToArray());
    }

    private async Task<byte[]> ExportToExcelAsync(ReportExecutionResult data, string reportName, 
        ReportBuilderConfiguration? configuration)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add(reportName);

        if (data.Data.Any())
        {
            var columns = data.Data.First().Keys.ToList();

            // Add headers
            for (int i = 0; i < columns.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = columns[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Add data
            for (int row = 0; row < data.Data.Count; row++)
            {
                var dataRow = data.Data[row];
                for (int col = 0; col < columns.Count; col++)
                {
                    var value = dataRow.ContainsKey(columns[col]) ? dataRow[columns[col]] : null;
                    worksheet.Cells[row + 2, col + 1].Value = value;
                }
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();
        }

        return await Task.FromResult(package.GetAsByteArray());
    }

    private async Task<byte[]> ExportToCsvAsync(ReportExecutionResult data, string reportName, 
        ReportBuilderConfiguration? configuration)
    {
        var csv = new StringBuilder();

        if (data.Data.Any())
        {
            var columns = data.Data.First().Keys.ToList();

            // Add headers
            csv.AppendLine(string.Join(",", columns.Select(c => $"\"{c}\"")));

            // Add data
            foreach (var row in data.Data)
            {
                var values = columns.Select(col => 
                {
                    var value = row.ContainsKey(col) ? row[col]?.ToString() ?? "" : "";
                    return $"\"{value.Replace("\"", "\"\"")}\""; // Escape quotes
                });
                csv.AppendLine(string.Join(",", values));
            }
        }
        else
        {
            // Even with no data, provide a basic CSV structure with report info
            csv.AppendLine($"\"Report Name\",\"{reportName}\"");
            csv.AppendLine($"\"Generated At\",\"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\"");
            csv.AppendLine($"\"Total Records\",\"{data.TotalRecords}\"");
            csv.AppendLine("\"Data\",\"No data available\"");
        }

        return await Task.FromResult(Encoding.UTF8.GetBytes(csv.ToString()));
    }

    private async Task<byte[]> ExportToJsonAsync(ReportExecutionResult data, string reportName)
    {
        var exportData = new
        {
            ReportName = reportName,
            GeneratedAt = DateTime.UtcNow,
            TotalRecords = data.TotalRecords,
            ExecutionTime = data.ExecutionTime.TotalMilliseconds,
            Data = data.Data
        };

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        return await Task.FromResult(Encoding.UTF8.GetBytes(json));
    }

    private async Task<byte[]> ExportToXmlAsync(ReportExecutionResult data, string reportName)
    {
        var xml = new StringBuilder();
        xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        xml.AppendLine($"<Report name=\"{reportName}\" generatedAt=\"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}\">");
        xml.AppendLine($"  <Summary totalRecords=\"{data.TotalRecords}\" executionTime=\"{data.ExecutionTime.TotalMilliseconds}ms\" />");
        xml.AppendLine("  <Data>");

        foreach (var row in data.Data)
        {
            xml.AppendLine("    <Record>");
            foreach (var kvp in row)
            {
                var value = kvp.Value?.ToString()?.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") ?? "";
                xml.AppendLine($"      <{kvp.Key}>{value}</{kvp.Key}>");
            }
            xml.AppendLine("    </Record>");
        }

        xml.AppendLine("  </Data>");
        xml.AppendLine("</Report>");

        return await Task.FromResult(Encoding.UTF8.GetBytes(xml.ToString()));
    }

    private async Task<byte[]> ExportToHtmlAsync(ReportExecutionResult data, string reportName, 
        ReportBuilderConfiguration? configuration)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine($"  <title>{reportName}</title>");
        html.AppendLine("  <style>");
        html.AppendLine("    body { font-family: Arial, sans-serif; margin: 20px; }");
        html.AppendLine("    table { border-collapse: collapse; width: 100%; }");
        html.AppendLine("    th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        html.AppendLine("    th { background-color: #f2f2f2; font-weight: bold; }");
        html.AppendLine("    .header { text-align: center; margin-bottom: 20px; }");
        html.AppendLine("    .info { text-align: right; margin-bottom: 20px; font-size: 12px; color: #666; }");
        html.AppendLine("  </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine($"  <div class=\"header\"><h1>{reportName}</h1></div>");
        html.AppendLine($"  <div class=\"info\">Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</div>");

        if (data.Data.Any())
        {
            var columns = data.Data.First().Keys.ToList();
            
            html.AppendLine("  <table>");
            html.AppendLine("    <thead>");
            html.AppendLine("      <tr>");
            foreach (var column in columns)
            {
                html.AppendLine($"        <th>{column}</th>");
            }
            html.AppendLine("      </tr>");
            html.AppendLine("    </thead>");
            html.AppendLine("    <tbody>");

            foreach (var row in data.Data)
            {
                html.AppendLine("      <tr>");
                foreach (var column in columns)
                {
                    var value = row.ContainsKey(column) ? row[column]?.ToString() ?? "" : "";
                    html.AppendLine($"        <td>{value}</td>");
                }
                html.AppendLine("      </tr>");
            }

            html.AppendLine("    </tbody>");
            html.AppendLine("  </table>");
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return await Task.FromResult(Encoding.UTF8.GetBytes(html.ToString()));
    }
}
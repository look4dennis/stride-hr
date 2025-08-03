using StrideHR.Core.Models.DataImportExport;

namespace StrideHR.Core.Interfaces.Services;

public interface IDataImportExportService
{
    // Import Operations
    Task<ValidationResultDto> ValidateImportDataAsync(ImportRequestDto request);
    Task<ImportResultDto> ImportDataAsync(ImportRequestDto request);
    Task<ImportResultDto> ImportEmployeesAsync(ImportRequestDto request);
    Task<ImportResultDto> ImportAttendanceAsync(ImportRequestDto request);
    Task<ImportResultDto> ImportLeaveRequestsAsync(ImportRequestDto request);
    Task<ImportResultDto> ImportProjectsAsync(ImportRequestDto request);
    
    // Export Operations
    Task<ExportResultDto> ExportDataAsync(ExportRequestDto request);
    Task<ExportResultDto> ExportEmployeesAsync(ExportRequestDto request);
    Task<ExportResultDto> ExportAttendanceAsync(ExportRequestDto request);
    Task<ExportResultDto> ExportLeaveRequestsAsync(ExportRequestDto request);
    Task<ExportResultDto> ExportPayrollAsync(ExportRequestDto request);
    Task<ExportResultDto> ExportProjectsAsync(ExportRequestDto request);
    
    // Data Migration Operations
    Task<ImportResultDto> MigrateDataAsync(DataMigrationRequestDto request);
    
    // Utility Operations
    Task<byte[]> GenerateImportTemplateAsync(string entityType);
    Task<List<string>> GetSupportedEntityTypesAsync();
    Task<Dictionary<string, string>> GetEntityFieldMappingsAsync(string entityType);
}